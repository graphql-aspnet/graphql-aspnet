﻿// *************************************************************
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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts.SuppliedValues;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An object capable of taking a <see cref="DocumentSuppliedValue"/> from a document and converting it into a
    /// core .NET type that represents the value within the target schema.
    /// </summary>
    public class ArgumentGenerator
    {
        private readonly IInputArgumentCollectionDocumentPart _suppliedArguments;
        private readonly InputResolverMethodGenerator _inputResolverGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGenerator" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="suppliedArguments">A collection of arguments passed on a user's query
        /// to be used first in the chain of object resolution for any created arguments.</param>
        public ArgumentGenerator(ISchema schema, IInputArgumentCollectionDocumentPart suppliedArguments)
        {
            _suppliedArguments = Validation.ThrowIfNullOrReturn(suppliedArguments, nameof(suppliedArguments));
            Validation.ThrowIfNull(schema, nameof(schema));
            _inputResolverGenerator = new InputResolverMethodGenerator(schema);
        }

        /// <summary>
        /// Attempts to create a qualified input argument for the supplied schema field.
        /// </summary>
        /// <param name="argument">The argument defined on schema that needs to have
        /// an input value created fro.</param>
        /// <returns>Task.</returns>
        public ArgumentGenerationResult CreateInputArgument(IGraphArgument argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));

            if (!_suppliedArguments.ContainsKey(argument.Name))
            {
                return new ArgumentGenerationResult(new ResolvedInputArgumentValue(argument.Name, argument.DefaultValue));
            }

            var coreValue = _suppliedArguments[argument.Name].Value;
            var resolver = _inputResolverGenerator.CreateResolver(argument.TypeExpression);

            if (this.ShouldDeferResolution(coreValue))
                return new ArgumentGenerationResult(new DeferredInputArgumentValue(coreValue, resolver));

            try
            {
                var data = resolver.Resolve(coreValue);
                return new ArgumentGenerationResult(new ResolvedInputArgumentValue(argument.Name, data));
            }
            catch (UnresolvedValueException svce)
            {
                var message = new GraphExecutionMessage(
                   GraphMessageSeverity.Critical,
                   svce.Message,
                   Constants.ErrorCodes.INVALID_ARGUMENT,
                   coreValue.Parent.SourceLocation.AsOrigin(),
                   exception: svce.InnerException);

                return new ArgumentGenerationResult(message);
            }
            catch (Exception ex)
            {
                var message = new GraphExecutionMessage(
                    GraphMessageSeverity.Critical,
                    "Invalid argument value. See exception for details.",
                    Constants.ErrorCodes.INVALID_ARGUMENT,
                    coreValue.Parent.SourceLocation.AsOrigin(),
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