using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JSeeker.Common.Services;
using Microsoft.Win32;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JSeekerBot.UI
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private readonly SettingsBuilder SettingsBuilder = new SettingsBuilder();
        
        public SettingsControl()
        {
            InitializeComponent();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            //Load settings config from JSON file
            var config = SettingsBuilder.LoadConfig();
            
            //set settings Control values to the current settings file values
            ResultPathTextbox.Text = config.ResultFolderPath;
            DefaultTextboxAnswer.Text = config.DefaultTextboxResponse;
            DefaultComboBoxAnswer.Text = config.DefaultComboBoxResponse;
            DefaultTRadioButtonAnswer.Text = config.DefaultRadioButtonResponse;
            JobSearchRoleTextbox.Text = config.JobSearchRole;
            JobSearchLocationTextbox.Text = config.JobSearchLocation;

            var bc = new BrushConverter();
            ValidationsText.Foreground = (Brush)bc.ConvertFrom("#4BB543");
            ValidationsText.Text = "Settings loaded successfully!";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Open folder dialog for selecting path to output results
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
            var bc = new BrushConverter();

            //Save Settings
            var config = new SettingsBuilder.SettingsConfig(ResultPathTextbox.Text, DefaultTextboxAnswer.Text,
                DefaultComboBoxAnswer.Text, DefaultTRadioButtonAnswer.Text, JobSearchRoleTextbox.Text, JobSearchLocationTextbox.Text);
            var success = SettingsBuilder.SaveConfig(config);

            if (success)
            {
                ValidationsText.Foreground = (Brush)bc.ConvertFrom("#4BB543");
                ValidationsText.Text = "Settings saved successfully!";
            }
            else
            {
                ValidationsText.Foreground = (Brush)bc.ConvertFrom("##FF7D020");
                ValidationsText.Text = $"Failed to save settings file to disk";
            }
            

            ValidationsText.Foreground = (Brush)bc.ConvertFrom("#4BB543");
            ValidationsText.Text = "Settings saved successfully!";
        }
    }
}
