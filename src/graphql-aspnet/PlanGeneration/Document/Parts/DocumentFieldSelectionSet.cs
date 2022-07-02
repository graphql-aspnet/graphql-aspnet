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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A collection of fields (from a <see cref="IGraphType"/>) that are requested by a user and defined
    /// on their query document. Selected fields are keyed by the return value (a.k.a. the field alias) requested by the user.
    /// </summary>
    [DebuggerDisplay("FIELD SET: Graph Type: {GraphType.Name}, Fields = {Count}")]
    internal class DocumentFieldSelectionSet : DocumentPartBase, IFieldSelectionSetDocumentPart, IDocumentPart
    {
        private readonly Dictionary<ReadOnlyMemory<char>, List<IFieldDocumentPart>> _fieldsByAlias;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldSelectionSet" /> class.
        /// </summary>
        /// <param name="parent">The parent document part that owns this set.</param>
        public DocumentFieldSelectionSet(IDocumentPart parent)
            : base(parent, EmptyNode.Instance)
        {
            _fieldsByAlias = new Dictionary<ReadOnlyMemory<char>, List<IFieldDocumentPart>>(new MemoryOfCharComparer());
            this.AssignGraphType(parent.GraphType);
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart, int relativeDepth)
        {
            if (relativeDepth == 1)
            {
                // for any direct children of this selection set:
                //      * fields
                //      * top level fields of inline fragments
                //      * top level fields of named framgents
                // make sure to add their aliases to the set of known aliases
                // at this level
                if (childPart is IFieldDocumentPart fds)
                {
                    if (!_fieldsByAlias.ContainsKey(fds.Alias))
                        _fieldsByAlias.Add(fds.Alias, new List<IFieldDocumentPart>());

                    _fieldsByAlias[fds.Alias].Add(fds);
                }
                else if (childPart is IFragmentDocumentPart fragPart)
                {
                    foreach (var child in childPart.Children)
                        this.OnChildPartAdded(child, relativeDepth);
                }
                else if (childPart is IFragmentSpreadDocumentPart fragSpread)
                {
                    this.OnChildPartAdded(fragSpread.Fragment, relativeDepth);
                    fragSpread.NamedFragmentAssigned += (o, e) => this.OnChildPartAdded(e.TargetDocumentPart, e.RelativeDepth);
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<IFieldDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias)
        {
            var list = new List<IFieldDocumentPart>();
            if (_fieldsByAlias.ContainsKey(alias))
                list.AddRange(_fieldsByAlias[alias]);

            return list;
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FieldSelectionSet;

        /// <inheritdoc />
        public int Count => this.Children[DocumentPartType.Field]
                .OfType<IFieldDocumentPart>()
                .Count();

        public IFieldDocumentPart this[int index]
            => this.Children[DocumentPartType.Field][index] as IFieldDocumentPart;

        /// <inheritdoc />
        public IEnumerator<IFieldDocumentPart> GetEnumerator()
        {
            return this.Children[DocumentPartType.Field]
                .OfType<IFieldDocumentPart>()
                .GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}