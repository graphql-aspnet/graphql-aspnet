// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A collection of fields (from a <see cref="IGraphType"/>) that are requested by a user and defined
    /// on their query document. Selected fields are keyed by the return value (a.k.a. the field alias) requested by the user.
    /// </summary>
    [DebuggerDisplay("FIELD SET: Graph Type: {GraphType.Name}")]
    internal class DocumentFieldSelectionSet : DocumentPartBase, IFieldSelectionSetDocumentPart, IDocumentPart
    {
        private Dictionary<string, List<IFieldDocumentPart>> _fieldsByAlias = null;
        private List<IFieldDocumentPart> _allResolvedFields = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldSelectionSet" /> class.
        /// </summary>
        /// <param name="parent">The parent document part that owns this set.</param>
        public DocumentFieldSelectionSet(IDocumentPart parent)
            : base(parent, EmptyNode.Instance)
        {
            this.AssignGraphType(parent.GraphType);
        }

        private void EnsureExecutableFieldSet()
        {
            if (_allResolvedFields != null)
                return;

            _allResolvedFields = new List<IFieldDocumentPart>();
            _fieldsByAlias = new Dictionary<string, List<IFieldDocumentPart>>();

            this.GatherFieldSet(this, new HashSet<INamedFragmentDocumentPart>());
        }

        private void GatherFieldSet(
            IFieldSelectionSetDocumentPart fieldSet,
            HashSet<INamedFragmentDocumentPart> walkedNamedFrags)
        {
            if (fieldSet == null)
                return;

            foreach (var docPart in fieldSet.Children)
            {
                if (docPart is IFieldDocumentPart fd)
                {
                    _allResolvedFields.Add(fd);
                    this.AddFieldAlias(fd);
                }
                else if (docPart is IInlineFragmentDocumentPart inlineFrag)
                {
                    this.GatherFieldSet(inlineFrag.FieldSelectionSet, walkedNamedFrags);
                }
                else if (docPart is IFragmentSpreadDocumentPart fragSpread && fragSpread.Fragment != null)
                {
                    if (!walkedNamedFrags.Contains(fragSpread.Fragment))
                    {
                        // its possible, though not legal, for named fragment spreads to form cycles
                        // if this occurs stop when a cycle is found (the document is in error and will be
                        // failed, no need to continue gathering top level fields)
                        walkedNamedFrags.Add(fragSpread.Fragment);
                        this.GatherFieldSet(fragSpread.Fragment.FieldSelectionSet, walkedNamedFrags);
                        walkedNamedFrags.Remove(fragSpread.Fragment);
                    }
                }
            }
        }

        private void AddFieldAlias(IFieldDocumentPart fieldPart)
        {
            if (!_fieldsByAlias.ContainsKey(fieldPart.Alias.ToString()))
                _fieldsByAlias.Add(fieldPart.Alias.ToString(), new List<IFieldDocumentPart>());

            _fieldsByAlias[fieldPart.Alias.ToString()].Add(fieldPart);
        }

        /// <inheritdoc />
        public IReadOnlyList<IFieldDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias)
        {
            this.EnsureExecutableFieldSet();
            if (_fieldsByAlias.ContainsKey(alias.ToString()))
                return _fieldsByAlias[alias.ToString()];

            return new List<IFieldDocumentPart>();
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FieldSelectionSet;

        /// <inheritdoc />
        public IReadOnlyList<IFieldDocumentPart> ExecutableFields
        {
            get
            {
                this.EnsureExecutableFieldSet();
                return _allResolvedFields;
            }
        }
    }
}