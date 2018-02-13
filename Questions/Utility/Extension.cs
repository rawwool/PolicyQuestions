using Questions.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Questions.Utility
{
    public static class Extension
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        /*
        contents.jewelleryAndWatches.value
        contents.jewelleryAndWatches.valuableItems[]
        contents.jewelleryAndWatches.hasSafe
        contents.jewelleryAndWatches
        contents.jewelleryAndWatches.valuableItems.type
        contents.jewelleryAndWatches.valuableItems.desc
        contents2.jewelleryAndWatches.hasSafe
        contents2.jewelleryAndWatches
        contents2.jewelleryAndWatches.valuableItems.type
        contents.jewelleryAndWatches.valuableItems.desc
        contents.jewelleryAndWatches.valuableItems.yearsOwned
        contents.jewelleryAndWatches.valuableItems.value
        contents.artAndCollections[]
        contents.artAndCollections.category
        contents.artAndCollections.value
        contents.artAndCollections.valuableItems[]
        contents.artAndCollections.valuableItems.desc
        contents.artAndCollections.valuableItems.value
        contents.artAndCollections.valuableItems.valuationYear
        contents.otherValuableItems[]
        contents.otherValuableItems.desc
        contents.otherValuableItems.value
        contents.totalValue

        will be ordered as

        contents.jewelleryAndWatches
        contents.jewelleryAndWatches.value
        contents.jewelleryAndWatches.hasSafe
        contents.jewelleryAndWatches.valuableItems[]
        contents.jewelleryAndWatches.valuableItems.type
        contents.jewelleryAndWatches.valuableItems.desc
        contents.jewelleryAndWatches.valuableItems.yearsOwned
        contents.jewelleryAndWatches.valuableItems.value
        contents.artAndCollections[]
        contents.artAndCollections.category
        contents.artAndCollections.value
        contents.artAndCollections.valuableItems[]
        contents.artAndCollections.valuableItems.desc
        contents.artAndCollections.valuableItems.value
        contents.artAndCollections.valuableItems.valuationYear
        contents.otherValuableItems[]
        contents.otherValuableItems.desc
        contents.otherValuableItems.value
        contents.totalValue
        contents2.jewelleryAndWatches.hasSafe
        contents2.jewelleryAndWatches
        contents2.jewelleryAndWatches.valuableItems.type
        */

        /// <summary>
        /// Orders the questions based on the hierarchy of Question.APIRequestField as per the example above.
        /// 
        /// </summary>
        /// <param name="questions"></param>
        /// <returns></returns>
        public static IEnumerable<Question> OrderByHierarchy(this IEnumerable<Question> questions)
        {
            return QuestionNode.GetOrderedQuestionTree(questions);
        }


        public class QuestionNode
        {
            public Question Question { get; set; }
            private List<QuestionNode> Children { get; }
            public QuestionNode Parent { get; set; }

            public QuestionNode(Question question)
            {
                Children = new List<QuestionNode>();
                Question = question;
            }

            public void AddChild(QuestionNode node)
            {
                Children.Add(node);
                node.Parent = this;
            }

            public override string ToString()
            {
                return Question.APIRequestField;
            }

            /// <summary>
            /*
            contents.jewelleryAndWatches.value
            contents.jewelleryAndWatches.valuableItems[]
            contents.jewelleryAndWatches.hasSafe
            contents.jewelleryAndWatches.valuableItems.type
            contents.jewelleryAndWatches.valuableItems.desc
            contents2.jewelleryAndWatches.hasSafe
            contents2.jewelleryAndWatches
            contents2.jewelleryAndWatches.valuableItems.type
            contents.jewelleryAndWatches.valuableItems.desc
            contents.jewelleryAndWatches.valuableItems.yearsOwned
            contents.jewelleryAndWatches.valuableItems.value
            contents.artAndCollections[]
            contents.artAndCollections.category
            contents.artAndCollections.value
            contents.artAndCollections.valuableItems[]
            contents.artAndCollections.valuableItems.desc
            contents.artAndCollections.valuableItems.value
            contents.artAndCollections.valuableItems.valuationYear
            contents.otherValuableItems[]
            contents.otherValuableItems.desc
            contents.otherValuableItems.value
            contents.totalValue

            returns:

            contents.jewelleryAndWatches
                contents.jewelleryAndWatches.value
                contents.jewelleryAndWatches.hasSafe
                contents.jewelleryAndWatches.valuableItems[]
                    contents.jewelleryAndWatches.valuableItems.type
                    contents.jewelleryAndWatches.valuableItems.desc
                    contents.jewelleryAndWatches.valuableItems.yearsOwned
                    contents.jewelleryAndWatches.valuableItems.value
            contents.artAndCollections[]
                contents.artAndCollections.category
                contents.artAndCollections.value
                contents.artAndCollections.valuableItems[]
                    contents.artAndCollections.valuableItems.desc
                    contents.artAndCollections.valuableItems.value
                    contents.artAndCollections.valuableItems.valuationYear
            contents.otherValuableItems[]
                contents.otherValuableItems.desc
                contents.otherValuableItems.value
            contents.totalValue
            contents2.jewelleryAndWatches.hasSafe
                contents2.jewelleryAndWatches
                contents2.jewelleryAndWatches.valuableItems.type


            */
            /// </summary>
            /// <param name="fields"></param>
            /// <returns></returns>
            public static IEnumerable<Question> GetOrderedQuestionTree(IEnumerable<Question> questions)
            {
                Dictionary<string, QuestionNode> dict = new Dictionary<string, QuestionNode>();
                foreach (var question in questions)
                {
                    var key = question.APIRequestField.TrimEnd('[', ']');
                    if (!dict.ContainsKey(key))
                        dict.Add(key, new QuestionNode(question));
                    else
                        throw new InvalidOperationException("Duplicate API Request Field" + question.Ref);
                }
                List<string> keysAddedtoTree = new List<string>();
                foreach (var question in questions)
                {
                    var key = question.APIRequestField.TrimEnd('[', ']');
                    var splits = key.Split('.');
                    var parentKeySplits = splits.Take(splits.Count() - 1);
                    if (parentKeySplits.Count() > 0)
                    {
                        var parentKey = parentKeySplits.Aggregate((a, b) => $"{a}.{b}");
                        if (dict.ContainsKey(parentKey) && dict.ContainsKey(key))
                        {
                            dict[parentKey].AddChild(dict[key]);
                            keysAddedtoTree.Add(key);
                        }
                    }
                }

                keysAddedtoTree.ForEach(s => dict.Remove(s));
                var tree = dict.Values.Select(s => s);

                //Now walk the tree and flatten it.
                List<Question> list = new List<Question>();
                tree.ToList().ForEach(s =>
                {
                    RecursivelyAdd(list, s);
                });

                return list;
            }


            private static void RecursivelyAdd(List<Question> list, QuestionNode node)
            {
                list.Add(node.Question);
                node.Children.ForEach(s =>
                {
                    RecursivelyAdd(list, s);
                });
            }
        }

        /// <summary>
        /// contents.artAndCollections.valuableItems.valuationYear 
        /// returns 
        /// contents
        /// contents.artAndCollections
        /// contents.artAndCollections.valuableItems
        /// contents.artAndCollections.valuableItems.valuationYear
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetKeys(this string value)
        {
            var splits = value.Split('.');
            string key = string.Empty;
            List<string> result = new List<string>();
            //foreach(var s in splits)
            for (int i = 0; i < splits.Count(); i++)
            {
                if (key.Length == 0)
                    key = splits[i];
                else
                    key = key + '.' + splits[i];
                result.Add(key);
            }

            return result;
        }

        public static List<Tuple<string, string>> GetEnumNameDescriptionAttribueValuePairs<T>() where T : struct
        {
            return Enum.GetNames(typeof(T))
                .Select(s =>
                {
                    Type type = typeof(T);
                    var memInfo = type.GetMember(s);
                    var attributes = memInfo[0].GetCustomAttributes(typeof(Attribute), false);
                    return new Tuple<string, string>(s, (attributes.Length > 0) ? ((DescriptionAttribute)attributes[0]).Description : "");

                })
                .ToList();
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", " ", RegexOptions.Compiled);
        }
    }
}
