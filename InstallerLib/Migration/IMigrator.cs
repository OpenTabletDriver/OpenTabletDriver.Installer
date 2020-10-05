namespace InstallerLib.Migration
{
    public interface IMigrator
    {
        void Migrate();
        bool NeedsMigration { get; }
        string MigrationPrompt { get; }
    }
}