using Questions.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Model
{
    public class Question
    {
        public string Category { get; set; }
        public Question Parent { get; set; }

        //ParentQuestion, ThisQuestio'sResponse, IsthatPositive, Full Response
        //public Tuple<Question, string, bool, string> ConditionForPresentation { get; set; }

        public Expressions Expressions { get; set; }
        public List<Question> Children { get; set; }
        public List<Question> LogicalChildren { get; set; }
        public string Ref { get; set; }
        public string Text { get; set; }

        public enumType Type { get; set; }

        public enumDataCaptureType DataCaptureType { get; set; }

        public List<string> ResponseChoices { get; set; }

        //public string ParentResponseForInvokingThisChildQuestion { get; set; }

        public string DefaultResponse { get; set; }

        public string UserResponse { get; set; }
        public string HelpText { get; set; }

        public string InternalInfo
        {
            get
            {
                if (Expressions != null)
                {
                    return Expressions.ToString();
                }

                return string.Empty;
            }
        }

        public Action<bool> ShowHide;

        public Question()
        {
            Children = new List<Question>();
            LogicalChildren = new List<Question>();
        }

        public bool InvokeThisQuestion()
        {
            /*
            //ConditionForPresentation >> ParentQuestion, ThisQuestio'sResponse, IsthatPositive, Full Response
            if (ConditionForPresentation == null || ConditionForPresentation.Item1 == null || ConditionForPresentation.Item2 == null) return true;
            if (Fuzzy.GetExactMatch(new string[] { "NA", "N/A", "Not applicable" }, ConditionForPresentation.Item2, true) != null)
                return true;

            if (ConditionForPresentation.Item1.UserResponse == null && ConditionForPresentation.Item2 == null) return true;
            bool result = Fuzzy.AreSimilar(ConditionForPresentation.Item1.UserResponse, ConditionForPresentation.Item2);

            return ConditionForPresentation.Item3 ? result : !result;
            */
            return this.Expressions.Evaluate();
        }

        public override string ToString()
        {
            var children = this.LogicalChildren.Select(s => s.Ref);
            string childrenRef = "None";
            if (children != null && children.FirstOrDefault() != null)
            {
                childrenRef = children.Aggregate((a, b) => a + ", " + b);
            }
            return $"{Ref} {Text} Logical Children: {childrenRef}";
        }
    }
}
