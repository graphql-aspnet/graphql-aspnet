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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// For a given directive, defines where in a query document the directive is allowed to appear.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DirectiveLocationsAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveLocationsAttribute"/> class.
        /// </summary>
        /// <param name="locations">The set of locations in a query document where the directive can be declared.</param>
        public DirectiveLocationsAttribute(ExecutableDirectiveLocation locations)
        {
            this.Locations = (DirectiveLocation)locations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveLocationsAttribute"/> class.
        /// </summary>
        /// <param name="locations">The set of types within the type system that
        /// this directive can target.</param>
        public DirectiveLocationsAttribute(TypeSystemDirectiveLocation locations)
        {
            this.Locations = (DirectiveLocation)locations;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveLocationsAttribute"/> class.
        /// </summary>
        /// <param name="locations">The set of types within the type system that
        /// this directive can target.</param>
        public DirectiveLocationsAttribute(DirectiveLocation locations)
        {
            this.Locations = locations;
        }

        /// <summary>
        /// Gets the locations where this directive can be defined.
        /// </summary>
        /// <value>The locations.</value>
        public DirectiveLocation Locations { get; }
    }
}