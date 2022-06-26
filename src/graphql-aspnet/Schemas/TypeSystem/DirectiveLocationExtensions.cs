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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;

    /// <summary>
    /// Helper methods for the <see cref="DirectiveLocation"/> enumeration.
    /// </summary>
    public static class DirectiveLocationExtensions
    {
        /// <summary>
        /// Determines the directive location corrisponding with to the supplied schema item (Type System Locations).
        /// </summary>
        /// <param name="item">The item to inspect.</param>
        /// <returns>DirectiveLocation.</returns>
        public static DirectiveLocation AsDirectiveLocation(this ISchemaItem item)
        {
            Validation.ThrowIfNull(item, nameof(item));

            switch (item)
            {
                case ISchema _:
                    return DirectiveLocation.SCHEMA;

                case IScalarGraphType _:
                    return DirectiveLocation.SCALAR;

                case IObjectGraphType _:
                    return DirectiveLocation.OBJECT;

                case IInterfaceGraphType _:
                    return DirectiveLocation.INTERFACE;

                case IUnionGraphType _:
                    return DirectiveLocation.UNION;

                case IEnumGraphType _:
                    return DirectiveLocation.ENUM;

                case IEnumValue _:
                    return DirectiveLocation.ENUM_VALUE;

                case IInputObjectGraphType _:
                    return DirectiveLocation.INPUT_OBJECT;

                case IGraphField field:
                    if (field.Parent is IObjectGraphType)
                        return DirectiveLocation.FIELD_DEFINITION;
                    if (field.Parent is IInputObjectGraphType)
                        return DirectiveLocation.INPUT_FIELD_DEFINITION;
                    break;

                case IGraphArgument _:
                    return DirectiveLocation.ARGUMENT_DEFINITION;
            }

            return DirectiveLocation.NONE;
        }

        /// <summary>
        /// Determines the directive location corrisponding with this syntax node in the AST representing
        /// a query document. (Execution Locations).
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns>DirectiveLocation.</returns>
        public static DirectiveLocation AsDirectiveLocation(this SyntaxNode node)
        {
            // all syntax nodes (those parsed on a document) will be
            // "execution phase" locations.
            switch (node)
            {
                case NamedFragmentNode _:
                    return DirectiveLocation.FRAGMENT_DEFINITION;

                case FragmentSpreadNode _:
                    return DirectiveLocation.FRAGMENT_SPREAD;

                case InlineFragmentNode _:
                    return DirectiveLocation.INLINE_FRAGMENT;

                case FieldNode _:
                    return DirectiveLocation.FIELD;

                case OperationNode otn:
                    var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(otn.OperationType.ToString());
                    switch (operationType)
                    {
                        case GraphOperationType.Query:
                            return DirectiveLocation.QUERY;

                        case GraphOperationType.Mutation:
                            return DirectiveLocation.MUTATION;

                        case GraphOperationType.Subscription:
                            return DirectiveLocation.SUBSCRIPTION;
                    }

                    break;
            }

            return DirectiveLocation.NONE;
        }

        /// <summary>
        /// Determines whether the provided location is one declared duing the
        /// execution of a query document.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns><c>true</c> if the location specifies an execution location; otherwise, <c>false</c>.</returns>
        public static bool IsExecutionLocation(this DirectiveLocation location)
        {
            return (location & DirectiveLocation.AllExecutionLocations) > 0;
        }

        /// <summary>
        /// Determines whether the provided location is one declared during
        /// the creation of a <see cref="ISchema"/> and its associated types.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns><c>true</c> if the location specifies an type system location; otherwise, <c>false</c>.</returns>
        public static bool IsTypeDeclarationLocation(this DirectiveLocation location)
        {
            return (location & DirectiveLocation.AllTypeSystemLocations) > 0;
        }
    }
}