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

            // Obtiene automáticamente la versión actual del ejecutable
            string currentVersion = Assembly.GetExecutingAssembly()
                                            .GetName()
                                            .Version
                                            .ToString();

            // Llama al checker con la versión actual
            await UpdateChecker.CheckForUpdatesAsync(currentVersion);

            // Aquí puedes continuar con la inicialización normal del launcher
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}