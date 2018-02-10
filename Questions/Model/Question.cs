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

        public string APIRequestField { get; set; }

        public string APIResource { get; set; }
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
            return $"{Ref} {Text}\n{HelpText}\nLogical Children: {childrenRef}";
        }
    }
}
