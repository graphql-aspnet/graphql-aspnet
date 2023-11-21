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
    /// A decorator for a controller method to declare it as an extension of another
    /// graph type (instead of as a query or mutation field). This attribute indicates that the
    /// method should be invoked in an individual format, being executed for each object
    /// being resolved.
    /// </summary>
    /// <remarks>
    /// See documentation for further details.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class TypeExtensionAttribute : GraphFieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeExtensionAttribute" /> class.
        /// </summary>
        /// <param name="typeToExtend">The concrete type to be extended.</param>
        /// <param name="fieldName">Name of the field on the type being extended (will be subjected to and altered according to schema naming rules).</param>
        public TypeExtensionAttribute(Type typeToExtend, string fieldName)
         : this(typeToExtend, fieldName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeExtensionAttribute" /> class.
        /// </summary>
        /// <param name="typeToExtend">The concrete type to be extended.</param>
        /// <param name="fieldName">Name of the field on the type being extended (will be subjected to and altered according to schema naming rules).</param>
        /// <param name="returnType">The type of the data object returned from this method. If this type implements
        /// <see cref="IGraphUnionProxy"/> this field will be declared as returning the union defined by the type.</param>
        public TypeExtensionAttribute(Type typeToExtend, string fieldName, Type returnType)
            : base(false, SchemaItemCollections.Types, fieldName, returnType)
        {
            this.TypeToExtend = typeToExtend;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeExtensionAttribute" /> class.
        /// </summary>
        /// <param name="typeToExtend">The concrete type to be extended.</param>
        /// <param name="fieldName">Name of the field on the type being extended (will be subjected to and altered according to schema naming rules).</param>
        /// <param name="unionTypeName">Name of the union type this type extension will return.</param>
        /// <param name="unionTypeA">The first of two required types to include in the union.</param>
        /// <param name="unionTypeB">The second of two required types to include in the union.</param>
        /// <param name="additionalUnionTypes">Any additional union types.</param>
        public TypeExtensionAttribute(
            Type typeToExtend,
            string fieldName,
            string unionTypeName,
            Type unionTypeA,
            Type unionTypeB,
            params Type[] additionalUnionTypes)
            : base(
                false,
                SchemaItemCollections.Types,
                fieldName,
                unionTypeName,
                unionTypeA.AsEnumerable().Concat(unionTypeB.AsEnumerable()).Concat(additionalUnionTypes).ToArray())
        {
            this.TypeToExtend = typeToExtend;
        }

        /// <summary>
        /// Gets the concrete type to be extended.
        /// </summary>
        /// <value>The type to extend.</value>
        public Type TypeToExtend { get; }
    }
}