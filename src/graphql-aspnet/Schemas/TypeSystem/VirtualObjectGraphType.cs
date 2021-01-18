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
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Fields;

    /// <summary>
    /// A graph type used to expose an abstract item, not tied to a physical object, as an object type on the graph.
    /// Used as a mechanism to convert virtual paths in <see cref="GraphRouteAttribute"/> declarations into fields on the object graph.
    /// </summary>
    [DebuggerDisplay("OBJECT (virtual) {Name}")]
    public class VirtualObjectGraphType : BaseObjectGraphType, IObjectGraphType
    {
        // Implementation Note:
        //
        // This object represents the "binder" between controllers, actions and the type system.
        // The controller is an abstract concept, not tied to any "piece of data" (like a real "Person" object would be) so this virtual graph type
        // is used to expose the controller's action methods as though they were fields on a class
        // to allow for proper navigation of an object structure in graphql. This object is generated dynamically from the parsed
        // metadata of a controller.

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualObjectGraphType" /> class.
        /// </summary>
        /// <param name="name">The name to assign to this type.</param>
        public VirtualObjectGraphType(string name)
         : base(name)
        {
            // add the __typename as a field for this virtual object
            this.GraphFieldCollection.AddField(new Introspection_TypeNameMetaField(name));
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public override bool ValidateObject(object item)
        {
            return true;
        }

        /// <summary>
        /// Extends this graph type by adding a new field to its collection. An exception may be thrown if
        /// a field with the same name already exists.
        /// </summary>
        /// <param name="newField">The new field.</param>
        public void Extend(IGraphField newField)
        {
            this.GraphFieldCollection.AddField(newField);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this graph types points to a concrete type
        /// defined by a developer.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public override bool IsVirtual => true;
    }
}