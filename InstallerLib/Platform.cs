using System;
using System.Runtime.InteropServices;

namespace InstallerLib
{
    public static class Platform
    {
        public static RuntimePlatform ActivePlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return RuntimePlatform.Windows;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return RuntimePlatform.Linux;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return RuntimePlatform.MacOS;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                    return RuntimePlatform.FreeBSD;
                else
                    return 0;
            }
        }

        public static string ExecutableFileExtension
        {
            get
            {
                switch (ActivePlatform)
                {
                    case RuntimePlatform.Windows:
                        return ".exe";
                    case RuntimePlatform.MacOS:
                        return ".app";
                    default:
                        return "";
                }
            }
        }
    }
}