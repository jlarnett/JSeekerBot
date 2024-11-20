using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Win32;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JSeekerBot.UI
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
            InitalizeSettings();
        }

        private void InitalizeSettings()
        {
            var config = new SettingsConfig();

            //Load Settings File
            using (StreamReader reader = new StreamReader("..\\..\\..\\..\\Settings\\JSeekerSettings.json"))
            {
                string json = reader.ReadToEnd();
                config = JsonSerializer.Deserialize<SettingsConfig>(json);
            }

            //set settings Control values to the current settings file values
            ResultPathTextbox.Text = config.ResultFolderPath;
            DefaultTextboxAnswer.Text = config.DefaultTextboxResponse;
            DefaultComboBoxAnswer.Text = config.DefaultComboBoxResponse;
            DefaultTRadioButtonAnswer.Text = config.DefaultRadioButtonResponse;
            JobSearchRoleTextbox.Text = config.JobSearchRole;
            JobSearchLocationTextbox.Text = config.JobSearchLocation;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();

            bool? success = folderDialog.ShowDialog();

            if (success == true)
            {
                string path = folderDialog.FolderName;
                ResultPathTextbox.Text = path;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Save Settings
            var config = new SettingsConfig(ResultPathTextbox.Text, DefaultTextboxAnswer.Text,
                DefaultComboBoxAnswer.Text, DefaultTRadioButtonAnswer.Text, JobSearchRoleTextbox.Text, JobSearchLocationTextbox.Text);
            var json = JsonSerializer.Serialize(config);


            File.WriteAllText("..\\..\\..\\..\\Settings\\JSeekerSettings.json",json);
        }

        [Serializable]
        struct SettingsConfig
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
        }
    }
}
