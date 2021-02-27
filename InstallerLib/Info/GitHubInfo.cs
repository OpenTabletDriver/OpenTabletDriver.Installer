namespace InstallerLib.Info
{
    public static class GitHubInfo
    {
        public const string Owner = "OpenTabletDriver";
        public const string MainRepository = "OpenTabletDriver";
        public const string InstallerRepository = "OpenTabletDriver.Installer";

        public static string InstallerReleaseUrl => $"https://github.com/{Owner}/{InstallerRepository}/releases";
    }
}
