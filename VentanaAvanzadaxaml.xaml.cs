using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MentumLauncher
{
    public partial class VentanaAvanzada : Window
    {
        private MainWindow _mainWindow;

        // Constructor que recibe la referencia al MainWindow
        public VentanaAvanzada(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
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