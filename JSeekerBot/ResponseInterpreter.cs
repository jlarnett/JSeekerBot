﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSeekerBot
{
    public class ResponseInterpreter
    {
        private Dictionary<string, string> questionResponseDictionary = new Dictionary<string, string>();

        public void AddQuestionResponse(List<QuestionResponsePair> pairs)
        {
            foreach (var pair in pairs)
            {
                questionResponseDictionary.Add(pair.QuestionKey.ToUpper(), pair.CorrectResponse);
            }
        }

        public void AddQuestionResponse(string questionKeyword, string correctResponse)
        {
            questionResponseDictionary.Add(questionKeyword.ToUpper(), correctResponse);
        }

        public string? GetQuestionResponse(string question, String jobTitle = "")
        {
            var upper = question.ToUpper();
            foreach (var questionResponsePair in questionResponseDictionary)
            {
                if (upper.Contains(questionResponsePair.Key))
                {
                    if (questionResponsePair.Key == "COVER LETTER")
                    {
                        return questionResponsePair.Value.Replace("{jobtitlerole}", jobTitle);
                    }
                    else
                    {
                        return questionResponsePair.Value;
                    }
                    
                }
            }

            return null;
        }
    }

    public struct QuestionResponsePair
    {
        public string QuestionKey { get; set; }
        public string CorrectResponse { get; set; }
    }
}
