using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MentumLauncher
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 🔎 Obtiene automáticamente la versión bonita desde AssemblyInformationalVersion
            string currentVersion = UpdateChecker.GetInformationalVersion();

            // Inicialización normal del launcher
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            // Llama al checker pasando la ventana como owner
            await UpdateChecker.CheckForUpdatesAsync(mainWindow);
        }
    }
}
