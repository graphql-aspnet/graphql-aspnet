// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A factory to generate appropriate query input values for a parsed document.
    /// </summary>
    public static class QueryInputValueFactory
    {
        /// <summary>
        /// Converts a node read on a query document into a value representation that can be resolved
        /// to a usable .NET type in a query plan.
        /// </summary>
        /// <param name="valueNode">The value node.</param>
        /// <returns>IQueryInputValue.</returns>
        public static QueryInputValue CreateInputValue(InputValueNode valueNode)
        {
            if (valueNode == null)
                return null;

            switch (valueNode)
            {
                case ListValueNode lvn:
                    return new QueryListInputValue(lvn);

                case NullValueNode nvn:
                    return new QueryNullInputValue(nvn);

                case ComplexValueNode cvn:
                    return new QueryComplexInputValue(cvn);

                case ScalarValueNode svn:
                    return new QueryScalarInputValue(svn);

                case EnumValueNode evn:
                    return new QueryEnumInputValue(evn);

                case VariableValueNode vvn:
                    return new QueryVariableReferenceInputValue(vvn);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(valueNode),
                        $"Unknown type '{valueNode?.GetType().FriendlyName()}'. " +
                        $"Factory is unable to generate a '{nameof(QueryInputValue)}' to fulfill the request.");
            }
        }
    }
}