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

namespace JSeekerBot.UI
{
    /// <summary>
    /// Interaction logic for CredentialsControl.xaml
    /// </summary>
    public partial class CredentialsControl : UserControl
    {
        public CredentialsControl()
        {
            InitializeComponent();
            LoadCredentials();
        }

        private async void LoadCredentials()
        {
            using StreamReader reader = new StreamReader(@"..\..\..\..\Envs\.env");
            var email = await reader.ReadLineAsync();
            var pass = await reader.ReadLineAsync();

            if (email != null)
                EmailAddressTextbox.Text = email.Split("=")[1];

            if(pass != null)
                PasswordTextbox.Password = pass.Split("=")[1];
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Save .env credentials
            var bc = new BrushConverter();
            var currentDirectory = Directory.GetCurrentDirectory();

            try
            {
                using StreamWriter writer = new StreamWriter(@"..\..\..\..\Envs\.env");
                writer.WriteLine($"LINKEDIN_USERNAME={EmailAddressTextbox.Text}");
                writer.WriteLine($"LINKEDIN_PASSWORD={PasswordTextbox.Password}");
            }
            catch (Exception exception)
            {
                ValidationsText.Foreground = (Brush)bc.ConvertFrom("##FF7D020");
                ValidationsText.Text = $"Failed to save credentials file to disk - {exception.Message}";
                return;
            }

            ValidationsText.Foreground = (Brush)bc.ConvertFrom("#4BB543");
            ValidationsText.Text = "Credentials saved successfully!";
        }
    }
}
