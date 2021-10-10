namespace TT.Deliveries.Core.Configuration
{
    public class DatabaseOptions
    {
        public const string AppSettingsSection = "Database";

        public string? ConnectionString { get; set; }
    }
}