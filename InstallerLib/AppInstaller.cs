using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using Octokit;

namespace InstallerLib
{
    public class AppInstaller : Progress<double>
    {
        public AppInstaller(DirectoryInfo installationDirectory)
        {
            InstallationDirectory = installationDirectory;
        }

        public DirectoryInfo InstallationDirectory { private set; get; }
        public FileInfo VersionInfoFile => new FileInfo(Path.Join(InstallationDirectory.FullName, "versionInfo.json"));

        private const int BufferSize = 81920;

        public bool IsInstalled => VersionInfoFile.Exists;

        private readonly DirectoryInfo TempDirectory = new DirectoryInfo(Path.GetTempPath());

        /// <summary>
        /// Download, extract, and move all binary files from the latest GitHub release.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> InstallBinaries()
        {
            if (await Downloader.CheckIfCanDownload())
            {
                InstallationDirectory.Delete(true);
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
                            await CopyStreamWithProgress(asset.Size, httpStream, fileStream);

                        // Extract to directory
                        ZipFile.ExtractToDirectory(file.FullName, InstallationDirectory.FullName);
                        // Clean up files
                        file.Delete();
                    }
                    else if (extension == "gz")
                    {
                        using (var memoryStream = new MemoryStream(asset.Size))
                        {
                            await CopyStreamWithProgress(asset.Size, httpStream, memoryStream);
                            using (var decompressionStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                            using (var tar = TarArchive.CreateInputTarArchive(decompressionStream))
                            {    
                                // Extract to directory
                                tar.ExtractContents(InstallationDirectory.FullName);
                            }
                        }

                        var newDirectories = InstallationDirectory.GetDirectories();
                        if (newDirectories.Length == 1)
                        {
                            var directory = newDirectories.First();
                            if (directory.Name != "Configurations")
                            {
                                foreach (var fsinfo in directory.GetFileSystemInfos())
                                {
                                    if (fsinfo is FileInfo file)
                                        file.MoveTo(Path.Join(InstallationDirectory.FullName, file.Name));
                                    else if (fsinfo is DirectoryInfo dir)
                                        dir.MoveTo(Path.Join(InstallationDirectory.FullName, dir.Name));
                                }
                            }
                            directory.Delete();
                        }

                        var executables = from file in InstallationDirectory.EnumerateFiles()
                            where file.Name == Launcher.AppName || file.Name == Launcher.DaemonName || file.Name == Launcher.ConsoleName
                            select file;

                        foreach (var file in executables)
                        {
                            switch (Platform.ActivePlatform)
                            {
                                case RuntimePlatform.Linux:
                                case RuntimePlatform.MacOS:
                                case RuntimePlatform.FreeBSD:
                                    Process.Start("chmod", $"+rwx \"{file.FullName}\"");
                                    break;
                            }
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

                return true;
            }
            return false;
        }

        private async Task CopyStreamWithProgress(int length, Stream source, Stream target)
        {
            var buffer = new byte[BufferSize];
            int i = 0;
            while (true)
            {
                this.OnReport((float)i / (float)length);

                int bytesRead = await source.ReadAsync(buffer, 0, BufferSize);
                if (bytesRead == 0)
                    break;

                await target.WriteAsync(buffer[0..bytesRead], 0, bytesRead);
                i += BufferSize;
            }

            // Reset stream position
            target.Position = 0;

            // Report copy complete
            this.OnReport(1f);
        }

        /// <summary>
        /// Remove all binaries associated with OpenTabletDriver.
        /// </summary>
        public void DeleteBinaries()
        {
            var fsInfos = InstallationDirectory.GetFileSystemInfos("*", SearchOption.TopDirectoryOnly);
            int i = 0;
            foreach (var item in fsInfos)
            {
                if (item is DirectoryInfo dir)
                    dir.Delete(true);
                else if (item is FileInfo file)
                    file.Delete();
                i++;
                OnReport(i / fsInfos.Length);
            }
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
                    if (await Downloader.CheckIfCanDownload())
                    {
                        var release = await Downloader.GetLatestRelease();
                        return release.TagName != version.InstalledVersion;
                    }
                    else
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