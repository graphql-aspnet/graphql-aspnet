// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Source
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A logical represention of a path through a set of fields, using field names and array index numbers
    /// to assist in locating the source of their error in a query.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{ToDotString()}")]
    public class SourcePath : IReadOnlyList<object>
    {
        /// <summary>
        /// Gets a singlton instance representing no path.
        /// </summary>
        /// <value>The none.</value>
        public static SourcePath None { get; } = new SourcePath();

        private readonly List<object> _segments;

        /// <summary>
        /// Initializes static members of the <see cref="SourcePath"/> class.
        /// </summary>
        static SourcePath()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourcePath" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of path items to allocate.</param>
        public SourcePath(int? capacity = null)
        {
            if (capacity.HasValue)
                _segments = new List<object>(capacity.Value);
            else
                _segments = new List<object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourcePath" /> class.
        /// </summary>
        /// <param name="capacity">The initial capacity of path items to allocate.</param>
        /// <param name="pathItems">The items to add to the initial path.</param>
        private SourcePath(int capacity, IEnumerable<object> pathItems)
            : this(capacity)
        {
            _segments.AddRange(pathItems);
        }

        /// <summary>
        /// Converts this instance into its equivilant origin item.
        /// </summary>
        /// <returns>SourceOrigin.</returns>
        public SourceOrigin AsOrigin()
        {
            return new SourceOrigin(this);
        }

        /// <summary>
        /// <para>Adds the name of the current field to the path. Note: This should be the field alias if the
        /// user supplied one.</para>
        /// <para>Spec: <see href="https://graphql.github.io/graphql-spec/October2021/#sec-Errors"/>.</para>
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        public void AddFieldName(string fieldName)
        {
            _segments.Add(fieldName);
        }

        /// <summary>
        /// <para>Adds the index of the current array being iterated to the path.</para>
        /// <para>Spec: <see href="https://graphql.github.io/graphql-spec/October2021/#sec-Errors"/> .</para>
        /// </summary>
        /// <param name="index">The current index.</param>
        public void AddArrayIndex(int index)
        {
            _segments.Add(index);
        }

        /// <summary>
        /// Converts this path into mixed array of field names and array index values.
        /// </summary>
        /// <returns>System.Object[].</returns>
        public object[] ToArray()
        {
            return _segments.ToArray();
        }

        /// <summary>
        /// Creates a deep clone of this path item.
        /// </summary>
        /// <returns>SourcePath.</returns>
        public SourcePath Clone()
        {
            return new SourcePath(_segments.Count, _segments);
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _segments.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is an index item of a field or is just a
        /// plain old field itself.
        /// </summary>
        /// <value><c>true</c> if this instance is indexed item; otherwise, <c>false</c>.</value>
        public bool IsIndexedItem => _segments.Count > 0 && _segments.Last() is int;

        /// <summary>
        /// Creats a new path instance representing the parent of this item.
        /// </summary>
        /// <returns>SourcePath.</returns>
        public SourcePath MakeParent()
        {
            if (_segments == null || _segments.Count == 0)
                return new SourcePath();

            // remove any indexers to walk back to the last entry
            // if the item is currently pointing at an int back up to the non-indexed reference
            var parentIndex = _segments.Count - 2;
            while (parentIndex >= 0 && _segments[parentIndex] is int)
                parentIndex--;

            if (parentIndex < 0)
                return new SourcePath();

            return new SourcePath(parentIndex + 1, _segments.Take(parentIndex + 1));
        }

        /// <summary>
        /// Gets the <see cref="object" /> with the specified error.
        /// </summary>
        /// <param name="index">The index of the segment to retrieve.</param>
        /// <returns>System.Object.</returns>
        public object this[int index] => _segments[index];

        /// <inheritdoc />
        public IEnumerator<object> GetEnumerator()
        {
            return _segments.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToDotString();
        }

        /// <summary>
        /// Creates a string of this path in a json serialized array format. (e.g. ["Path0", "Path1", 0, "Path2"]).
        /// </summary>
        /// <returns>System.String.</returns>
        public string ToArrayString()
        {
            var builder = new StringBuilder();
            builder.Append("[");

            for (var i = 0; i < _segments.Count; i++)
            {
                if (_segments[i] is string str)
                {
                    builder.Append($"\"{str}\"");
                }
                else
                {
                    builder.Append($"{_segments[i]}");
                }

                if (i < _segments.Count - 1)
                    builder.Append(", ");
            }

            builder.Append("]");
            return builder.ToString();
        }

        /// <summary>
        /// Create a string of this path using a dot notation. (e.g. "Path0.Path1[0].Path2").
        /// </summary>
        /// <returns>System.String.</returns>
        public string ToDotString()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < _segments.Count; i++)
            {
                if (_segments[i] is string str)
                {
                    builder.Append(str);
                }
                else if (_segments[i] is int index)
                {
                    builder.Append($"[{index}]");
                }

                if (i < _segments.Count - 1 && _segments[i + 1] is string)
                    builder.Append(".");
            }

            return builder.ToString();
        }
    }
}