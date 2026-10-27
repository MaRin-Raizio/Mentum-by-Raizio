using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MentumLauncher
{
    // ══ Modelo de una fila de la lista: encabezado de sección o comando ══
    public class CommandEntry
    {
        public bool IsHeader { get; set; }
        public bool IsCommand => !IsHeader;
        public string Category { get; set; } = "";
        public string Command { get; set; } = "";
        public string Description { get; set; } = "";

        // Comandos que abren su propia ventana (winver, msinfo32, dxdiag, etc.)
        // no deben capturarse ni bloquear Mentum mientras el usuario los usa.
        public bool IsGuiApp { get; set; }
    }

    public partial class VentanaAvanzada : Window
    {
        private MainWindow _mainWindow;
        private List<CommandEntry> _master = new List<CommandEntry>();

        // El aviso de modo avanzado solo se muestra una vez por sesión de
        // Mentum, sin importar cuántas veces se abra esta ventana.
        private static bool _warningShownThisSession = false;

        // Palabras clave que indican que el equipo necesita reiniciarse
        // para que un cambio se aplique por completo.
        private static readonly string[] RebootKeywords = {
            "restart", "reboot", "reinici", "se reiniciar",
            "next time the system starts", "next time windows starts",
            "programad", "scheduled", "schedule this volume"
        };

        private static bool ContainsRebootKeyword(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return RebootKeywords.Any(k => text.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public VentanaAvanzada(MainWindow mainWindow)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
                BuildCommandList();
                ApplyFilter("");

                // El aviso se muestra en Loaded, no aquí: asignar Owner a
                // una ventana que todavía no se ha mostrado (ShowDialog aún
                // no se ha llamado sobre esta misma ventana) lanza
                // "No se puede establecer la propiedad Owner en una Window
                // que no se ha mostrado previamente."
                this.Loaded += VentanaAvanzada_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al abrir VentanaAvanzada:\n\n" + ex.Message + "\n\n" + ex.InnerException?.Message,
                    "Error de inicialización",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                this.Close();
            }
        }

        private void VentanaAvanzada_Loaded(object sender, RoutedEventArgs e)
        {
            ShowSessionWarningIfNeeded();
        }

        private void ShowSessionWarningIfNeeded()
        {
            if (_warningShownThisSession) return;
            _warningShownThisSession = true;

            var aviso = new VentanaMensaje(
                titulo: "Modo avanzado",
                mensaje: "Estás en modo avanzado. Estos comandos se ejecutan directamente " +
                         "sin protecciones adicionales — algunos pueden modificar la configuración " +
                         "de red, el firewall o archivos del sistema.\n\n" +
                         "Procede solo si sabes lo que hace cada comando.",
                tipo: MensajeTipo.Advertencia,
                mostrarCancelar: false);
            aviso.Owner = this;
            aviso.ShowDialog();
        }

        // ══ Lista maestra de comandos, organizada por categoría ══
        private void BuildCommandList()
        {
            var list = new List<CommandEntry>();

            void Header(string cat) => list.Add(new CommandEntry { IsHeader = true, Category = cat });
            void Cmd(string cmd, string desc, bool gui = false) =>
                list.Add(new CommandEntry { Command = cmd, Description = desc, IsGuiApp = gui });

            Header("REPARACIÓN DEL SISTEMA");
            Cmd("sfc /scannow", "repara archivos del sistema");
            Cmd("sfc /verifyonly", "solo verifica, no repara");
            Cmd("DISM /Online /Cleanup-Image /CheckHealth", "chequeo rápido de la imagen");
            Cmd("DISM /Online /Cleanup-Image /ScanHealth", "escaneo más profundo");
            Cmd("DISM /Online /Cleanup-Image /RestoreHealth", "repara la imagen de Windows");
            Cmd("DISM /Online /Cleanup-Image /AnalyzeComponentStore", "analiza el almacén de componentes");
            Cmd("DISM /Online /Cleanup-Image /StartComponentCleanup", "limpia versiones viejas de componentes");
            Cmd("DISM /Online /Cleanup-Image /RevertPendingActions", "revierte acciones pendientes que bloquean el arranque");

            Header("DISCO");
            Cmd("chkdsk C: /F /R", "repara errores del disco (con reinicio)");
            Cmd("chkdsk C: /scan", "escaneo en segundo plano, sin reiniciar");
            Cmd("defrag C: /O", "optimiza automáticamente (HDD o SSD)");
            Cmd("defrag C: /A", "solo analiza, no optimiza");
            Cmd("fsutil behavior query DisableDeleteNotify", "verifica si TRIM está activo (SSD)");
            Cmd("fsutil dirty query C:", "verifica si el volumen está marcado como \"sucio\"");
            Cmd("wmic diskdrive get status,model,size", "estado físico del disco");
            Cmd("wmic logicaldisk get size,freespace,caption", "espacio libre por unidad");
            Cmd("wmic partition get name,size,type", "info detallada de particiones");
            Cmd("echo list disk | diskpart", "lista los discos del equipo");
            Cmd("echo list volume | diskpart", "lista los volúmenes del equipo");

            Header("LIMPIEZA");
            Cmd("cleanmgr /sagerun:1", "limpieza estándar de Windows");
            Cmd("cleanmgr /d C:", "abre el liberador de espacio en disco para C:");
            Cmd(@"rd /s /q %systemroot%\SoftwareDistribution\Download", "limpia caché de Windows Update");
            Cmd(@"del /q /f /s %TEMP%\*", "limpieza rápida de archivos temporales");
            Cmd("wevtutil cl Application", "limpia el registro de eventos de aplicación");

            Header("RED");
            Cmd("ipconfig /all", "toda la configuración de red");
            Cmd("ipconfig /release", "libera la IP actual");
            Cmd("ipconfig /renew", "solicita nueva IP");
            Cmd("ipconfig /flushdns", "limpia caché DNS");
            Cmd("ipconfig /displaydns", "muestra la caché DNS actual");
            Cmd("netsh winsock reset", "repara el catálogo Winsock");
            Cmd("netsh int ip reset", "reinicia la pila TCP/IP");
            Cmd("netsh wlan show profiles", "perfiles de WiFi guardados");
            Cmd("netstat -ano", "conexiones activas con PID");
            Cmd("arp -a", "tabla de direcciones ARP");
            Cmd("nslookup google.com", "prueba de resolución DNS");
            Cmd("ping 8.8.8.8 -n 4", "prueba de conectividad básica");
            Cmd("netsh interface show interface", "estado de las interfaces de red");
            Cmd("tracert 8.8.8.8", "rastreo de ruta (puede tardar 1-3 min)");
            Cmd("netsh advfirewall reset", "reinicia la configuración del firewall");
            Cmd("netsh interface ipv4 show subinterfaces", "estadísticas de interfaces");
            Cmd("pathping 8.8.8.8", "diagnóstico combinado (puede tardar 1-3 min)");

            Header("SISTEMA Y ARRANQUE");
            Cmd("systeminfo", "información completa del sistema");
            Cmd("bcdedit", "configuración de arranque");
            Cmd("bcdedit /enum all", "lista completa de entradas de arranque");
            Cmd("gpupdate /force", "fuerza actualización de políticas de grupo");
            Cmd("whoami /all", "usuario actual y sus permisos");
            Cmd("tasklist", "procesos en ejecución");
            Cmd("driverquery", "lista de drivers instalados");
            Cmd("schtasks /query /fo LIST", "tareas programadas");
            Cmd("wmic os get caption,version,buildnumber", "versión exacta de Windows");
            Cmd(@"reg query HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "programas que arrancan con Windows");
            Cmd("eventvwr", "abre el visor de eventos", gui: true);

            Header("ENERGÍA");
            Cmd("powercfg /energy", "reporte de eficiencia energética");
            Cmd("powercfg /batteryreport", "reporte de batería (laptops)");
            Cmd("powercfg /a", "estados de suspensión disponibles");
            Cmd("powercfg /devicequery wake_armed", "dispositivos que pueden despertar el equipo");
            Cmd("powercfg /sleepstudy", "informe detallado de suspensión");
            Cmd("powercfg /lastwake", "qué dispositivo despertó el equipo");
            Cmd("perfmon /report", "informe de rendimiento (tarda ~60s)", gui: true);

            Header("INFORMACIÓN DE HARDWARE");
            Cmd("winver", "versión de Windows", gui: true);
            Cmd("msinfo32", "información detallada del sistema", gui: true);
            Cmd("dxdiag", "diagnóstico gráfico y de sonido", gui: true);
            Cmd("wmic cpu get name,numberofcores,maxclockspeed", "datos del procesador");
            Cmd("wmic memorychip get capacity,speed", "módulos de RAM instalados");
            Cmd("wmic bios get serialnumber", "número de serie del equipo");
            Cmd("wmic baseboard get product,manufacturer,serialnumber", "datos de la tarjeta madre");
            Cmd("wmic nic get name,macaddress", "adaptadores de red");
            Cmd("wmic printer get name,status", "estado de impresoras");

            _master = list;
        }

        // ══ Filtro de búsqueda: conserva encabezados solo si su sección
        //    tiene al menos un comando que coincide ══
        private void ApplyFilter(string query)
        {
            query = (query ?? "").Trim();

            List<CommandEntry> visible;
            if (string.IsNullOrEmpty(query))
            {
                visible = _master;
            }
            else
            {
                visible = new List<CommandEntry>();
                string currentCategory = "";
                var pendingHeader = default(CommandEntry);
                bool categoryHasMatch = false;

                foreach (var entry in _master)
                {
                    if (entry.IsHeader)
                    {
                        // cerrar la categoría anterior
                        if (pendingHeader != null && categoryHasMatch)
                            visible.Add(pendingHeader);

                        pendingHeader = entry;
                        currentCategory = entry.Category;
                        categoryHasMatch = false;
                    }
                    else
                    {
                        bool matches =
                            entry.Command.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                            entry.Description.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;

                        if (matches)
                        {
                            if (pendingHeader != null && !categoryHasMatch)
                            {
                                visible.Add(pendingHeader);
                                categoryHasMatch = true;
                            }
                            visible.Add(entry);
                        }
                    }
                }
            }

            ListaComandos.ItemsSource = visible;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            TxtBuscarPlaceholder.Visibility = string.IsNullOrEmpty(TxtBuscar.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            ApplyFilter(TxtBuscar.Text);
        }

        private void ListaComandos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnEjecutar.IsEnabled = ListaComandos.SelectedItem is CommandEntry entry && entry.IsCommand;
        }

        //botones nuevos 

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // ══ Ejecutar el comando seleccionado ══
        // Reescrito como async para no congelar Mentum mientras el comando
        // corre — antes se ejecutaba de forma síncrona en el hilo de UI,
        // lo cual dejaba la ventana completamente inmóvil, especialmente
        // grave con comandos que abren su propia ventana (winver, dxdiag,
        // msinfo32, eventvwr), donde Mentum se quedaba "congelado" hasta
        // que el usuario cerraba esa otra ventana.
        private async void BtnEjecutar_Click(object sender, RoutedEventArgs e)
        {
            if (!(ListaComandos.SelectedItem is CommandEntry entry) || entry.IsHeader)
            {
                _mainWindow.AppendLog("[INFO] Selecciona un comando de la lista antes de ejecutar.");
                return;
            }

            string comando = entry.Command;
            BtnEjecutar.IsEnabled = false;

            try
            {
                if (entry.IsGuiApp)
                {
                    await LaunchGuiAppAsync(comando);
                }
                else
                {
                    await RunCapturedCommandAsync(comando);
                }
            }
            finally
            {
                BtnEjecutar.IsEnabled = ListaComandos.SelectedItem is CommandEntry e2 && e2.IsCommand;
            }
        }

        // Para comandos que abren su propia ventana: se lanzan y no se
        // espera a que el usuario los cierre, así Mentum sigue respondiendo.
        private Task LaunchGuiAppAsync(string comando)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + comando,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
                _mainWindow.AppendLog($"[OK] Abriendo: {comando} (no bloquea Mentum)");
            }
            catch (Exception ex)
            {
                _mainWindow.AppendLog($"[EXCEPTION] {comando}\n{ex.Message}");
            }
            return Task.CompletedTask;
        }

        // Para comandos normales: se ejecutan de forma asíncrona y se
        // captura su salida, igual que en la ventana principal.
        private async Task RunCapturedCommandAsync(string comando)
        {
            Process? process = null;
            try
            {
                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C " + comando,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.Start();

                // Alimenta "Y" por si el comando pregunta algo por consola
                // (p. ej. confirmaciones); se ignora sin efecto si no aplica.
                try { await process.StandardInput.WriteLineAsync("Y"); }
                catch { }
                finally { try { process.StandardInput.Close(); } catch { } }

                string salida = await process.StandardOutput.ReadToEndAsync();
                string errores = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit());

                if (!string.IsNullOrWhiteSpace(salida))
                    _mainWindow.AppendLog($"[OK] {comando}\n{salida}");

                if (!string.IsNullOrWhiteSpace(errores))
                    _mainWindow.AppendLog($"[ERROR] {comando}\n{errores}");

                if (ContainsRebootKeyword(salida) || ContainsRebootKeyword(errores))
                    _mainWindow.AppendLog("🔄 Es posible que necesites reiniciar el equipo para que este cambio se aplique por completo.");
            }
            catch (Exception ex)
            {
                _mainWindow.AppendLog($"[EXCEPTION] {comando}\n{ex.Message}");
            }
        }

        // Cerrar ventana avanzada y regresar al menú principal
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
