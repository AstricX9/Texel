using System;
using System.IO;
using Newtonsoft.Json;
using Texel.Classes;

namespace Texel.Classes
{
    [Serializable]
    public class AppSettings
    {
        // Default project settings
        public string DefaultMinecraftVersion { get; set; } = "1.20.1";
        public int DefaultResolution { get; set; } = 16;
        
        // Auto-save settings
        public bool AutoSaveEnabled { get; set; } = true;
        public int AutoSaveIntervalMinutes { get; set; } = 5;
        
        // UI settings
        public bool ShowGrid { get; set; } = true;
        public bool UseDarkTheme { get; set; } = false;
        
        // File paths
        public string LastProjectDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        
        // Static methods for loading/saving settings
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Texel Editor",
            "settings.json");
            
        public static AppSettings Load()
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();
                
            try
            {
                string json = File.ReadAllText(SettingsPath);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }
        
        public static void Save(AppSettings settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                    
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Silently fail if we can't save settings
            }
        }
    }
}