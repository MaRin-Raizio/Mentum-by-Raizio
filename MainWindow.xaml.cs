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
using Microsoft.Win32;

namespace MentumLauncher
{
    public partial class MainWindow : Window
    {
        private bool _isRunning = false;
        private System.Threading.CancellationTokenSource? _cts = null;
        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        private System.Windows.Threading.DispatcherTimer _timerUI = new System.Windows.Threading.DispatcherTimer();
        private DateTime _lastOperationTime = DateTime.MinValue;

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

            _timerUI.Interval = TimeSpan.FromSeconds(1);
            _timerUI.Tick += (s, e) =>
            {
                if (_stopwatch.IsRunning)
                    TimerLabel.Text = $"  {_stopwatch.Elapsed:mm\\:ss}";
            };
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
            _cts = new System.Threading.CancellationTokenSource();
            SetButtonsEnabled(false);
            BtnCancel.Visibility = System.Windows.Visibility.Visible;

            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Restart();
                _timerUI.Start();
            }

            Process? process = null;
            try
            {
                AppendLog($"▶ Ejecutando: {cmd}");

                process = new Process
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

                await Task.Run(() =>
                {
                    while (!process.HasExited)
                    {
                        if (_cts.Token.IsCancellationRequested)
                        {
                            try { process.Kill(entireProcessTree: true); } catch { }
                            break;
                        }
                        System.Threading.Thread.Sleep(200);
                    }
                });

                if (_cts.Token.IsCancellationRequested)
                {
                    AppendLog("⏹ Operación cancelada por el usuario.");
                    return false;
                }

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
                _cts?.Dispose();
                _cts = null;
                SetButtonsEnabled(true);
                BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                _stopwatch.Stop();
                _timerUI.Stop();
                _lastOperationTime = DateTime.Now;
                TimerLabel.Text = $"  última: {_stopwatch.Elapsed:mm\\:ss}";
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
            BtnDeepClean.IsEnabled = enabled;
            BtnCleanHistory.IsEnabled = enabled;
            BtnFixShortcuts.IsEnabled = enabled;
            BtnStartupScan.IsEnabled = enabled;
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
            var confirm = new VentanaMensaje("Confirmar CHKDSK", "CHKDSK puede tardar varios minutos y requerir un reinicio del sistema.\n\n¿Deseas continuar?", MensajeTipo.Advertencia, true);
            confirm.Owner = this;
            confirm.ShowDialog();
            if (!confirm.Confirmado) return;

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


        // ══ Botón: Limpieza profunda ══
        private async void BtnDeepClean_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            int step = 100 / 8;
            bool allOk = true;

            var confirmClean = new VentanaMensaje("Confirmar limpieza profunda", "Esta operación eliminará cachés, temporales, Prefetch y la papelera de reciclaje.\n\nNo se puede deshacer. ¿Deseas continuar?", MensajeTipo.Advertencia, true);
            confirmClean.Owner = this;
            confirmClean.ShowDialog();
            if (!confirmClean.Confirmado) return;

            AppendLog("🧹 Iniciando limpieza profunda del sistema...");

            // 1. Configurar cleanmgr para limpiar todo via registro
            AppendLog("▶ Configurando cleanmgr para limpieza completa...");
            await Task.Run(() =>
            {
                try
                {
                    string[] categories = {
                        "Active Setup Temp Folders", "BranchCache", "Content Indexer Cleaner",
                        "D3D Shader Cache", "Delivery Optimization Files", "Device Driver Packages",
                        "Diagnostic Data Viewer database files", "Downloaded Program Files",
                        "Internet Cache Files", "Memory Dump Files", "Offline Pages Files",
                        "Old ChkDsk Files", "Previous Installations", "Recycle Bin",
                        "RetailDemo Offline Content", "Service Pack Cleanup",
                        "Setup Log Files", "System error memory dump files",
                        "System error minidump files", "Temporary Files",
                        "Temporary Setup Files", "Temporary Sync Files",
                        "Thumbnail Cache", "Update Cleanup", "Upgrade Discarded Files",
                        "User file versions", "Windows Defender", "Windows Error Reporting Files",
                        "Windows ESD installation files", "Windows Upgrade Log Files"
                    };
                    foreach (var cat in categories)
                    {
                        try
                        {
                            using var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(
                                $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\{cat}");
                            key?.SetValue("StateFlags0064", 2, Microsoft.Win32.RegistryValueKind.DWord);
                        }
                        catch { /* categoría no existe en este sistema, se omite */ }
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AppendLog($"⚠️ Error configurando registro: {ex.Message}"));
                }
            });
            AppendLog("✅ Cleanmgr configurado.");
            AdvanceProgress(step);

            // 2. Ejecutar cleanmgr con todas las categorías
            allOk &= await RunCommand("cleanmgr /sagerun:64", "Limpieza completa con cleanmgr");
            AdvanceProgress(step);

            // 3. Detener Windows Update y limpiar caché
            allOk &= await RunCommand("net stop wuauserv", "Detener servicio Windows Update");
            AdvanceProgress(step);
            await Task.Run(() =>
            {
                try
                {
                    string wuPath = @"C:\Windows\SoftwareDistribution\Download";
                    if (System.IO.Directory.Exists(wuPath))
                    {
                        foreach (var f in System.IO.Directory.GetFiles(wuPath, "*", System.IO.SearchOption.AllDirectories))
                            try { System.IO.File.Delete(f); } catch { }
                        foreach (var d in System.IO.Directory.GetDirectories(wuPath))
                            try { System.IO.Directory.Delete(d, true); } catch { }
                    }
                    Dispatcher.Invoke(() => AppendLog("✅ Caché de Windows Update eliminada."));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => AppendLog($"⚠️ Error limpiando Windows Update: {ex.Message}"));
                }
            });
            await RunCommand("net start wuauserv", "Reiniciar servicio Windows Update");
            AdvanceProgress(step);

            // 4. Limpiar carpetas Temp
            await Task.Run(() =>
            {
                string[] tempPaths = {
                    System.IO.Path.GetTempPath(),
                    @"C:\Windows\Temp"
                };
                int deleted = 0;
                foreach (var path in tempPaths)
                {
                    if (!System.IO.Directory.Exists(path)) continue;
                    foreach (var f in System.IO.Directory.GetFiles(path))
                        try { System.IO.File.Delete(f); deleted++; } catch { }
                    foreach (var d in System.IO.Directory.GetDirectories(path))
                        try { System.IO.Directory.Delete(d, true); deleted++; } catch { }
                }
                Dispatcher.Invoke(() => AppendLog($"✅ Carpetas Temp limpiadas ({deleted} elementos eliminados)."));
            });
            AdvanceProgress(step);

            // 5. Limpiar Prefetch
            await Task.Run(() =>
            {
                string prefetch = @"C:\Windows\Prefetch";
                int deleted = 0;
                if (System.IO.Directory.Exists(prefetch))
                {
                    foreach (var f in System.IO.Directory.GetFiles(prefetch, "*.pf"))
                        try { System.IO.File.Delete(f); deleted++; } catch { }
                }
                Dispatcher.Invoke(() => AppendLog($"✅ Prefetch limpiado ({deleted} archivos eliminados)."));
            });
            AdvanceProgress(step);

            // 6. Vaciar papelera de reciclaje
            allOk &= await RunPowerShell(
                "Clear-RecycleBin -Force -ErrorAction SilentlyContinue",
                "Vaciar papelera de reciclaje");
            AdvanceProgress(step);

            // 7. Limpiar caché de miniaturas
            await Task.Run(() =>
            {
                string thumbPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Microsoft\Windows\Explorer");
                int deleted = 0;
                if (System.IO.Directory.Exists(thumbPath))
                {
                    foreach (var f in System.IO.Directory.GetFiles(thumbPath, "thumbcache_*.db"))
                        try { System.IO.File.Delete(f); deleted++; } catch { }
                }
                Dispatcher.Invoke(() => AppendLog($"✅ Caché de miniaturas limpiada ({deleted} archivos eliminados)."));
            });
            AdvanceProgress(step);

            UpdateProgressColor(allOk);
            AppendLog(allOk
                ? "✅ Limpieza profunda completada."
                : "⚠️ Limpieza profunda finalizada con algunos errores.");
            NotifyCompletion(allOk);
        }

