using System.IO;
using Newtonsoft.Json;

namespace InstallerLib.Info
{
    public class VersionInfo
    {
        public VersionInfo(string version) : this()
        {
            InstalledVersion = version;
        }

        private VersionInfo()
        {
        }

        public string InstalledVersion { set; get; }

        public void Serialize(Stream stream)
        {
            var serializer = new JsonSerializer();
            using (var sw = new StreamWriter(stream))
                serializer.Serialize(sw, this);
        }

        public static VersionInfo Deserialize(Stream stream)
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(sr))
                return serializer.Deserialize<VersionInfo>(jsonReader);
        }

        public override string ToString() => InstalledVersion;
    }
}