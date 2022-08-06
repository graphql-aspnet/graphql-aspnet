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

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GraphTypeExpression left, GraphTypeExpression right)
        {
            return left?.Equals(right) ?? right?.Equals(left) ?? true;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(GraphTypeExpression left, GraphTypeExpression right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns true if a value of the the
        /// <paramref name="supplied"/> expression can be used
        /// as an input value to a item with the <paramref name="target"/> expression.
        /// </summary>
        /// <remarks>This method is a direct implementation of the "ArtTypesCompatiable"
        /// algorithm defined in the graphql spec rule 5.8.5.<br/>
        /// (https://spec.graphql.org/October2021/#sec-All-Variable-Usages-are-Allowed).
        /// </remarks>
        /// <param name="target">The target expression to which a value is being given.</param>
        /// <param name="supplied">The expression of the value to be supplied to the target.</param>
        /// <returns><c>true</c> if a value of the <paramref name="supplied"/> expression is
        /// compatiable and could be supplied or used as an input value to an item of the <paramref name="target"/> expression, <c>false</c> otherwise.</returns>
        public static bool AreTypesCompatiable(GraphTypeExpression target, GraphTypeExpression supplied)
        {
            if (target == null || supplied == null)
                return false;

            // when root types don't match they can never be compatible
            if (target.TypeName != supplied.TypeName)
                return false;

            if (!target.IsNullable)
            {
                if (supplied.IsNullable)
                    return false;

                return AreTypesCompatiable(target.UnWrapExpression(), supplied.UnWrapExpression());
            }
            else if (!supplied.IsNullable)
            {
                return AreTypesCompatiable(target, supplied.UnWrapExpression());
            }
            else if (target.IsListOfItems)
            {
                if (!supplied.IsListOfItems)
                    return false;

                return AreTypesCompatiable(target.UnWrapExpression(), supplied.UnWrapExpression());
            }
            else if (supplied.IsListOfItems)
            {
                return false;
            }

            return target == supplied;
        }
    }
}