// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document
{
    using System;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// Helper methods for managing the <see cref="DocumentPartType"/>
    /// enumeration.
    /// </summary>
    public static class DocumentPartTypeExtensions
    {

        /// <summary>
        /// Determines the related system level interface that any part of the given type
        /// should implement.
        /// </summary>
        /// <param name="partType">The part to inspect.</param>
        /// <returns>A type representing the interface.</returns>
        public static Type RelatedInterface(this DocumentPartType partType)
        {
            switch (partType)
            {
                case DocumentPartType.Operation:
                    return typeof(IQueryOperationDocumentPart);

                case DocumentPartType.Variable:
                    return typeof(IQueryVariableDocumentPart);

                case DocumentPartType.VariableCollection:
                    return typeof(IQueryVariableCollectionDocumentPart);

                case DocumentPartType.FieldSelection:
                    return typeof(IFieldSelectionDocumentPart);

                case DocumentPartType.FieldSelectionSet:
                    return typeof(IFieldSelectionSetDocumentPart);

                case DocumentPartType.SuppliedValue:
                    return typeof(ISuppliedValueDocumentPart);

                case DocumentPartType.InputArgument:
                    return typeof(IQueryArgumentDocumentPart);

                case DocumentPartType.Directive:
                    return typeof(IDirectiveDocumentPart);

                case DocumentPartType.Fragment:
                    return typeof(IFragmentDocumentPart);

                case DocumentPartType.FragmentCollection:
                    return typeof(IFragmentCollectionDocumentPart);

                case DocumentPartType.OperationCollection:
                    return typeof(IQueryOperationCollectionDocumentPart);

                case DocumentPartType.InputArgumentCollection:
                    return typeof(IQueryInputArgumentCollectionDocumentPart);
            }

            throw new InvalidOperationException($"Unsupported {partType}, no related interface exists.");
        }
    }
}