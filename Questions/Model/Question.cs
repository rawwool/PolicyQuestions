using Questions.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Model
{
    [Serializable]
    public class Question
    {
        public string Category { get; set; }

        public string Id { get; set; }
        public string ParentId { get; set; }

        //ParentQuestion, ThisQuestio'sResponse, IsthatPositive, Full Response
        //public Tuple<Question, string, bool, string> ConditionForPresentation { get; set; }

        public Expressions Expressions { get; set; }

        [NonSerialized]
        /// <summary>
        /// Represents the count of 'sets' of choldren in the Cildren collection
        /// For example, if the template defines 4 questions to be children in 
        /// a question of array type, each set of 4 questions will count as 1.
        /// The user interface can add or remove one set of question at a time.
        /// This count comes in consideration only for 'array' type of parent 
        /// questions (with APIField with [] at the end)
        /// </summary>
        public int ChildSetCount = 0;
        public List<Question> Children { get; set; }
        public List<string> LogicalChildren { get; set; }
        public string Ref { get; set; }
        public string Text { get; set; }

        public enumType Type { get; set; }

        public enumDataCaptureType DataCaptureType { get; set; }

        [Serializable]
        public class ResponseChoice
        {
            public string Display { get; set; }
            public string Value { get; set; }

            public ResponseChoice() { }
            public ResponseChoice(string value)
            {
                Display = value;
                Value = value;
            }
        }
        public List<ResponseChoice> ResponseChoices { get; set; }

        //public string ParentResponseForInvokingThisChildQuestion { get; set; }

        public string DefaultResponse { get; set; }

        public ResponseChoice UserResponse { get; set; }
        public string HelpText { get; set; }

        public string APIRequestField { get; set; }

        public bool HasArrayOfChildren {  get { return APIRequestField.TrimEnd().EndsWith("[]"); } }

        public string UIValdationMessage { get; set; }
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

        public string DisplayRule { get; internal set; }

        [NonSerialized]
        public Action<bool> ShowHide;

        public Question()
        {
            Id = Guid.NewGuid().ToString();
            Children = new List<Question>();
            LogicalChildren = new List<string>();
            UserResponse = new ResponseChoice();
            ChildSetCount = 0;
        }

        public bool InvokeThisQuestion()
        {
            return this.Expressions.Evaluate();
        }

        public override string ToString()
        {
            var children = this.LogicalChildren.Select(s => s);
            string childrenRef = "None";
            if (children != null && children.FirstOrDefault() != null)
            {
                childrenRef = children.Aggregate((a, b) => a + ", " + b);
            }
            return $"{Ref} {Text}\n{HelpText}\n{APIResource}\n{APIRequestField}\nLogical Children: {childrenRef}\nUI Validation: {UIValdationMessage}";
        }
    }
}
