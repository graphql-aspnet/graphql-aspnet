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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using RouteConstants = GraphQL.AspNet.Constants.Routing;

    /// <summary>
    /// A representation of a hierarchical path to a single field in within a graph schema.
    /// </summary>
    [DebuggerDisplay("{Path}")]
    public class GraphFieldPath : IEnumerable<string>
    {
        /// <summary>
        /// Gets a special route path used to identify a "root level" fragment that has no path
        /// but is considered valid.
        /// </summary>
        /// <value>The empty.</value>
        public static GraphFieldPath Empty { get; }

        /// <summary>
        /// Initializes static members of the <see cref="GraphFieldPath"/> class.
        /// </summary>
        static GraphFieldPath()
        {
            Empty = new GraphFieldPath(string.Empty);
            Empty.IsValid = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldPath" /> class.
        /// </summary>
        /// <param name="collection">The collection the route belongs to.</param>
        /// <param name="pathSegments">The individual path segments of the route.</param>
        public GraphFieldPath(GraphCollection collection, params string[] pathSegments)
            : this(GraphFieldPath.Join(collection, pathSegments))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldPath"/> class.
        /// </summary>
        /// <param name="fullPath">The full path.</param>
        public GraphFieldPath(string fullPath)
        {
            this.Raw = fullPath;

            // set an initial unknown state of this object
            this.IsValid = false;
            this.Path = string.Empty;
            this.Name = string.Empty;
            this.RootCollection = GraphCollection.Unknown;

            var workingPath = GraphFieldPath.NormalizeFragment(this.Raw);

            // split the path into its fragments
            List<string> pathFragments = workingPath.Split(new[] { RouteConstants.PATH_SEPERATOR }, StringSplitOptions.None).ToList();

            switch (pathFragments[0])
            {
                case RouteConstants.QUERY_ROOT:
                    this.RootCollection = GraphCollection.Query;
                    break;
                case RouteConstants.MUTATION_ROOT:
                    this.RootCollection = GraphCollection.Mutation;
                    break;
                case RouteConstants.TYPE_ROOT:
                    this.RootCollection = GraphCollection.Types;
                    break;
                case RouteConstants.ENUM_ROOT:
                    this.RootCollection = GraphCollection.Enums;
                    break;
                case RouteConstants.DIRECTIVE_ROOT:
                    this.RootCollection = GraphCollection.Directives;
                    break;
                case RouteConstants.SUBSCRIPTION_ROOT:
                    this.RootCollection = GraphCollection.Subscription;
                    break;
            }

            // ensure each fragment matches the naming specification
            foreach (var fragment in pathFragments.Skip(this.RootCollection == GraphCollection.Unknown ? 0 : 1))
            {
                if (!this.ValidateFragment(fragment))
                    return;
            }

            this.Name = pathFragments[pathFragments.Count - 1];
            if (pathFragments.Count > 1)
                this.Parent = new GraphFieldPath(string.Join(RouteConstants.PATH_SEPERATOR, pathFragments.Take(pathFragments.Count - 1)));

            this.IsTopLevelField = pathFragments.Count == 1 || (pathFragments.Count == 2 && this.RootCollection > GraphCollection.Unknown); // e.g. "[query]/name"
            this.IsValid = this.Name.Length > 0;
            this.Path = this.GeneratePathString(pathFragments);
        }

        /// <summary>
        /// Generates the fully qualified path string of this instance.
        /// </summary>
        /// <param name="pathFragments">The path fragments to join.</param>
        /// <returns>System.String.</returns>
        protected virtual string GeneratePathString(IReadOnlyList<string> pathFragments)
        {
            return string.Join(RouteConstants.PATH_SEPERATOR, pathFragments);
        }

        /// <summary>
        /// Validates that the given route fragment is valid and usable.
        /// </summary>
        /// <param name="fragment">The fragment.</param>
        /// <returns>System.Boolean.</returns>
        protected virtual bool ValidateFragment(string fragment)
        {
            return Constants.RegExPatterns.NameRegex.IsMatch(fragment);
        }

        /// <summary>
        /// Determines whether the given route represents the same path as this route.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns><c>true</c> if the routes point to the same location; otherwise, <c>false</c>.</returns>
        public bool IsSameRoute(GraphFieldPath route)
        {
            return (route?.Path?.Length ?? 0) > 0 && route.Path == this.Path;
        }

        /// <summary>
        /// Determines whether this instance contains, as a child, the given path.
        /// </summary>
        /// <param name="route">The path.</param>
        /// <returns><c>true</c> if this path contains the supplied path; otherwise, <c>false</c>.</returns>
        public bool HasChildRoute(GraphFieldPath route)
        {
            return (route?.Path?.Length ?? 0) > 0 && route.Path.StartsWith(this.Path);
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
        public string Path { get; }

        /// <summary>
        /// Gets a value indicating whether this path segment is formatting correctly.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Gets the root collection represented by this route represents (e.g. query, mutation, type system etc.).
        /// </summary>
        /// <value>The type of the field.</value>
        public GraphCollection RootCollection { get; }

        /// <summary>
        /// Gets this item's parent path, if any.
        /// </summary>
        /// <value>The parent.</value>
        public GraphFieldPath Parent { get; }

        /// <summary>
        /// Gets the name of this item on the graph.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether this path represents a first-level field in one of the
        /// graph collection roots. (e.g. [query]/myField  vs. [query]/topField/myField).
        /// </summary>
        /// <value><c>true</c> if this instance is a top level field; otherwise, <c>false</c>.</value>
        public bool IsTopLevelField { get; }

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
        /// Normalizes a given route path fragement removing duplicate seperators, ensuring starting and tail end seperators
        /// are correct etc.
        /// </summary>
        /// <param name="routefragment">The fragment to normalize.</param>
        /// <returns>System.String.</returns>
        public static string NormalizeFragment(string routefragment)
        {
            // ensure a working path
            routefragment = routefragment?.Trim() ?? string.Empty;
            routefragment = routefragment.Replace(RouteConstants.ALT_PATH_SEPERATOR, RouteConstants.PATH_SEPERATOR);

            // doubled up seperators may happen if a 3rd party is joining route fragments (especially if the seperator is a '/')
            // trim them down
            while (routefragment.Contains(RouteConstants.DOUBLE_PATH_SEPERATOR))
            {
                routefragment = routefragment.Replace(RouteConstants.DOUBLE_PATH_SEPERATOR, RouteConstants.PATH_SEPERATOR);
            }

            // if the path ends with or starts with a seperator (indicating a potential group segment)
            // thats fine but is not needed to identify the segment in and of itself, trim it off
            while (routefragment.EndsWith(RouteConstants.PATH_SEPERATOR))
            {
                routefragment = routefragment.Substring(0, routefragment.Length - RouteConstants.PATH_SEPERATOR.Length);
            }

            while (routefragment.StartsWith(RouteConstants.PATH_SEPERATOR))
            {
                routefragment = routefragment.Substring(routefragment.IndexOf(RouteConstants.PATH_SEPERATOR, StringComparison.Ordinal) + RouteConstants.PATH_SEPERATOR.Length);
            }

            return routefragment;
        }

        /// <summary>
        /// Creates a list of all the fully qualified parent paths this path is nested under.
        /// </summary>
        /// <returns>List&lt;GraphRoutePath&gt;.</returns>
        public List<GraphFieldPath> GenerateParentPathSegments()
        {
            if (this.IsTopLevelField || !this.IsValid)
                return new List<GraphFieldPath>();

            var list = this.Parent.GenerateParentPathSegments();
            list.Add(this.Parent);

            return list;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>GraphFieldPath.</returns>
        public GraphFieldPath Clone()
        {
            return new GraphFieldPath(this.Path);
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
            if (obj is GraphFieldPath grp)
                return grp.Path == this.Path;

            return false;
        }

        /// <summary>
        /// Joins a parent and child route segments under the top level field type provided.
        /// </summary>
        /// <param name="routeSegments">The route segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(params string[] routeSegments)
        {
            var fragment = string.Join(RouteConstants.PATH_SEPERATOR, routeSegments);
            return GraphFieldPath.NormalizeFragment(fragment);
        }

        /// <summary>
        /// Joins a parent and child route segments under the top level field type provided.
        /// </summary>
        /// <param name="fieldType">Type of the field to prepend a root key to the path.</param>
        /// <param name="routeSegments">The route segments to join.</param>
        /// <returns>System.String.</returns>
        public static string Join(GraphCollection fieldType, params string[] routeSegments)
        {
            return GraphFieldPath.Join(fieldType.ToRouteRoot().AsEnumerable().Concat(routeSegments).ToArray());
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GraphFieldPath left, GraphFieldPath right)
        {
            return left?.Equals(right) ?? right?.Equals(left) ?? true;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="left">The left side operand.</param>
        /// <param name="right">The right side operand.</param>
        /// <returns>The result of the operation.</returns>
        public static bool operator !=(GraphFieldPath left, GraphFieldPath right)
        {
            return !(left == right);
        }
    }
}