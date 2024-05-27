// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem.Introspection.Fields;

    /// <summary>
    /// A graph type used to expose an abstract item, not tied to a physical object, as an object type on the graph.
    /// Used as a mechanism to convert virtual paths in <see cref="GraphRouteAttribute"/> declarations into fields on the object graph.
    /// </summary>
    [DebuggerDisplay("OBJECT (virtual) {Name}")]
    public class VirtualObjectGraphType : ObjectGraphTypeBase, IObjectGraphType, IInternalSchemaItem
    {
        // Implementation Note:
        //
        // This object represents the "binder" between controllers, actions and the type system.
        // The controller is an abstract concept, not tied to any "piece of data" (like a real "Person" object would be)
        // so this virtual graph type is used to expose the controller's action methods as though they were fields on a class
        // to allow for proper navigation of an object structure in graphql. This object is generated dynamically from the parsed
        // metadata of a controller.

        /// <summary>
        /// Constructs a virtual type from the path template extracted from a controller action method.
        /// </summary>
        /// <param name="pathTemplate">The path template base this virtual type off of.</param>
        /// <returns>VirtualObjectGraphType.</returns>
        public static VirtualObjectGraphType FromControllerFieldPathTemplate(ItemPath pathTemplate)
        {
            var tempName = MakeSafeTypeNameFromItemPath(pathTemplate);
            return new VirtualObjectGraphType(tempName, pathTemplate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualObjectGraphType" /> class.
        /// </summary>
        /// <param name="typeName">The formal name to assign to the type.</param>
        /// <param name="pathTemplate">The path template that generated this virutal graph type.</param>
        private VirtualObjectGraphType(string typeName, ItemPath pathTemplate)
         : base(
               typeName,
               $"{nameof(VirtualObjectGraphType)}_{typeName}",
               new ItemPath(ItemPathRoots.Types, typeName))
        {
            this.ItemPathTemplate = Validation.ThrowIfNullOrReturn(pathTemplate, nameof(pathTemplate));

            // add the __typename as a field for this virtual object
            this.Extend(new Introspection_TypeNameMetaField(typeName));
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            return true;
        }

        /// <inheritdoc />
        public override IGraphType Clone(string typeName = null)
        {
            typeName = typeName?.Trim() ?? this.Name;
            return new VirtualObjectGraphType(
                typeName,
                this.ItemPathTemplate);
        }

        /// <inheritdoc />
        public override bool IsVirtual => true;

        /// <summary>
        /// Gets the raw path template that was used to define this virtual type.
        /// </summary>
        /// <value>The item path template.</value>
        public ItemPath ItemPathTemplate { get; private set; }

        /// <inheritdoc />
        public Type ObjectType => typeof(VirtualObjectGraphType);

        /// <summary>
        /// Converts a route path into a unique graph type, removing special control characters
        /// but retaining its uniqueness.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <param name="segmentNameFormatter">An optional formatter that will apply
        /// special casing to a path segment before its added to the name.</param>
        /// <returns>System.String.</returns>
        public static string MakeSafeTypeNameFromItemPath(
            ItemPath path,
            Func<string, string> segmentNameFormatter = null)
        {
            Validation.ThrowIfNull(path, nameof(path));

            var segments = new List<string>();
            foreach (var pathSegmentName in path)
            {
                switch (pathSegmentName)
                {
                    case Constants.Routing.QUERY_ROOT:
                        segments.Add(Constants.ReservedNames.QUERY_TYPE_NAME);
                        break;

                    case Constants.Routing.MUTATION_ROOT:
                        segments.Add(Constants.ReservedNames.MUTATION_TYPE_NAME);
                        break;

                    case Constants.Routing.SUBSCRIPTION_ROOT:
                        segments.Add(Constants.ReservedNames.SUBSCRIPTION_TYPE_NAME);
                        break;

                    default:
                        var segmentName = pathSegmentName;
                        if (segmentNameFormatter != null)
                            segmentName = segmentNameFormatter(pathSegmentName);

                        segments.Add(segmentName);
                        break;
                }
            }

            segments.Reverse();
            return string.Join("_", segments);
        }
    }
}