// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.InputValueNodeSteps
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentConstruction.Common;

    /// <summary>
    /// Assigns the active input value as a child of the active argument on the context.
    /// </summary>
    internal class InputValue_AssignValueToArgumentOrValue
        : DocumentConstructionStep<InputValueNode>
    {
        /// <inheritdoc />
        public override bool Execute(DocumentConstructionContext context)
        {
            // for whatever the active argument in context is
            // it could be an argument on a field, on a directive or
            // even an argument that is part of a nested input object inside a field argument set
            // add a container to hold the active value within said object
            var node = (InputValueNode)context.ActiveNode;

            // if this is directly attached to an input (i.e. not part of a list)
            // grab the name of the argument its attached to
            // and assign it as a key identifying this value for easier future reference
            string keyValue = null;
            if (context.ParentPart is IInputArgumentDocumentPart argumentPart)
            {
                keyValue = argumentPart.Name;
                if (string.IsNullOrWhiteSpace(keyValue))
                    keyValue = null;
            }

            var suppliedValue = DocumentSuppliedValueFactory.CreateInputValue(context.ParentPart, node, keyValue);

            // the graph type of the supplied value is that of its owning argument
            suppliedValue.AssignGraphType(context.ParentPart.GraphType);

            context.AssignPart(suppliedValue);
            return true;
        }
    }
}