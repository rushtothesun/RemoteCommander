using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace RemoteCommander
{
    public class CommanderSettings
    {
        private static readonly string SettingsPath = "RemoteCommanderSettings.json";
        public ObservableCollection<string> RemoteBots { get; set; } = new ObservableCollection<string>();
        
        public bool AlwaysOnTop { get; set; } = false;
        public double WindowOpacity { get; set; } = 1.0;
        public bool MutedColors { get; set; } = false;

        public static CommanderSettings Load()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<CommanderSettings>(json);
                    if (settings != null)
                        return settings;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"Failed to load settings: {ex.Message}");
                }
            }
            return new CommanderSettings();
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}
