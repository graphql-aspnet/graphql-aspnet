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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Tokens;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A declaration of the usage of a single graph type (with appropriate wrappers).
    /// This is an object represention of the field and variable declaration
    /// syntax used by GraphQL (e.g. '[SomeType]!').
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
        /// <remarks>
        /// Schema Lang: <c>Type</c><br/>
        /// --.
        /// </remarks>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] SingleItem { get; } = new MetaGraphTypes[0];

        /// <summary>
        /// Gets a wrapper set representing a return value of a single item. The item must be provided, it cannot be null.
        /// </summary>
        /// <remarks>
        /// Schema Lang: <c>Type!</c><br/>
        /// --.
        /// </remarks>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] RequiredSingleItem { get; } = { MetaGraphTypes.IsNotNull };

        /// <summary>
        /// Gets a wrapper set representing a return value of a list of items. Both the list and the items within the list could be null.
        /// </summary>
        /// <remarks>
        /// Schema Lang: <c>[Type]</c><br/>
        /// --.
        /// </remarks>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] List { get; } = { MetaGraphTypes.IsList };

        /// <summary>
        /// Gets a wrapper set representing a return value of list items. The list is required to be returned, but the items within the list may be null.
        /// </summary>
        /// <remarks>
        /// Schema Lang: <c>[Type]!</c><br/>
        /// --.
        /// </remarks>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] RequiredList { get; } = { MetaGraphTypes.IsNotNull, MetaGraphTypes.IsList };

        /// <summary>
        /// Gets a wrapper set representing a return value of list items. The list is can be null but when returned all the items within the list must not be null.
        /// </summary>
        /// <remarks>
        /// Schema Lang: <c>[Type!]</c><br/>
        /// --.
        /// </remarks>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] ListRequiredItem { get; } = { MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull };

        /// <summary>
        /// Gets a wrapper set representing a return value of list items. The list itself is required to be returned and all items in the list must not be null.
        /// </summary>
        /// <remarks>
        /// Schema Lang: <c>[Type!]!</c><br/>
        /// --.
        /// </remarks>
        /// <value>A set of wrappers.</value>
        public static MetaGraphTypes[] RequiredListRequiredItem { get; } = { MetaGraphTypes.IsNotNull, MetaGraphTypes.IsList, MetaGraphTypes.IsNotNull };

        /// <summary>
        /// Parses an expression in syntax definition language (e.g. '[SomeType!]!') into a usable <see cref="GraphTypeExpression" />.
        /// </summary>
        /// <param name="typeExpression">The type expression.</param>
        /// <returns>GraphTypeDeclaration.</returns>
        [DebuggerStepperBoundary]
        public static GraphTypeExpression FromDeclaration(string typeExpression)
        {
            ReadOnlySpan<char> data = ReadOnlySpan<char>.Empty;
            if (!string.IsNullOrWhiteSpace(typeExpression))
                data = typeExpression.Trim().AsSpan();

            return FromDeclaration(data);
        }

        /// <summary>
        /// Parses an expression in syntax definition language (e.g. '[SomeType!]!') into a usable <see cref="GraphTypeExpression" />.
        /// </summary>
        /// <param name="typeExpression">The type expression to parse.</param>
        /// <returns>GraphTypeDeclaration.</returns>
        [DebuggerStepperBoundary]
        public static GraphTypeExpression FromDeclaration(ReadOnlySpan<char> typeExpression)
        {
            if (typeExpression.IsEmpty)
                return GraphTypeExpression.Invalid;

            // inspect the first and last characters for type modifiers and extract as necessary
            if (typeExpression[typeExpression.Length - 1] == TokenTypeNames.BANG)
            {
                var ofType = FromDeclaration(typeExpression.Slice(0, typeExpression.Length - 1));

                // the expression 'SomeType!!' is invalid.
                if (ofType.Wrappers.Any() && ofType.Wrappers[0] == MetaGraphTypes.IsNotNull)
                    return GraphTypeExpression.Invalid;

                ofType = ofType.WrapExpression(MetaGraphTypes.IsNotNull);
                return ofType;
            }

            if (typeExpression[0] == TokenTypeNames.BRACKET_LEFT)
            {
                // must be at a minimum  '[a]'
                if (typeExpression.Length < 3 || typeExpression[typeExpression.Length - 1] != TokenTypeNames.BRACKET_RIGHT)
                    return GraphTypeExpression.Invalid;

                var ofType = FromDeclaration(typeExpression.Slice(1, typeExpression.Length - 2))
                             .WrapExpression(MetaGraphTypes.IsList);
                return ofType;
            }

            // account for an erronous close bracket, dont parse for anything else at this stage
            // meaning typename could be 'BOB![SMITH' and be expected in this method.
            if (typeExpression[typeExpression.Length - 1] == TokenTypeNames.BRACKET_RIGHT)
                return GraphTypeExpression.Invalid;

            return new GraphTypeExpression(typeExpression.ToString());
        }

        /// <summary>
        /// Inspects the provided type and generates a type expression to represent it in the object grpah.
        /// </summary>
        /// <param name="typeToCheck">The complete type specification to check.</param>
        /// <param name="typeWrappers">An optional set of wrappers to use as a set of overrides on the type provided.</param>
        /// <returns>GraphFieldOptions.</returns>
        public static GraphTypeExpression FromType(Type typeToCheck, MetaGraphTypes[] typeWrappers = null)
        {
            Validation.ThrowIfNull(typeToCheck, nameof(typeToCheck));

            if (typeWrappers != null)
            {
                typeToCheck = GraphValidation.EliminateWrappersFromCoreType(
                    typeToCheck,
                    eliminateEnumerables: true,
                    eliminateTask: true,
                    eliminateNullableT: true);

                return new GraphTypeExpression(typeToCheck.FriendlyGraphTypeName(), typeWrappers);
            }

            // strip out Task{T} before doin any type inspections
            typeToCheck = GraphValidation.EliminateWrappersFromCoreType(
                typeToCheck,
                eliminateEnumerables: false,
                eliminateTask: true,
                eliminateNullableT: false);

            var wrappers = new List<MetaGraphTypes>();
            if (GraphValidation.IsValidListType(typeToCheck))
            {
                // auto generated type expressions will always allow for a nullable list (since class references can be null)
                // unwrap any nested lists as necessary:  e.g.  IEnumerable<IEnumerable<IEnumerable<T>>>
                while (true)
                {
                    var unwrappedType = GraphValidation.EliminateNextWrapperFromCoreType(
                        typeToCheck,
                        eliminateEnumerables: true,
                        eliminateTask: false,
                        eliminateNullableT: false);

                    if (unwrappedType == typeToCheck)
                        break;

                    wrappers.Add(MetaGraphTypes.IsList);
                    typeToCheck = unwrappedType;
                }

                if (GraphValidation.IsNotNullable(typeToCheck))
                {
                    wrappers.Add(MetaGraphTypes.IsNotNull);
                }
            }
            else if (GraphValidation.IsNotNullable(typeToCheck))
            {
                wrappers.Add(MetaGraphTypes.IsNotNull);
            }

            typeToCheck = GraphValidation.EliminateWrappersFromCoreType(
                typeToCheck,
                eliminateEnumerables: false,
                eliminateTask: false,
                eliminateNullableT: true);

            return new GraphTypeExpression(typeToCheck.FriendlyGraphTypeName(), wrappers);
        }

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
        /// Spect: <see href="https://spec.graphql.org/October2021/#sec-All-Variable-Usages-are-Allowed" />.
        /// </remarks>
        /// <param name="target">The target expression to which a value is being given.</param>
        /// <param name="supplied">The expression of the value to be supplied to the target.</param>
        /// <param name="matchTypeName">When true, the inner type name of the <paramref name="supplied"/> expression must match exactly (case-sensitive) to
        /// that of the <paramref name="target"/> expression.</param>
        /// <returns><c>true</c> if a value of the <paramref name="supplied"/> expression is
        /// compatiable and could be supplied or used as an input value to an item of the <paramref name="target"/> expression, <c>false</c> otherwise.</returns>
        public static bool AreTypesCompatiable(GraphTypeExpression target, GraphTypeExpression supplied, bool matchTypeName = true)
        {
            if (target == null || supplied == null)
                return false;

            if (!target.IsNullable)
            {
                if (supplied.IsNullable)
                    return false;

                return AreTypesCompatiable(target.UnWrapExpression(), supplied.UnWrapExpression(), matchTypeName);
            }
            else if (!supplied.IsNullable)
            {
                return AreTypesCompatiable(target, supplied.UnWrapExpression(), matchTypeName);
            }
            else if (target.IsListOfItems)
            {
                if (!supplied.IsListOfItems)
                    return false;

                return AreTypesCompatiable(target.UnWrapExpression(), supplied.UnWrapExpression(), matchTypeName);
            }
            else if (supplied.IsListOfItems)
            {
                return false;
            }

            // when root types don't match they can never be compatible
            if (matchTypeName)
                return target == supplied;

            return true;
        }
    }
}