// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts.SuppliedValues
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;

    /// <summary>
    /// An input value representing a complex input object read from a user's query document.
    /// </summary>
    [DebuggerDisplay("ComplexInputValue (Children = {Children.Count})")]
    internal class DocumentComplexSuppliedValue : DocumentSuppliedValue, IComplexSuppliedValueDocumentPart, IDescendentDocumentPartSubscriber
    {
        private readonly DocumentInputObjectFieldCollection _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentComplexSuppliedValue"/> class.
        /// </summary>
        /// <param name="parentPart">The document part that owns this input field.</param>
        /// <param name="location">The location in the source text where this field originated.</param>
        /// <param name="key">A unique key assigned to this instance. Used by various look up
        /// functions during execution. Typically this key is the input argument name.</param>
        public DocumentComplexSuppliedValue(
            IDocumentPart parentPart,
            SourceLocation location,
            string key = null)
            : base(parentPart, location, key)
        {
            _fields = new DocumentInputObjectFieldCollection(this);
        }

        /// <inheritdoc cref="IDescendentDocumentPartSubscriber.OnDescendentPartAdded" />
        void IDescendentDocumentPartSubscriber.OnDescendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            if (decendentPart.Parent == this && decendentPart is IInputObjectFieldDocumentPart iia)
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