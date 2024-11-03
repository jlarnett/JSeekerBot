using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSeekerBot
{
    public class ResponseInterpreter
    {
        private Dictionary<string, string> questionResponseDictionary = new Dictionary<string, string>();

        public void AddQuestionResponse(string questionKeyword, string correctResponse)
        {
            questionResponseDictionary.Add(questionKeyword, correctResponse);
        }

        public string? GetQuestionResponse(string question)
        {
            foreach (var questionResponsePair in questionResponseDictionary)
            {
                if (question.Contains(questionResponsePair.Key))
                {
                    return questionResponsePair.Value;
                }
            }

            return null;
        }
    }
}
