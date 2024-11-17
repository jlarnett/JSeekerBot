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
using ClosedXML.Excel;

namespace JSeekerBot.UI
{
    /// <summary>
    /// Interaction logic for ResponseHelperControl.xaml
    /// </summary>
    public partial class ResponseHelperControl : UserControl
    {
        public ResponseHelperControl()
        {
            InitializeComponent();
            LoadQuestionResponseFile();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            bool keyExists = false;

            //Check for key
            foreach (var item in QuestionResponsePairsList.Items)
            {
                var key = item.ToString().Split("|")[0].Replace(" ", "").ToUpper();
                if(key == QuestionKeyTextbox.Text.ToUpper())
                    keyExists = true;
            }

            if(!keyExists)
                QuestionResponsePairsList.Items.Add($"{QuestionKeyTextbox.Text}|{QuestionResponseTextbox.Text}");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            QuestionResponsePairsList.Items.RemoveAt(QuestionResponsePairsList.SelectedIndex);
        }

        private void WriteResponseFile(object sender, RoutedEventArgs e)
        {
            bool resultFileExists = File.Exists("..//..//..//..//QuestionResponseFiles//qr_mapping.xlsx");
            XLWorkbook wb;
            IXLWorksheet workSheet;

            if (resultFileExists)
            {
                wb = new XLWorkbook("..//..//..//..//QuestionResponseFiles//qr_mapping.xlsx");
                workSheet = wb.Worksheet("QuestionResponsePairs");
            }
            else
            {
                wb = new XLWorkbook();
                workSheet = wb.AddWorksheet($"QuestionResponsePairs");
            }

            workSheet.Cell(1, 1).Value = "Question Key";
            workSheet.Cell(1, 2).Value = "Preferred Response";

            for (int i = 0; i < QuestionResponsePairsList.Items.Count; i++)
            {
                var split = QuestionResponsePairsList.Items[i].ToString().Split("|");
                workSheet.Cell(i + 2, 1).Value = split[0];
                workSheet.Cell(i+2, 2).Value = split[1];
            }

            wb.SaveAs($"..//..//..//..//QuestionResponseFiles//qr_mapping.xlsx");
        }

        private void LoadQuestionResponseFile()
        {
            bool resultFileExists = File.Exists("..//..//..//..//QuestionResponseFiles//qr_mapping.xlsx");
            XLWorkbook wb;

            //Ternary to check if file exists
            wb = resultFileExists ? new XLWorkbook("..//..//..//..//QuestionResponseFiles//qr_mapping.xlsx") : new XLWorkbook();
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

                QuestionResponsePairsList.Items.Add($"{questionKey} | {questionResponse}");
                i++;
            }

        }
    }
}
