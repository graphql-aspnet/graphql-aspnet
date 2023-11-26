// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Structural
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Execution;
    using PathConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// A representation of a hierarchical path to a single item tracked (type, field, argument etc.) within a graph schema.
    /// </summary>
    [DebuggerDisplay("{Path}")]
    public partial class ItemPath : IEnumerable<string>
    {
        /// <summary>
        /// Gets a special path used to identify a "root level" fragment that has no path
        /// but is considered valid.
        /// </summary>
        /// <value>The empty.</value>
        public static ItemPath Empty { get; }

        /// <summary>
        /// Initializes static members of the <see cref="ItemPath"/> class.
        /// </summary>
        static ItemPath()
        {
            Empty = new ItemPath(string.Empty);
            Empty._isValid = true;
            Empty._pathInitialized = true;
        }

        private object _lock = new object();
        private bool _pathInitialized;
        private ItemPathRoots _rootCollection;
        private string _path;
        private string _pathMinusCollection;
        private ItemPath _parentField;
        private string _name;
        private bool _isTopLevelField;
        private bool _isValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPath" /> class.
        /// </summary>
        /// <param name="collection">The path root the instance belongs to.</param>
        /// <param name="pathSegments">The individual path segments of the path.</param>
        public ItemPath(ItemPathRoots collection, params string[] pathSegments)
            : this(ItemPath.Join(collection, pathSegments))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPath"/> class.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        public ItemPath(string fullPath)
        {
            this.Raw = fullPath;

            // set an initial unknown state of this object
            _pathInitialized = false;
            _parentField = null;
            _isTopLevelField = false;
            _path = string.Empty;
            _pathMinusCollection = string.Empty;
            _name = string.Empty;
            _isValid = false;
            _rootCollection = ItemPathRoots.Unknown;
        }

        private void EnsurePathInitialized()
        {
            if (_pathInitialized)
                return;

            lock (_lock)
            {
                if (_pathInitialized)
                    return;

                var workingPath = ItemPath.NormalizeFragment(this.Raw);

                // split the path into its fragments
                List<string> pathFragments = workingPath.Split(new[] { PathConstants.PATH_SEPERATOR }, StringSplitOptions.None).ToList();
                switch (pathFragments[0])
                {
                    case PathConstants.QUERY_ROOT:
                        _rootCollection = ItemPathRoots.Query;
                        break;

                    case PathConstants.MUTATION_ROOT:
                        _rootCollection = ItemPathRoots.Mutation;
                        break;

                    case PathConstants.SUBSCRIPTION_ROOT:
                        _rootCollection = ItemPathRoots.Subscription;
                        break;

                    case PathConstants.TYPE_ROOT:
                        _rootCollection = ItemPathRoots.Types;
                        break;

                    case PathConstants.DIRECTIVE_ROOT:
                        _rootCollection = ItemPathRoots.Directives;
                        break;

                    case PathConstants.INTROSPECTION_ROOT:
                        _rootCollection = ItemPathRoots.Introspection;
                        break;

                    case PathConstants.SCHEMA_ROOT:
                        _rootCollection = ItemPathRoots.Schemas;
                        break;
                }

                // ensure each fragment matches the naming specification
                foreach (var fragment in pathFragments.Skip(_rootCollection == ItemPathRoots.Unknown ? 0 : 1))
                {
                    if (!this.ValidateFragment(fragment))
                        return;
                }

                _name = pathFragments[pathFragments.Count - 1];
                if (pathFragments.Count > 1)
                    _parentField = new ItemPath(string.Join(PathConstants.PATH_SEPERATOR, pathFragments.Take(pathFragments.Count - 1)));

                _isTopLevelField = pathFragments.Count == 1 || (pathFragments.Count == 2 && _rootCollection > ItemPathRoots.Unknown); // e.g. "[query]/name"
                _isValid = _name.Length > 0;
                _path = this.GeneratePathString(pathFragments);

                if (_rootCollection == ItemPathRoots.Unknown)
                    _pathMinusCollection = this.GeneratePathString(pathFragments);
                else
                    _pathMinusCollection = $"{PathConstants.PATH_SEPERATOR}{this.GeneratePathString(pathFragments.Skip(1).ToList())}";

                _pathInitialized = true;
            }
        }

        /// <summary>
        /// Generates the fully qualified path string of this instance.
        /// </summary>
        /// <param name="pathFragments">The path fragments to join.</param>
        /// <returns>System.String.</returns>
        protected virtual string GeneratePathString(IReadOnlyList<string> pathFragments)
        {
            return string.Join(PathConstants.PATH_SEPERATOR, pathFragments);
        }

        /// <summary>
        /// Validates that the given path fragment is valid and usable.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        /// <returns>System.Boolean.</returns>
        protected virtual bool ValidateFragment(string fragment)
        {
            return Constants.RegExPatterns.NameRegex.IsMatch(fragment);
        }

        /// <summary>
        /// Determines whether the given path represents the same path as this instance.
        /// </summary>
        /// <param name="otherPath">The other item path to compare against.</param>
        /// <returns><c>true</c> if the paths point to the same location; otherwise, <c>false</c>.</returns>
        public bool IsSamePath(ItemPath otherPath)
        {
            return (otherPath?.Path?.Length ?? 0) > 0 && otherPath.Path == this.Path;
        }

        /// <summary>
        /// Determines whether this instance contains, as a child, the given path.
        /// </summary>
        /// <param name="otherPath">The other item path to compare against.</param>
        /// <returns><c>true</c> if this path contains the supplied path; otherwise, <c>false</c>.</returns>
        public bool HasChildPath(ItemPath otherPath)
        {
            return (otherPath?.Path?.Length ?? 0) > 0 && otherPath.Path.StartsWith(this.Path);
        }

        /// <summary>
        /// Deconstructs the instance into its constituent parts.
        /// </summary>
        /// <param name="pathRoot">The collection this item belongs to.</param>
        /// <param name="path">The path within the collection that points to this item.</param>
        public void Deconstruct(out ItemPathRoots pathRoot, out string path)
        {
            pathRoot = this.Root;
            path = _pathMinusCollection;
        }

        /// <summary>
        /// Gets the raw string provided to this instance.
        /// </summary>
        /// <value>The raw path.</value>
        public string Raw { get; }

        /// <summary>
        /// Gets the path representing this instance.
        /// </summary>
        /// <value>The path.</value>
        public string Path
        {
            get
            {
                this.EnsurePathInitialized();
                return _path;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this path segment is formatting correctly.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid
        {
            get
            {
                this.EnsurePathInitialized();
                return _isValid;
            }
        }

        /// <summary>
        /// Gets the root path collection this path belongs to(e.g. query, mutation, type system etc.).
        /// </summary>
        /// <value>The type of the field.</value>
        public ItemPathRoots Root
        {
            get
            {
                this.EnsurePathInitialized();
                return _rootCollection;
            }
        }

        /// <summary>
        /// Gets this item's parent path, if any.
        /// </summary>
        /// <value>The parent.</value>
        public ItemPath Parent
        {
            get
            {
                this.EnsurePathInitialized();
                return _parentField;
            }
        }

        /// <summary>
        /// Gets the name of this item on the graph.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                this.EnsurePathInitialized();
                return _name;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this path represents a first-level field in one of the
        /// graph collection roots. (e.g. [query]/myField  vs. [query]/topField/myField).
        /// </summary>
        /// <value><c>true</c> if this instance is a top level field; otherwise, <c>false</c>.</value>
        public bool IsTopLevelField
        {
            get
            {
                this.EnsurePathInitialized();
                return _isTopLevelField;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the provided item path root represents an
        /// path on one of the top level graphql operations.
        /// </summary>
        /// <value><c>true</c> if the root value represents
        /// a path on ; otherwise, <c>false</c>.</value>
        public bool IsOperationRoot
        {
            get
            {
                this.EnsurePathInitialized();
                switch (this.Root)
                {
                    case ItemPathRoots.Query:
                    case ItemPathRoots.Mutation:
                    case ItemPathRoots.Subscription:
                        return true;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Path;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection of path fragments.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            var item = this;

            do
            {
                yield return item.Name;
                item = item.Parent;
            }
            while (item != null);
        }

        /// <summary>
        /// Creates a list of all the fully qualified parent paths this path is nested under.
        /// </summary>
        /// <returns>List&lt;ItemPath&gt;.</returns>
        public List<ItemPath> GenerateParentPathSegments()
        {
            if (this.IsTopLevelField || !this.IsValid)
                return new List<ItemPath>();

            var list = this.Parent.GenerateParentPathSegments();
            list.Add(this.Parent);

            return list;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>SchemaItemPath.</returns>
        public ItemPath Clone()
        {
            return new ItemPath(this.Path);
        }

        /// <summary>
        /// Clones this instance to a new path and adds the additional segments
        /// to said path.
        /// </summary>
        /// <param name="pathSegments">The path segments to append
        /// to the current path.</param>
        /// <returns>SchemaItemPath.</returns>
        public virtual ItemPath CreateChild(params string[] pathSegments)
        {
            var list = new List<string>();
            list.Add(this.Path);
            list.AddRange(pathSegments);
            return new ItemPath(ItemPath.Join(list.ToArray()));
        }

        /// <summary>
        /// Clones this instance to a new path and adds the additional segments
        /// to the front of said path making the path a "child" of the provided
        /// segments.
        /// </summary>
        /// <param name="pathSegments">The path segments to prepend
        /// to the current path.</param>
        /// <returns>SchemaItemPath.</returns>
        public ItemPath ReParent(params string[] pathSegments)
        {
            var list = new List<string>();
            list.AddRange(pathSegments);
            list.Add(this.Path);
            return new ItemPath(ItemPath.Join(list.ToArray()));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this.Path?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ItemPath grp)
                return grp.Path == this.Path;

            return false;
        }
    }
}