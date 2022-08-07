// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.RulesEngine.RuleSets.DirectiveExecution;

    /// <summary>
    /// A rule processor that handles rules related to the invocation of directives at runtime,
    /// be that invocations on a query document or those added to <see cref="ISchemaItem"/>
    /// during schema construction.
    /// </summary>
    internal class DirectiveValidationRuleProcessor : RuleProcessor<GraphDirectiveExecutionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveValidationRuleProcessor"/> class.
        /// </summary>
        public DirectiveValidationRuleProcessor()
            : base(DirectiveValidationRulePackage.Instance)
        {
        }
    }
}