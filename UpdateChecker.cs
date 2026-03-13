using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

class UpdateChecker
{
    static string ExtractNumericVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0";

        if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            version = version.Substring(1);

        int dashIndex = version.IndexOf('-');
        if (dashIndex >= 0)
            version = version.Substring(0, dashIndex);

        return version;
    }

    static bool IsRemoteNewer(string local, string remote)
    {
        var localParts = local.Split('.').Select(int.Parse).ToArray();
        var remoteParts = remote.Split('.').Select(int.Parse).ToArray();

        for (int i = 0; i < Math.Min(localParts.Length, remoteParts.Length); i++)
        {
            if (remoteParts[i] > localParts[i]) return true;
            if (remoteParts[i] < localParts[i]) return false;
        }

        return remoteParts.Length > localParts.Length;
    }

    public static void CheckUpdate(string currentVersion, string latestVersion, string releaseUrl)
    {
        var local = ExtractNumericVersion(currentVersion);
        var remote = ExtractNumericVersion(latestVersion);

        if (IsRemoteNewer(local, remote))
        {
            var result = MessageBox.Show(
                $"Mentum ha seguido su curso y una nueva versión ha nacido ({remote}).\n\nCada chispa que se enciende nos recuerda que el futuro se construye juntos.\n\n¿Deseas abrir la senda hacia la actualización y ser parte de este horizonte compartido?",
                "Mentum — Un nuevo horizonte",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = releaseUrl,
                    UseShellExecute = true
                });
            }
        }
        else if (local == remote)
        {
            MessageBox.Show(
                "Mentum y tu camino están en armonía.\n\nLa chispa que llevas es la misma que ilumina a la comunidad.\nGracias por ser parte de este viaje donde cada paso fortalece el futuro que soñamos.",
                "Mentum — La luz compartida",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        else
        {
            MessageBox.Show(
                $"Tu versión ({local}) es más reciente que la última publicada ({remote}).\n\nEstás caminando con una chispa que aún no se ha compartido con el mundo.\nGracias por encender el futuro de Mentum antes de que llegue al horizonte común.",
                "Mentum — El sendero anticipado",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }

    public static async Task CheckForUpdatesAsync(string currentVersion)
    {
        try
        {
            (string latestVersion, string releaseUrl) = await GetLatestVersionFromGitHub();
            CheckUpdate(currentVersion, latestVersion, releaseUrl);
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Mentum buscó señales en el horizonte, pero la conexión se desvaneció.\n\nTu camino sigue iluminado con la chispa que ya llevas.\nGracias por mantener viva la esperanza de futuros compartidos.",
                "Mentum — La senda continúa",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }

    private static async Task<(string, string)> GetLatestVersionFromGitHub()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "MentumLauncher");

        var response = await client.GetStringAsync("https://api.github.com/repos/MaRin-Raizio/Mentum-by-Raizio/releases/latest");
        using var doc = JsonDocument.Parse(response);

        string rawVersion = doc.RootElement.GetProperty("tag_name").GetString();
        string releaseUrl = doc.RootElement.GetProperty("html_url").GetString();

        return (rawVersion, releaseUrl);
    }
}