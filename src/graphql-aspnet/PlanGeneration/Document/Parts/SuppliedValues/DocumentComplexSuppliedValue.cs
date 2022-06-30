// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a complex input object read from a user's query document.
    /// </summary>
    [DebuggerDisplay("ComplexInputValue (Children = {Children.Count})")]
    internal class DocumentComplexSuppliedValue : DocumentSuppliedValue, IComplexSuppliedValueDocumentPart
    {
        private Dictionary<string, IInputArgumentDocumentPart> _arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentComplexSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentComplexSuppliedValue(IDocumentPart parentPart, ComplexValueNode node, string key = null)
            : base(parentPart, node, key)
        {
            _arguments = new Dictionary<string, IInputArgumentDocumentPart>();
        }

        protected override void OnChildPartAdded(IDocumentPart childPart)
        {
            base.OnChildPartAdded(childPart);
            if (childPart is IInputArgumentDocumentPart iia)
            {
                if (!_arguments.ContainsKey(iia.Name))
                    _arguments.Add(iia.Name, iia);
            }
        }

        /// <inheritdoc />
        public bool TryGetField(string fieldName, out IResolvableValueItem foundField)
        {
            foundField = default;
            if(this.TryGetArgument(fieldName, out var arg))
            {
                foundField = arg.Value;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool TryGetArgument(string fieldName, out IInputArgumentDocumentPart foundArgument)
        {
            return _arguments.TryGetValue(fieldName, out foundArgument);
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            if (value == null || !(value is IComplexSuppliedValueDocumentPart))
                return false;

            var otherComplexValue = value as IComplexSuppliedValueDocumentPart;
            foreach (var argument in _arguments.Values)
            {
                if (!otherComplexValue.TryGetArgument(argument.Name, out var otherArg))
                    return false;

                if (!argument.Value.IsEqualTo(otherArg.Value))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool ContainsArgument(string argumentName)
        {
            if (argumentName == null)
                return false;

            return _arguments.ContainsKey(argumentName);
        }

    }
}