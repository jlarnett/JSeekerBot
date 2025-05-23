using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ClosedXML.Excel;
using JSeeker.Common.Services;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace JSeekerBot.UI
{
    /// <summary>
    /// Interaction logic for ResponseHelperControl.xaml
    /// </summary>
    public partial class ResponseHelperControl : UserControl
    {
        public ObservableCollection<QuestionResponsePair> Pairs = [];
        private string currentProfileFilePath = "";
        private SettingsBuilder SettingsBuilder = new SettingsBuilder();
        
        public ResponseHelperControl()
        {
            InitializeComponent();
            QuestionResponsePairsList.ItemsSource = Pairs;
            LoadQuestionResponseFile();
            QuestionResponseProfilePath.Text = SettingsBuilder.LoadConfig().ActiveProfilePath;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool keyExistsInList = false;
            
            foreach (var pair in Pairs)
            {
                if (pair.QuestionMatchKey.Equals(QuestionKeyTextbox.Text))
                {
                    keyExistsInList = true;
                }
            }

            var bc = new BrushConverter();

            if (!keyExistsInList)
            {
                Pairs.Add(new QuestionResponsePair
                {
                    QuestionMatchKey = QuestionKeyTextbox.Text,
                    DesiredResponse = QuestionResponseTextbox.Text
                });
                
                ValidationsText.Foreground = (Brush)bc.ConvertFrom("#4BB543");
                ValidationsText.Text = "response added successfully!";
            }
            else
            {
                ValidationsText.Foreground = (Brush)bc.ConvertFrom("#FFD32F2F");
                ValidationsText.Text = "Key already exists in list. Please edit it instead!";
            }

        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(QuestionResponsePairsList.SelectedIndex != -1)
                Pairs.RemoveAt(QuestionResponsePairsList.SelectedIndex);
        }
        
        private void SelectResponseProfile(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Trying to select response file");
            
            string relativePath = @"..\\..\\..\\..\\QuestionResponseFiles";
            string path = Path.GetFullPath(relativePath);
            
            var fileDialog = new OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = path
            };
            
            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                currentProfileFilePath = fileDialog.RootDirectory + fileDialog.FileName;
                QuestionResponseProfilePath.Text = currentProfileFilePath;
                

                var config = SettingsBuilder.LoadConfig();
                
                if (!config.ActiveProfilePath.Equals(currentProfileFilePath))
                {
                    config.ActiveProfilePath = currentProfileFilePath;
                    SettingsBuilder.SaveConfig(config);
                    
                    //Load Profile
                    LoadQuestionResponseFile();
                }
            }
        }

        private void WriteResponseFile(object sender, RoutedEventArgs e)
        {
            var bc = new BrushConverter();

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Enter Response File Name .xlsx";
            saveFileDialog.DefaultExt = ".xlsx";
            saveFileDialog.Filter = "Excel |*.xlsx";
            saveFileDialog.FileName = "QuestionResponseFile.xlsx";
            saveFileDialog.ShowDialog();
            
            if (saveFileDialog.FileName != "")
            {
                //User entered file name for response new response file
                var filePath = saveFileDialog.RootDirectory + saveFileDialog.FileName;
                bool resultFileExists = File.Exists(filePath);

                XLWorkbook wb;
                IXLWorksheet workSheet;

                if (resultFileExists)
                {
                    try
                    {
                        wb = new XLWorkbook(filePath);
                        workSheet = wb.Worksheet("QuestionResponsePairs");
                    }
                    catch (Exception exception)
                    {
                        ValidationsText.Foreground = (Brush)bc.ConvertFrom("#FF7D020");
                        ValidationsText.Text = $"Failed to load response file from disk - {exception.Message}";
                        return;
                    }

                }
                else
                {
                    wb = new XLWorkbook();
                    workSheet = wb.AddWorksheet($"QuestionResponsePairs");
                }

                workSheet.Cell(1, 1).Value = "Question Key";
                workSheet.Cell(1, 2).Value = "Preferred Response";

                for (int i = 0; i < Pairs.Count; i++)
                {
                    workSheet.Cell(i + 2, 1).Value = Pairs[i].QuestionMatchKey;
                    workSheet.Cell(i+2, 2).Value = Pairs[i].DesiredResponse;
                }

                try
                {
                    wb.SaveAs(filePath);
                }
                catch (Exception exception)
                {
                    ValidationsText.Foreground = (Brush)bc.ConvertFrom("#FF7D020");
                    ValidationsText.Text = $"Failed to save response file to disk - {exception.Message}";
                    return;
                }

                ValidationsText.Foreground = (Brush)bc.ConvertFrom("#4BB543");
                ValidationsText.Text = "response file saved successfully!";
            }
            
            

        }

        private void LoadQuestionResponseFile()
        {
            Pairs.Clear();
            
            bool resultFileExists = File.Exists(SettingsBuilder.Config.ActiveProfilePath);
            XLWorkbook wb;

            //Ternary to check if file exists
            wb = resultFileExists ? new XLWorkbook(SettingsBuilder.Config.ActiveProfilePath) : new XLWorkbook();
            IXLWorksheet workSheet;

            if(resultFileExists)
                workSheet = wb.Worksheet("QuestionResponsePairs");
            else
                workSheet = wb.AddWorksheet("QuestionResponsePairs");

            int i = 0;
            string questionKey = "-1";
            string questionResponse = "-1";

            while (questionKey != "" && questionResponse != "")
            {
                questionKey = workSheet.Cell(i + 2, 1).Value.ToString();
                questionResponse = workSheet.Cell(i + 2, 2).Value.ToString();

                if (questionKey == "" && questionResponse == "") break;
                
                Pairs.Add(new QuestionResponsePair(questionKey, questionResponse));
                //QuestionResponsePairsList.Items.Add($"{questionKey} | {questionResponse}");
                i++;
            }

            var bc = new BrushConverter();
            ValidationsText.Foreground = (Brush)bc.ConvertFrom("#4BB543");
            ValidationsText.Text = "response file loaded successfully!";
        }
    }

    public class QuestionResponsePair()
    {
        public string QuestionMatchKey { get; set; }
        public string DesiredResponse { get; set; }
        
        public QuestionResponsePair(string key, string response) : this()
        {
            QuestionMatchKey = key;
            DesiredResponse = response;
        }
    }
}
