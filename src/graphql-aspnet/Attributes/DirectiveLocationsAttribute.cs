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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// For a given directive, defines where the directive is allowed to be applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DirectiveLocationsAttribute : BaseGraphAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveLocationsAttribute"/> class.
        /// </summary>
        /// <param name="locations">The bitwise set of locations where
        /// this directive can be applied.</param>
        public DirectiveLocationsAttribute(DirectiveLocation locations)
        {
            this.Locations = locations;
        }

        /// <summary>
        /// Gets the locations where this directive can be applied.
        /// </summary>
        /// <value>The locations.</value>
        public DirectiveLocation Locations { get; }
    }
}