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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Fields;
    using GraphQL.AspNet.Schemas.Structural;

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
         : base(
               name,
               new GraphFieldPath(GraphCollection.Types, name))
        {
            // add the __typename as a field for this virtual object
            this.GraphFieldCollection.AddField(new Introspection_TypeNameMetaField(name));
        }

        /// <inheritdoc />
        public override bool ValidateObject(object item)
        {
            return true;
        }

        /// <inheritdoc />
        public IGraphField Extend(IGraphField newField)
        {
            return this.GraphFieldCollection.AddField(newField);
        }

        /// <inheritdoc />
        public override bool IsVirtual => true;
    }
}