// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.ExecutionEvents.PropertyItems
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Logging.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A formatted entry for a graph type in a <see cref="ISchema"/> that can be safely logged.
    /// </summary>
    public class SchemaGraphTypeLogItem : GraphLogPropertyCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaGraphTypeLogItem" /> class.
        /// </summary>
        /// <param name="graphType">The graph type to create this log item from.</param>
        /// <param name="concreteType">The associated .NET concrete type, if any.</param>
        public SchemaGraphTypeLogItem(IGraphType graphType, Type concreteType = null)
        {
            this.GraphTypeName = graphType.Name;
            this.GraphTypeKind = graphType.Kind.ToString();
            this.GraphTypeType = concreteType?.FriendlyName(true);

            if (graphType is IGraphFieldContainer fieldContainer)
            {
                this.GraphFieldCount = fieldContainer.Fields.Count;
            }

            this.IsPublished = graphType.Publish;
        }

        /// <summary>
        /// Gets or sets a string representation of the <see cref="TypeKind"/> of the item.
        /// </summary>
        /// <value>The kind of the graph type.</value>
        public string GraphTypeKind
        {
            get => this.GetProperty<string>(LogPropertyNames.GRAPH_TYPE_KIND);
            set => this.SetProperty(LogPropertyNames.GRAPH_TYPE_KIND, value);
        }

        /// <summary>
        /// Gets or sets the graph type name of this item, as its declared and formatted in its
        /// owner schema.
        /// </summary>
        /// <value>The graph type name.</value>
        public string GraphTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.GRAPH_TYPE_NAME);
            set => this.SetProperty(LogPropertyNames.GRAPH_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets or sets the name of the concrete .NET type associated to this graph type in the
        /// schema. May be null for virtual types.
        /// </summary>
        /// <value>The graph type .NET type name.</value>
        public string GraphTypeType
        {
            get => this.GetProperty<string>(LogPropertyNames.GRAPH_TYPE_TYPE_NAME);
            set => this.SetProperty(LogPropertyNames.GRAPH_TYPE_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets or sets the total number of fields mapped to the graph type. If the grpah type does not
        /// support fields (such as scalars) this property is not included.
        /// </summary>
        /// <value>The number of fields registered to the graph type.</value>
        public int GraphFieldCount
        {
            get => this.GetProperty<int>(LogPropertyNames.GRAPH_TYPE_FIELD_COUNT);
            set => this.SetProperty(LogPropertyNames.GRAPH_TYPE_FIELD_COUNT, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the graph type is published on introspection queries.
        /// </summary>
        /// <value><c>true if the graph type is published</c>; otherwise <c>false</c>.</value>
        public bool IsPublished
        {
            get => this.GetProperty<bool>(LogPropertyNames.GRAPH_TYPE_IS_PUBLISHED);
            set => this.SetProperty(LogPropertyNames.GRAPH_TYPE_IS_PUBLISHED, value);
        }
    }
}