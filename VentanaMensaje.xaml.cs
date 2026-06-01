using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MentumLauncher
{
    // Tipos de ícono disponibles
    public enum MensajeTipo
    {
        Info,       // ℹ morado
        Exito,      // ✅ verde
        Advertencia,// ⚠️ ámbar
        Error,      // ❌ rojo
        Despedida   // 💙 especial para el cierre
    }

    public partial class VentanaMensaje : Window
    {
        // Resultado del diálogo (para ventanas Sí/No)
        public bool Confirmado { get; private set; } = false;

        public VentanaMensaje(
            string titulo,
            string mensaje,
            MensajeTipo tipo = MensajeTipo.Info,
            bool mostrarCancelar = false)
        {
            InitializeComponent();

            TitleText.Text   = titulo;
            MessageText.Text = mensaje;

            AplicarTipo(tipo);
            ConstruirBotones(mostrarCancelar);
        }

        private void AplicarTipo(MensajeTipo tipo)
        {
            switch (tipo)
            {
                case MensajeTipo.Exito:
                    BigIcon.Text      = "✅";
                    IconText.Text     = "✅";
                    BigIcon.Foreground  = System.Windows.Media.Brushes.Transparent;
                    // borde verde
                    ((System.Windows.Controls.Border)BigIcon.Parent).BorderBrush =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(0x16, 0x65, 0x34));
                    ((System.Windows.Controls.Border)BigIcon.Parent).Background =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(0x0D, 0x23, 0x18));
                    break;

                case MensajeTipo.Advertencia:
                    BigIcon.Text  = "⚠️";
                    IconText.Text = "⚠️";
                    ((System.Windows.Controls.Border)BigIcon.Parent).BorderBrush =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(0x78, 0x35, 0x0F));
                    ((System.Windows.Controls.Border)BigIcon.Parent).Background =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(0x1C, 0x14, 0x08));
                    break;

                case MensajeTipo.Error:
                    BigIcon.Text  = "❌";
                    IconText.Text = "❌";
                    ((System.Windows.Controls.Border)BigIcon.Parent).BorderBrush =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(0x7F, 0x1D, 0x1D));
                    ((System.Windows.Controls.Border)BigIcon.Parent).Background =
                        new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(0x20, 0x0F, 0x0F));
                    break;

                case MensajeTipo.Despedida:
                    BigIcon.Text  = "💙";
                    IconText.Text = "💙";
                    break;

                default: // Info
                    BigIcon.Text  = "ℹ";
                    IconText.Text = "ℹ";
                    break;
            }
        }

        private void ConstruirBotones(bool mostrarCancelar)
        {
            if (mostrarCancelar)
            {
                // Botón Sí (verde)
                var btnSi = new Button
                {
                    Content = "Sí, actualizar",
                    Style   = (Style)FindResource("BtnGreen"),
                    Margin  = new Thickness(0, 0, 8, 0)
                };
                btnSi.Click += (s, e) => { Confirmado = true; this.Close(); };
                ButtonPanel.Children.Add(btnSi);

                // Botón No (neutro)
                var btnNo = new Button
                {
                    Content = "Ahora no",
                    Style   = (Style)FindResource("BtnNeutral")
                };
                btnNo.Click += (s, e) => { Confirmado = false; this.Close(); };
                ButtonPanel.Children.Add(btnNo);
            }
            else
            {
                // Solo Aceptar (verde)
                var btnOk = new Button
                {
                    Content = "Aceptar",
                    Style   = (Style)FindResource("BtnGreen")
                };
                btnOk.Click += (s, e) => this.Close();
                ButtonPanel.Children.Add(btnOk);
            }
        }

        // Método estático para mostrar fácilmente desde cualquier parte
        public static bool Mostrar(
            string titulo,
            string mensaje,
            MensajeTipo tipo = MensajeTipo.Info,
            bool pregunta = false)
        {
            var ventana = new VentanaMensaje(titulo, mensaje, tipo, pregunta);
            ventana.ShowDialog();
            return ventana.Confirmado;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            this.Close();
        }
    }
}
