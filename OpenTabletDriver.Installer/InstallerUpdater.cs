using System.Reflection;
using System.Threading.Tasks;
using InstallerLib;
using InstallerLib.Info;

namespace OpenTabletDriver.Installer
{
    public static class InstallerUpdater
    {
        public static async Task<bool> CheckForUpdate()
        {
            try
            {
                if (await Downloader.CheckIfCanDownload())
                {
                    var repo = await Downloader.GetRepository(GitHubInfo.InstallerRepository);
                    var release = await Downloader.GetLatestRelease(repo);
                    string currentTag = $"v{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";
                    return release.TagName != currentTag;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}