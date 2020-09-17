using System.Reflection;
using System.Threading.Tasks;

namespace InstallerLib
{
    public static class InstallerInfo
    {
        public static async Task<bool> CheckForUpdate()
        {
            if (await Downloader.CheckIfCanDownload())
            {
                var repo = await Downloader.GetRepository(GitHubInfo.InstallerRepository);
                var release = await Downloader.GetLatestRelease(repo);
                string currentTag = $"v{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";
                return release.TagName != currentTag;
            }
            return false;
        }
    }
}