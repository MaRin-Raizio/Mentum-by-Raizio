using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MentumLauncher
{
    public partial class VentanaInfoSistema : Window
    {
        private DispatcherTimer updateTimer;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private float ramTotal;
        private float netMaxMbps = 100f; // cap for progress bar

        // Red: se usa NetworkInterface en lugar de PerformanceCounter porque
        // la categoría "Network Interface" de PerformanceCounter no está
        // disponible o tiene nombres de instancia inconsistentes en algunas
        // instalaciones de Windows 10, lo que hacía que la velocidad de red
        // se quedara siempre en 0.
        private long _netPrevReceived;
        private long _netPrevSent;
        private DateTime _netPrevSampleTime;
        private bool _netAvailable;

        // Cliente reutilizable para la prueba de velocidad manual
        private static readonly HttpClient _speedTestClient =
            new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public VentanaInfoSistema()
        {
            InitializeComponent();

            // Inicializar contadores
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            ramTotal = GetTotalMemoryInMBytes();

            // Red: lectura inicial de referencia con NetworkInterface.
            // Se suman todas las interfaces activas y no-loopback para
            // que funcione sin importar cómo Windows haya nombrado el
            // adaptador (WiFi, Ethernet, virtuales, etc.).
            try
            {
                (long recv, long sent) = GetNetworkTotals();
                _netPrevReceived = recv;
                _netPrevSent = sent;
                _netPrevSampleTime = DateTime.Now;
                _netAvailable = true;
            }
            catch
            {
                _netAvailable = false;
            }

            // Timer para actualizar cada segundo
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(1);
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // Mostrar discos
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    DiskList.Items.Add(
                        $"Disco {drive.Name} - Total: {drive.TotalSize / (1024 * 1024 * 1024)} GB, Libre: {drive.AvailableFreeSpace / (1024 * 1024 * 1024)} GB"
                    );
                }
            }

            // Mostrar información adicional del sistema
            ShowSystemDetails();

            // Enganchar evento Closing para capturar cierre con la X
            this.Closing += VentanaInfoSistema_Closing;
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // CPU
            float cpuUsage = cpuCounter.NextValue();
            CpuBar.Value = cpuUsage;
            CpuLabel.Text = $"{cpuUsage:F1}%";

            // RAM
            float ramAvailable = ramCounter.NextValue();
            float ramUsed = ramTotal - ramAvailable;
            float ramUsagePercent = (ramUsed / ramTotal) * 100;

            RamBar.Value = ramUsagePercent;
            RamLabel.Text = $"{ramUsed:F0} MB usados / {ramTotal:F0} MB totales";

            // Red
            if (_netAvailable)
            {
                try
                {
                    (long recvNow, long sentNow) = GetNetworkTotals();
                    double elapsed = (DateTime.Now - _netPrevSampleTime).TotalSeconds;
                    if (elapsed <= 0) elapsed = 1;

                    float recvBytes = (float)Math.Max(0, (recvNow - _netPrevReceived) / elapsed);
                    float sentBytes = (float)Math.Max(0, (sentNow - _netPrevSent) / elapsed);

                    string recvStr = recvBytes >= 1024 * 1024
                        ? $"{recvBytes / (1024f * 1024f):F1} MB/s"
                        : $"{recvBytes / 1024f:F0} KB/s";
                    string sentStr = sentBytes >= 1024 * 1024
                        ? $"{sentBytes / (1024f * 1024f):F1} MB/s"
                        : $"{sentBytes / 1024f:F0} KB/s";
                    NetLabel.Text = $"↓ {recvStr}  ↑ {sentStr}";

                    float totalMb = (recvBytes + sentBytes) / (1024f * 1024f);
                    NetBar.Value = Math.Min((totalMb / netMaxMbps) * 100f, 100f);

                    _netPrevReceived = recvNow;
                    _netPrevSent = sentNow;
                    _netPrevSampleTime = DateTime.Now;
                }
                catch
                {
                    NetLabel.Text = "↓ -- KB/s  ↑ -- KB/s";
                }
            }
            else
            {
                NetLabel.Text = "Sin datos de red disponibles";
            }
        }

        // ══ Chequeo rápido de conectividad ══
        // Antes de lanzar la prueba completa (que puede tardar hasta 20s por
        // URL si no hay respuesta), se hace una verificación corta con un
        // límite de 3 segundos. Se usa el mismo dominio que la prueba
        // principal para no dar falsos negativos si, por ejemplo, un
        // firewall bloquea ping/ICMP pero sí permite tráfico HTTP normal.
        private async Task<bool> QuickConnectivityCheckAsync()
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(3));
                using var response = await _speedTestClient.GetAsync(
                    "https://speed.cloudflare.com/__down?bytes=1000",
                    HttpCompletionOption.ResponseHeadersRead,
                    cts.Token);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ══ Prueba de velocidad manual (descarga real, no estimación) ══
        private async void BtnSpeedTest_Click(object sender, RoutedEventArgs e)
        {
            BtnSpeedTest.IsEnabled = false;
            SpeedTestResult.Text = "Verificando conexión...";

            bool hasConnection = await QuickConnectivityCheckAsync();
            if (!hasConnection)
            {
                SpeedTestResult.Text = "Sin conexión a internet detectada. Verifica tu red e intenta de nuevo.";
                BtnSpeedTest.IsEnabled = true;
                return;
            }

            SpeedTestResult.Text = "Probando descarga...";

            // Se intenta primero con Cloudflare; si falla, se usa un
            // respaldo por si esa URL está bloqueada en la red del usuario.
            string[] urls = {
                "https://speed.cloudflare.com/__down?bytes=15000000",
                "https://proof.ovh.net/files/10Mb.dat"
            };

            Exception? lastError = null;

            foreach (var url in urls)
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    using var response = await _speedTestClient.GetAsync(
                        url, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    long totalRead = 0;
                    var buffer = new byte[81920];
                    using var stream = await response.Content.ReadAsStreamAsync();
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        totalRead += read;
                    }
                    sw.Stop();

                    double seconds = sw.Elapsed.TotalSeconds;
                    double mbps = seconds > 0
                        ? (totalRead * 8.0 / 1_000_000.0) / seconds
                        : 0;

                    SpeedTestResult.Text = $"Resultado: {mbps:F1} Mbps de descarga";
                    BtnSpeedTest.IsEnabled = true;
                    return; // éxito, no probar más URLs
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    // se intenta con la siguiente URL de la lista
                }
            }

            // Si llegamos aquí, ninguna URL funcionó: mostrar el motivo real
            string detalle = lastError?.InnerException?.Message ?? lastError?.Message ?? "error desconocido";
            SpeedTestResult.Text = "Error: " + detalle;
            BtnSpeedTest.IsEnabled = true;
        }

        // Suma el tráfico acumulado de todas las interfaces de red activas
        // (excluyendo loopback), evitando así depender de PerformanceCounter.
        private static (long received, long sent) GetNetworkTotals()
        {
            long totalReceived = 0;
            long totalSent = 0;

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up
                          && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var ni in interfaces)
            {
                try
                {
                    var stats = ni.GetIPv4Statistics();
                    totalReceived += stats.BytesReceived;
                    totalSent += stats.BytesSent;
                }
                catch { /* interfaz sin estadísticas disponibles: se omite */ }
            }

            return (totalReceived, totalSent);
        }

        private float GetTotalMemoryInMBytes()
        {
            float totalMemory = 0;
            var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                totalMemory = Convert.ToSingle(obj["TotalPhysicalMemory"]) / (1024 * 1024);
            }
            return totalMemory;
        }

        private void ShowSystemDetails()
        {
            try
            {
                // Procesador
                var cpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                foreach (ManagementObject obj in cpuSearcher.Get())
                {
                    SystemDetailsList.Items.Add($"Procesador: {obj["Name"]}");
                }

                // Arquitectura
                var archSearcher = new ManagementObjectSearcher("SELECT SystemType FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in archSearcher.Get())
                {
                    SystemDetailsList.Items.Add($"Arquitectura: {obj["SystemType"]}");
                }

                // Versión de Windows
                var osSearcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in osSearcher.Get())
                {
                    SystemDetailsList.Items.Add($"Sistema operativo: {obj["Caption"]} (Versión {obj["Version"]})");
                }

                // Tarjetas gráficas
                var gpuSearcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (ManagementObject obj in gpuSearcher.Get())
                {
                    SystemDetailsList.Items.Add($"Tarjeta gráfica: {obj["Name"]}");
                }
            }
            catch (Exception ex)
            {
                SystemDetailsList.Items.Add($"Error al obtener detalles del sistema: {ex.Message}");
            }
        }

        private void BtnRegresar_Click(object? sender, RoutedEventArgs e)
        {
            if (this.Owner is MainWindow main)
            {
                main.LogExternalAction("Volviendo al menú principal desde Información del sistema...");
            }
            this.Close();
        }

        // Evento Closing para capturar cierre con la X
        private void VentanaInfoSistema_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            updateTimer?.Stop();
            cpuCounter?.Dispose();
            ramCounter?.Dispose();
            if (this.Owner is MainWindow main)
            {
                main.LogExternalAction("Volviendo al menú principal desde Información del sistema...");
            }
        }
    }
}