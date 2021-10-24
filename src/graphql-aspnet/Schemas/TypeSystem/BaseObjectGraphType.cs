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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A represention of a graphql object in the schema. This object defines all the exposed
    /// fields that can be "requested" when this type is queried.
    /// </summary>
    [DebuggerDisplay("OBJECT: {Name} (Fields = {Fields.Count})")]
    public abstract class BaseObjectGraphType : IGraphType
    {
        private readonly GraphFieldCollection _graphFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseObjectGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type as it is displayed in the __type information.</param>
        /// <param name="graphFields">The initial set of graph fields to add to this instance.</param>
        protected BaseObjectGraphType(string name, IEnumerable<IGraphField> graphFields = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            _graphFields = new GraphFieldCollection(this);
            this.InterfaceNames = new HashSet<string>();
            this.Publish = true;
            if (graphFields != null)
            {
                foreach (var field in graphFields)
                    _graphFields.AddField(field);
            }
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public abstract bool ValidateObject(object item);

        /// <summary>
        /// Gets the mutatable collection of graph fields.
        /// </summary>
        /// <value>The graph field collection.</value>
        protected GraphFieldCollection GraphFieldCollection => _graphFields;

        /// <summary>
        /// Gets the collection of fields, keyed on their name, of all the fields nested or contained within this field.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyGraphFieldCollection Fields => _graphFields;

        /// <summary>
        /// Gets the <see cref="IGraphField"/> with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public virtual IGraphField this[string fieldName] => this.Fields[fieldName];

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public virtual string Name { get; }

        /// <summary>
        /// Gets or sets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets the value indicating what type of graph type this instance is in the type system. (object, scalar etc.)
        /// </summary>
        /// <value>The kind.</value>
        public virtual TypeKind Kind => TypeKind.OBJECT;

        /// <summary>
        /// Gets the internal collection of interfaces this object graph type implements.
        /// </summary>
        /// <value>The interfaces.</value>
        public HashSet<string> InterfaceNames { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphType" /> is published on an introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public virtual bool Publish { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this graph types points to a concrete type
        /// defined by a developer.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public virtual bool IsVirtual => false;
    }
}