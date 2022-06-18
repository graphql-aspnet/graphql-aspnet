// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// A factory to generate appropriate query input values for a parsed document.
    /// </summary>
    internal static class DocumentSuppliedValueFactory
    {
        /// <summary>
        /// Converts a node read on a query document into a value representation that can be resolved
        /// to a usable .NET type in a query plan.
        /// </summary>
        /// <param name="valueNode">The value node.</param>
        /// <returns>IQueryInputValue.</returns>
        public static ISuppliedValueDocumentPart CreateInputValue(InputValueNode valueNode)
        {
            if (valueNode == null)
                return null;

            switch (valueNode)
            {
                case ListValueNode lvn:
                    return new DocumentListSuppliedValue(lvn);

                case NullValueNode nvn:
                    return new DocumentNullSuppliedValue(nvn);

                case ComplexValueNode cvn:
                    return new DocumentComplexSuppliedValue(cvn);

                case ScalarValueNode svn:
                    return new DocumentScalarSuppliedValue(svn);

                case EnumValueNode evn:
                    return new DocumentEnumSuppliedValue(evn);

                case VariableValueNode vvn:
                    return new DocumentVariableReferenceInputValue(vvn);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(valueNode),
                        $"Unknown type '{valueNode?.GetType().FriendlyName()}'. " +
                        $"Factory is unable to generate a '{nameof(ISuppliedValueDocumentPart)}' to fulfill the request.");
            }
        }
    }
}