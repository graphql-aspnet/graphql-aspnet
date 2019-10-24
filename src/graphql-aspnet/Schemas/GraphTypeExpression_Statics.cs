// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A declaration of the usage of a single graph type (with appropriate wrappers).
    /// This is an object represention of the field and variable declaration
    /// schema syntax (e.g. '[SomeType]!').
    /// </summary>
    public partial class GraphTypeExpression
    {
        /// <summary>
        /// Gets a singleton instance of a declaration representing an expression that is not valid.
        /// </summary>
        /// <value>a type expression.</value>
        public static GraphTypeExpression Invalid { get; } = new GraphTypeExpression(string.Empty);

        /// <summary>
        /// Gets a wrapper set representing a return value of a single item. That item may be returned as null.
        /// </summary>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] SingleItem { get; } = new MetaGraphTypes[0];

        /// <summary>
        /// Gets a wrapper set representing a return value of a single item. The item must be provided, it cannot be null.
        /// </summary>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] RequiredSingleItem { get; } = { MetaGraphTypes.IsNotNull };

        /// <summary>
        /// Gets a wrapper set representing a return value of a list of items. Both the list and the items within the list could be null.
        /// </summary>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] List { get; } = { MetaGraphTypes.IsList };

        /// <summary>
        /// Gets a wrapper set representing a return value of list items. The list is required to be returned, but the items within the list may be null.
        /// </summary>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] RequiredList { get; } = { MetaGraphTypes.IsNotNull, MetaGraphTypes.IsList };

        /// <summary>
        /// Gets a wrapper set representing a return value of list items. The list is can be null but when returned all the items within the list must not be null.
        /// </summary>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] ListRequiredItem { get; } = { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull };

        /// <summary>
        /// Gets a wrapper set representing a return value of list items. The list itself is required to be returned and all items in the list must not be null.
        /// </summary>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] RequiredListRequiredItem { get; } = { MetaGraphTypes.IsNotNull, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull };
    }
}