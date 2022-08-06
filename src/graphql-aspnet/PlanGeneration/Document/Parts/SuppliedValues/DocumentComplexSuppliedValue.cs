﻿// *************************************************************
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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values;

    /// <summary>
    /// An input value representing a complex input object read from a user's query document.
    /// </summary>
    [DebuggerDisplay("ComplexInputValue (Children = {Children.Count})")]
    internal class DocumentComplexSuppliedValue : DocumentSuppliedValue, IComplexSuppliedValueDocumentPart
    {
        private readonly DocumentInputObjectFieldCollection _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentComplexSuppliedValue" /> class.
        /// </summary>
        /// <param name="parentPart">The parent document part, if any, that owns this instance.</param>
        /// <param name="node">The node that represents this input value in the user query document.</param>
        /// <param name="key">An optional key indicating the name of this supplied value, if one was given.</param>
        public DocumentComplexSuppliedValue(IDocumentPart parentPart, ComplexValueNode node, string key = null)
            : base(parentPart, node, key)
        {
            _fields = new DocumentInputObjectFieldCollection(this);
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            if (relativeDepth == 1 && childPart is IInputObjectFieldDocumentPart iia)
            {
                if (!_fields.ContainsKey(iia.Name))
                    _fields.AddField(iia);
            }
        }

        /// <inheritdoc />
        bool IResolvableFieldSet.TryGetField(string fieldName, out IResolvableValueItem foundField)
        {
            foundField = default;
            if (this.TryGetField(fieldName, out IInputObjectFieldDocumentPart field))
            {
                foundField = field.Value;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool TryGetField(string fieldName, out IInputObjectFieldDocumentPart foundArgument)
        {
            return _fields.TryGetValue(fieldName, out foundArgument);
        }

        /// <inheritdoc />
        public override bool IsEqualTo(ISuppliedValueDocumentPart value)
        {
            if (value == null || !(value is IComplexSuppliedValueDocumentPart))
                return false;

            var otherComplexValue = value as IComplexSuppliedValueDocumentPart;
            foreach (var argument in _fields.Values)
            {
                if (!otherComplexValue.TryGetField(argument.Name, out var otherArg))
                    return false;

                if (!argument.Value.IsEqualTo(otherArg.Value))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool ContainsField(string argumentName)
        {
            if (argumentName == null)
                return false;

            return _fields.ContainsKey(argumentName);
        }

        /// <inheritdoc />
        public IInputObjectFieldCollectionDocumentPart Fields => _fields;

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IResolvableValueItem>> ResolvableFields
        {
            get
            {
                foreach (var kvp in _fields)
                    yield return new KeyValuePair<string, IResolvableValueItem>(kvp.Key, kvp.Value.Value);
            }
        }

        /// <inheritdoc />
        public override string Description => "Complex Input Value";
    }
}