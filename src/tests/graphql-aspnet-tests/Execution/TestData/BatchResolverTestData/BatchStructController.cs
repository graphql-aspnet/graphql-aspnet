// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.BatchResolverTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("batch")]
    public class BatchStructController : GraphController
    {
        private readonly IBatchCounterService _counterService;

        public BatchStructController(IBatchCounterService counterService)
        {
            _counterService = Validation.ThrowIfNullOrReturn(counterService, nameof(counterService));
        }

        private void AddCounter(string name)
        {
            if (!_counterService.CallCount.ContainsKey(name))
                _counterService.CallCount.Add(name, 0);

            _counterService.CallCount[name] += 1;
        }

        [Query("fetchData", typeof(TwoPropertyStruct), TypeExpression = TypeExpressions.IsList)]
        public IGraphActionResult PrimaryDataFetch()
        {
            AddCounter(nameof(this.PrimaryDataFetch));

            var list = new List<TwoPropertyStruct>();
            list.Add(new TwoPropertyStruct()
            {
                Property1 = "object0",
                Property2 = 0,
            });
            list.Add(new TwoPropertyStruct()
            {
                Property1 = "object1",
                Property2 = 1,
            });
            list.Add(new TwoPropertyStruct()
            {
                Property1 = "object2",
                Property2 = 2,
            });

            return this.Ok(list);
        }

        [BatchTypeExtension(typeof(TwoPropertyStruct), "kids", typeof(IEnumerable<ChildTestObject>))]
        public IGraphActionResult FetchChildren(IEnumerable<TwoPropertyStruct> sourceData)
        {
            AddCounter(nameof(this.FetchChildren));

            var listResult = new List<ChildTestObject>();

            // multiple results per source
            foreach (var sourceItem in sourceData)
            {
                int i = 0;
                while (i < 2)
                {
                    listResult.Add(new ChildTestObject()
                    {
                        ParentId = sourceItem.Property1,
                        Name = sourceItem.Property1 + "_child_" + i++,
                    });
                }
            }

            return this.StartBatch()
                .FromSource(sourceData, source => source.Property1)
                .WithResults(listResult, result => result.ParentId)
                .Complete();
        }

        [BatchTypeExtension(typeof(TwoPropertyStruct), "sybling", typeof(SyblingTestObject))]
        public IGraphActionResult FetchSibling(IEnumerable<TwoPropertyStruct> sourceData)
        {
            AddCounter(nameof(this.FetchSibling));

            var listResult = new List<SyblingTestObject>();

            // only one result per source item
            foreach (var sourceItem in sourceData)
            {
                listResult.Add(new SyblingTestObject()
                {
                    SyblingId = sourceItem.Property1,
                    Name = sourceItem.Property1 + "_sybling",
                });
            }

            return this.StartBatch()
                .FromSource(sourceData, source => source.Property1)
                .WithResults(listResult, result => result.SyblingId)
                .Complete();
        }
    }
}