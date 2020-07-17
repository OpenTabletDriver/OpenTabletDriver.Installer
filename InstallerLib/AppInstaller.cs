using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Octokit;

namespace InstallerLib
{
    public class AppInstaller
    {
        public AppInstaller(DirectoryInfo installationDirectory)
        {
            InstallationDirectory = installationDirectory;
        }

        public DirectoryInfo InstallationDirectory { private set; get; }
        public FileInfo VersionInfoFile => new FileInfo(Path.Join(InstallationDirectory.FullName, "versionInfo.json"));

        public bool IsInstalled
        {
            get
            {
                InstallationDirectory.Refresh();
                return InstallationDirectory.Exists && InstallationDirectory.EnumerateFileSystemInfos().Count() > 0;
            }
        }

        private readonly DirectoryInfo TempDirectory = new DirectoryInfo(Path.GetTempPath());

        /// <summary>
        /// Download, extract, and move all binary files from the latest GitHub release.
        /// </summary>
        /// <returns></returns>
        public async Task InstallBinaries()
        {
            InstallationDirectory.Create();

            var release = await Downloader.GetLatestRelease();
            var asset = await Downloader.GetCurrentPlatformAsset(release);
            var extension = asset.Name.Split('.').Last();
            
            using (var httpStream = await Downloader.GetAssetStream(asset))
            {
                if (extension == "zip")
                {
                    // Create a temporary file to extract from
                    var filename = $"{Guid.NewGuid()}.zip";
                    var file = new FileInfo(Path.Join(TempDirectory.FullName, filename));
                    using (var fileStream = file.Create())
                    {
                        // Download zip to temporary file to extract from
                        await httpStream.CopyToAsync(fileStream);
                    }
                    // Extract to directory
                    ZipFile.ExtractToDirectory(file.FullName, InstallationDirectory.FullName);
                    // Clean up files
                    file.Delete();
                }
                else if (extension == "gz")
                {
                    using (var decompressionStream = new GZipStream(httpStream, CompressionMode.Decompress))
                    using (var tar = TarArchive.CreateInputTarArchive(decompressionStream))
                    {    
                        // Extract to directory
                        tar.ExtractContents(InstallationDirectory.FullName);
                    }
                }
                else
                {
                    throw new Exception("Unknown file extension, unable to decompress.");
                }
            }

            var versionInfo = new VersionInfo(release.TagName);
            using (var fs = VersionInfoFile.OpenWrite())
                versionInfo.Serialize(fs);
        }

        /// <summary>
        /// Remove all binaries associated with OpenTabletDriver.
        /// </summary>
        public async Task DeleteBinaries()
        {
            await Task.Run(() => 
            {
                foreach (var dir in InstallationDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly))
                    dir.Delete(true);
                foreach (var file in InstallationDirectory.GetFiles("*", SearchOption.TopDirectoryOnly))
                    file.Delete();
            });
        }

        /// <summary>
        /// Get the currently installed version of OpenTabletDriver, or null if it is not installed.
        /// </summary>
        public VersionInfo GetInstalledVersion()
        {
            if (VersionInfoFile.Exists)
                using (var fs = VersionInfoFile.OpenRead())
                    return VersionInfo.Deserialize(fs);
            return null;
        }

        /// <summary>
        /// Check whether an update is available or not.
        /// Returns true if an update is available.
        /// </summary>
        /// <returns>True if update is available.</returns>
        public async Task<bool> CheckForUpdate()
        {
            if (VersionInfoFile.Exists)
            {
                using (var fs = VersionInfoFile.OpenRead())
                {
                    var version = VersionInfo.Deserialize(fs);
                    try 
                    {
                        var release = await Downloader.GetLatestRelease();
                        return release.TagName != version.InstalledVersion;
                    }
                    catch (RateLimitExceededException)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return true;
            }
        }
    }
}