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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing2;

    /// <summary>
    /// A factory to generate appropriate query input values for a parsed document.
    /// </summary>
    internal static class DocumentSuppliedValueFactory
    {
        /// <summary>
        /// Converts a node read on a query document into a value representation that can be resolved
        /// to a usable .NET type in a query plan.
        /// </summary>
        /// <param name="ownerPart">The document part which will own the created value.</param>
        /// <param name="valueNode">The AST node from which the value should be created.</param>
        /// <param name="key">A key value, if any, to assign to the value node.</param>
        /// <returns>IQueryInputValue.</returns>
        public static ISuppliedValueDocumentPart CreateInputValue(Parsing2.Lexing.Source.SourceText sourceText, IDocumentPart ownerPart, SynNode valueNode, string key = null)
        {
            switch (valueNode.NodeType)
            {
                case SynNodeType.ListValue:
                    return new DocumentListSuppliedValue(ownerPart, valueNode.Location, key);

                case SynNodeType.NullValue:
                    return new DocumentNullSuppliedValue(ownerPart, valueNode.Location, key);

                case SynNodeType.ComplexValue:
                    return new DocumentComplexSuppliedValue(ownerPart, valueNode.Location, key);

                case SynNodeType.ScalarValue:
                    return new DocumentScalarSuppliedValue(
                        ownerPart,
                        sourceText.Slice(valueNode.PrimaryValue.TextBlock).ToString(),
                        valueNode.PrimaryValue.ValueType,
                        valueNode.Location,
                        key);

                case SynNodeType.EnumValue:
                    return new DocumentEnumSuppliedValue(
                        ownerPart,
                        sourceText.Slice(valueNode.PrimaryValue.TextBlock).ToString(),
                        valueNode.Location,
                        key);

                case SynNodeType.VariableValue:
                    return new DocumentVariableUsageValue(
                        ownerPart,
                        sourceText.Slice(valueNode.PrimaryValue.TextBlock).ToString(),
                        valueNode.Location,
                        key);

                case SynNodeType.Empty:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(valueNode),
                        $"Unknown type '{valueNode.NodeType}'. " +
                        $"Factory is unable to generate a '{nameof(ISuppliedValueDocumentPart)}' to fulfill the request.");
            }
        }
    }
}