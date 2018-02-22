using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Questions.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Utility
{
    public class APIRequest
    {
        public bool IsComplete { get; set; }
        public string ValidationError { get; set; }
        public string RelativeURL { get; set; }
        public string JSONBody { get; set; }

        public override string ToString()
        {
            return $"{RelativeURL}{Environment.NewLine}{JSONBody}";
        }
    }

    public class ReferenceData
    {
        public string Path { get; set; }
        public string Data { get; set; }

        public override string ToString()
        {
            return $"{Path}{Environment.NewLine}{Data}";
        }
    }

    public class Questions
    {
        private static List<Question> _Questions;
        private static Question _CurrentQuestion;

        public static bool DeserialiseQuestions(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    _Questions = JsonConvert.DeserializeObject<List<Question>>(File.ReadAllText(file));
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static void SerilaiseQuestions(string file)
        {
            Serialise(file);
        }

        public static void LoadQuestions(string file, IEnumerable<string> tabs)
        {
           
            _Questions = Reader.Read(file, tabs);
            
        }

        private static void Serialise(string fileName)
        {
            //var questions = new List<Question>();
            //questions.AddRange(_Questions.SelectMany(s => s.Children));
            //questions.AddRange(_Questions);

            using (var file = File.CreateText(fileName))
            {
                file.Write(JsonConvert.SerializeObject(_Questions, Formatting.Indented, new JsonSerializerSettings()));
                file.Flush();
            }
            
            // serialize JSON directly to a file
            //using (StreamWriter file = File.CreateText(@"Rio.json"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(file, questions);
            //}
        }

        public static IEnumerable<ReferenceData> GetReferenceData()
        {
            var questions = new List<Question>();
            questions.AddRange(_Questions.SelectMany(s => s.Children));
            questions.AddRange(_Questions);

            return questions
                .GroupBy(s => s.APIResource)
                .OrderBy(s => s.Key)
                .Select(s => new ReferenceData() { Path = s.Key, Data = JsonConvert.SerializeObject(new { Questions = s }, Formatting.Indented, new JsonSerializerSettings()) });
        }

        public static IEnumerable<APIRequest> GetResponseJSON()
        {
            var questions = new List<Question>();
            questions.AddRange(_Questions.SelectMany(s => s.Children));
            questions.AddRange(_Questions);
            // var groups = _Questions.GroupBy(s => s.APIResource);
            return questions
                .GroupBy(s => s.APIResource)
                .OrderBy(s => s.Key)
                //.Select(s => new { Resource = s.Key, JSON = s.AsJSON() /*GetResponseJSON(s)*/ })
                .Select(s =>
                {
                    var unAnsweredQuestions = s.Where(d => d.Type == enumType.Mandaory && d.UserResponse == null);
                    var apir = new APIRequest()
                    {
                        RelativeURL = s.Key,
                        JSONBody = s.AsJSON(),
                        IsComplete = unAnsweredQuestions.FirstOrDefault() == null,
                        ValidationError = unAnsweredQuestions.FirstOrDefault() == null ? "" : "Unanswered questions: " + unAnsweredQuestions.Select(d => d.Ref).Aggregate((a, b) => $"{a}, {b}")
                    };
                    return apir;
                });
                //.Select(s => $"{s.Resource}{Environment.NewLine}{s.JSON}")
                //.Aggregate((a, b) => $"{a}{Environment.NewLine}{Environment.NewLine}{b}");                
        }

        /// <summary>
        /// dynamic response = new JObject();
        //response.ProductName = "Elbow Grease";
        //response.Enabled = true;
        //product.Price = 4.90m;
        //product.StockCount = 9000;
        //product.StockValue = 44100;
        //product.Tags = new JArray("Real", "OnSale");
        //Console.WriteLine(product.ToString());
        /// </summary>
        /// <param name="questions"></param>
        /// <returns></returns>
        public static string BuildResponseJSON(IEnumerable<Question> questions)
        {
            dynamic response = new JObject();
            Dictionary<string, JContainer> dict = new Dictionary<string, JContainer>();
            questions
                .OrderByHierarchy()
                .Where(s=> !string.IsNullOrEmpty(s.APIRequestField))
                .ToList()
                .ForEach(s =>
                {

                });
            return null;
        }

        [Obsolete]
        public static string GetResponseJSON(IEnumerable<Question> listOfQuestions)
        {
            dynamic response = new JObject();
            //response.ProductName = "Elbow Grease";
            //response.Enabled = true;
            //product.Price = 4.90m;
            //product.StockCount = 9000;
            //product.StockValue = 44100;
            //product.Tags = new JArray("Real", "OnSale");

            //Console.WriteLine(product.ToString());
            Dictionary<string, JContainer> dict = new Dictionary<string, JContainer>();

            listOfQuestions
                .OrderByHierarchy()
                .ToList()
                .ForEach(s =>
            {
                if (s.APIRequestField != null)
                {
                    var parent = response;
                    var splits = s.APIRequestField.Split('.');
                    //previousClaims[]
                    //previuosClaims.Year
                    //
                    if (splits.Length == 1)
                    {
                        //if (response.ContainsKey(s.APIRequestField) == false)
                        if (DoesKeyExist(response, s.APIRequestField.TrimEnd('[', ']')) == false)
                        {
                            if (s.APIRequestField.EndsWith("[]"))
                            {
                                dict.Add(s.APIRequestField.TrimEnd('[', ']'), new JArray());

                                response.Add(s.APIRequestField.TrimEnd('[', ']'), dict[s.APIRequestField.TrimEnd('[', ']')]);
                                //parent = dict[s.APIRequestField.TrimEnd('[', ']')];
                            }
                            else
                            {
                                response.Add(s.APIRequestField, s.UserResponse);
                            }
                        }
                    }
                    else
                    {
                        IEnumerable<string> keys = GetPossibleKeys(s.APIRequestField);

                        foreach (string key in keys)
                        {
                            if (dict.ContainsKey(key.TrimEnd('[',']')) == false)
                            {
                                if (key.EndsWith("[]"))
                                {
                                    dict.Add(key.TrimEnd('[', ']'), new JArray());
                                }
                                else
                                {
                                    dict.Add(key, new JObject());
                                }
                                if (DoesKeyExist(parent, key.Split('.').Last().TrimEnd('[', ']')) == false)
                                {
                                    parent.Add(key.Split('.').Last().TrimEnd('[',']'), dict[key.TrimEnd('[', ']')]);
                                }
                            }
                            parent = dict[key.TrimEnd('[', ']')];
                        }

                        if (!keys.Last().EndsWith("[]"))
                        {
                            if (dict[keys.Last()] is JObject && DoesKeyExist(dict[keys.Last()] as JObject, s.APIRequestField.Split('.').Last()) == false)
                                (dict[keys.Last()] as JObject).Add(s.APIRequestField.Split('.').Last(), s.UserResponse.Value);
                            if (dict[keys.Last()] is JArray)
                            {
                                string propName = s.APIRequestField.Split('.').Last();
                                JObject jobject = GetExistingOrNewJObject(dict[keys.Last()] as JArray, propName);
                                jobject.Add(propName, s.UserResponse.Value);
                            }
                        }
                    }
                }
            });

            return response.ToString();
        }

        private static string GetSortField(Question s)
        {
            var splits = s.APIRequestField.Split('.');
            if (splits.Length > 1)
            {
                return splits.Take(splits.Length - 1).Aggregate((a, b) => $"{a}.{b}");
            }
            return s.APIRequestField;
        }

        private static JObject GetExistingOrNewJObject(JArray array, string name)
        {
            JObject jo = array.Children<JObject>()
                //.FirstOrDefault(o => o["text"] != null && o["text"].ToString() == "Two");
                .FirstOrDefault(o => o[name] == null);
            if (jo == null)
            {
                jo = new JObject();
                array.Add(jo);
                return jo;
            }
            return jo;
        }

        private static bool DoesKeyExist(JArray array, string key)
        {
            JObject jo = array.Children<JObject>()
                //.FirstOrDefault(o => o["text"] != null && o["text"].ToString() == "Two");
                .FirstOrDefault(o => o[key] == null);
            return jo != null;
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
        private static IEnumerable<string> GetPossibleKeys(string aPIRequestField)
        {
            IEnumerable<string> keys = aPIRequestField.Split('.');
            int count = aPIRequestField.EndsWith("[]")? keys.Count() : keys.Count() - 1;
            var list = keys.ToList();
            List<string> listReturn = new List<string>();
            //list.Add(aPIRequestField.First());

            for (int i = 0; i < count /*- 1*/; i++)
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

        public static Question GetQuestion(string id)
        {
            var q = _Questions.FirstOrDefault(s => s.Id == id);
            if (q == null)
            {
                q = _Questions.SelectMany(s => s.Children).FirstOrDefault(s => s.Id == id);
            }
            return q;
        }

        public static IEnumerable<Question> GetQuestions(string category)
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
            
            if (_CurrentQuestion.ParentId == null)
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
            Question parent = _Questions.First(s => s.Id == _CurrentQuestion.ParentId);
            if (parent == null)
                return null;
            var question = GetNextQuestion(/*_CurrentQuestion.Parent.Children*/parent.Children, _CurrentQuestion);
            if (question == null)
            {
                // Go back to top level
                return GetNextQuestion(_Questions, parent);
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