        private async void BtnFullMaintenance_Click(object sender, RoutedEventArgs e)
        {
            var confirmFull = new VentanaMensaje("Confirmar mantenimiento completo", "El mantenimiento completo ejecuta SFC, DISM, CHKDSK y desfragmentación en secuencia.\n\nPuede tardar más de 30 minutos. ¿Deseas continuar?", MensajeTipo.Advertencia, true);
            confirmFull.Owner = this;
            confirmFull.ShowDialog();
            if (!confirmFull.Confirmado) return;

            ProgressBar.Value = 0;
            int fullStep = 100 / 6;
            bool allOk = true;

            allOk &= await RunCommand("sfc /scannow", "Verificación de archivos del sistema"); AdvanceProgress(fullStep);
            allOk &= await RunCommand("DISM /Online /Cleanup-Image /RestoreHealth", "Restauración de imagen DISM"); AdvanceProgress(fullStep);
            allOk &= await RunCommand("chkdsk C: /F /R", "Comprobación de disco CHKDSK"); AdvanceProgress(fullStep);
            allOk &= await RunCommand("defrag C: /O", "Optimización de disco"); AdvanceProgress(fullStep);
            await CleanHistoryInternal(); AdvanceProgress(fullStep);
            await FixShortcutsInternal(); AdvanceProgress(fullStep);

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


        // ══ Helper para ejecutar scripts de PowerShell directamente ══
        private async Task<bool> RunPowerShell(string script, string description)
        {
            if (_isRunning)
            {
                AppendLog("⚠️ Ya hay un proceso en ejecución. Espera a que termine.");
                return false;
            }

            _isRunning = true;
            _cts = new System.Threading.CancellationTokenSource();
            SetButtonsEnabled(false);
            BtnCancel.Visibility = System.Windows.Visibility.Visible;

            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Restart();
                _timerUI.Start();
            }

            Process? process = null;
            try
            {
                AppendLog($"▶ Ejecutando (PS): {script}");

                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoProfile -NonInteractive -EncodedCommand " + Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(script)),
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

                await Task.Run(() =>
                {
                    while (!process.HasExited)
                    {
                        if (_cts.Token.IsCancellationRequested)
                        {
                            try { process.Kill(entireProcessTree: true); } catch { }
                            break;
                        }
                        System.Threading.Thread.Sleep(200);
                    }
                });

                if (_cts.Token.IsCancellationRequested)
                {
                    AppendLog("⏹ Operación cancelada por el usuario.");
                    return false;
                }

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
                    return false;
                }
            }
            catch (Exception ex)
            {
                AppendLog($"❌ Error inesperado ejecutando PowerShell: {ex.Message}");
                return false;
            }
            finally
            {
                _isRunning = false;
                _cts?.Dispose();
                _cts = null;
                SetButtonsEnabled(true);
                BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
                _stopwatch.Stop();
                _timerUI.Stop();
                _lastOperationTime = DateTime.Now;
                TimerLabel.Text = $"  última: {_stopwatch.Elapsed:mm\\:ss}";
            }
        }

        private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Toggle();
            BtnThemeToggle.Content = ThemeManager.IsDark ? "☀" : "🌙";
            AppendLog(ThemeManager.IsDark ? "🌙 Tema oscuro activado." : "☀️ Tema claro activado.");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            AppendLog("⏹ Solicitud de cancelación enviada...");
        }

        private void BtnExportLog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filename = $"mentum_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string path = System.IO.Path.Combine(desktop, filename);

                var range = new System.Windows.Documents.TextRange(
                    LogBox.Document.ContentStart,
                    LogBox.Document.ContentEnd);
                string logText = range.Text;

                System.IO.File.WriteAllText(path, logText, Encoding.UTF8);
                AppendLog($"💾 Log exportado a: {filename}");
            }
            catch (Exception ex)
            {
                AppendLog($"❌ No se pudo exportar el log: {ex.Message}");
            }
        }


        // CleanHistory
        private async void BtnCleanHistory_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            AppendLog("🕵 Iniciando limpieza de historial...");
            await CleanHistoryInternal();
            ProgressBar.Value = 100;
            UpdateProgressColor(true);
            AppendLog("✅ Limpieza de historial completada.");
            NotifyCompletion(true);
        }

        private async Task CleanHistoryInternal()
        {
            await Task.Run(() =>
            {
                int cleaned = 0;
                string recent = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                foreach (var f in System.IO.Directory.GetFiles(recent))
                    try { System.IO.File.Delete(f); cleaned++; } catch { }
                try
                {
                    using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths", true);
                    if (key != null)
                        foreach (var v in key.GetValueNames())
                            try { key.DeleteValue(v); cleaned++; } catch { }
                }
                catch { }
                try
                {
                    using var key2 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\WordWheelQuery", true);
                    if (key2 != null)
                        foreach (var v in key2.GetValueNames())
                            try { key2.DeleteValue(v); cleaned++; } catch { }
                }
                catch { }
                try { Dispatcher.Invoke(() => System.Windows.Clipboard.Clear()); } catch { }
                string jumpLists = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Recent\AutomaticDestinations");
                if (System.IO.Directory.Exists(jumpLists))
                    foreach (var f in System.IO.Directory.GetFiles(jumpLists))
                        try { System.IO.File.Delete(f); cleaned++; } catch { }
                Dispatcher.Invoke(() => AppendLog(
                    $"✅ Historial limpiado ({cleaned} elementos eliminados)."));
            });
        }

        // FixShortcuts
        private async void BtnFixShortcuts_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Value = 0;
            AppendLog("🔗 Buscando accesos directos rotos...");
            int removed = await FixShortcutsInternal();
            ProgressBar.Value = 100;
            UpdateProgressColor(true);
            AppendLog(removed > 0
                ? $"✅ Se eliminaron {removed} accesos directos rotos."
                : "✅ No se encontraron accesos directos rotos.");
            NotifyCompletion(true);
        }

        private async Task<int> FixShortcutsInternal()
        {
            return await Task.Run(() =>
            {
                int removed = 0;
                string[] searchPaths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                };
                foreach (var folder in searchPaths)
                {
                    if (!System.IO.Directory.Exists(folder)) continue;
                    foreach (var lnk in System.IO.Directory.GetFiles(
                        folder, "*.lnk", System.IO.SearchOption.AllDirectories))
                    {
                        try
                        {
                            var shell = new IWshRuntimeLibrary.WshShell();
                            var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(lnk);
                            string target = shortcut.TargetPath;
                            if (!string.IsNullOrEmpty(target) &&
                                !System.IO.File.Exists(target) &&
                                !System.IO.Directory.Exists(target))
                            {
                                System.IO.File.Delete(lnk);
                                removed++;
                                Dispatcher.Invoke(() => AppendLog(
                                    $"  🗑 Eliminado: {System.IO.Path.GetFileName(lnk)}"));
                            }
                        }
                        catch { }
                    }
                }
                return removed;
            });
        }

        // StartupScan
        private void BtnStartupScan_Click(object sender, RoutedEventArgs e)
        {
            AppendLog("🚀 Programas configurados al inicio de Windows:");
            int found = 0;
            string[] regPaths = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
            };
            foreach (var path in regPaths)
            {
                foreach (var hive in new[] {
                    Microsoft.Win32.Registry.CurrentUser,
                    Microsoft.Win32.Registry.LocalMachine })
                {
                    try
                    {
                        using var key = hive.OpenSubKey(path);
                        if (key == null) continue;
                        foreach (var name in key.GetValueNames())
                        {
                            AppendLog($"  ▶ {name}: {key.GetValue(name)}");
                            found++;
                        }
                    }
                    catch { }
                }
            }
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (System.IO.Directory.Exists(startupFolder))
                foreach (var f in System.IO.Directory.GetFiles(startupFolder))
                {
                    AppendLog($"  📂 {System.IO.Path.GetFileName(f)}");
                    found++;
                }
            AppendLog(found > 0
                ? $"✅ {found} programas de inicio encontrados."
                : "✅ No se encontraron programas de inicio adicionales.");
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