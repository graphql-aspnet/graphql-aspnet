// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Execution.ExecutionDirectiveTestData
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class DirectiveTestController : GraphController
    {
        [QueryRoot]
        public TwoPropertyObject RetrieveObject()
        {
            return new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 2,
            };
        }

        [QueryRoot]
        public TwoPropertyObject RetrieveSingleObject(string id)
        {
            return new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 2,
            };
        }

        [SubscriptionRoot]
        public TwoPropertyObject OnChanged(TwoPropertyObject sourceData)
        {
            return new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 2,
            };
        }

        [MutationRoot]
        public TwoPropertyObject MutateObject()
        {
            return new TwoPropertyObject()
            {
                Property1 = "value1",
                Property2 = 2,
            };
        }

        [QueryRoot]
        public List<TwoPropertyObject> RetrieveObjects()
        {
            var list = new List<TwoPropertyObject>();
            for (var i = 0; i <= 2; i++)
            {
                list.Add(new TwoPropertyObject()
                {
                    Property1 = "value" + i,
                    Property2 = i,
                });
            }

            return list;
        }

        [TypeExtension(typeof(TwoPropertyObject), "property3")]
        public string Prop3Extension(TwoPropertyObject sourceData)
        {
            return sourceData.Property1 + " prop 3";
        }

        [BatchTypeExtension(typeof(TwoPropertyObject), "child", typeof(TwoPropertyObject))]
        public IGraphActionResult Prop4Extension(IEnumerable<TwoPropertyObject> sourceData)
        {
            var listOut = new List<TwoPropertyObject>();
            foreach (var item in sourceData)
            {
                listOut.Add(new TwoPropertyObject()
                {
                    Property1 = "child prop " + item.Property1,
                    Property2 = item.Property2,
                });
            }

            return this.StartBatch()
                .FromSource(sourceData, x => x.Property2)
                .WithResults(listOut, x => x.Property2)
                .Complete();
        }
    }
}