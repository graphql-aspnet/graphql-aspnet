// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A decorator attribute to identify a controller action method as a field on the graph.
    /// The field will be nested inside a field or set of fields representing the controller that
    /// defines the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class QueryAttribute : GraphFieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        public QueryAttribute()
         : this(Constants.Routing.ACTION_METHOD_META_NAME)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        public QueryAttribute(string template)
         : this(template, null as Type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="unionTypeName">Name of the union type.</param>
        public QueryAttribute(string template, string unionTypeName)
         : this(template, unionTypeName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type.</param>
        public QueryAttribute(Type returnType)
        : this(Constants.Routing.ACTION_METHOD_META_NAME, returnType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type. In the event that the return type is an interface
        /// be sure to supply any additional concrete types so that they may be included in the object graph.</param>
        /// <param name="additionalTypes">Any additional types to include in the object graph on behalf of this method.</param>
        public QueryAttribute(Type returnType, params Type[] additionalTypes)
            : this(Constants.Routing.ACTION_METHOD_META_NAME, returnType, additionalTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type.</param>
        public QueryAttribute(string template, Type returnType)
            : base(false, SchemaItemCollections.Query, template, returnType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type. In the event that the return type is an interface
        /// be sure to supply any additional concrete types so that they may be included in the object graph.</param>
        /// <param name="additionalTypes">Any additional types to include in the object graph on behalf of this method.</param>
        public QueryAttribute(string template, Type returnType, params Type[] additionalTypes)
            : base(false, SchemaItemCollections.Query, template, (new Type[] { returnType }).Concat(additionalTypes ?? Enumerable.Empty<Type>()).ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAttribute" /> class.
        /// </summary>
        /// <param name="template">The template naming scheme to use to generate a graph field from this method.</param>
        /// <param name="unionTypeName">Name of the union type.</param>
        /// <param name="unionTypeA">The first type to include in the union.</param>
        /// <param name="additionalUnionTypes">Any additional union types to include.</param>
        public QueryAttribute(string template, string unionTypeName, Type unionTypeA, params Type[] additionalUnionTypes)
         : base(
               false,
               SchemaItemCollections.Query,
               template,
               unionTypeName,
               (new Type[] { unionTypeA }).Concat(additionalUnionTypes ?? Enumerable.Empty<Type>()).ToArray())
        {
        }
    }
}