using Questions.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace Questions.Utility
{
    public class Reader
    {
        public static List<Question> Read(string excelFile, IEnumerable<string> tabs)
        {
            List<Question> listOfQUestions = new List<Question>();
            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(excelFile);
            //Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[6];

            Dictionary<string, Excel._Worksheet> dict = new Dictionary<string, Excel._Worksheet>();
            foreach (Excel._Worksheet worksheet in xlWorkbook.Worksheets)
            {
                dict.Add(worksheet.Name, worksheet);
            }

            tabs.ToList().ForEach(s =>
            LoadWorksheet(listOfQUestions, GetWorksheet(dict, s)));
            

            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad


            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            return listOfQUestions;
        }

        private static Excel._Worksheet GetWorksheet(Dictionary<string, Excel._Worksheet> dict, string s)
        {
            string key = Fuzzy.GetBestMatch(dict.Keys, s);
            if (key != null)
                return dict[key];
            else return null;
        }

        private static void LoadWorksheet(List<Question> listOfQUestions, Excel._Worksheet xlWorksheet)
        {
            if (xlWorksheet == null) return;

            Excel.Range xlRange;

            xlRange = xlWorksheet.UsedRange;
            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            //iterate over the rows and columns and print to the console as it appears in the file
            //excel is not zero based!!
            var columns = GetColumns(xlRange, 1);

            int answersColumnIndex = GetColumnIndex(columns, "Answers");
            int referenceColumnIndex = GetColumnIndex(columns, "Question Ref");
            int mandatoryOptionalColumnIndex = GetColumnIndex(columns, "Field Display");
            int displayRuleColumnIndex = GetColumnIndex(columns, "Field Display Rule");
            int dataTypeColumnIndex = GetColumnIndex(columns, "Data Type (Hiscox UI)");
            int helpColumnIndex = GetColumnIndex(columns, "Help copy");
            int questionTextColumnIndex = GetColumnIndex(columns, "Question Text");
            int subQuestionTextColumnIndex = GetColumnIndex(columns, "sub-Question");
            Question parentQuestion = null;
            for (int i = 2; i <= rowCount; i++)
            {
                var reference = GetValue(xlRange, i, referenceColumnIndex);
                string questionText = GetValue(xlRange, i, questionTextColumnIndex);
                string subQuestionText = GetValue(xlRange, i, subQuestionTextColumnIndex);
                if (string.IsNullOrWhiteSpace(reference) 
                    && string.IsNullOrWhiteSpace(questionText)
                    && string.IsNullOrWhiteSpace(subQuestionText)) continue;
                //var conditionParent = GetParentAndResponseForInvokingThisChildQuestion(GetValue(xlRange, i, displayRuleColumnIndex));
                Question question = new Question
                {
                    Category = xlWorksheet.Name,
                    Ref = reference,
                    Type = ConvertToType<enumType>(GetValue(xlRange, i, mandatoryOptionalColumnIndex)),
                    //ConditionParent = conditionParent == null? null : listOfQUestions.FirstOrDefault(f => f.Ref.Trim() == conditionParent.Item1),//Assuming that question has already been loaded
                    //ParentResponseForInvokingThisChildQuestion = conditionParent == null ? null : conditionParent.Item2,
                    ConditionForPresentation = GetConditionForPresentation(listOfQUestions, GetValue(xlRange, i, displayRuleColumnIndex)),
                    DataCaptureType = ConvertToType<enumDataCaptureType>(GetValue(xlRange, i, dataTypeColumnIndex)),
                    HelpText = GetValue(xlRange, i, helpColumnIndex),
                    ResponseChoices = GetValue(xlRange, i, answersColumnIndex).Split('\n').Where(s => s.Trim().Length > 0).ToList()
                };

                question.Text = questionText;
                if (string.IsNullOrEmpty(question.Text.Trim()))
                {
                    question.Text = subQuestionText;
                    question.Parent = parentQuestion;
                    parentQuestion.Children.Add(question);
                }
                else
                {
                    parentQuestion = question;
                    listOfQUestions.Add(question);
                }
            }
            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

        }

        [Obsolete]
        private static int GetColumnIndex(Excel.Range xlRange, string v)
        {
            int colCount = xlRange.Columns.Count;
            List<string> names = new List<string>();
            for (int i = 1; i <= colCount; i++)
            {
                var name = GetValue(xlRange, 1, i);
                names.Add(name);
            }
            string match = Fuzzy.GetBestMatch(names, v);
            return names.IndexOf(match) + 1;
        }

        private static IEnumerable<string> GetColumns(Excel.Range xlRange, int rownNumber)
        {
            int colCount = xlRange.Columns.Count;
            List<string> names = new List<string>();
            for (int i = 1; i <= colCount; i++)
            {
                var name = GetValue(xlRange, rownNumber, i);
                //Just take the string before the line break.
                names.Add(name.Split('\n').FirstOrDefault());
            }

            return names.Where(s=> !string.IsNullOrWhiteSpace(s));
        }

        private static int GetColumnIndex(IEnumerable<string> columnNames, string v)
        {
            string match = Fuzzy.GetBestMatch(columnNames, v);
            return columnNames.ToList().IndexOf(match) + 1;
        }

        private static string SanitiseName(string worksheetName)
        {
            worksheetName = worksheetName.Trim();
            if (worksheetName.Length > 0 && char.IsDigit(worksheetName[0]))
            {
                worksheetName = worksheetName.Split(' ').Skip(1).Aggregate((a, b) => $"{a} {b}");
            }
            return worksheetName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static Tuple<Question, string, bool, string> GetConditionForPresentation(List<Question> listOfQuestions, string line)
        {
            //In the spreadhseet the response if enclosed in double quotes
            //\w* *is *"([^\"]*)"
            bool positive = true;
            var result = from Match match in Regex.Matches(line, "\\w* *is *\"([^\"]*)\"")
                         select match.ToString().Trim();
            if (result.FirstOrDefault() == null)
            {
                positive = false;
                // Check for Is Not
                result = from Match match in Regex.Matches(line, "\\w* *is *not *\"([^\"]*)\"")
                         select match.ToString().Replace("not", "").Trim();
            }
            Tuple<Question, string, bool, string> returnResult =
            result.Select(s=> 
            {
                var t=  s.Split(' ');
                Match response = Regex.Match(s, "\"([^\"]*)\"");
                Question question = listOfQuestions.FirstOrDefault(f => f.Ref.Trim() == t.First());

                if (response.Success)
                {
                    return new Tuple<Question, string, bool, string>(question, response.Value.Trim().Trim('"').Trim(), positive, line);
                }
                else
                {
                    return new Tuple<Question, string, bool, string>(question, null, positive, line);
                }
            })
            .FirstOrDefault();

            if (returnResult == null)
            {
                returnResult = new Tuple<Question, string, bool, string>(null, null, true, line);
            }

            return returnResult;
        }

        //https://stackoverflow.com/a/3877738
        private static T ConvertToType<T>(string str) where T: struct
        {
            try
            {
                str = Fuzzy.GetBestMatch(Extension.GetEnumNameDescriptionAttribueValuePairs<T>(), str);
                //str = Fuzzy.GetBestMatch(Enum.GetNames(typeof(T)).ToList(), str);
                T res = (T)Enum.Parse(typeof(T), str);
                if (!Enum.IsDefined(typeof(T), res)) return default(T);
                return res;
            }
            catch
            {
                return default(T);
            }
        }
    

        private static string GetValue(Excel.Range xlRange, int i, int j)
        {
            if (!xlRange.Cells[i, j].EntireRow.Hidden && xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                return xlRange.Cells[i, j].Value2.ToString();
            else
                return string.Empty;
        }
    }
}
