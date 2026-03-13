using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows;
using System.Windows.Threading;

namespace MentumLauncher
{
    public partial class VentanaInfoSistema : Window
    {
        private DispatcherTimer updateTimer;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private float ramTotal;

        public VentanaInfoSistema()
        {
            InitializeComponent();

            // Inicializar contadores
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            ramTotal = GetTotalMemoryInMBytes();

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
            if (this.Owner is MainWindow main)
            {
                main.LogExternalAction("Volviendo al menú principal desde Información del sistema...");
            }
        }
    }
}