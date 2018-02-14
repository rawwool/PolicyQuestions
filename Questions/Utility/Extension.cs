﻿using Newtonsoft.Json.Linq;
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
            public bool IsDummy { get; set; }
            public Question Question { get; set; }
            private List<QuestionNode> Children { get; }
            private List<List<QuestionNode>> ArrayOfChildren { get; }
            public QuestionNode Parent { get; set; }
            public string NodeName { get; set; }
            public string NameSpace { get; set; }
            public string ParentNameSpace {  get
                {
                    if (string.IsNullOrEmpty(NameSpace) || NameSpace.Split('.').Count() == 1)
                    {
                        return null;
                    }
                    else
                    {
                        return NameSpace.Split('.').Take(NameSpace.Split('.').Count() - 1).Aggregate((a, b) => $"{a}.{b}");
                    }
                } }
            public QuestionNode()
            {
                IsDummy = true;
                ArrayOfChildren = new List<List<QuestionNode>>();
                Children = new List<QuestionNode>();
            }

            public QuestionNode(Question question)
            {
                IsDummy = false;
                //if (question.APIRequestField.EndsWith("[]"))
                {
                    ArrayOfChildren = new List<List<QuestionNode>>();
                }
                //else
                {
                    Children = new List<QuestionNode>();
                }
                
                Question = question;
                NodeName = question.APIRequestField.Split('.').Last().TrimEnd('[', ']');
            }

            public void AddChild(QuestionNode node)
            {
                Children.Add(node);
                node.Parent = this;
            }

            public void AddToArrayOfChildren(QuestionNode node)
            {
                if (ArrayOfChildren != null)
                {
                    var list = ArrayOfChildren.FirstOrDefault(s => s.FirstOrDefault(d => d.Question.APIRequestField == node.Question.APIRequestField) == null);
                    if (list == null)
                    {
                        list = new List<QuestionNode>();
                        ArrayOfChildren.Add(list);
                    }
                    list.Add(node);
                }
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


            public static string GetJSON(IEnumerable<Question> questions)
            {
                //INCOMPLETE
                var tree = GetQuestionTree(questions);
                dynamic response = new JObject();
                //Iterate the top level list of QuestionNodes
                foreach (QuestionNode node in tree)
                {
                    // First make sure we have the right JObject to add the question node to
                    //If the node's APIRequestID has a hierarchy, we have to make sure all the parent nodes exist as JProperty
                    Process(response as JObject, node);   
                }

                return response.ToString();
            }

            private static void Process(JObject response, QuestionNode node)
            {
                var parent = EnsureAllParentNodesExist(response, node);
                if (!node.IsDummy) Add(parent, node);
                foreach(var child in node.Children)
                {
                    Process(response, child);
                }
                foreach(var child in node.ArrayOfChildren)
                {
                    child.ForEach(s => Process(response, s));
                }
            }
            private static JContainer EnsureAllParentNodesExist(JObject response, QuestionNode node)
            {
                //var splits = node.Question.APIRequestField.Split('.');
                //var nodeNames = splits.Take(splits.Count() - 1);
                IEnumerable<string> nodeNames = null;
                if (node.IsDummy)
                {
                    nodeNames = node.NameSpace.Split('.');
                }
                else
                {
                    var splits = node.Question.APIRequestField.Split('.');
                    nodeNames = splits.Take(splits.Count() - 1);
                }
                if (nodeNames.Count() < 2) return response;
                JContainer container = response;
                foreach(var nodeName in nodeNames)
                {
                    JContainer childContainer = GetExistingChildContainerByName(container, nodeName.TrimEnd('[', ']'));
                    if (childContainer == null)
                    {
                        if (nodeName.EndsWith("[]"))
                        {
                            var newContainer = new JArray();
                            if (container is JObject)
                                (container as JObject).Add(nodeName.TrimEnd('[', ']'), newContainer);
                            else if (container is JArray)
                                (container as JArray).Add(newContainer);
                            container = newContainer;
                        }
                        else
                        {
                            var newContainer = new JObject();

                            if (container is JObject)
                                (container as JObject).Add(nodeName.TrimEnd('[', ']'), newContainer);
                            else if (container is JArray)
                                (container as JArray).Add(newContainer);
                            container = newContainer;
                        }
                    }
                    else{
                        container = childContainer;
                    }
                }
                return container;
            }

            private static void Add(JContainer container, QuestionNode node)
            {
                bool hasChildren = false;
                if (node.Children != null && node.Children.Count() > 0)
                {
                    hasChildren = true;
                    AddToContainer(container, node);
                }
                if (node.ArrayOfChildren != null && node.ArrayOfChildren.SelectMany(s=>s).FirstOrDefault() != null)
                {
                    hasChildren = true;
                    //AddToJArray(response, node);
                }
                if (hasChildren ==false)
                {
                    if (container is JObject)
                    {
                        (container as JObject).Add(node.NodeName, node.Question.UserResponse);
                    }
                }
            }

            private static void AddToContainer(JContainer container, QuestionNode node)
            {
                if (container is JObject)
                {
                    //Does a JObject named node.NodeName exist in response?
                    var childJObject = GetChildJObjectByName(container as JObject, node.NodeName);
                    if (childJObject == null)
                    {
                        childJObject = new JObject();
                        (container as JObject).Add(node.NodeName, childJObject);
                    }
                    foreach (var childNode in node.Children)
                    {
                        Add(childJObject, childNode);
                    }
                }
            }

            public static IEnumerable<Question> GetOrderedQuestionTree(IEnumerable<Question> questions)
            {
                var tree = GetQuestionTree(questions);
                List<Question> list = new List<Question>();
                tree.ToList().ForEach(s =>
                {
                    RecursivelyAdd(list, s);
                });

                return list;
            }
            
            /// </summary>
            /// <param name="fields"></param>
            /// <returns></returns>
            public static IEnumerable<QuestionNode> GetQuestionTree(IEnumerable<Question> questions)
            {
                var temp = questions.Select(s =>
                {
                    var splits = s.APIRequestField.Split('.');
                    var groupName = s.APIRequestField.TrimEnd('[', ']');
                    bool isArrayHeader = false;
                    if (splits.Count() > 1)
                    {
                        if (!s.APIRequestField.EndsWith("[]"))
                        {
                            groupName = splits.Take(splits.Count() - 1).Aggregate((a, b) => $"{a}.{b}");
                            isArrayHeader = false;
                        }
                        else
                        {
                            isArrayHeader = true;
                        }
                    }
                    return new { Q = s, GroupName = groupName, IsArrayHeader = isArrayHeader };
                })
                .GroupBy(s => s.GroupName)
                .Select(t =>
                {
                    bool isArray = t.FirstOrDefault(f => f.IsArrayHeader) != null;
                    QuestionNode node = new QuestionNode();
                    node.NodeName = t.Key.Split('.').Last();
                    if (isArray) node = new QuestionNode(t.FirstOrDefault(f => f.IsArrayHeader).Q);
                    node.NameSpace = t.Key;
                    foreach (var q in t)
                    {
                        if (q.IsArrayHeader) continue;

                        if (isArray)
                        {
                            node.AddToArrayOfChildren(new QuestionNode(q.Q));
                        }
                        else
                        {
                            node.AddChild(new QuestionNode(q.Q));
                        }

                    }

                    return node;
                }).ToList();

                List<QuestionNode> nodesToRemove = new List<QuestionNode>();
                foreach(var questionNode in temp)
                {
                    if (questionNode.ParentNameSpace != null)
                    {
                        var parent = temp.FirstOrDefault(s => s.NameSpace == questionNode.ParentNameSpace);
                        if (parent != null)
                        {
                            if (parent.IsDummy) parent.Children.Add(questionNode); else parent.AddToArrayOfChildren(questionNode);
                            nodesToRemove.Add(questionNode);
                        }
                    }
                }
                nodesToRemove.ForEach(s => temp.Remove(s));

                return temp;
                /*
                //new { Q = s, Group = s.APIRequestField.EndsWith("[]")? s.APIRequestField.Split('.').Take()})
                Dictionary<string, QuestionNode> dict = new Dictionary<string, QuestionNode>();
                foreach (var question in questions)
                {
                    var key = question.APIRequestField;//.TrimEnd('[', ']');
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
                        else if (dict.ContainsKey(parentKey+"[]") && dict.ContainsKey(key))
                        {
                            dict[parentKey + "[]"].AddToArrayOfChildren(dict[key]);
                            keysAddedtoTree.Add(key);
                        }
                    }
                }

                keysAddedtoTree.ForEach(s => dict.Remove(s));
                var tree = dict.Values.Select(s => s);
                return tree;
                */
                /*
                dynamic response = new JObject();
                //response.ProductName = "Elbow Grease";
                //response.Enabled = true;
                //product.Price = 4.90m;
                //product.StockCount = 9000;
                //product.StockValue = 44100;
                //product.Tags = new JArray("Real", "OnSale");

                //Console.WriteLine(product.ToString());
                var jobject = response;
                foreach(var item in tree)
                {
                    var splits = item.Question.APIRequestField.Split('.');
                    //foreach(var jsonNode in splits)
                    for (int i = 0; i < splits.Count(); i++)
                    {
                        var key = splits[i].TrimEnd('[', ']');
                        if (DoesKeyExist(jobject, key) == false)
                        {
                            if (i == splits.Count() - 1 && splits[i].EndsWith("[]"))
                            {
                                JArray array = new JArray();
                                jobject.Add(key, array);
                            }
                            else
                            {
                                JObject newjo = new JObject();
                                jobject.Add(key, newjo);
                                jobject = newjo;
                            }
                        }

                    }
                }
                */

                //Now walk the tree and flatten it.
                
            }

            private static JObject GetExistingOrNewJObject(JArray array, string name)
            {
                JObject jo = array.Children<JObject>()
                    //.FirstOrDefault(o => o["text"] != null && o["text"].ToString() == "Two");
                    .FirstOrDefault(o => o[name] == null);
                if (jo == null)
                {
                    jo = new JObject();
                    array.Add(jo);
                    return jo;
                }
                return jo;
            }

            private static JObject GetChildJObjectByName(JObject jobject, string name)
            {
                var jproperty = jobject.Descendants()
                                .Where(t => t.Type == JTokenType.Property)
                                .FirstOrDefault(s => ((JProperty)s).Name == name);
                if (jproperty != null)
                    return ((JProperty)jproperty).Value as JObject;// jproperty.Value<JObject>(name);
                else
                    return null;
            }

            private static JContainer GetExistingChildContainerByName(JContainer container, string key)
            {
                if (container is JObject)
                {
                    //exists = (container as JObject).Descendants()
                    //            .Where(t => t.Type == JTokenType.Property /*&& ((JProperty)t).Name == "id"*/)
                    //            .Select(p => ((JProperty)p).Name)
                    //            .FirstOrDefault(s => s == key) != null;
                    return GetChildJObjectByName(container as JObject, key);
                }
                else if (container is JArray)
                {
                    JObject jo = (container as JArray).Children<JObject>()
                    //.FirstOrDefault(o => o["text"] != null && o["text"].ToString() == "Two");
                    .FirstOrDefault(o => o[key] == null);
                    //exists = jo != null;
                    return jo;
                }
                return null;
            }

            private static void RecursivelyAdd(List<Question> list, QuestionNode node)
            {
                list.Add(node.Question);
                if (node.Children !=null)
                node.Children.ForEach(s =>
                {
                    RecursivelyAdd(list, s);
                });
                if (node.ArrayOfChildren != null)
                {
                    foreach(var array in node.ArrayOfChildren)
                    {
                        foreach (var q in array)
                            RecursivelyAdd(list, q);
                    }
                }
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
