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
