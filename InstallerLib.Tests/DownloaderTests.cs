using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ByteSizeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InstallerLib.Tests
{
    [TestClass]
    public class DownloaderTests
    {
        [TestMethod]
        public async Task GetLatestTag()
        {
            var repo = await Downloader.GetRepository(GitHubInfo.MainRepository);
            var release = await Downloader.GetLatestRelease(repo);
            Console.WriteLine(release.TagName);
        }

        [TestMethod]
        public async Task DownloadToMemory()
        {
            var repo = await Downloader.GetRepository(GitHubInfo.MainRepository);
            var latestRelease = await Downloader.GetLatestRelease(repo);
            var asset = await Downloader.GetCurrentPlatformAsset(repo, latestRelease);

            var sw = Stopwatch.StartNew();
            using (var fs = await Downloader.GetAssetStream(asset))
            using (var mem = new MemoryStream())
            {
                await fs.CopyToAsync(mem);
                Debug.Assert(asset.Size == mem.Length);
            }
            sw.Stop();

            Console.WriteLine($"Took {sw.Elapsed} to download release to memory ({ByteSize.FromBytes(asset.Size)}).");
        }
    }
}
