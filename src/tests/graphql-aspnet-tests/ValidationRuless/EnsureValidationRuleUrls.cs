// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ValidationRuless
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.ValidationRules.Interfaces;
    using NUnit.Framework;

    [TestFixture]
    public class EnsureValidationRuleUrls
    {
        [Test]
        public void EnsureAllUrlsUniquePerRule()
        {
            // for all links to specification rules ensure that
            // each rule number has a valid link and that it is
            // unique
            var ruleTypes = typeof(GraphQLProviders)
                .Assembly
                .GetTypes()
                .Where(Validation.IsCastable<IValidationRule>)
                .Where(x => x != typeof(IValidationRule) &&
                           !x.IsAbstract && x.IsClass);

            var builder = new StringBuilder();

            var urlsByRule = new Dictionary<string, HashSet<string>>();
            var rulesByUrl = new Dictionary<string, HashSet<string>>();
            foreach (var type in ruleTypes)
            {
                var obj = InstanceFactory.CreateInstance(type) as IValidationRule;

                var ruleNumber = obj.RuleNumber;
                var url = obj.ReferenceUrl;

                if (!urlsByRule.ContainsKey(ruleNumber))
                    urlsByRule.Add(ruleNumber, new HashSet<string>());

                var uri = new Uri(url);
                Assert.IsTrue(uri.IsWellFormedOriginalString());

                if (!url.Contains("#"))
                {
                    builder.AppendLine($"Url for Rule {ruleNumber} ({type.Name}) does not contain an section anchor.");
                    continue;
                }

                urlsByRule[ruleNumber].Add(url);

                if (!rulesByUrl.ContainsKey(url))
                    rulesByUrl.Add(url, new HashSet<string>());

                rulesByUrl[url].Add(ruleNumber);
            }

            var multipleUrls = urlsByRule.Where(x => x.Value.Count > 1);
            if (multipleUrls.Any())
            {
                var ruleList = string.Join(", ", multipleUrls.Select(x => x.Key));
                builder.AppendLine($"Rules {ruleList} have multiple urls defined.");
            }

            var multipleRules = rulesByUrl.Where(x => x.Value.Count > 1);
            if (multipleRules.Any())
            {
                foreach (var kvp in multipleRules)
                {
                    // nested rules are allowed to double up
                    // for instance 5.2.3.1.1 can have the same
                    // url as 5.2.3.1 because not all sub rules are
                    // defined as their own sections
                    var list = kvp.Value
                        .OrderBy(x => x)
                        .ToList();

                    var unnestedRules = new HashSet<string>();
                    for (var i = 1; i < list.Count; i++)
                    {
                        var prev = list[i - 1];
                        var cur = list[i];

                        if (!cur.StartsWith(prev))
                        {
                            unnestedRules.Add(prev);
                            unnestedRules.Add(cur);
                        }
                    }

                    if (unnestedRules.Count > 0)
                    {
                        var ruleList = string.Join(", ", unnestedRules);
                        var msg = $"Rules {ruleList} must have unique urls. (url: {kvp.Key})";
                        builder.AppendLine(msg);
                    }
                }

                var str = builder.ToString();
                if (!string.IsNullOrEmpty(str))
                    Assert.Fail(str);
            }
        }
    }
}