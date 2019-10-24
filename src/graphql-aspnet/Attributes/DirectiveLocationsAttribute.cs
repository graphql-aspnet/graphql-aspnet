// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using GraphQL.AspNet.Directives;

    /// <summary>
    /// For a given directive, defines where in a query document the directive is allowed to appear.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DirectiveLocationsAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveLocationsAttribute"/> class.
        /// </summary>
        /// <param name="locations">The set of locations in a query document where the directive can be defined.</param>
        public DirectiveLocationsAttribute(ExecutableDirectiveLocation locations)
        {
            this.Locations = locations;
        }

        /// <summary>
        /// Gets the locations where this directive can be defined.
        /// </summary>
        /// <value>The locations.</value>
        public ExecutableDirectiveLocation Locations { get; }
    }
}