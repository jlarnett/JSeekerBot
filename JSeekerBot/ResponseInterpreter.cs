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
            questionResponseDictionary.Add(questionKeyword.ToUpper(), correctResponse.ToUpper());
        }

        public string? GetQuestionResponse(string question)
        {
            var upper = question.ToUpper();
            foreach (var questionResponsePair in questionResponseDictionary)
            {
                if (upper.Contains(questionResponsePair.Key))
                {
                    return questionResponsePair.Value;
                }
            }

            return null;
        }
    }
}
