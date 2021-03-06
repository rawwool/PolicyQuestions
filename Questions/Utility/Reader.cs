﻿using Newtonsoft.Json.Linq;
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
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public static List<Question> Read(string excelFile, IEnumerable<string> tabs)
        {
            Excel.Application xlApp = null;
            Excel.Workbook xlWorkbook = null;
            int hWnd = 0;
            try
            {
                List<Question> listOfQUestions = new List<Question>();
                xlApp = new Excel.Application();
                hWnd = xlApp.Hwnd;
                xlWorkbook = xlApp.Workbooks.Open(excelFile);
                //Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[6];

                Dictionary<string, Excel._Worksheet> dict = new Dictionary<string, Excel._Worksheet>();
                foreach (Excel._Worksheet worksheet in xlWorkbook.Worksheets)
                {
                    dict.Add(worksheet.Name, worksheet);
                }

                tabs.ToList().ForEach(s =>
                LoadWorksheet(listOfQUestions, GetWorksheet(dict, s)));
                List<Question> allQuestions = new List<Question>();

                allQuestions.AddRange(listOfQUestions);
                allQuestions.AddRange(listOfQUestions.SelectMany(s => s.Children));
                foreach (var question in allQuestions)
                {
                    question.Expressions = GetExpressionsForrPresentation(allQuestions, question.DisplayRule);
                    question.Expressions.Questions.ToList().ForEach(s => allQuestions.First(q => q.Id == s).LogicalChildren.Add(question.Id));
                }
                
                //rule of thumb for releasing com objects:
                //  never use two dots, all COM objects must be referenced and released individually
                //  ex: [somthing].[something].[something] is bad
                return listOfQUestions;
            }
            finally
            {
                //close and release
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);

                if (xlApp.Workbooks != null) Marshal.ReleaseComObject(xlApp.Workbooks);

                //quit and release
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);

                //cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();

                SendMessage((IntPtr)hWnd, 0x10, IntPtr.Zero, IntPtr.Zero);
            }

            /* https://www.add-in-express.com/creating-addins-blog/2013/11/05/release-excel-com-objects/
            if (sheets != null) Marshal.ReleaseComObject(sheets);
            if (book != null) Marshal.ReleaseComObject(book);
            if (books != null) Marshal.ReleaseComObject(books);
            if (app != null) Marshal.ReleaseComObject(app);
            */
        }


        private static Excel._Worksheet GetWorksheet(Dictionary<string, Excel._Worksheet> dict, string s)
        {
            string key = Fuzzy.GetBestMatch(dict.Keys, s);
            if (key != null)
                return dict[key];
            else return null;
        }

        //class Range
        //{
        //    public int RowCount { get;  }
        //    public int ColumnCount { get; }

        //    private string[,] Values;

        //    public Range(System.Array values)
        //    {
        //        if (values.Rank != 2) throw new InvalidOperationException("Only 2D arrays are accepted");
        //        int columncount = values.GetLength(0);
        //        for (int i = 1; i <= values.Length; i++)
        //        {

        //        }
        //}
        //private static string[,] ConvertToStringArray(System.Array values)
        //{
        //    List<>

        //    // loop through the 2-D System.Array and populate the 1-D String Array
        //    for (int i = 1; i <= values.Length; i++)
        //    {
        //        if (values.GetValue(1, i) == null)
        //            theArray[i - 1] = "";
        //        else
        //            theArray[i - 1] = (string)values.GetValue(1, i).ToString();
        //    }

        //    return theArray;
        //}

        private static string GetValue(System.Array values, int row, int column)
        {
            if (row * column == 0) return string.Empty;
            try
            {
                object value = values.GetValue(row, column);
                if (value != null) return value.ToString().Trim();
                else return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void LoadWorksheet(List<Question> listOfQUestions, Excel._Worksheet xlWorksheet)
        {
            if (xlWorksheet == null) return;


            Excel.Range xlRange = null;
            try
            {
                xlRange = xlWorksheet.UsedRange;
                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;

                System.Array xlRangeValues = (System.Array)xlRange.Cells.Value;


                //iterate over the rows and columns and print to the console as it appears in the file
                //excel is not zero based!!
                var columns = GetColumns(xlRangeValues, 1);

                int answersColumnIndex = GetColumnIndex(columns, "Answers");
                int referenceColumnIndex = GetColumnIndex(columns, "Question Ref");
                int mandatoryOptionalColumnIndex = GetColumnIndex(columns, "Field Display");
                int displayRuleColumnIndex = GetColumnIndex(columns, "Field Display Rule");
                int dataTypeColumnIndex = GetColumnIndex(columns, "Data Type (Hiscox UI)");
                int helpColumnIndex = GetColumnIndex(columns, "Help copy");
                int questionTextColumnIndex = GetColumnIndex(columns, "Question Text");
                int subQuestionTextColumnIndex = GetColumnIndex(columns, "sub-Question");
                int apiRequestFieldColumnIndex = GetColumnIndex(columns, "API Request Field");
                int apiResourceFieldIndex = GetColumnIndex(columns, "API Resource");
                int uiValidationIndex = GetColumnIndex(columns, "UI Validation");
                Question parentQuestion = null;
                for (int i = 2; i <= rowCount; i++)
                {
                    var reference = GetValue(xlRangeValues, i, referenceColumnIndex);
                    string questionText = GetValue(xlRangeValues, i, questionTextColumnIndex);
                    string subQuestionText = GetValue(xlRangeValues, i, subQuestionTextColumnIndex);
                    string displayRule = GetValue(xlRangeValues, i, displayRuleColumnIndex);

                    if (string.IsNullOrWhiteSpace(reference)
                        && string.IsNullOrWhiteSpace(questionText)
                        && string.IsNullOrWhiteSpace(subQuestionText)) continue;
                    //var conditionParent = GetParentAndResponseForInvokingThisChildQuestion(GetValue(xlRangeValues, i, displayRuleColumnIndex));
                    Question question = new Question
                    {
                        Category = xlWorksheet.Name,
                        Ref = reference,
                        Type = ConvertToType<enumType>(GetValue(xlRangeValues, i, mandatoryOptionalColumnIndex)),
                        //ConditionParent = conditionParent == null? null : listOfQUestions.FirstOrDefault(f => f.Ref.Trim() == conditionParent.Item1),//Assuming that question has already been loaded
                        //ParentResponseForInvokingThisChildQuestion = conditionParent == null ? null : conditionParent.Item2,
                        //ConditionForPresentation = GetConditionForPresentation(listOfQUestions, GetValue(xlRangeValues, i, displayRuleColumnIndex)),
                        //Expressions = GetExpressionsForrPresentation(listOfQUestions, displayRule),
                        DisplayRule = displayRule,
                        DataCaptureType = ConvertToType<enumDataCaptureType>(GetValue(xlRangeValues, i, dataTypeColumnIndex)),
                        APIRequestField = GetValue(xlRangeValues, i, apiRequestFieldColumnIndex),
                        APIResource = GetValue(xlRangeValues, i, apiResourceFieldIndex),
                        UIValdationMessage = GetValue(xlRangeValues, i, uiValidationIndex),
                        ResponseChoices = GetResponseChoice(GetValue(xlRangeValues, i, answersColumnIndex).Split('\n').Where(s => s.Trim().Length > 0).ToList())
                    };

                    //ConditionForPresentation >> ParentQuestion, ThisQuestio'sResponse, IsthatPositive, Full Response
                    //if (question.ConditionForPresentation.Item1 != null)
                    //{
                    //    question.ConditionForPresentation.Item1.LogicalChildren.Add(question);
                    //}
                    question.Text = questionText;
                    if (string.IsNullOrEmpty(question.Text.Trim()))
                    {
                        question.Text = subQuestionText;
                        question.ParentId = parentQuestion.Id;
                        parentQuestion.Children.Add(question);
                    }
                    else
                    {
                        parentQuestion = question;
                        listOfQUestions.Add(question);
                    }

                    //question.Expressions.Questions.ToList().ForEach(s => listOfQUestions.First(q=>q.Id == s).LogicalChildren.Add(question));

                    question.HelpText = $"{GetValue(xlRangeValues, i, helpColumnIndex)}\nDisplay rule:{displayRule}";
                }

                
            }
            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message + Environment.NewLine + "xlWorksheet: " + xlWorksheet.Name);
            }
            finally
            {
                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);
            }
        }

        private static List<Question.ResponseChoice> GetResponseChoice(List<string> list)
        {
            //LookUp='Dynamic QS Occupations'!E2070:F2097
            //If the list has one item and with the above signature, it is a look up of name + value columns
            if (list.Count() == 1 && list.First().StartsWith("LookUp"))
            {
                /*https://social.msdn.microsoft.com/Forums/office/en-US/afd01976-63d0-4f96-9ba4-e3e2b6cf8d55/excel-with-c-how-to-specify-a-range-?forum=vsto
                //New Application
                Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = true;
 
                //create a new blank workbook
                excelApp.Workbooks.Add(Type.Missing);
                //create a new blank worksheet in current workbook
                excelApp.Worksheets.Add(Type.Missing, Type.Missing, 1, Type.Missing);
 
                Excel.Worksheet ws = excelApp.ActiveSheet as Excel.Worksheet;
                //Get the range using number index
                Excel.Range newRng = excelApp.get_Range(ws.Cells[1,1], ws.Cells[4, 5]);
                newRng.Value2 = "Test String put in Cells";
                */
            }

            return list.Select(s =>
            {
                var splits = s.Split('=');
                return new Question.ResponseChoice() { Display = splits.First().Trim(), Value = splits.Last().Trim() };
            })
            .ToList();
        }

        private static IEnumerable<string> GetColumns(System.Array xlRange, int rownNumber)
        {
            int colCount = xlRange.GetLength(1);
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

        //[orand]* \w* *is *\"([^\"]*)\"
        //https://regex101.com/
        //[orand]* \w* *[is not]* *\"([^\"]*)\"
        //34534 34534 345645654A is "test" or 345645654A is"best" and terter is "fdkjghfd" or sfsd is not "rtre"
        //34534 34534 345645654A is not "test" and 345645654A is"best" and terter is "fdkjghfd" or sfsd is not "rtre"


        

        public static Expressions GetExpressionsForrPresentation(List<Question> listOfQuestions, string line)
        {
            try
            {
                //In the spreadhseet the response if enclosed in double quotes
                string regex = "(?'operator' or| and)? (?'question'\\w*) *(?'comparer'is|not|is not)? *\"(?'response'[^\"]*)\"";
                var clauses = from Match match in Regex.Matches(line, regex)
                              select match;
                var expressions = clauses.Select(s =>
                {
                    string op = s.Groups["operator"].Success ? s.Groups["operator"].Value.Trim() : "and";
                    string questionId = s.Groups["question"].Success ? s.Groups["question"].Value.Trim() : "none";
                    string response = s.Groups["response"].Success ? s.Groups["response"].Value.Trim() : "none";
                    string comparer = s.Groups["comparer"].Success ? s.Groups["comparer"].Value.Trim() : "none";
                    var q = GetQuestionFromTreeByRef(listOfQuestions, questionId);
                    var expression = new Expression()
                    {
                        Operator = op,
                        QuestionId = q.Id,
                        QuestionRef = q.Ref,
                        ValueToCompareWith = response,
                        Positive = Fuzzy.AreSimilar(comparer, "is"),
                    };
                    return expression;
                });


                return new Expressions(expressions);
            }
            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message + Environment.NewLine + "expression text: " + line);
            }
        }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="line"></param>
            /// <returns></returns>
        private static Tuple<Question, string, bool, string> GetConditionForPresentation(List<Question> listOfQuestions, string line)
        {
            //In the spreadhseet the response if enclosed in double quotes
            var clauses = from Match match in Regex.Matches(line, "[orand]* \\w* *[is not]* *\"([^\"]*)\"")
                          select match.ToString().Trim();
             var expressions =  clauses.Select(s=>
                          {
                              var t = s.Split(' ');


                              var expression = new Expression()
                              {
                                  Operator = t.Count() == 3 ? t.First().Trim() : "and",
                                  QuestionId = GetQuestionFromTreeByRef(listOfQuestions, t.First().Trim()).Id,
                                  ValueToCompareWith = t.Last().Trim()
                              };
                              return expression;
                          });
            


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
                //Question question = listOfQuestions.FirstOrDefault(f => f.Ref.Trim() == t.First());
                Question question = GetQuestionFromTreeByRef(listOfQuestions, t.First().Trim());
                if (response.Success)
                {
                    //Question, Response, IsthatPositive, Full Response
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

        private static Question GetQuestionFromTreeByRef(List<Question> listOfQuestions, string v)
        {
            if (string.IsNullOrWhiteSpace(v))
                throw new InvalidOperationException("The question reference is missing.\nTypical expression:  if 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"");
            Question question = listOfQuestions.FirstOrDefault(f => f.Ref.Trim() == v);
            if (question == null)
            {
                //look in the child questions
                question = listOfQuestions.SelectMany(s => s.Children).FirstOrDefault(f => f.Ref.Trim() == v);
            }

            if (question == null) throw new InvalidOperationException($"Question {v} not found.");

            return question;
        }

        //https://stackoverflow.com/a/3877738
        private static T ConvertToType<T>(string str) where T: struct
        {
            try
            {
                str = Fuzzy.GetBestMatch(Extension.GetEnumNameDescriptionAttribueValuePairs<T>(), str);
                //str = Fuzzy.GetBestMatch(Enum.GetNames(typeof(T)).ToList(), str);
                if (str == null) return default(T);
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
