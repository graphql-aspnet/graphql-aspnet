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
        /// Directives the directive location corrisponding with this schema item.
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

                case IEnumOption _:
                    return DirectiveLocation.ENUM_VALUE;

                case IInputObjectGraphType _:
                    return DirectiveLocation.INPUT_OBJECT;

                case IGraphField field:
                    if (field.Parent is IObjectGraphType)
                        return DirectiveLocation.FIELD_DEFINITION;
                    if (field.Parent is IInputObjectGraphType)
                        return DirectiveLocation.INPUT_FIELD_DEFINITION;
                    break;

                case IGraphFieldArgument _:
                    return DirectiveLocation.ARGUMENT_DEFINITION;
            }

            return DirectiveLocation.NONE;
        }

        /// <summary>
        /// Determines the directive location corrisponding with this syntax node.
        /// </summary>
        /// <param name="node">The node to inspect.</param>
        /// <returns>DirectiveLocation.</returns>
        public static DirectiveLocation AsDirectiveLocation(this SyntaxNode node)
        {
            // only supporting "execuable directives" at this time.
            switch (node)
            {
                case NamedFragmentNode _:
                    return DirectiveLocation.FRAGMENT_DEFINITION;

                case FragmentSpreadNode _:
                    return DirectiveLocation.FRAGMENT_SPREAD;

                case FragmentNode _:
                    return DirectiveLocation.INLINE_FRAGMENT;

                case FieldNode _:
                    return DirectiveLocation.FIELD;

                case InputItemNode _:
                    return DirectiveLocation.INPUT_OBJECT;

                case OperationTypeNode otn:
                    var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(otn.OperationType.ToString());
                    switch (operationType)
                    {
                        case GraphCollection.Query:
                            return DirectiveLocation.QUERY;

                        case GraphCollection.Mutation:
                            return DirectiveLocation.MUTATION;

                        case GraphCollection.Subscription:
                            return DirectiveLocation.SUBSCRIPTION;
                    }

                    break;
            }

            return DirectiveLocation.NONE;
        }
    }
}