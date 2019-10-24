// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputItemNodeSteps
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.QueryInputValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Assigns a <see cref="QueryInputArgument"/> to the current node context for the active node.
    /// </summary>
    internal class InputArgument_C_AssignContextQueryInputArgumentForInputObject
        : DocumentConstructionStep<InputItemNode>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentConstructionContext context)
        {
            return base.ShouldExecute(context) && context.ActiveNode.ParentNode?.ParentNode is ComplexValueNode &&
                context.FindContextItem<QueryInputValue>() is QueryComplexInputValue;
        }

        /// <summary>
        /// Executes the construction step the specified node to ensure it is "correct" in the context of the rule doing the valdiation.
        /// </summary>
        /// <param name="context">The validation context encapsulating a <see cref="SyntaxNode" /> that needs to be validated.</param>
        /// <returns><c>true</c> if the node is valid, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentConstructionContext context)
        {
            // at this stage we know thta we've encountered a complexvalue node
            // we can also garuntee that the active input value on the request is going to be the value container
            // for that ComplexvalueNode. It is to that container that we are adding a new argument.
            //
            // take for instance this sequence:
            //
            //      field(arg1: {childArg1: "value"  childArg2: value} )
            //
            // we are pointing at the argument "arg1"
            // which is a complexargument type having a value of QueryComplexInputValue which is a container of arguments
            // it is to that container that we are adding this new argument "childArg1" indicated by this InputItemNode
            // on the context.
            //
            // Note: this operation could be nested N levels deep such as with
            //       field(arg1: { childArg1:  {subChildArg1: value, subChildArg1: value} childArg2: 5} )
            // the scenario would be valid for:
            //      adding childArg1 or childArg2 to arg1
            //      adding subChildArg1 or subChildArg2 to childArg1
            var node = (InputItemNode)context.ActiveNode;
            var inputObject = context.FindContextItem<QueryInputValue>() as QueryComplexInputValue;

            var ownerGraphType = inputObject.OwnerArgument.GraphType as IInputObjectGraphType;
            var field = ownerGraphType.Fields[node.InputName.ToString()];
            var graphType = context.DocumentContext.Schema.KnownTypes.FindGraphType(field);

            var argument = new QueryInputArgument(node, graphType, field.TypeExpression);
            context.AddDocumentPart(argument);
            return true;
        }
    }
}