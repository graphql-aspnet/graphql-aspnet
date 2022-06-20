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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Ensures that when a named fragment targeting an object graph type is spread within a field context
    /// of a union or interface graph type that the named fragment CAN be spread into the given object.
    /// </summary>
    internal class Rule_5_5_2_3_3_ObjectFragmentSpreadInAbstractCanSpreadInContext
        : RuleBase_5_5_2_3_FragmentCanSpreadInContext
    {
        /// <summary>
        /// Determines if the target graph type COULD BE spread into the active context graph type.
        /// </summary>
        /// <param name="schema">The target schema in case any additional graph types need to be accessed.</param>
        /// <param name="typeInContext">The graph type currently active on the context.</param>
        /// <param name="targetGraphType">The target graph type of the spread named fragment.</param>
        /// <returns><c>true</c> if the target type can be spread in context; otherwise, false.</returns>
        protected override bool CanAcceptGraphType(ISchema schema, IGraphType typeInContext, IGraphType targetGraphType)
        {
            // when spreading an objet into an interface the object must implement the interface
            // when spreading an object into a union, the object must be a member of the union
            var typeSet = schema.KnownTypes.ExpandAbstractType(typeInContext);
            return typeSet.Contains(targetGraphType);
        }

        /// <summary>
        /// Gets the set of type kinds for the pointed at named fragment
        /// that this rule can validate for.
        /// </summary>
        /// <value>A list of type kinds.</value>
        protected override HashSet<TypeKind> AllowedTargetGraphTypeKinds { get; }
            = new HashSet<TypeKind> { TypeKind.OBJECT };

        /// <summary>
        /// Gets the set of type kinds for the "in context" graph type
        /// that this rule can validate for.
        /// </summary>
        /// <value>A list of type kinds.</value>
        protected override HashSet<TypeKind> AllowedContextGraphTypeKinds { get; }
            = new HashSet<TypeKind> { TypeKind.UNION, TypeKind.INTERFACE };

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.5.2.3.3";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Object-Spreads-In-Abstract-Scope";
    }
}