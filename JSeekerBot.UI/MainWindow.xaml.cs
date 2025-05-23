using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace JSeekerBot.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenRunControl(object sender, RoutedEventArgs e)
        {
            //OpenFolderDialog folderDialog = new OpenFolderDialog();

            //bool? success = folderDialog.ShowDialog();

            //if (success == true)
            //{
            //    string path = folderDialog.FolderName;
            //}

            
            SetActiveUserControlWindow(RunControl, runTabButtton);
        }

        private void OpenSettingsControl(object sender, RoutedEventArgs e)
        {
            //OpenFolderDialog folderDialog = new OpenFolderDialog();

            //bool? success = folderDialog.ShowDialog();

            //if (success == true)
            //{
            //    string path = folderDialog.FolderName;
            //}


            SetActiveUserControlWindow(SettingsControl, setttingsTabButtton);
        }

        private void OpenResponseHelperControl(object sender, RoutedEventArgs e)
        {
            //OpenFolderDialog folderDialog = new OpenFolderDialog();

            //bool? success = folderDialog.ShowDialog();

            //if (success == true)
            //{
            //    string path = folderDialog.FolderName;
            //}


            SetActiveUserControlWindow(ResponseHelperControl, responseTabButtton);
        }

        private void OpenCredentialsControl(object sender, RoutedEventArgs e)
        {
            //OpenFolderDialog folderDialog = new OpenFolderDialog();

            //bool? success = folderDialog.ShowDialog();

            //if (success == true)
            //{
            //    string path = folderDialog.FolderName;
            //}


            SetActiveUserControlWindow(CredentialsControl, credentiaslTabButton);
        }
        
        private void OpenTreeControl(object sender, RoutedEventArgs e)
        {
            //OpenFolderDialog folderDialog = new OpenFolderDialog();

            //bool? success = folderDialog.ShowDialog();

            //if (success == true)
            //{
            //    string path = folderDialog.FolderName;
            //}


            SetActiveUserControlWindow(TreeControl, treeTabButton);
        }


        /// <summary>
        /// Changes the active control window visibility
        /// </summary>
        /// <param name="control">the user control that should be active</param>
        /// <param name="actionButton">The nav button the user just clicked on / tried to navigate to</param>
        private void SetActiveUserControlWindow(UserControl control, Button actionButton)
        {
            //Sets all control visibilities
            RunControl.Visibility = Visibility.Collapsed;
            SettingsControl.Visibility = Visibility.Collapsed;
            ResponseHelperControl.Visibility = Visibility.Collapsed;
            CredentialsControl.Visibility = Visibility.Collapsed;
            TreeControl.Visibility = Visibility.Collapsed;


            var bc = new BrushConverter();
            runTabButtton.Background = (Brush)bc.ConvertFrom("#FF2F2F2F")!;
            setttingsTabButtton.Background = (Brush)bc.ConvertFrom("#FF2F2F2F")!;
            responseTabButtton.Background = (Brush)bc.ConvertFrom("#FF2F2F2F")!;
            credentiaslTabButton.Background = (Brush)bc.ConvertFrom("#FF2F2F2F")!;
            treeTabButton.Background = (Brush)bc.ConvertFrom("#FF2F2F2F")!;
            


            runTabButtton.Effect = null;
            setttingsTabButtton.Effect = null;
            responseTabButtton.Effect = null;
            credentiaslTabButton.Effect = null;
            treeTabButton.Effect = null;
            

            //Sets the passed in control parameter visibility to ON.
            control.Visibility = Visibility.Visible;
            actionButton.Background = (Brush)bc.ConvertFrom("#FF212121")!;
            
            DropShadowEffect shadow = new DropShadowEffect
            {
                Color = Colors.GhostWhite,
                BlurRadius = 40,
                ShadowDepth = 0,
                Direction = 360,
                Opacity = 0.5
            };

            actionButton.Effect = shadow;
        }
    }
}