// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.InputArguments
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts.SuppliedValues;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object capable of taking a <see cref="DocumentSuppliedValue"/> from a document and converting it into a
    /// core .NET type that represents the value within the target schema.
    /// </summary>
    internal class ArgumentGenerator
    {
        private readonly InputValueResolverMethodGenerator _inputResolverGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGenerator" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public ArgumentGenerator(ISchema schema)
        {
            Validation.ThrowIfNull(schema, nameof(schema));
            _inputResolverGenerator = new InputValueResolverMethodGenerator(schema);
        }

        /// <summary>
        /// Attempts to create a qualified input argument for the supplied schema field.
        /// </summary>
        /// <param name="suppliedArgumentData">The supplied arguments on a query document.</param>
        /// <param name="argumentDefinition">The argument defined on schema that needs to have
        /// an input value created.</param>
        /// <returns>Task.</returns>
        public ArgumentGenerationResult CreateInputArgument(
            IInputArgumentCollectionDocumentPart suppliedArgumentData,
            IGraphArgument argumentDefinition)
        {
            Validation.ThrowIfNull(suppliedArgumentData, nameof(suppliedArgumentData));
            Validation.ThrowIfNull(argumentDefinition, nameof(argumentDefinition));

            if (!suppliedArgumentData.ContainsKey(argumentDefinition.Name))
            {
                if (argumentDefinition.IsRequired)
                {
                    // this should be an impossible scenario due to validation middleware
                    // However, the pipeline can be changed by the developer so we must
                    // account for it
                    throw new GraphExecutionException(
                        $"Unable to generate an expected argument '{argumentDefinition.Name}' for field '{argumentDefinition.Parent.Name}' during field resolution. " +
                        $"The argument is marked as required but was not supplied on the generated query document. " +
                        $"This may be due to a pipeline configuration issue as this error should have be caught during query validation " +
                        $"but wasn't.");
                }

                return new ArgumentGenerationResult(
                    new InputArgument(
                        argumentDefinition,
                        new ResolvedInputArgumentValue(
                            argumentDefinition.Name,
                            argumentDefinition.DefaultValue),
                        SourceOrigin.None));
            }

            var suppliedArgument = suppliedArgumentData[argumentDefinition.Name];
            var coreValue = suppliedArgument.Value;
            var resolver = _inputResolverGenerator.CreateResolver(argumentDefinition);

            if (this.ShouldDeferResolution(coreValue))
            {
                return new ArgumentGenerationResult(
                    new InputArgument(
                        argumentDefinition,
                        new DeferredInputArgumentValue(coreValue, resolver),
                        suppliedArgument.Origin));
            }

            try
            {
                var data = resolver.Resolve(coreValue);
                return new ArgumentGenerationResult(
                    new InputArgument(
                        argumentDefinition,
                        new ResolvedInputArgumentValue(argumentDefinition.Name, data),
                        suppliedArgument.Origin));
            }
            catch (UnresolvedValueException svce)
            {
                var message = new GraphExecutionMessage(
                   GraphMessageSeverity.Critical,
                   svce.Message,
                   Constants.ErrorCodes.INVALID_ARGUMENT_VALUE,
                   suppliedArgument.Origin,
                   exception: svce.InnerException);

                return new ArgumentGenerationResult(message);
            }
            catch (Exception ex)
            {
                var message = new GraphExecutionMessage(
                    GraphMessageSeverity.Critical,
                    "Invalid argument value. See exception for details.",
                    Constants.ErrorCodes.INVALID_ARGUMENT_VALUE,
                    suppliedArgument.Origin,
                    ex);

                return new ArgumentGenerationResult(message);
            }
        }

        /// <summary>
        /// Inspects the input value to see if it contains any data  that must be resolved
        /// at execution time (i.e. variable references) thus deferring a complete resolution of the data value to a later time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if execution should be deferred, <c>false</c> otherwise.</returns>
        private bool ShouldDeferResolution(ISuppliedValueDocumentPart value)
        {
            switch (value)
            {
                case IVariableUsageDocumentPart _:
                    return true;

                case IListSuppliedValueDocumentPart liv:
                    foreach (var child in liv.ListItems)
                    {
                        if (this.ShouldDeferResolution(child))
                            return true;
                    }

                    break;

                case IComplexSuppliedValueDocumentPart civ:
                    foreach (var field in civ.Fields.Values)
                    {
                        if (this.ShouldDeferResolution(field.Value))
                            return true;
                    }

                    break;
            }

            return false;
        }
    }
}