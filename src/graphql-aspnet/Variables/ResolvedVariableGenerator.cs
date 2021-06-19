// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Variables
{
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.PlanGeneration;

    /// <summary>
    /// Attempts to convert the untyped keyvalue pairs supplied with a request into context-sensitive variables for a
    /// given operation.
    /// </summary>
    public class ResolvedVariableGenerator
    {
        private readonly ISchema _schema;
        private readonly IGraphFieldExecutableOperation _operation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvedVariableGenerator" /> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="operation">The operation.</param>
        public ResolvedVariableGenerator(ISchema schema, IGraphFieldExecutableOperation operation)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _operation = Validation.ThrowIfNullOrReturn(operation, nameof(operation));
        }

        /// <summary>
        /// Converts the input variable collection to their type-expression-bound values in context of the
        /// operation being executed.
        /// </summary>
        /// <param name="inputVariables">The input variables.</param>
        /// <returns>IResolvedVariableCollection.</returns>
        public IResolvedVariableCollection Resolve(IInputVariableCollection inputVariables)
        {
            if (inputVariables == null || inputVariables.Count == 0)
                return ResolvedVariableCollection.Empty;

            var resolverGenerator = new InputResolverMethodGenerator(_schema);
            var result = new ResolvedVariableCollection();

            foreach (var variable in _operation.DeclaredVariables.Values)
            {
                var resolver = resolverGenerator.CreateResolver(variable.TypeExpression);

                IResolvableItem resolvableItem = null;
                var found = inputVariables.TryGetVariable(variable.Name, out var suppliedValue);
                resolvableItem = found ? suppliedValue : variable.Value as IResolvableItem;

                var resolvedValue = resolver.Resolve(resolvableItem);

                var resolvedVariable = new ResolvedVariable(variable.Name, variable.TypeExpression, resolvedValue);
                result.AddVariable(resolvedVariable);
            }

            return result;
        }
    }
}