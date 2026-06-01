using System;
using System.Windows;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace MentumLauncher
{
    class UpdateChecker
    {
        public static string GetInformationalVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attribute?.InformationalVersion ?? "0.0.0";
        }

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

        public static void CheckUpdate(string currentVersion, string latestVersion, string releaseUrl, Window owner)
        {
            var local = ExtractNumericVersion(currentVersion);
            var remote = ExtractNumericVersion(latestVersion);

            if (IsRemoteNewer(local, remote))
            {
                // Hay actualización disponible — pregunta Sí/No
                var dlgUpdate = new VentanaMensaje(
                    titulo: "Mentum — Un nuevo horizonte",
                    mensaje: $"Mentum ha seguido su curso y una nueva versión ha nacido ({remote}).\n\n" +
                                      "Cada chispa que se enciende nos recuerda que el futuro se construye juntos.\n\n" +
                                      "¿Deseas abrir la senda hacia la actualización y ser parte de este horizonte compartido?",
                    tipo: MensajeTipo.Info,
                    mostrarCancelar: true
                );
                dlgUpdate.Owner = owner;
                dlgUpdate.ShowDialog();
                bool confirmar = dlgUpdate.Confirmado;

                if (confirmar)
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
                // Versión al día
                var dlgOk = new VentanaMensaje(
                    titulo: "Mentum — La luz compartida",
                    mensaje: "Mentum y tu camino están en armonía.\n\n" +
                                     "La chispa que llevas es la misma que ilumina a la comunidad.\n" +
                                     "Gracias por ser parte de este viaje donde cada paso fortalece el futuro que soñamos.",
                    tipo: MensajeTipo.Exito,
                    mostrarCancelar: false
                );
                dlgOk.Owner = owner;
                dlgOk.ShowDialog();
            }
            else
            {
                // Versión más nueva que la publicada (beta/dev)
                var dlgBeta = new VentanaMensaje(
                    titulo: "Mentum — El sendero anticipado",
                    mensaje: $"Tu versión ({local}) es más reciente que la última publicada ({remote}).\n\n" +
                                      "Estás caminando con una chispa que aún no se ha compartido con el mundo.\n" +
                                      "Gracias por encender el futuro de Mentum antes de que llegue al horizonte común.",
                    tipo: MensajeTipo.Advertencia,
                    mostrarCancelar: false
                );
                dlgBeta.Owner = owner;
                dlgBeta.ShowDialog();
            }
        }

        public static async Task CheckForUpdatesAsync(Window owner)
        {
            try
            {
                string currentVersion = GetInformationalVersion();
                (string latestVersion, string releaseUrl) = await GetLatestVersionFromGitHub();
                CheckUpdate(currentVersion, latestVersion, releaseUrl, owner);
            }
            catch (Exception)
            {
                // Error de conexión
                var dlgErr = new VentanaMensaje(
                    titulo: "Mentum — La senda continúa",
                    mensaje: "Mentum buscó señales en el horizonte, pero la conexión se desvaneció.\n\n" +
                                     "Tu camino sigue iluminado con la chispa que ya llevas.\n" +
                                     "Gracias por mantener viva la esperanza de futuros compartidos.",
                    tipo: MensajeTipo.Error,
                    mostrarCancelar: false
                );
                dlgErr.Owner = owner;
                dlgErr.ShowDialog();
            }
        }

        private static async Task<(string, string)> GetLatestVersionFromGitHub()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "MentumLauncher");

            var response = await client.GetStringAsync(
                "https://api.github.com/repos/MaRin-Raizio/Mentum-by-Raizio/releases/latest");

            using var doc = JsonDocument.Parse(response);

            string? rawVersion = doc.RootElement.GetProperty("tag_name").GetString();
            string? releaseUrl = doc.RootElement.GetProperty("html_url").GetString();

            return (rawVersion ?? "0.0.0", releaseUrl ?? "");
        }
    }
}
