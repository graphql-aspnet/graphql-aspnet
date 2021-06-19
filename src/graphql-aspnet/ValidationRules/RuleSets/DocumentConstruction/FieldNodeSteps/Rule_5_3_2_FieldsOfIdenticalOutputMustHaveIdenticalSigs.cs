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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Any fields within the current selection set must have identical signatures to be valid.
    /// </summary>
    internal class Rule_5_3_2_FieldsOfIdenticalOutputMustHaveIdenticalSigs : DocumentConstructionRuleStep<FieldNode>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && ((FieldNode)context.ActiveNode)
                       .FieldName
                       .Span
                       .SequenceNotEqual(Constants.ReservedNames.TYPENAME_FIELD.AsSpan());
        }

        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var node = (FieldNode)context.ActiveNode;
            var newField = context.FindContextItem<FieldSelection>();

            // do a fast lookup before enumerating the field selection to determine
            // if there would even be a name collision.
            if (!context.SelectionSet.ContainsAlias(node.FieldAlias))
                return true;

            var isValid = true;
            var existingFields = context.SelectionSet.FindFieldsOfAlias(node.FieldAlias);
            foreach (var existingField in existingFields)
            {
                // we may iterate through the field we are adding, it can exist with itself
                // just skip it
                if (existingField == newField)
                    continue;

                // fields with the same name in a given context
                // but targeting non-intersecting types can safely co-exist
                // in the same selection set.
                if (this.CanCoExist(context.DocumentContext.Schema, context.SelectionSet, existingField, newField))
                    continue;

                // fields that could cause a name collision for a type
                // must be mergable (i.e. have the same shape/signature).
                if (this.AreSameShape(existingField, newField))
                    continue;

                this.ValidationError(
                    context,
                    $"The selection set already contains a field with a name or alias of '{newField.Node.FieldAlias.ToString()}'. " +
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
        /// <param name="existingField">The existing field already commited to the active selection set.</param>
        /// <param name="newField">The new field having the same return name (field alias) as the already commited field. </param>
        /// <returns><c>true</c> if the field shape is identical; otherwise <c>false</c>.</returns>
        private bool AreSameShape(FieldSelection existingField, FieldSelection newField)
        {
            // one field could be referencing through an interface
            // and another through a concrete type so we cant check the IGraphField references.
            // Instead check to ensure the method invocation signatures are the same (field name, input args and return type).
            if (!MemoryOfCharComparer.Instance.Equals(newField.Name, existingField.Name))
                return false;

            if (existingField.GraphType != newField.GraphType)
                return false;

            // perform a node to node check for the arguments. newField might
            // not be fully populated at this level but in context, we have enough
            // information to successfully check equality.
            var existingArgs = existingField.Node?
                .Children?
                .FirstOrDefault<InputItemCollectionNode>()?
                .Children?
                .OfType<InputItemNode>()
                .ToList();

            var newArgs = newField.Node?
                .Children?
                .FirstOrDefault<InputItemCollectionNode>()?
                .Children?
                .OfType<InputItemNode>()
                .ToDictionary(x => x.InputName, x => x, MemoryOfCharComparer.Instance);

            // when no args are on the existing field
            // no args must be on the new field (either its not defined or none were defined; an empty set).
            if (existingArgs == null)
                return newArgs == null || newArgs.Count == 0;

            // when no args are on the new field ensure the existing field
            // also has no args defined (empty set).
            if (newArgs == null)
                return !existingArgs.Any();

            // both fields define a set of arguments, ensure they are equal
            // before doing an item by item check
            if (existingArgs.Count != newArgs.Count)
                return false;

            foreach (var existingArg in existingArgs)
            {
                // ensure input arg names exist on the new field
                var newArg = newArgs.ContainsKey(existingArg.InputName) ? newArgs[existingArg.InputName] : null;
                if (newArg == null)
                    return false;

                // grab the raw values of the existing and new input args
                var existingValue = existingArg.Children.FirstOrDefault<InputValueNode>()?.Value ?? ReadOnlyMemory<char>.Empty;
                var newValue = newArg.Children.FirstOrDefault<InputValueNode>()?.Value ?? ReadOnlyMemory<char>.Empty;

                // we can get away with checking the raw input text provided on the document
                // don't have to fully check parsed values to determine equaility in the sense that this
                // rule requires.
                if (!MemoryOfCharComparer.Instance.Equals(existingValue, newValue))
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
        /// <param name="selectionSet">The set of fields to which the new field needs to be added and the existing field is already
        /// a part of.</param>
        /// <param name="existingField">The field that has already been added to the field selection set.</param>
        /// <param name="newField">The new field that is being requested to be added to the field selection set.</param>
        /// <returns><c>true</c> if the fields can coexist in the same field selection set; otherwise, <c>false</c>.</returns>
        private bool CanCoExist(ISchema targetSchema, FieldSelectionSet selectionSet, FieldSelection existingField, FieldSelection newField)
        {
            var inContextGraphType = existingField.TargetGraphType ?? selectionSet.GraphType;
            var newFieldGraphType = newField.TargetGraphType ?? selectionSet.GraphType;

            // neither should be null at this point
            if (inContextGraphType == null)
            {
                throw new GraphExecutionException(
                    $"Attempting to resolve specification rule {this.RuleNumber} resulted in " +
                    "an invalid graph type comparrison. Unable to determine the target graph type of the " +
                    $"existing field aliased as '{existingField.Alias.ToString()}'. Query was aborted.");
            }

            if (newFieldGraphType == null)
            {
                throw new GraphExecutionException(
                    $"Attempting to resolve specification rule {this.RuleNumber} resulted in " +
                    "an invalid graph type comparrison. Unable to determine the target graph type of the " +
                    $"new field aliased as '{existingField.Alias.ToString()}'. Query was aborted.");
            }

            // if the graph types of either field "could" overlap at some point
            // then the two fields cannot safely co-exist.
            var inContextTypes = targetSchema.KnownTypes.ExpandAbstractType(inContextGraphType);
            var newfieldTypes = targetSchema.KnownTypes.ExpandAbstractType(newFieldGraphType);

            return !inContextTypes.Intersect(newfieldTypes).Any();
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