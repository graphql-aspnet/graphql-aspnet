// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collective rule to evaluate all variable targets (5.8.*) at once in context of
    /// the operation where they are defined.
    /// <br/>
    /// 5.8.1: All Variable Names must be unique per operation
    /// <br/>
    /// 5.8.2: All variable graph types must be valid input types (SCALAR, ENUM, INPUT_OBJECT)
    /// <br/>
    /// 5.8.3: All variable usages encountered within an operation are defined on the operation.
    /// <br/>
    /// 5.8.4: All variables defined on an operation are used at least once within the operation.
    /// <br/>
    /// 5.8.5: All variables usages are allowed in context. (Variable def. matches usage def.)
    /// </summary>
    internal class Rule_5_8_VariableDeclarationChecks
        : DocumentPartValidationStep<IOperationDocumentPart>
    {
        private const string RuleNumber_581 = "5.8.1";
        private const string RuleNumber_582 = "5.8.2";
        private const string RuleNumber_583 = "5.8.3";
        private const string RuleNumber_584 = "5.8.4";
        private const string RuleNumber_585 = "5.8.5";

        private const string AnchorTag_581 = "#sec-Variable-Uniqueness";
        private const string AnchorTag_582 = "#sec-Variables-Are-Input-Types";
        private const string AnchorTag_583 = "#sec-All-Variable-Uses-Defined";
        private const string AnchorTag_584 = "#sec-All-Variables-Used";
        private const string AnchorTag_585 = "#sec-All-Variable-Usages-are-Allowed";

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var passed = this.Check581(context);
            passed = passed && this.Check582(context);
            passed = passed && this.Check583(context);
            passed = passed && this.Check583(context);
            passed = passed && this.Check584(context);
            passed = passed && this.Check585(context);

            return passed;
        }

        /// <summary>
        /// Ensure that all variables are unique within the operation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>bool.</returns>
        private bool Check581(DocumentValidationContext context)
        {
            var operation = (IOperationDocumentPart)context.ActivePart;

            var noDuplicates = true;
            foreach (var dupVarName in operation.Variables.Duplicates)
            {
                this.ValidationError(
                      context,
                      RuleNumber_581,
                      AnchorTag_581,
                      operation.SourceLocation.AsOrigin(),
                      $"Duplicate Variable Name. The variable named '{dupVarName}' must be unique " +
                      "in its contained operation. Ensure that all variable names, per operation, are unique (case-sensitive).");

                noDuplicates = false;
            }

            return noDuplicates;
        }

        /// <summary>
        /// Ensure that all declared variables are of correct input variable types.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>bool.</returns>
        private bool Check582(DocumentValidationContext context)
        {
            var operation = (IOperationDocumentPart)context.ActivePart;
            var allVariablesPassed = true;

            foreach (var variable in operation.Variables)
            {
                if (variable.TypeExpression == null || !variable.TypeExpression.IsValid)
                {
                    this.ValidationError(
                       context,
                       RuleNumber_582,
                       AnchorTag_582,
                       variable.SourceLocation.AsOrigin(),
                       "Unknown Variable Type Expresssion. Could not determine the graph type expression of the variable " +
                       $"named '{variable.Name}'. Double check that your variable declaration is correct.");

                    allVariablesPassed = false;
                    continue;
                }
                else if (variable.GraphType == null)
                {
                    this.ValidationError(
                        context,
                        RuleNumber_582,
                        AnchorTag_582,
                        variable.SourceLocation.AsOrigin(),
                        $"Unknown Variable Graph Type. The variable named '{variable?.Name}' declares " +
                        $"itself as a graph type of '{variable.TypeExpression}' but the target graph type does not " +
                        "exist in the schema.");

                    allVariablesPassed = false;
                }
                else if (!variable.GraphType.Kind.IsValidInputKind())
                {
                    this.ValidationError(
                       context,
                       RuleNumber_582,
                       AnchorTag_582,
                       variable.SourceLocation.AsOrigin(),
                       $"Invalid Variable Graph Type. The variable named '${variable.Name}' references the graph type " +
                       $"'{variable.GraphType.Name}' which is of kind {variable.GraphType.Kind}.  Only " +
                       $"{TypeKind.SCALAR}, {TypeKind.ENUM} and '{TypeKind.INPUT_OBJECT}' are allowed for " +
                       "variable declarations.");

                    allVariablesPassed = false;
                }
            }

            return allVariablesPassed;
        }

        private bool Check583(DocumentValidationContext context)
        {
            var operation = (IOperationDocumentPart)context.ActivePart;
            operation.Variables.ClearReferences();
            return this.CheckReferencesForVariables583(
                context,
                operation.Variables,
                operation,
                new HashSet<INamedFragmentDocumentPart>());
        }

        private bool Check584(DocumentValidationContext context)
        {
            var operation = (IOperationDocumentPart)context.ActivePart;

            var allVariablesPassed = true;
            foreach (var variable in operation.Variables.UnreferencedVariables)
            {
                this.ValidationError(
                    context,
                    RuleNumber_584,
                    AnchorTag_584,
                    variable.SourceLocation.AsOrigin(),
                    $"The variable '{variable.Name}' was not used within the target operation. " +
                    "All declared variables must be used at least once.");

                allVariablesPassed = false;
            }

            return allVariablesPassed;
        }

        private bool Check585(DocumentValidationContext context)
        {
            var operation = (IOperationDocumentPart)context.ActivePart;
            operation.Variables.ClearReferences();
            return this.CheckReferencesForVariables585(
                context,
                operation.Variables,
                operation,
                new HashSet<INamedFragmentDocumentPart>());
        }

        private bool CheckReferencesForVariables583(
            DocumentValidationContext context,
            IVariableCollectionDocumentPart variables,
            IReferenceDocumentPart refPart,
            HashSet<INamedFragmentDocumentPart> walkedFragments)
        {
            var allUsagesFound = true;
            foreach (var variableUsage in refPart.VariableUsages)
            {
                var variableName = variableUsage.VariableName.ToString();
                if (!variables.Contains(variableName))
                {
                    this.ValidationError(
                        context,
                        RuleNumber_583,
                        AnchorTag_583,
                        variableUsage.SourceLocation.AsOrigin(),
                        $"The variable named '${variableName}' is not declared for the " +
                        $"operation '{variables.Operation.Name}'.");

                    allUsagesFound = false;
                }
                else
                {
                    variables.MarkAsReferenced(variableUsage.VariableName.ToString());
                }
            }

            foreach (var fragmentUsage in refPart.FragmentSpreads)
            {
                // can't evaluate unreferenced fragments (5.5.2.1 will pick it up)
                if (fragmentUsage.Fragment == null)
                    continue;

                // don't walk a named fragment more than once
                // cycles may appear that, although caught by 5.5.2.2, may carry forward here
                if (walkedFragments.Contains(fragmentUsage.Fragment))
                    continue;

                walkedFragments.Add(fragmentUsage.Fragment);
                allUsagesFound = this.CheckReferencesForVariables583(
                    context,
                    variables,
                    fragmentUsage.Fragment,
                    walkedFragments) && allUsagesFound;
            }

            return allUsagesFound;
        }

        private bool CheckReferencesForVariables585(
            DocumentValidationContext context,
            IVariableCollectionDocumentPart variables,
            IReferenceDocumentPart refPart,
            HashSet<INamedFragmentDocumentPart> walkedFragments)
        {
            var allValidUsages = true;
            foreach (var variableUsage in refPart.VariableUsages)
            {
                if (variables.TryGetValue(variableUsage.VariableName.ToString(), out var variable))
                {
                    allValidUsages = this.Check585_IsVariableUsageAllowed(
                        context,
                        variableUsage,
                        variable) && allValidUsages;
                }
            }

            foreach (var fragmentUsage in refPart.FragmentSpreads)
            {
                // can't evaluate unreferenced fragments (5.5.2.1 will pick it up)
                if (fragmentUsage.Fragment == null)
                    continue;

                // don't walk a named fragment more than once
                // cycles may appear that althogh caught by 5.5.2.2 may carry forward here
                if (walkedFragments.Contains(fragmentUsage.Fragment))
                    continue;

                walkedFragments.Add(fragmentUsage.Fragment);
                allValidUsages = this.CheckReferencesForVariables585(
                    context,
                    variables,
                    fragmentUsage.Fragment,
                    walkedFragments) && allValidUsages;
            }

            return allValidUsages;
        }

        /// <summary>
        /// This method is a direct implementation of the 'IsVariableUsageAllowed' algorithm
        /// defined on section 5.8.5 of the specification.
        /// </summary>
        /// <remarks>
        /// Spec: <see href="https://spec.graphql.org/October2021/#sec-All-Variable-Usages-are-Allowed" />.
        /// </remarks>
        /// <param name="context">The context being evaluated.</param>
        /// <param name="variableUsage">The variable usage where the variable
        /// was applied to the query document.</param>
        /// <param name="variable">The variable definition that was applied.</param>
        /// <returns><c>true</c> if the variable can be used where its applied, <c>false</c> otherwise.</returns>
        private bool Check585_IsVariableUsageAllowed(
            DocumentValidationContext context,
            IVariableUsageDocumentPart variableUsage,
            IVariableDocumentPart variable)
        {
            // can't evaluate this rule if the owner of the usage
            // is not set
            if (variableUsage.Parent == null)
                return true;

            var variableType = variable.TypeExpression;

            string argName;
            bool hasLocationDefaultValue;
            GraphTypeExpression originalLocationType;
            GraphTypeExpression checkedLocationType;

            switch (variableUsage.Parent)
            {
                case IInputArgumentDocumentPart argPart:
                    // this rule can't evaluate for unassigned graph arguments
                    if (argPart.Argument == null)
                        return true;

                    hasLocationDefaultValue = argPart.Argument.HasDefaultValue;
                    originalLocationType = argPart.Argument.TypeExpression;
                    argName = argPart.Name;
                    break;

                case IInputObjectFieldDocumentPart iof:
                    // this rule cant evaluate unassigned input fields
                    if (iof.Field == null)
                        return true;

                    // TODO: Add support for default input values on fields (github issue #70)
                    hasLocationDefaultValue = !iof.Field.IsRequired;
                    originalLocationType = iof.Field.TypeExpression;
                    argName = iof.Name;
                    break;

                case IListSuppliedValueDocumentPart lsv:
                    // this rule can't evaluate untyped input lists
                    if (lsv.ListItemTypeExpression == null)
                        return true;

                    hasLocationDefaultValue = false;
                    originalLocationType = lsv.ListItemTypeExpression;
                    argName = "<list>";
                    break;

                default:
                    this.ValidationError(
                       context,
                       RuleNumber_585,
                       AnchorTag_585,
                       variableUsage.SourceLocation.AsOrigin(),
                       $"Unsupported Variable Usage. Variable '{variable.Name}' used at " +
                       $"the target location is not supported.");

                    return false;
            }

            checkedLocationType = originalLocationType;

            // acount for allowed default values
            // on the variable or usage locations
            var defaultValueChecksPassed = true;
            if (!checkedLocationType.IsNullable && variableType.IsNullable)
            {
                // account for nullability and default values between
                // the target location and the variable
                var hasNonNullVariableDefaultValue = variable.DefaultValue != null
                        && !(variable.DefaultValue is INullSuppliedValueDocumentPart);

                if (!hasNonNullVariableDefaultValue && !hasLocationDefaultValue)
                    defaultValueChecksPassed = false;
                else
                    checkedLocationType = checkedLocationType.UnWrapExpression();
            }

            // ensure the type expressions are compatible at the location used
            if (!defaultValueChecksPassed || !GraphTypeExpression.AreTypesCompatiable(checkedLocationType, variableType))
            {
                this.ValidationError(
                    context,
                    RuleNumber_585,
                    AnchorTag_585,
                    variableUsage.SourceLocation.AsOrigin(),
                    "Invalid Variable Usage. The type expression for the variable used at " +
                    $"'{argName}' could not be successfully coerced to the required type. " +
                    $"Expected '{originalLocationType}' but got '{variableType}'.");

                return false;
            }

            return true;
        }

        private void ValidationError(
            DocumentValidationContext context,
            string ruleNumber,
            string anchorTag,
            SourceOrigin origin,
            string messageText)
        {
            var url = ReferenceRule.CreateFromAnchorTag(anchorTag);

            var message = GraphExecutionMessage.FromValidationRule(
                ruleNumber,
                url,
                this.ErrorCode,
                messageText,
                origin);

            context.Messages.Add(message);
        }
    }
}
