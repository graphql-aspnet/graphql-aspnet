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
    using GraphQL.AspNet.Execution;

    /// <summary>
    /// A decorator for a controller method to alter its behavior to be part of a concrete type represented on the object graph (instead of as a query or mutation field).
    /// This attribute indicates that the method should be invoked in a "batch" format, being executed once, regardless of the number of
    /// data items being resolved. See documentation for further details.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BatchTypeExtensionAttribute : TypeExtensionAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchTypeExtensionAttribute"/> class.
        /// </summary>
        /// <param name="typeToExtend">The concrete type to be extended.</param>
        /// <param name="fieldName">Name of the field in the object graph (will be subjected to and altered according to schema naming rules).</param>
        public BatchTypeExtensionAttribute(Type typeToExtend, string fieldName)
            : base(typeToExtend, fieldName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchTypeExtensionAttribute"/> class.
        /// </summary>
        /// <param name="typeToExtend">The concrete type to be extended.</param>
        /// <param name="fieldName">Name of the field in the object graph (will be subjected to and altered according to schema naming rules).</param>
        /// <param name="returnType">The expected data type to be returned "per item" in the batch.</param>
        public BatchTypeExtensionAttribute(Type typeToExtend, string fieldName, Type returnType)
            : base(typeToExtend, fieldName, returnType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchTypeExtensionAttribute" /> class.
        /// </summary>
        /// <param name="typeToExtend">The concrete type to be extended.</param>
        /// <param name="fieldName">Name of the field in the object graph (will be subjected to and altered according to schema naming rules).</param>
        /// <param name="unionTypeName">Name of the union type to be created.</param>
        /// <param name="unionTypeA">The first of two required types to include in the union.</param>
        /// <param name="unionTypeB">The second of two required types to include in the union.</param>
        /// <param name="additionalUnionTypes">Any additional union types.</param>
        public BatchTypeExtensionAttribute(
            Type typeToExtend,
            string fieldName,
            string unionTypeName,
            Type unionTypeA,
            Type unionTypeB,
            params Type[] additionalUnionTypes)
            : base(
                typeToExtend,
                fieldName,
                unionTypeName,
                unionTypeA,
                unionTypeB,
                additionalUnionTypes)
        {
        }

        /// <summary>
        /// Gets the mode indicating how the type system should interprete and process the results of this method.
        /// </summary>
        /// <value>The mode.</value>
        public override FieldResolutionMode ExecutionMode => FieldResolutionMode.Batch;
    }
}