// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Configuration.SchemaOptionsTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    internal class CountableLateBoundDirective : GraphDirective
    {
        public Dictionary<ISchemaItem, int> Invocations { get; }

        public CountableLateBoundDirective()
        {
            this.Invocations = new Dictionary<ISchemaItem, int>();
        }

        [DirectiveLocations(DirectiveLocation.AllTypeSystemLocations)]
        public IGraphActionResult PerformOperationon()
        {
            if (!(this.DirectiveTarget is ISchemaItem))
                throw new System.Exception("Unexpected directive target");

            var item = this.DirectiveTarget as ISchemaItem;
            if (!this.Invocations.ContainsKey(this.DirectiveTarget as ISchemaItem))
                this.Invocations[item] = 0;

            this.Invocations[item] += 1;

            // dont actually do anything, this is just a test for inclusion
            return this.Ok();
        }
    }
}