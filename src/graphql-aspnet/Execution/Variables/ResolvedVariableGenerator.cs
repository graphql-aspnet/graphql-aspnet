// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Variables
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.NodeBuilders;
    using GraphQL.AspNet.Execution.QueryPlans;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.Resolvers;

    /// <summary>
    /// An object that attempts to convert the untyped keyvalue pairs (usually pulled from a json doc)
    /// into context-sensitive variable values for a specific operation in a query document.
    /// </summary>
    public class ResolvedVariableGenerator
    {
        private readonly ISchema _schema;
        private readonly IVariableCollectionDocumentPart _variableCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedVariableGenerator" /> class.
        /// </summary>
        /// <param name="schema">A schema to resolve against.</param>
        /// <param name="variableCollection">A set of declared variable references
        /// on an operation (from a supplied query document).</param>
        public ResolvedVariableGenerator(
            ISchema schema,
            IVariableCollectionDocumentPart variableCollection)
            : this(schema, variableCollection, new GraphMessageCollection())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedVariableGenerator" /> class.
        /// </summary>
        /// <param name="schema">A schema to resolve against.</param>
        /// <param name="variableCollection">A set of declared variable references
        /// on an operation (from a supplied query document).</param>
        /// <param name="messagesToFill">A pre-existing collection of messages to write any
        /// generated messages to.</param>
        public ResolvedVariableGenerator(
            ISchema schema,
            IVariableCollectionDocumentPart variableCollection,
            IGraphMessageCollection messagesToFill)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _variableCollection = Validation.ThrowIfNullOrReturn(variableCollection, nameof(variableCollection));

            this.Messages = Validation.ThrowIfNullOrReturn(messagesToFill, nameof(messagesToFill));
        }

        /// <summary>
        /// Converts the input variable collection to their type-expression-bound values in context of the
        /// operation being executed.
        /// </summary>
        /// <param name="inputVariables">A set of values used to resolve the variable
        /// definitions supplied to this instance.</param>
        /// <returns>IResolvedVariableCollection.</returns>
        public IResolvedVariableCollection Resolve(IInputVariableCollection inputVariables)
        {
            var resolverGenerator = new InputValueResolverMethodGenerator(_schema);
            var result = new ResolvedVariableCollection();

            foreach (var variable in _variableCollection)
            {
                try
                {
                    var resolver = resolverGenerator.CreateResolver(variable.TypeExpression);

                    IResolvableValueItem resolvableItem = null;
                    IInputVariable suppliedValue = null;

                    var found = false;
                    if (inputVariables != null)
                        found = inputVariables.TryGetVariable(variable.Name, out suppliedValue);

                    if (!found && !variable.HasDefaultValue)
                    {
                        var operationName = _variableCollection.Operation.Name ?? "~anonymous~";
                        this.Messages.Critical(
                             $"The declared variable '{variable.Name}' for operation '{operationName}' is required but was not supplied.",
                             Constants.ErrorCodes.INVALID_VARIABLE_VALUE,
                             _variableCollection.Operation.SourceLocation.AsOrigin());

                        continue;
                    }
                    resolvableItem = found ? suppliedValue : variable.DefaultValue;


                    object resolvedValue = resolver.Resolve(resolvableItem);

                    var resolvedVariable = new ResolvedVariable(variable.Name, variable.TypeExpression, resolvedValue, !found);

                    // validate nullability of the provided value against the TypeExpression of the declaration
                    // on the operation
                    if (!resolvedVariable.TypeExpression.IsNullable && resolvedVariable.Value == null)
                    {
                        var operationName = _variableCollection.Operation.Name ?? "~anonymous~";
                        this.Messages.Critical(
                             "The resolved variable value of <null> is not valid for non-nullable variable " +
                             $"'{resolvedVariable.Name}' on operation '{operationName}'",
                             Constants.ErrorCodes.INVALID_VARIABLE_VALUE,
                             _variableCollection.Operation.SourceLocation.AsOrigin());
                        continue;
                    }

                    result.AddVariable(resolvedVariable);
                }
                catch (UnresolvedValueException svce)
                {
                    this.Messages.Critical(
                       svce.Message,
                       Constants.ErrorCodes.INVALID_VARIABLE_VALUE,
                       _variableCollection.Operation.SourceLocation.AsOrigin(),
                       exceptionThrown: svce.InnerException);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the collection of messages filled by this instance when it resolved variables.
        /// </summary>
        /// <value>The message collection.</value>
        public IGraphMessageCollection Messages { get; }
    }
}