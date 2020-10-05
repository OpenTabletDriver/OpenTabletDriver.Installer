using System;
using System.IO;

namespace InstallerLib.Migration
{
    public class BinaryFolderMigrator : IMigrator
    {
        public void Migrate()
        {
            if (NeedsMigration)
                OldBinFolder.MoveTo(NewBinFolder.FullName);
        }
        
        private DirectoryInfo OldBinFolder => new DirectoryInfo(Path.Join(Environment.CurrentDirectory, "bin"));
        private DirectoryInfo NewBinFolder => InstallationInfo.Current.InstallationDirectory;

        public bool NeedsMigration => OldBinFolder.Exists && !NewBinFolder.Exists;

        public string MigrationPrompt => $"The binary folder at {OldBinFolder} needs to be migrated to {NewBinFolder}. Do you wish to proceed?";
    }
}