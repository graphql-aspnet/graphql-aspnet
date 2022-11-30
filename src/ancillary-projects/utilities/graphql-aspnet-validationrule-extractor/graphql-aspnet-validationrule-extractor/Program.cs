namespace GraphQL.AspNet.Utility.ValidationRuleExtractor
{
    using System;
    using System.IO;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.ValidationRules.Interfaces;
    using GraphQL.AspNet.Common.Extensions;
    using System.Collections.Generic;

    internal class Program
    {
        private static void Main(string[] args)
        {
            // this utility scans the graphql aspnet libraries
            // extracting any IValidationRule classes
            // serialzing out the rule number, the URL it points to
            // and hte class implementing it to a CSV file
            // for easy manipulation
            //
            // especially useful when the spec changes
            // and urls need to be validated for accuracy

            var ruleTypes = typeof(GraphQLProviders)
                .Assembly
                .GetTypes()
                .Where(Validation.IsCastable<IValidationRule>)
                .Where(x => x != typeof(IValidationRule) &&
                           !x.IsAbstract && x.IsClass);

            // hack our way up to the solution directory
            var fi = new FileInfo(typeof(Program).Assembly.Location);
            var dir = fi.Directory;
            while(dir != null)
            {
                if (dir.GetFiles("*.sln").Any())
                    break;

                dir = dir.Parent;
            }

            Validation.ThrowIfNull(dir, "solution directory");

            var lines = new List<string>();
            var rules = new HashSet<string>();
            foreach (var type in ruleTypes)
            {
                var obj = InstanceFactory.CreateInstance(type) as IValidationRule;

                var ruleNumber = obj.RuleNumber;
                var url = obj.ReferenceUrl;

                var i = 1;
                var rulePrint = ruleNumber;
                while (rules.Contains(rulePrint))
                    rulePrint = $"{ruleNumber}-{i++}";
                rules.Add(rulePrint);

                Console.WriteLine("Exactracting Rule: {0}", rulePrint);
                lines.Add(string.Format("\"{0}\",{1},{2}",
                    ruleNumber,
                    type.FriendlyName(),
                    url));
            }

            lines = lines.OrderBy(x => x).ToList();
            var outputFile = Path.Combine(dir.FullName, "graphql-aspnet-rulesList.csv");
            using var writer = new StreamWriter(outputFile, false);

            foreach(var line in lines)
                writer.WriteLine(line);

            writer.Close();

            Console.WriteLine("Rule List Generated at: {1}{0}",
                outputFile,
                Environment.NewLine);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}