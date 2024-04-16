//using System;
//using System.Collections.Generic;
//using System.Linq.Dynamic.Core;
//using System.Linq.Expressions;

//namespace CRM_Sample.Common
//{
//    public class RulesEngine
//    {
//        static List<Rule> GetRulesFromDatabase()
//        {
//            // In a real application, this method would fetch rules from your database
//            // Here's a simplified example with hardcoded rules:
//            return new List<Rule>
//        {
//            new Rule { Id = 1, Name = "Contains 'John Doe'", Condition = "Description.Contains(\"John Doe\")" },
//            new Rule { Id = 2, Name = "Amount > 1000", Condition = "Amount > 1000.0" }
//            // Additional rules can be added dynamically by users
//        };
//        }

//        // Evaluate a rule condition dynamically
//        static bool EvaluateRule(string condition, Transaction transaction)
//        {
//            try
//            {

//                var e1 = DynamicExpressionParser.ParseLambda<Transaction, bool>(new ParsingConfig(), true, condition);
//                var parameters = Expression.Parameter(typeof(Transaction), "transactionParam");


//                var customers = context.Customers.ToList().AsQueryable().Where("@0(it) and @1(it)", e1, e2); return lambda.Compile().Invoke();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error evaluating rule: {ex.Message}");
//                return false;
//            }
//        }
//    }

//    class Transaction
//    {
//        public string Description { get; }
//        public double Amount { get; }

//        public Transaction(string description, double amount)
//        {
//            Description = description;
//            Amount = amount;
//        }
//    }

//    class Rule
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//        public string Condition { get; set; }
//    }
//}

