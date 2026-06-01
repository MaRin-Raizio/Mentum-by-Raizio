using System;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MentumLauncher
{
    public partial class MainWindow : Window
    {
        private bool _isRunning = false;

        // ══ Flash en barra de tareas (WinAPI) ══
        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_ALL = 3; // parpadea ícono + barra
        private const uint FLASHW_TIMERNOFG = 12; // parpadea hasta que la ventana tome foco

        private void FlashTaskbar()
        {
            var fwi = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = 5,
                dwTimeout = 0
            };
            FlashWindowEx(ref fwi);
        }

        // ══ Notificación al terminar una operación ══
        private void NotifyCompletion(bool success)
        {
            // 1. Sonido del sistema
            if (success)
                SystemSounds.Asterisk.Play();   // sonido de información ✅
            else
                SystemSounds.Exclamation.Play(); // sonido de advertencia ⚠️

            // 2. Restaurar ventana si está minimizada
            if (this.WindowState == WindowState.Minimized)
                this.WindowState = WindowState.Normal;

            // 3. Flash en barra de tareas (llama atención si el usuario cambió de ventana)
            this.Activate();
            FlashTaskbar();
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            //this.Loaded += async (s, e) => await UpdateChecker.CheckForUpdatesAsync(this);
        }

        // ══ Barra de título personalizada ══
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Logo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/MaRin-Raizio/Mentum-by-Raizio",
                    UseShellExecute = true
                });
                AppendLog("🌐 Abriendo repositorio en GitHub...");
            }
            catch (Exception ex)
            {
                AppendLog("❌ No se pudo abrir el repositorio: " + ex.Message);
            }
        }

        // ══ Helper principal para ejecutar comandos ══
        private async Task<bool> RunCommand(string cmd, string description)
        {
            if (_isRunning)
            {
                AppendLog("⚠️ Ya hay un proceso en ejecución. Espera a que termine.");
                return false;
            }

            _isRunning = true;
            SetButtonsEnabled(false);

            try
            {
                AppendLog($"▶ Ejecutando: {cmd}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C " + cmd,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    }
                };

                process.Start();

                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await Task.Run(() => process.WaitForExit());

                int exitCode = process.ExitCode;

                if (exitCode == 0)
                {
                    AppendLog($"✅ {description} ejecutado correctamente.");
                    return true;
                }
                else
                {
                    AppendLog($"❌ {description} devolvió un error (código {exitCode}).");
                    if (!string.IsNullOrWhiteSpace(error))
                        AppendLog("Detalles: " + error.Trim());
                    else if (!string.IsNullOrWhiteSpace(output))
                        AppendLog("Detalles: " + output.Trim());
                    return false;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"❌ Error inesperado al ejecutar '{cmd}': {ex.Message}");
                return false;
            }
            finally
            {
                _isRunning = false;
                SetButtonsEnabled(true);
            }
        }

        // ══ Métodos auxiliares ══

        public void AppendLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogBox.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            LogBox.ScrollToEnd();
        }

        public void LogExternalAction(string message)
        {
            AppendLog("🌐 " + message);
        }

        private void AdvanceProgress(int step)
        {
            ProgressBar.Value += step;
            if (ProgressBar.Value > ProgressBar.Maximum)
                ProgressBar.Value = ProgressBar.Maximum;
        }

        private void UpdateProgressColor(bool success)
        {
            if (ProgressBar.Template.FindName("PART_Indicator", ProgressBar) is Rectangle indicator)
            {
                indicator.Fill = success
                    ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
                    : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            BtnQuickRepair.IsEnabled = enabled;
            BtnDeepScan.IsEnabled = enabled;
            BtnRestoreImage.IsEnabled = enabled;
            BtnResetNetwork.IsEnabled = enabled;
            BtnDiskCheck.IsEnabled = enabled;
            BtnDiskOptimization.IsEnabled = enabled;
            BtnTempCleanup.IsEnabled = enabled;
            BtnFullMaintenance.IsEnabled = enabled;
            BtnSystemInfo.IsEnabled = enabled;
            BtnAvanzado.IsEnabled = enabled;
        }

        private bool _closingMessageShown = false;

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Evita que se dispare dos veces
            if (_closingMessageShown) return;
            _closingMessageShown = true;

            AppendLog("Gracias por creer en Mentum.");
            AppendLog("Este proyecto es mi regalo al mundo, mi manera de dejar una huella.");
            AppendLog("En la depresión encontré refugio en estas líneas de código, y hoy se transforman en esperanza compartida.");
            AppendLog("No puedo cambiar el mundo, pero puedo dejar esta chispa, este recordatorio de que incluso lo pequeño importa.");
            AppendLog("Hasta pronto… y que cada reparación sea también un nuevo comienzo.");
            AppendLog("- MaRin Raizio");

            var ventana = new VentanaMensaje(
                titulo: "Mentum — Hasta pronto",
                mensaje: "Gracias por creer en Mentum.\n" +
                                 "Este proyecto es mi regalo al mundo, mi manera de dejar una huella.\n\n" +
                                 "En la depresión encontré refugio en estas líneas de código, y hoy se transforman en esperanza compartida.\n" +
                                 "No puedo cambiar el mundo, pero puedo dejar esta chispa, este recordatorio de que incluso lo pequeño importa.\n\n" +
                                 "Hasta pronto… y que cada reparación sea también un nuevo comienzo.\n" +
                                 "— MaRin Raizio",
                tipo: MensajeTipo.Despedida,
                mostrarCancelar: false
            );
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        // ══ Botones de acción ══

        private async void BtnQuickRepair_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            int step = 100 / 2;
            bool allOk = true;

            allOk &= await RunCommand("sfc /scannow", "Verificación de archivos del sistema");
            AdvanceProgress(step);
            allOk &= await RunCommand("DISM /Online /Cleanup-Image /CheckHealth", "Chequeo rápido de imagen DISM");
            AdvanceProgress(step);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Reparación rápida completada." : "⚠️ Reparación rápida finalizada con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnRestoreImage_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = await RunCommand("DISM /Online /Cleanup-Image /RestoreHealth", "Restauración de imagen DISM");
            AdvanceProgress(100);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Restauración de imagen DISM completada." : "⚠️ Restauración finalizada con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnTempCleanup_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = await RunCommand("cleanmgr /sagerun:1", "Limpieza de archivos temporales");
            AdvanceProgress(100);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Limpieza completada." : "⚠️ Limpieza finalizada con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnResetNetwork_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            int step = 100 / 5;
            bool allOk = true;

            allOk &= await RunCommand("ipconfig /release", "Liberación de IP"); AdvanceProgress(step);
            allOk &= await RunCommand("ipconfig /renew", "Renovación de IP"); AdvanceProgress(step);
            allOk &= await RunCommand("ipconfig /flushdns", "Flush DNS"); AdvanceProgress(step);
            allOk &= await RunCommand("netsh winsock reset", "Reset Winsock"); AdvanceProgress(step);
            allOk &= await RunCommand("netsh int ip reset", "Reset TCP/IP"); AdvanceProgress(step);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Reinicio de red completado." : "⚠️ Reinicio de red finalizado con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnDeepScan_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = await RunCommand("DISM /Online /Cleanup-Image /ScanHealth", "Escaneo profundo DISM");
            AdvanceProgress(100);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Escaneo profundo completado." : "⚠️ Escaneo finalizado con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnDiskCheck_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = await RunCommand("chkdsk C: /F /R", "Comprobación de disco CHKDSK");
            AdvanceProgress(100);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Comprobación de disco completada." : "⚠️ CHKDSK finalizado con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnDiskOptimization_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = await RunCommand("defrag C: /O", "Optimización de disco");
            AdvanceProgress(100);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Optimización completada." : "⚠️ Optimización finalizada con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnFullMaintenance_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            int step = 100 / 4;
            bool allOk = true;

            allOk &= await RunCommand("sfc /scannow", "Verificación de archivos del sistema"); AdvanceProgress(step);
            allOk &= await RunCommand("DISM /Online /Cleanup-Image /RestoreHealth", "Restauración de imagen DISM"); AdvanceProgress(step);
            allOk &= await RunCommand("chkdsk C: /F /R", "Comprobación de disco CHKDSK"); AdvanceProgress(step);
            allOk &= await RunCommand("defrag C: /O", "Optimización de disco"); AdvanceProgress(step);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Mantenimiento completo finalizado." : "⚠️ Mantenimiento completo finalizado con errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnSystemInfo_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = await RunCommand("systeminfo", "Información del sistema");
            AdvanceProgress(100);

            UpdateProgressColor(allOk);
            AppendLog(allOk ? "✅ Información del sistema mostrada." : "⚠️ Información del sistema finalizada con errores.");

            VentanaInfoSistema ventana = new VentanaInfoSistema();
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void BtnAvanzado_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("▶ Opciones avanzadas abiertas.");
            VentanaAvanzada ventana = new VentanaAvanzada(this);
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("▶ Cerrando Mentum... Gracias por creer :)");
            this.Close();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
