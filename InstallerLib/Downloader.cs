using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InstallerLib.Info;
using Octokit;

namespace InstallerLib
{
    public static class Downloader
    {
        private static readonly GitHubClient Client = new GitHubClient(new ProductHeaderValue("OpenTabletDriver.Installer"));

        private const string PlatformFileFormat = "OpenTabletDriver.{0}.{1}";
        internal static string PackageName => SystemInfo.CurrentPlatform switch
        {
            RuntimePlatform.Windows => string.Format(PlatformFileFormat, "win-x64", "zip"),
            RuntimePlatform.Linux   => string.Format(PlatformFileFormat, "linux-x64", "tar.gz"),
            RuntimePlatform.MacOS   => string.Format(PlatformFileFormat, "osx-x64", "tar.gz"),
            _                       => throw new PlatformNotSupportedException()
        };

        public static async Task<MiscellaneousRateLimit> GetRateLimit()
        {
            return await Client.Miscellaneous.GetRateLimits();
        }

        public static async Task<bool> CheckIfCanDownload()
        {
            var rateLimit = await GetRateLimit();
            return rateLimit.Resources.Core.Remaining > 5;
        }

        public static async Task<Repository> GetRepository(string name)
        {
            return await Client.Repository.Get(GitHubInfo.Owner, name);
        }

        public static async Task<Release> GetLatestRelease(Repository repo)
        {
            var releases = await Client.Repository.Release.GetAll(repo.Id);
            return releases.First();
        }

        public static async Task<Release> GetRelease(Repository repo, string tag)
        {
            return await Client.Repository.Release.Get(repo.Id, tag);
        }

        public static async Task<ReleaseAsset> GetCurrentPlatformAsset(Repository repo, Release release)
        {
            var releases = await Client.Repository.Release.GetAllAssets(repo.Id, release.Id);
            return releases.FirstOrDefault(r => r.Name == PackageName);
        }

        public static async Task<Stream> GetAssetStream(ReleaseAsset asset)
        {
            using (var client = new HttpClient())
                return await client.GetStreamAsync(asset.BrowserDownloadUrl);
        }
    }
}