// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A class representing the application of a <see cref="GraphDirective"/>
    /// to a schema item.
    /// </summary>
    public class AppliedDirective
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirective" /> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive.</param>
        public AppliedDirective(Type directiveType)
        {
            this.DirectiveType = Validation.ThrowIfNullOrReturn(directiveType, nameof(directiveType));
            Validation.ThrowIfNotCastable<GraphDirective>(directiveType, nameof(directiveType));
        }

        /// <summary>
        /// Gets the concrete type of the directive that has been applied.
        /// </summary>
        /// <value>The type of the directive.</value>
        public Type DirectiveType { get; }

        /// <summary>
        /// Gets the collection of arguments supplied on the directive
        /// invocation. These arguments are passed to the <see cref="GraphDirective"/>
        /// created to process the <see cref="ISchemaItem"/>
        /// that owns instance.
        /// </summary>
        /// <value>The arguments.</value>
        public object[] Arguments { get; }
    }
}