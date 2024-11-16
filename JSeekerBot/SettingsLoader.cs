using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace JSeekerBot
{
    public class SettingsLoader
    {

        public string ResultFolderPath { get; set; }
        public string DefaultTextboxResponse { get; set; }
        public string DefaultComboBoxResponse { get; set; }
        public string DefaultRadioButtonResponse { get; set; }



        public static SettingsConfig LoadFile()
        {
            var config = new SettingsConfig();

            //Load Settings File
            using (StreamReader reader = new StreamReader("..\\..\\..\\..\\Settings\\JSeekerSettings.json"))
            {
                string json = reader.ReadToEnd();
                config = JsonSerializer.Deserialize<SettingsConfig>(json);
            }

            return config;
        }

        [Serializable]
        public struct SettingsConfig
        {
            public SettingsConfig()
            {
                
            }
            public SettingsConfig(string resultFolderPath, string dTextboxResponse, string dComboBoxResponse, string dRadioButtonResponse)
            {
                ResultFolderPath = resultFolderPath;
                DefaultComboBoxResponse = dComboBoxResponse;
                DefaultTextboxResponse = dTextboxResponse;
                DefaultRadioButtonResponse = dRadioButtonResponse;
            }

            public string ResultFolderPath { get; set; }
            public string DefaultTextboxResponse { get; set; }
            public string DefaultComboBoxResponse { get; set; }
            public string DefaultRadioButtonResponse { get; set; }
        }

        public static List<QuestionResponsePair> LoadQuestionResponseFile()
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

            List<QuestionResponsePair> pairs = new List<QuestionResponsePair>();

            while (questionKey != "" && questionResponse != "")
            {
                questionKey = workSheet.Cell(i + 2, 1).Value.ToString();
                questionResponse = workSheet.Cell(i + 2, 2).Value.ToString();

                if (questionKey == "" && questionResponse == "") break;

                pairs.Add(new QuestionResponsePair()
                {
                    QuestionKey = questionKey,
                    CorrectResponse = questionResponse
                });
                i++;
            }

            return pairs;
        }
    }
}
