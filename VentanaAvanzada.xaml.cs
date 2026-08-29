using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MentumLauncher
{
    public partial class VentanaAvanzada : Window
    {
        private MainWindow _mainWindow;

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

        // Constructor que recibe la referencia al MainWindow
        public VentanaAvanzada(MainWindow mainWindow)
        {
            try
            {
                InitializeComponent();
                _mainWindow = mainWindow;
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
        // Ejecutar el comando seleccionado y enviar salida al LogBox del MainWindow
        private void BtnEjecutar_Click(object sender, RoutedEventArgs e)
        {
            if (ListaComandos.SelectedItem is ListBoxItem item)
            {
                string comando = item.Content.ToString();

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + comando)
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };

                    using (Process proc = Process.Start(psi))
                    {
                        string salida = proc.StandardOutput.ReadToEnd();
                        string errores = proc.StandardError.ReadToEnd();

                        // Enviar salida al LogBox del MainWindow
                        if (!string.IsNullOrWhiteSpace(salida))
                            _mainWindow.AppendLog($"[OK] {comando}\n{salida}");

                        if (!string.IsNullOrWhiteSpace(errores))
                            _mainWindow.AppendLog($"[ERROR] {comando}\n{errores}");

                        if (ContainsRebootKeyword(salida) || ContainsRebootKeyword(errores))
                            _mainWindow.AppendLog("🔄 Es posible que necesites reiniciar el equipo para que este cambio se aplique por completo.");
                    }
                }
                catch (Exception ex)
                {
                    _mainWindow.AppendLog($"[EXCEPTION] {comando}\n{ex.Message}");
                }
            }
            else
            {
                _mainWindow.AppendLog("[INFO] Selecciona un comando de la lista antes de ejecutar.");
            }
        }

        // Cerrar ventana avanzada y regresar al menú principal
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}