using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MentumLauncher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing; // despedida al cerrar


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


        // ✅ Helper único para ejecutar comandos con descripción
        private async Task<bool> RunCommand(string cmd, string description)
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

            process.WaitForExit();
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
                {
                    AppendLog("Detalles: " + error.Trim());
                }
                else if (!string.IsNullOrWhiteSpace(output))
                {
                    AppendLog("Detalles: " + output.Trim());
                }

                return false;
            }
        }

        // 🔧 Métodos auxiliares
        public void AppendLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogBox.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            LogBox.ScrollToEnd();
        }

        private void UpdateProgressColor(Brush brush)
        {
            ProgressBar.Foreground = brush;
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

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppendLog("Gracias por creer en Mentum.");
            AppendLog("Este proyecto es mi regalo al mundo, mi manera de dejar una huella.");
            AppendLog("En la depresión encontré refugio en estas líneas de código, y hoy se transforman en esperanza compartida.");
            AppendLog("No puedo cambiar el mundo, pero puedo dejar esta chispa, este recordatorio de que incluso lo pequeño importa.");
            AppendLog("Hasta pronto… y que cada reparación sea también un nuevo comienzo.");
            AppendLog("- MaRin Raizio");

            MessageBox.Show("Gracias por creer en Mentum.\n" +
                            "Este proyecto es mi regalo al mundo, mi manera de dejar una huella.\n" +
                            "En la depresión encontré refugio en estas líneas de código, y hoy se transforman en esperanza compartida.\n" +
                            "No puedo cambiar el mundo, pero puedo dejar esta chispa, este recordatorio de que incluso lo pequeño importa.\n" +
                            "Hasta pronto… y que cada reparación sea también un nuevo comienzo.\n" +
                            "- MaRin Raizio",
                            "Mentum Launcher",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        // ✅ Botón: Reparación rápida (2 comandos)
        private async void BtnQuickRepair_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            int step = 100 / 2;
            bool allOk = true;

            allOk &= await RunCommand("sfc /scannow", "Verificación de archivos del sistema"); AdvanceProgress(step);
            allOk &= await RunCommand("DISM /Online /Cleanup-Image /CheckHealth", "Chequeo rápido de imagen DISM"); AdvanceProgress(step);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Reparación rápida completada." : "⚠️ Reparación rápida finalizada con errores.");
        }

        // ✅ Botón: Restaurar imagen DISM (1 comando)
        private async void BtnRestoreImage_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = true;

            allOk &= await RunCommand("DISM /Online /Cleanup-Image /RestoreHealth", "Restauración de imagen DISM");
            AdvanceProgress(100);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Restauración de imagen DISM completada." : "⚠️ Restauración de imagen DISM finalizada con errores.");
        }

        // ✅ Botón: Limpieza de temporales (1 comando)
        private async void BtnTempCleanup_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = true;

            allOk &= await RunCommand("cleanmgr /sagerun:1", "Limpieza de archivos temporales");
            AdvanceProgress(100);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Limpieza de archivos temporales completada." : "⚠️ Limpieza de temporales finalizada con errores.");
        }
        // ✅ Botón: Reinicio configuración de red (5 comandos)
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

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Reinicio de configuración de red completado." : "⚠️ Reinicio de red finalizado con errores.");
        }

        // ✅ Botón: Escaneo profundo DISM (1 comando)
        private async void BtnDeepScan_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = true;

            allOk &= await RunCommand("DISM /Online /Cleanup-Image /ScanHealth", "Escaneo profundo DISM");
            AdvanceProgress(100);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Escaneo profundo DISM completado." : "⚠️ Escaneo profundo DISM finalizado con errores.");
        }

        // ✅ Botón: CHKDSK disco (1 comando)
        private async void BtnDiskCheck_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = true;

            allOk &= await RunCommand("chkdsk C: /F /R", "Comprobación de disco CHKDSK");
            AdvanceProgress(100);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Comprobación de disco completada." : "⚠️ CHKDSK finalizado con errores.");
        }

        // ✅ Botón: Optimización de disco (1 comando)
        private async void BtnDiskOptimization_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = true;

            allOk &= await RunCommand("defrag C: /O", "Optimización de disco");
            AdvanceProgress(100);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Optimización de disco completada." : "⚠️ Optimización de disco finalizada con errores.");
        }

        // ✅ Botón: Mantenimiento completo (4 comandos)
        private async void BtnFullMaintenance_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            int step = 100 / 4;
            bool allOk = true;

            allOk &= await RunCommand("sfc /scannow", "Verificación de archivos del sistema"); AdvanceProgress(step);
            allOk &= await RunCommand("DISM /Online /Cleanup-Image /RestoreHealth", "Restauración de imagen DISM"); AdvanceProgress(step);
            allOk &= await RunCommand("chkdsk C: /F /R", "Comprobación de disco CHKDSK"); AdvanceProgress(step);
            allOk &= await RunCommand("defrag C: /O", "Optimización de disco"); AdvanceProgress(step);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Mantenimiento completo finalizado." : "⚠️ Mantenimiento completo finalizado con errores.");
        }

        // ✅ Botón: Información del sistema (1 comando)
        private async void BtnSystemInfo_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            bool allOk = true;

            allOk &= await RunCommand("systeminfo", "Información del sistema");
            AdvanceProgress(100);

            UpdateProgressColor(allOk ? Brushes.Green : Brushes.Red);
            AppendLog(allOk ? "✅ Información del sistema mostrada." : "⚠️ Información del sistema finalizada con errores.");
            // 👉 Abrir la ventana de información del sistema
            VentanaInfoSistema ventana = new VentanaInfoSistema();
            ventana.Owner = this;
            ventana.ShowDialog();

        }

        // ✅ Botón: Opciones avanzadas
        private void BtnAvanzado_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("▶ Opciones avanzadas abiertas.");
            VentanaAvanzada ventana = new VentanaAvanzada(this);
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        // ✅ Botón: Cerrar Mentum
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("▶ Cerrando Mentum... Gracias por creer :)");
            this.Close();
        }

        // ✅ Hipervínculos (TikTok, Ko-fi)
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}