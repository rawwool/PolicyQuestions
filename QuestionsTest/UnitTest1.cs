﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Questions.Model;
using Questions.Utility;

namespace QuestionsTest
{
    [TestClass]
    public class UnitTest1
    {
        /*
        34534 34534 gandor 345645654A is "test" or 345645654A is"best" and terter is "fdkjghfd" or sfsd is not "rtre"
        34534 34534 345645654A is not "test" and 345645654A is"best" and terter is "fdkjghfd" or sfsd is not "rtre"
        Mandatory and if response to question 1A015 is "I work full or part time" and 1A015 is "I work full or part time" or 1A015 is "I work full or part time"
        werwe or 1A015 is "I work full or part time"
        fand 1A015 is "I work full or part time"
        f and 1A015 is "I work full or part time" 
        */
        [TestMethod]
        public void TestMethod1()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A015", UserResponse = new Question.ResponseChoice( "I work part time") });
            questions.Add(new Question() { Ref = "1A016", UserResponse = new Question.ResponseChoice("I work full time") });
            questions.Add(new Question() { Ref = "1A017", UserResponse = new Question.ResponseChoice("I work full or part time") });
            string line = "Mandatory and if response to question 1A015 is \"I work part time\" and 1A016 is \"I work full time\" or 1A017 is \"I do not work\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions.ToString());
            Assert.IsTrue(expressions.Evaluate());
        }

