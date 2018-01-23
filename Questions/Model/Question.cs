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
        public List<Question> Children { get; set; }
        public string Ref { get; set; }
        public string Text { get; set; }

        public enumType Type { get; set; }

        public enumDataCaptureType DataCaptureType { get; set; }

        public List<string> ResponseChoices { get; set; }

        public string ParentResponseForInvokingThisChildQuestion { get; set; }

        public string DefaultResponse { get; set; }

        public string UserResponse { get; set; }
        public string HelpText { get; set; }

        public Question()
        {
            Children = new List<Question>();
        }

        internal bool InvokeThisQuestion()
        {
            if (Parent == null) return true;
            if (UserResponse == null && ParentResponseForInvokingThisChildQuestion == null) return true;
            return this.UserResponse == ParentResponseForInvokingThisChildQuestion;
        }

        public override string ToString()
        {
            return $"{Ref} {Text}";
        }
    }
}
