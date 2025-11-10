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
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation.OneOfDirectiveSteps;
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

                    // validate input arguments (which can be INPUT OBJECTs)
                    // to ensure they conform to @oneOf requirements
                    DocumentValidationRulePackage.Instance.AddCustomRule(
                        DocumentPartType.Argument,
                        new Rule_3_10_1_InputArguments());

                    // for every field that might be declared on an input argument
                    // (e.g. the argument is a INPUT_OBJECT and one of its declared fields is itself an input union)
                    // make sure that it conforms to the @oneOf requirements, this is naturally recursive.
                    DocumentValidationRulePackage.Instance.AddCustomRule(
                        DocumentPartType.InputField,
                        new Rule_3_10_1_InputFields());

                    // When an input argument's value is a variable reference
                    // we need to validate the supplied variable value after its resolved to ensure
                    // the variable data only supplied a single field etc.
                    VariableDataValidationRulePackage.Instance.AddCustomRule(
                        DocumentPartType.Argument,
                        new Rule_3_10_1_InputArgumentVariable());

                    // If a field of an input argument is a variable reference
                    // we need to validate the supplied value after its resolved to ensure
                    // the variable data only supplied an object with a single field to the target etc.
                    // (this is recursive)
                    VariableDataValidationRulePackage.Instance.AddCustomRule(
                        DocumentPartType.InputField,
                        new Rule_3_10_1_InputFieldVariable());
                }
            }

            // enforcement of this directive is accomplished via the additional validation rules
            // applied to every query document processed by the system
            return this.Ok();
        }
    }
}