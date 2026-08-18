using System;
using System.Threading.Tasks;
using System.Windows;

namespace MentumLauncher
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Cargar tema guardado antes de mostrar cualquier ventana
            bool isDark = ThemeManager.LoadSavedTheme();
            ThemeManager.Apply(isDark);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            await UpdateChecker.CheckForUpdatesAsync(mainWindow);
        }
    }
}
