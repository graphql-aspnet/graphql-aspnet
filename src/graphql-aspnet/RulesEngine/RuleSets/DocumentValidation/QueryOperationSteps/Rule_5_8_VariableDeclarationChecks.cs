// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.RulesEngine;

    /// <summary>
    /// A collective rule to evaluate all variable targets (5.8.*) at once.
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
            var docPart = (IOperationDocumentPart)context.ActivePart;


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
                      operation.Node.Location.AsOrigin(),
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
                       variable.Node.Location.AsOrigin(),
                       "Unknown Graph Type. Could not determine the graph type expression of the variable " +
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
                        variable.Node.Location.AsOrigin(),
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
                       variable.Node.Location.AsOrigin(),
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
                    variable.Node.Location.AsOrigin(),
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
                        variableUsage.Node.Location.AsOrigin(),
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
                // cycles may appear that althogh caught by 5.5.2.2 may carry forward here
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
                    allValidUsages = this.CheckForValidUsage585(
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


        private bool CheckForValidUsage585(
            DocumentValidationContext context,
            IVariableUsageDocumentPart variableUsage,
            IVariableDocumentPart variable)
        {
            // if the usage is not declared on an argument then this rule
            // cant be effectively evaluated
            var argument = variableUsage.Parent as IInputArgumentDocumentPart;
            if (argument == null)
                return true;

            // ensure the type expressions are compatible at the location used
            if (!variable.TypeExpression.Equals(argument.TypeExpression))
            {
                this.ValidationError(
                    context,
                    RuleNumber_585,
                    AnchorTag_585,
                    variableUsage.Node.Location.AsOrigin(),
                    "Invalid Variable Argument. The type expression for the variable used on the argument " +
                    $"'{argument.Name}' could " +
                    $"not be successfully coerced to the required type. Expected '{argument.TypeExpression}' but got '{variable.TypeExpression}'. Double check " +
                    $"the declared graph type of the variable and ensure it matches the required type of '{argument.Name}'.");

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
