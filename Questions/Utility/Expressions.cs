using Questions.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Utility
{
    public class Expression
    {
        public Question Question { get; set; }
        public string ValueToCompareWith { get; set; }
        public string Operator { get; set; }
        public bool Positive { get; set; }
    }

    public class Expressions
    {
        List<Expression> _Expressions = new List<Expression>();
        List<List<Expression>> _GroupedExpression = null;
        public void Add(IEnumerable<Expression> expressions)
        {
            _Expressions.AddRange(expressions);

            List<List<Expression>> groupedList = new List<List<Expression>>();
            List<Expression> group = new List<Expression>();
            foreach (var exp in _Expressions)
            {
                var expString = $"{exp.Question.Ref}" + (exp.Positive ? " == " : " != ") + $"\"{exp.ValueToCompareWith}\"";
                if (exp.Operator == "and")
                {
                    group = new List<Expression>();
                    group.Add(exp);
                    groupedList.Add(group);
                }
                else//"or"
                {
                    group.Add(exp);
                }
            }
            _GroupedExpression = groupedList;
        }

        public IEnumerable<Question> Questions
        {
            get
            {
                return _Expressions.Select(s => s.Question).Distinct();
            }
        }

        public Expressions(IEnumerable<Expression> expressions)
        {
            Add(expressions);
        }

        public override string ToString()
        {
            if (_GroupedExpression == null || _GroupedExpression.FirstOrDefault() == null) return String.Empty;
            var result = _GroupedExpression.Select(s =>
                {
                    var resultGroup =  s.Select(a =>
                        {
                            string op = a.Positive ? "is" : "is not";
                            string value = $"Answer({a.Question.Ref}) {op} \"{a.ValueToCompareWith}\"";
                            return value;
                        });
                    return resultGroup.Aggregate((a,b)=>$"{a} or {b}");
                }
            );
            return result.Aggregate((a, b) => $"( {a} ) and ( {b} )"); ;
        }

        public bool Evaluate()
        {
            if (_GroupedExpression == null || _GroupedExpression.FirstOrDefault() == null) return true;
            
            bool groupResult = true;
            foreach (var list in _GroupedExpression)
            {
                bool expResult = false;
                foreach (var exp in list)
                {
                    //These are all 'or' so return as soon as one 
                    if (exp.Positive)
                    {
                        if (Fuzzy.AreSimilar(exp.Question.UserResponse, exp.ValueToCompareWith))
                        {
                            expResult = true;
                            continue;
                        }
                        else break;
                    }
                    else
                    {
                        if (!Fuzzy.AreSimilar(exp.Question.UserResponse, exp.ValueToCompareWith))
                        {
                            expResult = true;
                            continue;
                        }
                        else break;
                    }
                }
                groupResult = groupResult && expResult;

                if (groupResult == false) break;
            }

            return groupResult;
        }
    }
}