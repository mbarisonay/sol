namespace user_panel.Settings
{
    public class GoogleMapsSettings
    {
        // This constant helps avoid magic strings in Program.cs
        public const string SectionName = "GoogleMaps";

        public string ApiKey { get; set; } = string.Empty;
    }
}