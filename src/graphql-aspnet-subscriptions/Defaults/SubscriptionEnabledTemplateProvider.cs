// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template provider that adds the ability to parse subscription marked fields on graph controllers
    /// in order to add them to the schema.
    /// </summary>
    public class SubscriptionEnabledTemplateProvider : DefaultTypeTemplateProvider
    {
        /// <summary>
        /// Makes a graph template from the given type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="kind">The kind of graph type to parse for.</param>
        /// <returns>IGraphItemTemplate.</returns>
        protected override IGraphTypeTemplate MakeTemplate(Type objectType, TypeKind kind)
        {
            if (Validation.IsCastable<GraphController>(objectType))
                return new GraphSubscriptionControllerTemplate(objectType);

            return base.MakeTemplate(objectType, kind);
        }
    }
}