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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;

    /// <summary>
    /// Helper methods for the <see cref="DirectiveLocation"/> enumeration.
    /// </summary>
    public static class DirectiveLocationExtensions
    {
        /// <summary>
        /// Determines the directive location corrisponding with this syntax node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>DirectiveLocation.</returns>
        public static DirectiveLocation DirectiveLocation(this SyntaxNode node)
        {
            // only supporting "execuable directives" at this time.
            switch (node)
            {
                case NamedFragmentNode _:
                    return TypeSystem.DirectiveLocation.FRAGMENT_DEFINITION;

                case FragmentSpreadNode _:
                    return TypeSystem.DirectiveLocation.FRAGMENT_SPREAD;

                case FragmentNode _:
                    return TypeSystem.DirectiveLocation.INLINE_FRAGMENT;

                case FieldNode _:
                    return TypeSystem.DirectiveLocation.FIELD;

                case InputItemNode _:
                    return TypeSystem.DirectiveLocation.INPUT_OBJECT;

                case OperationTypeNode otn:
                    var operationType = Constants.ReservedNames.FindOperationTypeByKeyword(otn.OperationType.ToString());
                    switch (operationType)
                    {
                        case GraphCollection.Query:
                            return TypeSystem.DirectiveLocation.QUERY;
                        case GraphCollection.Mutation:
                            return TypeSystem.DirectiveLocation.MUTATION;
                        case GraphCollection.Subscription:
                            return TypeSystem.DirectiveLocation.SUBSCRIPTION;
                    }

                    break;
            }

            return TypeSystem.DirectiveLocation.NONE;
        }
    }
}