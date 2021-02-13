// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.FragmentSpreadNodeSteps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Ensures that whereever a named fragment is spread that it is not, in the current path,
    /// nested inside itself forming an infinite resolution loop.
    /// </summary>
    internal class Rule_5_5_2_2_SpreadingANamedFragmentMustNotFormCycles : DocumentConstructionRuleStep<FragmentSpreadNode>
    {
        /// <summary>
        /// Validates the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            var pointer = (FragmentSpreadNode)context.ActiveNode;

            var fragmentPath = new Stack<ReadOnlyMemory<char>>();
            var formsACycle = this.DoesNodeFormAFragmentCycle(pointer, context.DocumentContext.Fragments, fragmentPath);
            if (formsACycle)
            {
                var list = fragmentPath.Select(x => $"'{x.ToString()}").ToList();
                list.Reverse();

                var cyclePath = string.Join(" -> ", list);
                this.ValidationError(
                    context,
                    $"Spreading named fragment '{pointer.PointsToFragmentName.ToString()}' forms an infinite cycle of " +
                    $"named fragment spreads. Recursive Fragment Path: {cyclePath}.");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Inspects the supplied node to see if its a named fragment pointer and walks that named fragment
        /// keeping track of the named fragments that are being traversed looking for any cyclic references that would
        /// result in an infinite loop of resolutions. If the node is not a fragment pointer, the fragment's children
        /// are traversed looking for additional pointers.
        /// </summary>
        /// <param name="node">The current node to inspect.</param>
        /// <param name="allKnownNamedFragments">A list of all known named fragments that can be looked for.</param>
        /// <param name="fragmentsUsed">A stack inidcating the path within the node tree being inspected of all
        /// the named fragments that are/have been traversed.</param>
        /// <returns><c>True</c> if a cyclic path was detected; otherwise, false.</returns>
        private bool DoesNodeFormAFragmentCycle(
            SyntaxNode node,
            QueryFragmentCollection allKnownNamedFragments,
            Stack<ReadOnlyMemory<char>> fragmentsUsed)
        {
            if (node is FragmentSpreadNode fsn)
            {
                // check to see if the current pointed at named fragment
                // is already in the built up path
                var fragmentAlreadyReferenced = this.IsInPath(fsn.PointsToFragmentName, fragmentsUsed);

                // push where we are to the chain to indicate this pointer is being traversed
                fragmentsUsed.Push(fsn.PointsToFragmentName);

                // when the pointed at named fragment was already seen once we have formed a
                // cycle and can't continue
                // the framgent path will now contain something like: Frag1 -> Frag2 -> Frag3 -> Frag2
                // which can be put into an error message.
                if (fragmentAlreadyReferenced)
                    return true;

                // fetch the named fragment and walk its node chain to see if any
                // cycles are formed given the current context.
                // i.e. if the named fragment references any other named fragments walk through them looking
                //      for cycles.
                var namedFragment = allKnownNamedFragments.FindFragment(fsn.PointsToFragmentName.ToString());
                if (namedFragment != null)
                {
                    if (this.DoesNodeFormAFragmentCycle(namedFragment.Node, allKnownNamedFragments, fragmentsUsed))
                        return true;
                }

                // back out of the current named fragment pointer
                fragmentsUsed.Pop();
            }
            else if (node.Children != null)
            {
                // when not pointed at a named fragment
                // inspect every child to see if the current node
                // ever references one deeper in its node chain
                foreach (var childNode in node.Children)
                {
                    if (this.DoesNodeFormAFragmentCycle(childNode, allKnownNamedFragments, fragmentsUsed))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Inspects the stack to determine i the given fragment name already exists within it.
        /// </summary>
        /// <param name="fragmentName">The name of the Named Fragment to search fro.</param>
        /// <param name="fragmentPath">The current named framgnet path in the current scope.</param>
        /// <returns><c>True</c> if the fragment name does exist in the stack; otherwise false.</returns>
        private bool IsInPath(ReadOnlyMemory<char> fragmentName, Stack<ReadOnlyMemory<char>> fragmentPath)
        {
            // the stack shouldnt usually be more than 3-5 elements deep
            // it only contains the named fragment names.
            var span = fragmentName.Span;
            foreach (var item in fragmentPath)
            {
                if (item.Span.SequenceEqual(span))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.5.2.2";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Fragment-spreads-must-not-form-cycles";
    }
}