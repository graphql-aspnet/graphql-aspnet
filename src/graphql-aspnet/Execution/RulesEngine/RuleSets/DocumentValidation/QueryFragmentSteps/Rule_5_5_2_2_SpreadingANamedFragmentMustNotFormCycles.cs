// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.QueryFragmentSteps
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// Ensures that where ever a named fragment is spread that it is not, in the current path,
    /// nested inside itself forming an infinite resolution loop.
    /// </summary>
    internal class Rule_5_5_2_2_SpreadingANamedFragmentMustNotFormCycles
        : DocumentPartValidationRuleStep<IFragmentSpreadDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            var pointer = (IFragmentSpreadDocumentPart)context.ActivePart;

            // this rule cant be properly validated if no named fragment was
            // referenced. Rule 5.5.1.2 will trigger when this happens, this rule can be ignored.
            if (pointer.Fragment == null)
                return true;

            var checkedFragments = this.GetOrAddMetaData(
                context,
                () => new HashSet<INamedFragmentDocumentPart>());

            // if a spread of this fragment has already been evaluated we dont
            // need to eval it again
            if (checkedFragments.Contains(pointer.Fragment))
                return true;

            checkedFragments.Add(pointer.Fragment);

            // tracked for use in an error message to display the found
            // cycle if needed
            var fragmentPath = new Stack<string>();
            var fragmentsVisited = new HashSet<INamedFragmentDocumentPart>();

            // used to track already visited fragments in this change,
            // if a repeat occurs we have a cycle and this rule fails
            var formsACycle = this.DoesSpreadFormAFragmentCycle(
                pointer,
                fragmentPath,
                fragmentsVisited);

            if (formsACycle)
            {
                var list = fragmentPath.ToList();
                list.Reverse();

                var cyclePath = string.Join(" -> ", list);
                this.ValidationError(
                    context,
                    $"Spreading named fragment '{pointer.FragmentName}' forms an infinite cycle of " +
                    $"named fragment spreads. Path: {cyclePath}.");

                // when a cycle was formed we can be sure that any frags left
                // in the visited chain also form a cycle
                // and can be marked as checked
                // e.g.  in the chain A -> B -> A , starting at A or B form a cycle
                foreach (var frag in fragmentsVisited)
                    checkedFragments.Add(frag);

                return false;
            }

            // if no rule was broken (meaning no cycles formed)
            // then this fragment can be considered checked with no cycles
            return true;
        }

        /// <summary>
        /// Inspects the supplied node to see if its a named fragment pointer and walks that named fragment
        /// keeping track of the named fragments that are being traversed looking for any cyclic references that would
        /// result in an infinite loop of resolutions. If the node is not a fragment pointer, the fragment's children
        /// are traversed looking for additional pointers.
        /// </summary>
        /// <param name="docPart">The document part being tested.</param>
        /// <param name="fragmentPath">A stack inidcating the path within the node tree being inspected of all
        /// the named fragments that are/have been traversed.</param>
        /// <param name="fragmentsVisited">The fragments visited.</param>
        /// <returns><c>True</c> if a cyclic path was detected; otherwise, false.</returns>
        private bool DoesSpreadFormAFragmentCycle(
            IFragmentSpreadDocumentPart docPart,
            Stack<string> fragmentPath,
            HashSet<INamedFragmentDocumentPart> fragmentsVisited)
        {
            // this rule cant be properly validated if no named fragment was
            // referenced. Rule 5.5.1.2 will trigger when this happens, this rule can be ignored.
            if (docPart.Fragment == null)
                return false;

            // push the path to aid in error messages if this inspection forms a
            // cycle: e.g   A -> B -> A
            fragmentPath.Push(docPart.Fragment.Name);
            if (fragmentsVisited.Contains(docPart.Fragment))
                return true;

            // mark this instance as visited
            fragmentsVisited.Add(docPart.Fragment);

            var childSpreads = docPart.Fragment
                .FieldSelectionSet?
                .Children[DocumentPartType.FragmentSpread]
                .OfType<IFragmentSpreadDocumentPart>();
            childSpreads = childSpreads ?? Enumerable.Empty<IFragmentSpreadDocumentPart>();

            foreach (var childSpread in childSpreads)
            {
                if (this.DoesSpreadFormAFragmentCycle(childSpread, fragmentPath, fragmentsVisited))
                    return true;
            }

            // "unvisit" this fragment
            fragmentPath.Pop();
            fragmentsVisited.Remove(docPart.Fragment);
            return false;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.5.2.2";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Fragment-spreads-must-not-form-cycles";
    }
}