        [TestMethod]
        public void TestMethod2()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A015", UserResponse = new Question.ResponseChoice("I work part time") });
            questions.Add(new Question() { Ref = "1A016", UserResponse = new Question.ResponseChoice("I do work full time") });
            questions.Add(new Question() { Ref = "1A017", UserResponse = new Question.ResponseChoice("I work full or part time") });
            string line = "Mandatory and if response to question 1A015 is \"I work part time\" and 1A016 is \"I work full time\" or 1A017 is \"I do not work\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions);
            Assert.IsFalse(expressions.Evaluate());
        }

        /*
        [TestMethod]
        public void TestMethod3()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A015", UserResponse = "I work part time" });
            questions.Add(new Question() { Ref = "1A016", UserResponse = "I do not work" });
            questions.Add(new Question() { Ref = "1A017", UserResponse = "I work full or part time" });
            string line = "Mandatory and if response to question 1A015 is \"I work part time\" and 1A016 is not \"I work full time\" or 1A017 is \"I do not work\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions);
            Assert.IsTrue(expressions.Evaluate());
        }

        [TestMethod]
        public void TestMethod4()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A015", UserResponse = "I sometimes work part time" });
            questions.Add(new Question() { Ref = "1A016", UserResponse = "I do not work" });
            questions.Add(new Question() { Ref = "1A017", UserResponse = "I work full or part time" });
            string line = "Mandatory and if response to question 1A015 is \"I work part time\" and 1A016 is not \"I work full time\" or 1A017 is \"I do not work\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions);
            Assert.IsFalse(expressions.Evaluate());
        }

        //Mandatory if response to question 1A001 is "Contents only" or "Buildings & Contents"
        [TestMethod]
        public void TestMethod5()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A001", UserResponse = "Contents only" });
            questions.Add(new Question() { Ref = "1A016", UserResponse = "I do not work" });
            questions.Add(new Question() { Ref = "1A017", UserResponse = "I work full or part time" });
            string line = "Mandatory if response to question 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions);
            Assert.IsTrue(expressions.Evaluate());
        }

        [TestMethod]
        public void TestMethod6()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A001", UserResponse = "Contents only", APIRequestField = "PolicyType" });
            questions.Add(new Question() { Ref = "1A016", UserResponse = "I do not work", APIRequestField = "WorkStatus" });
            questions.Add(new Question() { Ref = "1A017", UserResponse = "Architect", APIRequestField = "Work.Type" });
            questions.Add(new Question() { Ref = "1A018", UserResponse = "5", APIRequestField = "Work.Duration" });
            questions.Add(new Question() { Ref = "1A018", UserResponse = "Year", APIRequestField = "Work.Durations.Unit" });
            questions.Add(new Question() { Ref = "1A019", UserResponse = "2", APIRequestField = "previousClaims[]" });
            questions.Add(new Question() { Ref = "1A019a", UserResponse = "Year", APIRequestField = "previousClaims.Year" });
            questions.Add(new Question() { Ref = "1A019b", UserResponse = "Value", APIRequestField = "previousClaims.Value" });
            string line = "Mandatory if response to question 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions);
            Assert.IsTrue(expressions.Evaluate());
            Console.WriteLine(Questions.Utility.Questions.GetResponseJSON(questions));
        }

        [TestMethod]
        public void TestMethod7()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A019", UserResponse = "1", APIRequestField = "previousClaims[]" });
            questions.Add(new Question() { Ref = "1A019a", UserResponse = "1999", APIRequestField = "previousClaims.Year" });
            questions.Add(new Question() { Ref = "1A019b", UserResponse = "24,500", APIRequestField = "previousClaims.Value" });
            //string line = "Mandatory if response to question 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"";
            //var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            //Console.WriteLine(expressions);
            //Assert.IsTrue(expressions.Evaluate());
            Console.WriteLine(Questions.Utility.Questions.GetResponseJSON(questions));
        }


        [TestMethod]
        public void TestMethod8()
        {
            /*
             {
  "contents": {
    "jewelleryAndWatches": {
      "valuableItems[]": "1999",
      "value": "1",
      "hasSafe": "24,500",
      "valuableItems": {
        "type": "1999",
        "desc": "24,500",
        "yearsOwned": "1",
        "value": "1999"
      }
    },
    "artAndCollections[]": "1",
    "artAndCollections": {
      "category": "1999",
      "value": "24,500",
      "valuableItems[]": "1",
      "valuableItems": {
        "desc": "1999",
        "value": "24,500",
        "valuationYear": "24,500"
      }
    },
    "otherValuableItems": "24,500",
    "totalValue": "24,500"
  },
  "previousClaims": {
    "Value": "24,500"
  }
}
             */
             /*
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A019", UserResponse = "100000", APIRequestField = "contents.jewelleryAndWatches.value" });
            questions.Add(new Question() { Ref = "1A019a", UserResponse = "Invisible", APIRequestField = "contents.jewelleryAndWatches.valuableItems[]" });
            questions.Add(new Question() { Ref = "1A019b", UserResponse = "Yes", APIRequestField = "contents.jewelleryAndWatches.hasSafe" });
            questions.Add(new Question() { Ref = "1A019c", UserResponse = "Invisible", APIRequestField = "contents.jewelleryAndWatches" });
            questions.Add(new Question() { Ref = "1A019d", UserResponse = "Jewels", APIRequestField = "contents.jewelleryAndWatches.valuableItems.type" });
            questions.Add(new Question() { Ref = "1A019e", UserResponse = "diamond", APIRequestField = "contents.jewelleryAndWatches.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019f", UserResponse = "21", APIRequestField = "contents.jewelleryAndWatches.valuableItems.yearsOwned" });
            questions.Add(new Question() { Ref = "1A019g", UserResponse = "20000", APIRequestField = "contents.jewelleryAndWatches.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019h", UserResponse = "24500", APIRequestField = "previousClaims.Value" });
            questions.Add(new Question() { Ref = "1A019i", UserResponse = "Invisible", APIRequestField = "contents.artAndCollections[]" });
            questions.Add(new Question() { Ref = "1A019j", UserResponse = "Paintings", APIRequestField = "contents.artAndCollections.category" });
            questions.Add(new Question() { Ref = "1A019k", UserResponse = "29500", APIRequestField = "contents.artAndCollections.value" });
            questions.Add(new Question() { Ref = "1A019l", UserResponse = "Invisible", APIRequestField = "contents.artAndCollections.valuableItems[]" });
            questions.Add(new Question() { Ref = "1A019m", UserResponse = "Picaso", APIRequestField = "contents.artAndCollections.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019n", UserResponse = "240500", APIRequestField = "contents.artAndCollections.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019o", UserResponse = "2015", APIRequestField = "contents.artAndCollections.valuableItems.valuationYear" });
            questions.Add(new Question() { Ref = "1A019p", UserResponse = "Invisible", APIRequestField = "contents.otherValuableItems" });
            questions.Add(new Question() { Ref = "1A019q", UserResponse = "Tesla", APIRequestField = "contents.otherValuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019r", UserResponse = "84500", APIRequestField = "contents.otherValuableItems.value" });
            questions.Add(new Question() { Ref = "1A019s", UserResponse = "624500", APIRequestField = "contents.totalValue" });
            //string line = "Mandatory if response to question 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"";
            //var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            //Console.WriteLine(expressions);
            //Assert.IsTrue(expressions.Evaluate());
            var result = Questions.Utility.Extension.QuestionNode.GetOrderedQuestionTree(questions);
            foreach (var res in result)
                Console.WriteLine(res.APIRequestField);

            var json = Questions.Utility.Questions.GetResponseJSON(questions);
            Console.WriteLine(json);
            //Assert.IsTrue(json == )
        }

        [TestMethod]
        public void TestMethod9()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A019", UserResponse = "100000", APIRequestField = "contents.jewelleryAndWatches.value" });
            questions.Add(new Question() { Ref = "1A019a", UserResponse = "Invisible", APIRequestField = "contents.jewelleryAndWatches.valuableItems[]" });
            questions.Add(new Question() { Ref = "1A019b", UserResponse = "Yes", APIRequestField = "contents.jewelleryAndWatches.hasSafe" });
            questions.Add(new Question() { Ref = "1A019c", UserResponse = "Invisible", APIRequestField = "contents.jewelleryAndWatches.Have" });
            questions.Add(new Question() { Ref = "1A019d", UserResponse = "Jewels", APIRequestField = "contents.jewelleryAndWatches.valuableItems.type" });
            questions.Add(new Question() { Ref = "1A019e", UserResponse = "diamond", APIRequestField = "contents.jewelleryAndWatches.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019f", UserResponse = "21", APIRequestField = "contents.jewelleryAndWatches.valuableItems.yearsOwned" });
            questions.Add(new Question() { Ref = "1A019g", UserResponse = "20000", APIRequestField = "contents.jewelleryAndWatches.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019d1", UserResponse = "Jewels", APIRequestField = "contents.jewelleryAndWatches.valuableItems.type" });
            questions.Add(new Question() { Ref = "1A019e1", UserResponse = "diamond", APIRequestField = "contents.jewelleryAndWatches.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019f1", UserResponse = "21", APIRequestField = "contents.jewelleryAndWatches.valuableItems.yearsOwned" });
            questions.Add(new Question() { Ref = "1A019g1", UserResponse = "20000", APIRequestField = "contents.jewelleryAndWatches.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019h", UserResponse = "1", APIRequestField = "previousClaims.Count" });
            questions.Add(new Question() { Ref = "1A019h", UserResponse = "24500", APIRequestField = "previousClaims.Value" });
            questions.Add(new Question() { Ref = "1A019i", UserResponse = "Invisible", APIRequestField = "contents.artAndCollections[]" });
            questions.Add(new Question() { Ref = "1A019j", UserResponse = "Paintings", APIRequestField = "contents.artAndCollections.category" });
            questions.Add(new Question() { Ref = "1A019k", UserResponse = "29500", APIRequestField = "contents.artAndCollections.value" });
            questions.Add(new Question() { Ref = "1A019l", UserResponse = "Invisible", APIRequestField = "contents.artAndCollections.valuableItems[]" });
            questions.Add(new Question() { Ref = "1A019m", UserResponse = "Picaso", APIRequestField = "contents.artAndCollections.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019n", UserResponse = "240500", APIRequestField = "contents.artAndCollections.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019o", UserResponse = "2015", APIRequestField = "contents.artAndCollections.valuableItems.valuationYear" });
            questions.Add(new Question() { Ref = "1A019m1", UserResponse = "Picaso", APIRequestField = "contents.artAndCollections.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019n1", UserResponse = "240500", APIRequestField = "contents.artAndCollections.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019o1", UserResponse = "2015", APIRequestField = "contents.artAndCollections.valuableItems.valuationYear" });
            questions.Add(new Question() { Ref = "1A019p", UserResponse = "Invisible", APIRequestField = "contents.otherValuableItems[]" });
            questions.Add(new Question() { Ref = "1A019q", UserResponse = "Tesla", APIRequestField = "contents.otherValuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019r", UserResponse = "84500", APIRequestField = "contents.otherValuableItems.value" });
            questions.Add(new Question() { Ref = "1A019s", UserResponse = "624500", APIRequestField = "contents.totalValue" });
            //string line = "Mandatory if response to question 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"";
            //var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            //Console.WriteLine(expressions);
            //Assert.IsTrue(expressions.Evaluate());
            //var result = Questions.Utility.Extension.QuestionNode.GetOrderedQuestionTree(questions);
            //foreach (var res in result)
            //    Console.WriteLine(res.APIRequestField);

            //var json = Questions.Utility.Questions.GetResponseJSON(questions);
            //Console.WriteLine(json);

            var json = Questions.Utility.Extension.QuestionNode.GetJSON(questions);
            Console.WriteLine(json);
            //Assert.IsTrue(json == )
        }


        [TestMethod]
        public void TestMethod10()
        {
            List<Question> questions = new List<Question>();
           
            questions.Add(new Question() { Ref = "1A019", UserResponse = "100000", APIRequestField = "contents.jewelleryAndWatches.value" });
            questions.Add(new Question() { Ref = "1A019a", UserResponse = "Invisible", APIRequestField = "contents.jewelleryAndWatches.valuableItems[]" });
            questions.Add(new Question() { Ref = "1A019b", UserResponse = "Yes", APIRequestField = "contents.jewelleryAndWatches.hasSafe" });
            questions.Add(new Question() { Ref = "1A019c", UserResponse = "Invisible", APIRequestField = "contents.jewelleryAndWatches.Have" });
            questions.Add(new Question() { Ref = "1A019d", UserResponse = "Jewels", APIRequestField = "contents.jewelleryAndWatches.valuableItems.type" });
            questions.Add(new Question() { Ref = "1A019e", UserResponse = "diamond", APIRequestField = "contents.jewelleryAndWatches.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019f", UserResponse = "21", APIRequestField = "contents.jewelleryAndWatches.valuableItems.yearsOwned" });
            questions.Add(new Question() { Ref = "1A019g", UserResponse = "20000", APIRequestField = "contents.jewelleryAndWatches.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019d1", UserResponse = "Jewels", APIRequestField = "contents.jewelleryAndWatches.valuableItems.type" });
            questions.Add(new Question() { Ref = "1A019e1", UserResponse = "diamond", APIRequestField = "contents.jewelleryAndWatches.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019f1", UserResponse = "21", APIRequestField = "contents.jewelleryAndWatches.valuableItems.yearsOwned" });
            questions.Add(new Question() { Ref = "1A019g1", UserResponse = "20000", APIRequestField = "contents.jewelleryAndWatches.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019h1", UserResponse = "Invisible", APIRequestField = "previousClaims[]" });
            questions.Add(new Question() { Ref = "1A019h", UserResponse = "1", APIRequestField = "previousClaims.Count" });
            questions.Add(new Question() { Ref = "1A019h", UserResponse = "24500", APIRequestField = "previousClaims.Value" });
            questions.Add(new Question() { Ref = "1A019i", UserResponse = "Invisible", APIRequestField = "contents.artAndCollections[]" });
            questions.Add(new Question() { Ref = "1A019j", UserResponse = "Paintings", APIRequestField = "contents.artAndCollections.category" });
            questions.Add(new Question() { Ref = "1A019k", UserResponse = "29500", APIRequestField = "contents.artAndCollections.value" });
            questions.Add(new Question() { Ref = "1A019l", UserResponse = "Invisible", APIRequestField = "contents.artAndCollections.valuableItems[]" });
            questions.Add(new Question() { Ref = "1A019m", UserResponse = "Invisible", APIRequestField = "contents.artAndCollections.valuableItems.desc2[]" });
            questions.Add(new Question() { Ref = "1A019m1", UserResponse = "Picaso", APIRequestField = "contents.artAndCollections.valuableItems.desc2.Artiste" });
            questions.Add(new Question() { Ref = "1A019m2", UserResponse = "1960", APIRequestField = "contents.artAndCollections.valuableItems.desc2.Year" });
            questions.Add(new Question() { Ref = "1A019n", UserResponse = "240500", APIRequestField = "contents.artAndCollections.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019o", UserResponse = "2015", APIRequestField = "contents.artAndCollections.valuableItems.valuationYear" });
            questions.Add(new Question() { Ref = "1A019m1", UserResponse = "Picaso2", APIRequestField = "contents.artAndCollections.valuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019n1", UserResponse = "240500", APIRequestField = "contents.artAndCollections.valuableItems.value" });
            questions.Add(new Question() { Ref = "1A019o1", UserResponse = "2015", APIRequestField = "contents.artAndCollections.valuableItems.valuationYear" });
           
            questions.Add(new Question() { Ref = "1A019p", UserResponse = "Invisible", APIRequestField = "contents.otherValuableItems[]" });
            questions.Add(new Question() { Ref = "1A019q", UserResponse = "Tesla", APIRequestField = "contents.otherValuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019r", UserResponse = "84500", APIRequestField = "contents.otherValuableItems.value" });
            questions.Add(new Question() { Ref = "1A019s", UserResponse = "624500", APIRequestField = "contents.totalValue" });
            questions.Add(new Question() { Ref = "1A019p", UserResponse = "Invisible", APIRequestField = "otherValuableItems[]" });
            questions.Add(new Question() { Ref = "1A019q", UserResponse = "Tesla", APIRequestField = "otherValuableItems.desc" });
            questions.Add(new Question() { Ref = "1A019r", UserResponse = "84500", APIRequestField = "otherValuableItems.value" });
            //questions.Add(new Question() { Ref = "1A019s", UserResponse = "624500", APIRequestField = "contents.totalValue" });


            questions.Add(new Question() { Ref = "1A019s", UserResponse = "Aayush", APIRequestField = "Name" });
            questions.Add(new Question() { Ref = "1A019s", UserResponse = "12", APIRequestField = "Age" });
            //string line = "Mandatory if response to question 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"";
            //var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            //Console.WriteLine(expressions);
            //Assert.IsTrue(expressions.Evaluate());
            var result = Questions.Utility.Extension.QuestionNode.GetOrderedQuestionTree(questions);
            foreach (var res in result)
                Console.WriteLine(res.APIRequestField);

            var test = Questions.Utility.Extension.QuestionNode.GetTokenTree(questions);

            Console.WriteLine(Questions.Utility.Extension.QuestionNode.GetJSON2(questions));
            //var json = Questions.Utility.Questions.GetResponseJSON(questions);
            //Console.WriteLine(json);

            //var json = Questions.Utility.Extension.QuestionNode.GetJSON(questions);
            //Console.WriteLine(json);
            //Assert.IsTrue(json == )
        }
        */
    }
}
