using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;

namespace InstallerLib
{
    public static class Downloader
    {
        const string Owner = "InfinityGhost";
        const string RepositoryName = "OpenTabletDriver";
        private static readonly GitHubClient Client = new GitHubClient(new ProductHeaderValue("OpenTabletDriver.Installer"));

        internal static string GetCurrentPlatformFileName()
        {
            string fileFormat = "OpenTabletDriver.{0}.{1}";
            switch (Platform.ActivePlatform)
            {
                case RuntimePlatform.Windows:
                    return string.Format(fileFormat, "win-x64", "zip");
                case RuntimePlatform.Linux:
                    return string.Format(fileFormat, "linux-x64", "tar.gz");
                case RuntimePlatform.MacOS:
                    return string.Format(fileFormat, "macos-x64", "tar.gz");
                default:
                    throw new PlatformNotSupportedException("The active platform is unsupported, therefore a filename cannot be provided.");
            }
        }

        public static async Task<Repository> GetRepository()
        {
            return await Client.Repository.Get(Owner, RepositoryName);
        }

        public static async Task<Release> GetLatestRelease()
        {
            var repo = await GetRepository();
            var releases = await Client.Repository.Release.GetAll(repo.Id);
            return releases.First();
        }

        public static async Task<ReleaseAsset> GetCurrentPlatformAsset(Release release)
        {
            using (var webClient = new HttpClient())
            {
                var repo = await GetRepository();
                var releases = await Client.Repository.Release.GetAllAssets(repo.Id, release.Id);
                var filename = GetCurrentPlatformFileName();
                return releases.FirstOrDefault(r => r.Name == filename);
            }
        }

        public static async Task<Stream> GetAssetStream(ReleaseAsset asset)
        {
            using (var webClient = new HttpClient())
                return await webClient.GetStreamAsync(asset.BrowserDownloadUrl);
        }
    }
}