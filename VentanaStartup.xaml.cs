using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace MentumLauncher
{
    public partial class VentanaStartup : Window
    {
        private const string DisabledPrefix = "MENTUM_DISABLED__";

        private MainWindow _mainWindow;
        private List<StartupEntry> _entries = new List<StartupEntry>();

        public VentanaStartup(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            CargarEntradas();
        }

        // ══ Modelo de una entrada de inicio ══
        private class StartupEntry
        {
            public string Name = "";
            public string Origin = "";       // texto legible: "Registro (usuario)", "Carpeta de inicio", etc.
            public bool IsDisabled;

            // Para entradas de registro
            public RegistryHive? Hive;
            public string RegistryPath = "";
            public string ValueName = "";    // nombre actual (puede incluir el prefijo de deshabilitado)
            public string CommandValue = "";

            // Para entradas de carpeta de inicio
            public string FilePath = "";     // ruta actual del .lnk (en la carpeta normal o en "Disabled")
            public string FolderRoot = "";   // carpeta base de inicio (sin \Disabled)

            public bool IsRegistryEntry => Hive != null;

            public override string ToString()
            {
                string icon = IsDisabled ? "⛔" : "✅";
                return $"{icon}  {Name}\n     {Origin}";
            }
        }

        // ══ Escaneo de todas las fuentes de inicio ══
        private void CargarEntradas()
        {
            _entries.Clear();
            StartupList.Items.Clear();

            string[] regPaths = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce",
            };

            foreach (var path in regPaths)
            {
                ScanRegistryHive(RegistryHive.CurrentUser, Registry.CurrentUser, path, "Registro (usuario)");
                ScanRegistryHive(RegistryHive.LocalMachine, Registry.LocalMachine, path, "Registro (equipo)");
            }

            ScanStartupFolder(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Carpeta de inicio (usuario)");
            ScanStartupFolder(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Carpeta de inicio (todos)");

            foreach (var entry in _entries)
                StartupList.Items.Add(entry.ToString());

            _mainWindow.AppendLog(_entries.Count > 0
                ? $"✅ {_entries.Count} programas de inicio encontrados."
                : "✅ No se encontraron programas de inicio adicionales.");
        }

        private void ScanRegistryHive(RegistryHive hiveEnum, RegistryKey hive, string path, string originLabel)
        {
            try
            {
                using var key = hive.OpenSubKey(path, writable: false);
                if (key == null) return;

                foreach (var valueName in key.GetValueNames())
                {
                    bool disabled = valueName.StartsWith(DisabledPrefix, StringComparison.OrdinalIgnoreCase);
                    string displayName = disabled ? valueName.Substring(DisabledPrefix.Length) : valueName;
                    string data = key.GetValue(valueName)?.ToString() ?? "";

                    _entries.Add(new StartupEntry
                    {
                        Name = displayName,
                        Origin = originLabel + (disabled ? " · deshabilitado" : ""),
                        IsDisabled = disabled,
                        Hive = hiveEnum,
                        RegistryPath = path,
                        ValueName = valueName,
                        CommandValue = data
                    });
                }
            }
            catch { /* clave no accesible: se omite sin interrumpir el escaneo */ }
        }

        private void ScanStartupFolder(string folder, string originLabel)
        {
            try
            {
                if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;

                // Activos: archivos directamente en la carpeta
                foreach (var f in Directory.GetFiles(folder))
                {
                    _entries.Add(new StartupEntry
                    {
                        Name = Path.GetFileNameWithoutExtension(f),
                        Origin = originLabel,
                        IsDisabled = false,
                        FilePath = f,
                        FolderRoot = folder
                    });
                }

                // Deshabilitados: archivos movidos a la subcarpeta "Disabled" por Mentum
                string disabledFolder = Path.Combine(folder, "Disabled");
                if (Directory.Exists(disabledFolder))
                {
                    foreach (var f in Directory.GetFiles(disabledFolder))
                    {
                        _entries.Add(new StartupEntry
                        {
                            Name = Path.GetFileNameWithoutExtension(f),
                            Origin = originLabel + " · deshabilitado",
                            IsDisabled = true,
                            FilePath = f,
                            FolderRoot = folder
                        });
                    }
                }
            }
            catch { /* carpeta no accesible: se omite */ }
        }

        // ══ Alternar habilitado / deshabilitado ══
        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            int idx = StartupList.SelectedIndex;
            if (idx < 0 || idx >= _entries.Count)
            {
                _mainWindow.AppendLog("⚠️ Selecciona un programa de la lista antes de continuar.");
                return;
            }

            var entry = _entries[idx];

            try
            {
                if (entry.IsRegistryEntry)
                    ToggleRegistryEntry(entry);
                else
                    ToggleFolderEntry(entry);

                _mainWindow.AppendLog(entry.IsDisabled
                    ? $"⛔ '{entry.Name}' deshabilitado del inicio de Windows."
                    : $"✅ '{entry.Name}' habilitado en el inicio de Windows.");
            }
            catch (Exception ex)
            {
                _mainWindow.AppendLog($"❌ No se pudo cambiar '{entry.Name}': {ex.Message}");
            }

            CargarEntradas();
        }

        private void ToggleRegistryEntry(StartupEntry entry)
        {
            RegistryKey hive = entry.Hive == RegistryHive.CurrentUser
                ? Registry.CurrentUser
                : Registry.LocalMachine;

            using var key = hive.OpenSubKey(entry.RegistryPath, writable: true);
            if (key == null)
                throw new InvalidOperationException("No se pudo abrir la clave de registro (¿faltan permisos de administrador?).");

            if (entry.IsDisabled)
            {
                // Rehabilitar: quitar el prefijo del nombre de valor
                string originalName = entry.ValueName.Substring(DisabledPrefix.Length);
                key.SetValue(originalName, entry.CommandValue);
                key.DeleteValue(entry.ValueName, throwOnMissingValue: false);
            }
            else
            {
                // Deshabilitar: renombrar el valor con el prefijo de Mentum
                string disabledName = DisabledPrefix + entry.ValueName;
                key.SetValue(disabledName, entry.CommandValue);
                key.DeleteValue(entry.ValueName, throwOnMissingValue: false);
            }
        }

        private void ToggleFolderEntry(StartupEntry entry)
        {
            string disabledFolder = Path.Combine(entry.FolderRoot, "Disabled");

            if (entry.IsDisabled)
            {
                // Rehabilitar: mover de vuelta a la carpeta de inicio normal
                string destino = Path.Combine(entry.FolderRoot, Path.GetFileName(entry.FilePath));
                File.Move(entry.FilePath, destino, overwrite: true);
            }
            else
            {
                // Deshabilitar: mover a la subcarpeta "Disabled"
                if (!Directory.Exists(disabledFolder))
                    Directory.CreateDirectory(disabledFolder);

                string destino = Path.Combine(disabledFolder, Path.GetFileName(entry.FilePath));
                File.Move(entry.FilePath, destino, overwrite: true);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            CargarEntradas();
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
