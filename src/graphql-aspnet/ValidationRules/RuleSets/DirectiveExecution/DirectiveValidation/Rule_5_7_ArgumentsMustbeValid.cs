// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.ValidationRules.RuleSets.DirectiveExecution.DirectiveValidation
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.ValidationRules.RuleSets.DirectiveExecution.Common;

    /// <summary>
    /// A rule to ensure that the location where the directive is being
    /// executed is valid for the target directive.
    /// </summary>
    internal class Rule_5_7_ArgumentsMustbeValid : DirectiveValidationRuleStep
    {
        /// <inheritdoc />
        public override bool ShouldExecute(GraphDirectiveExecutionContext context)
        {
            return base.ShouldExecute(context) &&
                (context.Directive.Arguments.Count > 0 || context.Request.InvocationContext.Arguments.Count > 0);
        }

        /// <inheritdoc />
        public override bool Execute(GraphDirectiveExecutionContext context)
        {
            var directive = context.Directive;
            var directiveArgs = context.Directive.Arguments;
            var suppliedArgs = context.Request.InvocationContext.Arguments;
            var completedSuccessfully = true;

            // ensure every  argument on the request
            // matches a decalration in the directive
            var touchedArgs = new HashSet<IGraphArgument>();
            foreach (var suppliedArg in suppliedArgs)
            {
                var directiveArg = directiveArgs.FindArgument(suppliedArg.Name);
                if (directiveArg == null)
                {
                    this.ValidationError(
                        context,
                        $"Invalid Directive Invocation. The supplied argument named '{suppliedArg.Name}' does not " +
                        $"match any known argument on the directive '{directive.Name}'.");

                    completedSuccessfully = false;
                    continue;
                }

                completedSuccessfully = this.CompareArguments(context, directiveArg, suppliedArg) && completedSuccessfully;
                touchedArgs.Add(directiveArg);
            }

            // add an error for any required args that
            // were not touched
            var untouchedArgs = directive.Arguments
                .Where(x => !touchedArgs.Contains(x) && !x.HasDefaultValue);

            if (untouchedArgs.Any())
            {
                var args = string.Join(", ", untouchedArgs.Select(x => $"'{x.Name}'"));
                this.ValidationError(
                        context,
                        $"Invalid Directive Invocation. The directive '{directive.Name}' " +
                        $"declares some required arguments that were not provided. Missing arguments: {args}.");

                completedSuccessfully = false;
            }

            return completedSuccessfully;
        }

        /// <summary>
        /// Compares the arguments to ensure that the <paramref name="suppliedArg"/>
        /// can be used to fulfill the <paramref name="directiveArg"/>.
        /// </summary>
        /// <param name="context">The owning context.</param>
        /// <param name="directiveArg">The directive argument.</param>
        /// <param name="suppliedArg">The supplied argument.</param>
        private bool CompareArguments(
            GraphDirectiveExecutionContext context,
            IGraphArgument directiveArg,
            InputArgument suppliedArg)
        {
            var suppliedData = suppliedArg.Value.Resolve(context.VariableData);
            var completedSuccessfully = true;

            // when no value is supplied
            if (suppliedData == null)
            {
                if (!directiveArg.TypeExpression.IsNullable)
                {
                    this.ValidationError(
                            context,
                            $"Invalid Directive Invocation. The directive '{context.Directive.Name}' " +
                            $"requires that the value for argument '{suppliedArg.Name}' not be null.");
                    completedSuccessfully = false;
                }

                return completedSuccessfully;
            }

            var coreSuppliedType = suppliedData.GetType();
            if (coreSuppliedType != null)
                coreSuppliedType = GraphValidation.EliminateWrappersFromCoreType(coreSuppliedType);

            // when the core types don't match
            if (!Validation.IsCastable(coreSuppliedType, directiveArg.ObjectType))
            {
                var argType = context.Schema.KnownTypes.FindGraphType(directiveArg.ObjectType);

                var exception = new GraphExecutionException(
                    $"The supplied argument type '{coreSuppliedType?.FriendlyName()}' cannot be " +
                    $"cast to the expected type '{directiveArg.ObjectType.FriendlyName()}' for directive '{context.Directive.Name}', parameter: '{directiveArg.ParameterName}'.",
                    context.Request.Origin);

                this.ValidationError(
                        context,
                        $"Invalid Directive Invocation. The directive '{context.Directive.Name}' " +
                        $"requires that the argument '{suppliedArg.Name}' be coercable to type '{directiveArg.TypeExpression.CloneTo(argType.Name)}'. " +
                        $"See exception for details.",
                        exception);
                completedSuccessfully = false;
            }

            // is the value of a valid type expression
            if (!directiveArg.TypeExpression.Matches(suppliedData))
            {
                var argType = context.Schema.KnownTypes.FindGraphType(directiveArg.ObjectType);

                this.ValidationError(
                 context,
                 $"Invalid Directive Invocation. The argument value for '{directiveArg.Name}' on directive '{context.Directive.Name}' " +
                 $"cannot be coerced to '{directiveArg.TypeExpression.CloneTo(argType.Name)}'");
                completedSuccessfully = false;
            }

            return completedSuccessfully;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.7";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Validation.Directives";
    }
}