using System;
using System.Windows;

namespace MentumLauncher
{
    public static class ThemeManager
    {
        private static bool _isDark = true;
        public static bool IsDark => _isDark;

        public static void Apply(bool dark)
        {
            _isDark = dark;
            var uri = new Uri(
                dark ? "ThemeDark.xaml" : "ThemeLight.xaml",
                UriKind.Relative);

            var dict = new ResourceDictionary { Source = uri };

            // Reemplaza el diccionario de tema actual
            var merged = Application.Current.Resources.MergedDictionaries;
            for (int i = merged.Count - 1; i >= 0; i--)
            {
                var src = merged[i].Source?.OriginalString ?? "";
                if (src.Contains("ThemeDark") || src.Contains("ThemeLight"))
                {
                    merged.RemoveAt(i);
                    break;
                }
            }
            merged.Add(dict);

            // Guarda preferencia en registro
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                    @"SOFTWARE\MentumLauncher");
                key?.SetValue("Theme", dark ? "dark" : "light");
            }
            catch { }
        }

        public static bool LoadSavedTheme()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\MentumLauncher");
                var val = key?.GetValue("Theme")?.ToString();
                return val != "light"; // default dark
            }
            catch { return true; }
        }

        public static void Toggle() => Apply(!_isDark);
    }
}
