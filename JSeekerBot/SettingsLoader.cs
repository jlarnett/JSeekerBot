using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;
using JSeeker.Common.Services;

namespace JSeekerBot
{
    public class SettingsLoader
    {
        private static readonly SettingsBuilder SettingsBuilder = new SettingsBuilder();
        
        public static SettingsBuilder.SettingsConfig LoadFile()
        {
            return SettingsBuilder.LoadConfig();
        }

        public static List<ProfileManager.QuestionResponsePair> LoadQuestionResponseFile()
        {
            return ProfileManager.LoadQuestionResponseFile();
        }
    }
}
