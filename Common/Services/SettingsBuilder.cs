using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JSeeker.Common.Services;

public class SettingsBuilder
{
    public string SettingsFilePath
    {
        get
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..\Settings\JSeekerSettings.json");
            return path;
        }
    }

    public SettingsConfig Config => LoadConfig();

    /// <summary>
    /// Takes in SettingsConfig struct and attempts to save to local settings file
    /// </summary>
    /// <param name="config">SettingsConfig struct with populated setting values for save</param>
    /// <returns></returns>
    public bool SaveConfig(SettingsConfig config)
    {
        var json = JsonSerializer.Serialize(config);

        try
        {
            File.WriteAllText(SettingsFilePath,json);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Error saving settings configuration {exception}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Loads config from JSON File and returns config to caller
    /// </summary>
    /// <returns>SettingsConfig struct with settings values</returns>
    public SettingsConfig LoadConfig()
    {
        SettingsConfig config;
            
        //Load Settings File
        using StreamReader reader = new StreamReader(SettingsFilePath);

        try
        {
            string json = reader.ReadToEnd();
            config = JsonSerializer.Deserialize<SettingsConfig>(json);
        }
        catch (JsonSerializationException e)
        {
            Console.WriteLine(e);
            throw;
        }

        return config;
    }
    
    /// <summary>
    /// Settings Config struct for passing around settings in JSeeker
    /// </summary>
    [Serializable]
    public struct SettingsConfig
    {

        public SettingsConfig(string resultFolderPath, string dTextboxResponse,
            string dComboBoxResponse, string dRadioButtonResponse,
            string role, string location)
        {
            ResultFolderPath = resultFolderPath;
            DefaultComboBoxResponse = dComboBoxResponse;
            DefaultTextboxResponse = dTextboxResponse;
            DefaultRadioButtonResponse = dRadioButtonResponse;
            JobSearchRole = role;
            JobSearchLocation = location;
        }

        public string ResultFolderPath { get; set; }
        public string DefaultTextboxResponse { get; set; }
        public string DefaultComboBoxResponse { get; set; }
        public string DefaultRadioButtonResponse { get; set; }
        public string JobSearchRole { get; set; }
        public string JobSearchLocation { get; set; }
        public string ActiveProfilePath { get; set; } = "";
    }
}