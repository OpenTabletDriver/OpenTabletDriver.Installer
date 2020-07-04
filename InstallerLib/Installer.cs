using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

namespace InstallerLib
{
    public class Installer
    {
        public Installer(DirectoryInfo installationDirectory)
        {
            InstallationDirectory = installationDirectory;
        }

        public DirectoryInfo InstallationDirectory { private set; get; }
        private readonly DirectoryInfo TempDirectory = new DirectoryInfo(Path.GetTempPath());

        public async Task InstallBinaries()
        {
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
                        // Extract to directory
                        ZipFile.ExtractToDirectory(file.FullName, InstallationDirectory.FullName);
                    }
                    // Clean up files
                    file.Delete();
                }
                else if (extension == "gz")
                {
                    // Decompress the stream and get tar archive from stream
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
        }

        public async Task DeleteBinaries()
        {
            await Task.Run(() => InstallationDirectory.Delete(true));
        }
    }
}