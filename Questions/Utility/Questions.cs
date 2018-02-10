using Newtonsoft.Json.Linq;
using Questions.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Utility
{
    public class Questions
    {
        private static List<Question> _Questions;
        private static Question _CurrentQuestion;

        public static void LoadQuestions(string file, IEnumerable<string> tabs)
        {
            _Questions = Reader.Read(file, tabs);
        }

        public static string GetResponseJSON()
        {
            return _Questions.GroupBy(s => s.APIResource)
                .OrderBy(s=>s.Key)
                .Select(s => new { Resource = s.Key, JSON = GetResponseJSON(s.ToList()) })
                .Select(s => $"{s.Resource}{Environment.NewLine}{s.JSON}")
                .Aggregate((a, b) => $"{a}{Environment.NewLine}{Environment.NewLine}{b}");
        }
        public static string GetResponseJSON(List<Question> listOfQuestions)
        {
            dynamic response = new JObject();
            //response.ProductName = "Elbow Grease";
            //response.Enabled = true;
            //product.Price = 4.90m;
            //product.StockCount = 9000;
            //product.StockValue = 44100;
            //product.Tags = new JArray("Real", "OnSale");

            //Console.WriteLine(product.ToString());
            Dictionary<string, JObject> dict = new Dictionary<string, JObject>();

            listOfQuestions.ForEach(s =>
            {
                if (s.APIRequestField != null)
                {
                    var splits = s.APIRequestField.Split('.');
                    if (splits.Length == 1)
                    {
                        //if (response.ContainsKey(s.APIRequestField) == false)
                        if (DoesKeyExist(response, s.APIRequestField) == false)
                        {
                            response.Add(s.APIRequestField, s.UserResponse);
                        }
                        //if (dict.ContainsKey(s.APIRequestField) == false)
                        //{
                        //    dict.Add(s.APIRequestField, new JObject());
                        //}
                    }
                    else
                    {
                        IEnumerable<string> keys = GetPossibleKeys(splits);
                        var parent = response;
                        foreach (string key in keys)
                        {
                            if (dict.ContainsKey(key) == false)
                            {
                                dict.Add(key, new JObject());
                                if (DoesKeyExist(parent, key.Split('.').Last()) == false) parent.Add(key.Split('.').Last(), dict[key]);
                            }
                            parent = dict[key];
                        }

                    if (DoesKeyExist(dict[keys.Last()], s.APIRequestField.Split('.').Last()) == false)
                        dict[keys.Last()].Add(s.APIRequestField.Split('.').Last(), s.UserResponse);

                    }
                }
            });

            return response.ToString();
        }

        private static bool DoesKeyExist(JObject jobject, string key)
        {
            bool exists = false;

            exists = jobject.Descendants()
                            .Where(t => t.Type == JTokenType.Property /*&& ((JProperty)t).Name == "id"*/)
                            .Select(p => ((JProperty)p).Name)
                            .FirstOrDefault(s => s == key) != null;

            //if (templateIdList.IndexOf(key) != -1)
            //{
            //    exists = true;
            //}
            return exists;
        }
        /*
         * 
{
  "PolicyType": "Contents only",
  "Work": "I do not work",
  "Work.Type": {
    "Type": "Architect"
  },
  "Work.Duration": {
    "Duration": "5"
  },
  "Work.Duration.Unit": {
    "Unit": "Year"
  }
}
         * 
         * 
         */
        private static IEnumerable<string> GetPossibleKeys(IEnumerable<string> aPIRequestField)
        {
            int count = aPIRequestField.Count();
            var list = aPIRequestField.ToList();
            List<string> listReturn = new List<string>();
            //list.Add(aPIRequestField.First());
            for (int i = 0; i < count - 1; i++)
            {
                string key = string.Empty;
                for (int j = 0; j <= i; j++)
                {
                    key = key + "." + list[j];
                }
                key = key.TrimStart('.');
                listReturn.Add(key);
            }
            return listReturn;
        }

        public static IEnumerable<string> GetCategtories()
        {
            if (_Questions != null)
            {
                return _Questions.Select(s => s.Category).Distinct();
            }
            return new string[] { "Empty" };
        }

        public static IEnumerable<Question> GetQuestion(string category)
        {
            string key = Fuzzy.GetBestMatch(_Questions.Select(s => s.Category), category);
            if (key != null)
            {
                return _Questions.Where(s => s.Category == key);
            }
            else return null;
        }

        public static Question GetNextQuestion()
        {
            _CurrentQuestion = GetNextQuestionPrivate();
            return _CurrentQuestion;
        }

        private static Question GetNextQuestionPrivate()
        {
            if (_Questions == null) return null;
            if (_CurrentQuestion == null) return _Questions.FirstOrDefault();
            
            if (_CurrentQuestion.Parent == null)
            {
                var firstEligibleChildQuestion = _CurrentQuestion.Children.FirstOrDefault(s => s.InvokeThisQuestion());
                if (firstEligibleChildQuestion != null)
                {
                    return firstEligibleChildQuestion;
                }
                else
                {
                    //We are in the top level
                    return GetNextQuestion(_Questions, _CurrentQuestion);
                }
            }

            // We are in the children level
            var question = GetNextQuestion(_CurrentQuestion.Parent.Children, _CurrentQuestion);
            if (question == null)
            {
                // Go back to top level
                return GetNextQuestion(_Questions, _CurrentQuestion.Parent);
            }
            return question;
        }

        private static Question GetNextQuestion(List<Question> questions, Question question)
        {
            int index = questions.IndexOf(question) + 1;
            while (index >= 0 && index < questions.Count && !questions[index].InvokeThisQuestion())
            {
                index++;
            }
            if (index < questions.Count) return questions[index]; else return null;
        }
    }
}
