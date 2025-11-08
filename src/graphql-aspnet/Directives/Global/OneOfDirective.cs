// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Directives.Global
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.OneOfDirectiveSteps;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// <para>
    /// A directive, applicable to an INPUT OBJECT type, that defines a validation scheme such that only
    /// one value of all available fields can be supplied in a document.
    /// </para>
    /// <para>
    /// https://spec.graphql.org/September2025/#sec-OneOf-Input-Objects
    /// </para>
    /// </summary>
    [GraphType(Constants.ReservedNames.ONEOF_DIRECTIVE)]
    [Description("A type system directive that enforces the 'one of' rule set for the target input object.")]
    public class OneOfDirective : GraphDirective
    {
        private static readonly object _locker = new object();
        private static bool _isGlobalValidationRuleSetUpdated;

        /// <summary>
        /// Executes the directive on the target input type.
        /// </summary>
        [DirectiveLocations(DirectiveLocation.INPUT_OBJECT)]
        public IGraphActionResult Execute()
        {
            lock (_locker)
            {
                if (!_isGlobalValidationRuleSetUpdated)
                {
                    _isGlobalValidationRuleSetUpdated = true;

                    // inject the global rules to validate oneof input objects
                    // when supplied as an object literal
                    DocumentValidationRulePackage.Instance.AddCustomRule(
                        DocumentPartType.Argument,
                        new Rule_3_10_1_LiteralValueChecks());

                    // when supplied as a complete variable reference
                    // or field value as a variable referencxe
                    DocumentValidationRulePackage.Instance.AddCustomRule(
                        DocumentPartType.Operation,
                        new Rule_3_10_1_VariableDeclarationChecks());
                }
            }

            // enforcement of this directive is accomplished via the additional validation rules
            // applied to every query document processed by the system
            return this.Ok();
        }
    }
}