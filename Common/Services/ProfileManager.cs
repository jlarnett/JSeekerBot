using ClosedXML.Excel;

namespace JSeeker.Common.Services
{
    public class ProfileManager
    {
        private static readonly SettingsBuilder SettingsBuilder = new SettingsBuilder();
        
        
        public static List<QuestionResponsePair> LoadQuestionResponseFile()
        {
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
        
        public struct QuestionResponsePair
        {
            public string QuestionKey { get; set; }
            public string CorrectResponse { get; set; }
        }
    }
}
