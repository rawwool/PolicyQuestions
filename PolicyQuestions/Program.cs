using Questions.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Questions.Utility;

namespace PolicyQuestions
{
    class Program
    {
        static void Main(string[] args)
        {
            ////Questions.Utility.Reader.Read(Path.Combine(Environment.CurrentDirectory, "Questions.xlsx"));
            //Questions.Utility.Questions.LoadQuestions(Path.Combine(Environment.CurrentDirectory, "Questions.xlsx"));
            //var question = Questions.Utility.Questions.GetNextQuestion();
            //while (question != null)
            //{
            //    Console.WriteLine(question);
            //    question = Questions.Utility.Questions.GetNextQuestion();
            //}
            string[] source = { "Radio", "Radio button", "Button", "Drop down", "Combo", "Text box", "Date", "Calendar", "Text box & not sure button", "N/A, NA" };

            string match = Fuzzy.GetBestMatch(source, "N/A");

            int dist = Fuzzy.DamerauLevenshteinDistance("yes", "no", 3);
            int dist2 = Fuzzy.LevenshteinDistance("yes", "no");

            var test = "sub-question".RemoveSpecialCharacters();

        }
    }
}
