// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FieldNodeSteps
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// Any fields with the same exposed name, within the current selection set must
    /// have identical signatures to be valid.
    /// </summary>
    internal class Rule_5_3_2_FieldsOfIdenticalOutputMustHaveIdenticalSigs
        : DocumentPartValidationRuleStep<IFieldDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context) && ((IFieldDocumentPart)context.ActivePart)
                       .Name
                       .Span
                       .SequenceNotEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var docPart = (IFieldDocumentPart)context.ActivePart;
            var selectionSet = context.ParentPart as IFieldSelectionSetDocumentPart;

            // this rule will execute against every field in a selection
            // we don't need to validate it more than once if there are mergable fields found
            var key = $"Rule_5_3_2|{selectionSet.Path.DotString()}|alias:{docPart.Alias.ToString()}";
            if (context.ChecksComplete.Contains(key))
                return true;

            context.ChecksComplete.Add(key);

            var fields = selectionSet.FindFieldsOfAlias(docPart.Alias);
            if (fields.Count == 1)
                return true;

            var isValid = true;
            for (var i = 0; i <= fields.Count - 2; i++)
            {
                // we may iterate through the field we are adding, it can exist with itself
                // just skip it
                var leftField = fields[i];
                var rightField = fields[i + 1];

                // fields with the same name in a given context
                // but targeting non-intersecting types can safely co-exist
                // in the same selection set.
                if (this.CanCoExist(context.Schema, leftField, rightField))
                    continue;

                // fields that could cause a name collision for a type
                // must be mergable (i.e. have the same shape/signature).
                if (this.AreSameShape(leftField, rightField))
                    continue;

                this.ValidationError(
                    context,
                    $"The selection set already contains a field with a name or alias of '{docPart.Alias}'. " +
                    "An attempt was made to add another field with the same name or alias to the selection set but with a different " +
                    "return graph type or input arguments. Fields with the same output name must have identicial signatures " +
                    "within a single selection set.");

                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Inspects both field selections to determine if they have identicial signatures (referencing the same field in hte shema
        /// with the same input arguments).
        /// </summary>
        /// <param name="leftField">The existing field already commited to the active selection set.</param>
        /// <param name="rightField">The new field having the same return name (field alias) as the already commited field. </param>
        /// <returns><c>true</c> if the field shape is identical; otherwise <c>false</c>.</returns>
        private bool AreSameShape(IFieldDocumentPart leftField, IFieldDocumentPart rightField)
        {
            // one field could be referencing through an interface
            // and another through a concrete type so we cant check the IGraphField references.
            // Instead check to ensure the method invocation signatures are the same (field name, input args and return type).
            if (!MemoryOfCharComparer.Instance.Equals(rightField.Name, leftField.Name))
                return false;

            if (leftField.GraphType != rightField.GraphType)
                return false;

            // perform a check for the arguments.
            var leftArgs = leftField.GatherArguments();
            var rightArgs = rightField.GatherArguments();

            // when no args are on the existing field
            // no args must be on the new field (either its not defined or none were defined; an empty set).
            if (leftArgs == null)
                return rightArgs == null || rightArgs.Count == 0;

            // when no args are on the new field ensure the existing field
            // also has no args defined (empty set).
            if (rightArgs == null)
                return leftArgs.Count == 0;

            // both fields define a set of arguments, ensure they are equal
            // before doing an item by item check
            if (leftArgs.Count != rightArgs.Count)
                return false;

            foreach (var leftArg in leftArgs.Values)
            {
                // ensure input arg names exist on the new field
                var rightArg = rightArgs.ContainsKey(leftArg.Name) ? rightArgs[leftArg.Name] : null;
                if (rightArg == null)
                    return false;

                if (!leftArg.Value.IsEqualTo(rightArg.Value))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Inspects both fields to see if any target graph type restrctions exist (such as with fragmetn spreads)
        /// such that the fields could not be included in the same object resolution. For example if the existing field targets a
        /// Dog object and the new field targets a Cat object the fields will never be resolved together for a given object and thus
        /// can coexist without issue.  Returns true when it is garunteed that the fields can coexist,
        /// otherwise returns false if the fields cannot co exist and must be merged.
        /// </summary>
        /// <param name="targetSchema">The schema to which this rule is validating against.</param>
        /// <param name="leftField">One of two fields being checked.</param>
        /// <param name="rightField">The other field being compared to <paramref name="leftField" />.</param>
        /// <returns><c>true</c> if the fields can coexist in the same field selection set; otherwise, <c>false</c>.</returns>
        private bool CanCoExist(ISchema targetSchema, IFieldDocumentPart leftField, IFieldDocumentPart rightField)
        {
            IGraphType leftSourceGraphType = null;
            IGraphType rightSourceGraphType = null;
            if (leftField.Parent is IFieldSelectionSetDocumentPart fsdl)
                leftSourceGraphType = fsdl.GraphType;
            if (rightField.Parent is IFieldSelectionSetDocumentPart fsdr)
                rightSourceGraphType = fsdr.GraphType;

            // neither should be null at this point
            if (leftSourceGraphType == null)
            {
                throw new GraphExecutionException(
                    $"Attempting to resolve specification rule {this.RuleNumber} resulted in " +
                    "an invalid graph type comparrison. Unable to determine the target graph type of the " +
                    $"existing field aliased as '{leftField.Alias.ToString()}'. Query was aborted.");
            }

            if (rightSourceGraphType == null)
            {
                throw new GraphExecutionException(
                    $"Attempting to resolve specification rule {this.RuleNumber} resulted in " +
                    "an invalid graph type comparrison. Unable to determine the target graph type of the " +
                    $"new field aliased as '{rightField.Alias.ToString()}'. Query was aborted.");
            }

            // if the source graph types of either field "could" overlap at some point
            // then the two fields cannot safely co-exist.
            var leftTypes = targetSchema.KnownTypes.ExpandAbstractType(leftSourceGraphType);
            var rightTypes = targetSchema.KnownTypes.ExpandAbstractType(rightSourceGraphType);

            return !leftTypes.Intersect(rightTypes).Any();
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.3.2";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Field-Selection-Merging";
    }
}