// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// An execution set that manages the actual fields to be resolved within
    /// a given field selection set. This includes fields directly included as well as those
    /// that would be incorporated via a fragment spread or inline fragment.
    /// </summary>
    internal class ExecutionFieldSet : IExecutableFieldSelectionSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionFieldSet"/> class.
        /// </summary>
        /// <param name="owner">The master field selection set on which this instance
        /// is based.</param>
        public ExecutionFieldSet(IFieldSelectionSetDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
        }

        /// <inheritdoc />
        public IEnumerable<IFieldDocumentPart> FilterByAlias(ReadOnlyMemory<char> alias)
        {
            var enumerator = new ExecutableFieldSetEnumerator(this.Owner);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Alias.Span.SequenceEqual(alias.Span))
                    yield return enumerator.Current;
            }
        }

        /// <inheritdoc />
        public IEnumerator<IFieldDocumentPart> GetEnumerator()
        {
            return new ExecutableFieldSetEnumerator(this.Owner);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IFieldDocumentPart this[int index]
        {
            get
            {
                var enumerator = this.GetEnumerator();

                var i = 0;
                while (enumerator.MoveNext())
                {
                    if (i++ == index)
                        return enumerator.Current;
                }

                throw new IndexOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public IEnumerable<IFieldDocumentPart> IncludedOnly
        {
            get
            {
                var enumerator = new ExecutableFieldSetEnumerator(this.Owner, true);
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }

        /// <inheritdoc />
        public IFieldSelectionSetDocumentPart Owner { get; }
    }
}