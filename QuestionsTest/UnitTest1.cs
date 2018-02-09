using System;
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
            questions.Add(new Question() { Ref = "1A015", UserResponse = "I work part time" });
            questions.Add(new Question() { Ref = "1A016", UserResponse = "I work full time" });
            questions.Add(new Question() { Ref = "1A017", UserResponse = "I work full or part time" });
            string line = "Mandatory and if response to question 1A015 is \"I work part time\" and 1A016 is \"I work full time\" or 1A017 is \"I do not work\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions.ToString());
            Assert.IsTrue(expressions.Evaluate());
        }

        [TestMethod]
        public void TestMethod2()
        {
            List<Question> questions = new List<Question>();
            questions.Add(new Question() { Ref = "1A015", UserResponse = "I work part time" });
            questions.Add(new Question() { Ref = "1A016", UserResponse = "I do work full time" });
            questions.Add(new Question() { Ref = "1A017", UserResponse = "I work full or part time" });
            string line = "Mandatory and if response to question 1A015 is \"I work part time\" and 1A016 is \"I work full time\" or 1A017 is \"I do not work\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions);
            Assert.IsFalse(expressions.Evaluate());
        }

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
            string line = "Mandatory if response to question 1A001 is \"Contents only\" or 1A001 is \"Buildings & Contents\"";
            var expressions = Reader.GetExpressionsForrPresentation(questions, line);
            Console.WriteLine(expressions);
            Assert.IsTrue(expressions.Evaluate());
            Console.WriteLine(Reader.GetResponseJSON(questions));
        }


    }
}
