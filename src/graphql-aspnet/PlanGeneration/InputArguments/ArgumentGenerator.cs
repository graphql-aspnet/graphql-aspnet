// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.InputArguments
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;

    /// <summary>
    /// An object capable of taking a <see cref="QueryInputValue"/> from a document and converting it into a
    /// core .NET type that represents the value within the target schema.
    /// </summary>
    public class ArgumentGenerator
    {
        private readonly IQueryInputArgumentCollection _suppliedArguments;
        private readonly ISchema _schema;
        private readonly InputResolverMethodGenerator _inputResolverGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentGenerator" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="suppliedArguments">A collection of arguments passed on a user's query
        /// to be used first in the chain of object resolution for any created arguments.</param>
        public ArgumentGenerator(ISchema schema, IQueryInputArgumentCollection suppliedArguments)
        {
            _suppliedArguments = Validation.ThrowIfNullOrReturn(suppliedArguments, nameof(suppliedArguments));
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _inputResolverGenerator = new InputResolverMethodGenerator(_schema);
        }

        /// <summary>
        /// Attempts to create a qualified input argument for the supplied schema field.
        /// </summary>
        /// <param name="argument">The argument defined on schema that needs to have
        /// an input value created fro.</param>
        /// <returns>Task.</returns>
        public ArgumentGenerationResult CreateInputArgument(IGraphFieldArgument argument)
        {
            Validation.ThrowIfNull(argument, nameof(argument));

            if (!_suppliedArguments.ContainsKey(argument.Name))
            {
                return new ArgumentGenerationResult(new ResolvedInputArgumentValue(argument.Name, argument.DefaultValue));
            }

            var coreValue = _suppliedArguments[argument.Name].Value;
            var resolver = _inputResolverGenerator.CreateResolver(coreValue.OwnerArgument.TypeExpression);

            if (this.ShouldDeferResolution(coreValue))
                return new ArgumentGenerationResult(new DeferredInputArgumentValue(coreValue, resolver));

            try
            {
                var data = resolver.Resolve(coreValue);
                return new ArgumentGenerationResult(new ResolvedInputArgumentValue(coreValue.OwnerArgument.Name, data));
            }
            catch (UnresolvedValueException svce)
            {
                var message = new GraphExecutionMessage(
                   GraphMessageSeverity.Critical,
                   svce.Message,
                   Constants.ErrorCodes.INVALID_ARGUMENT,
                   coreValue.OwnerArgument.Value.ValueNode.Location.AsOrigin(),
                   exception: svce.InnerException);

                return new ArgumentGenerationResult(message);
            }
            catch (Exception ex)
            {
                var message = new GraphExecutionMessage(
                    GraphMessageSeverity.Critical,
                    "Invalid argument value.",
                    Constants.ErrorCodes.INVALID_ARGUMENT,
                    coreValue.OwnerArgument.Value.ValueNode.Location.AsOrigin(),
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
        private bool ShouldDeferResolution(QueryInputValue value)
        {
            switch (value)
            {
                case QueryVariableReferenceInputValue _:
                    return true;

                case QueryListInputValue liv:
                    foreach (var child in liv.ListItems)
                    {
                        if (this.ShouldDeferResolution(child))
                            return true;
                    }

                    break;

                case QueryComplexInputValue civ:
                    foreach (var argument in civ.Arguments.Values)
                    {
                        if (this.ShouldDeferResolution(argument.Value))
                            return true;
                    }

                    break;
            }

            return false;
        }
    }
}