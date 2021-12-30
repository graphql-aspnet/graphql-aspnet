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
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// When applied to a graph type, field or argument the directive
    /// is called during schema creation and may alter the <see cref="ISchemaItem"/>
    /// definition before it is added to the schema.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface |
        AttributeTargets.Struct | AttributeTargets.Enum |
        AttributeTargets.Method | AttributeTargets.Property |
        AttributeTargets.Parameter | AttributeTargets.Field,
        AllowMultiple = true)]
    public class ApplyDirectiveAttribute : BaseGraphAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyDirectiveAttribute" /> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive.</param>
        /// <param name="arguments">The arguments used to invoke the directive. The argument type and order
        /// must match the signature of the directive being applied.</param>
        public ApplyDirectiveAttribute(Type directiveType, params object[] arguments)
        {
            this.DirectiveType = directiveType;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Gets the directive type to be applied to the target schema item.
        /// </summary>
        /// <value>The directive types.</value>
        public Type DirectiveType { get; }

        /// <summary>
        /// Gets the arguments values passed to the <see cref="DirectiveType"/>
        /// when its invoked.
        /// </summary>
        /// <value>The arguments.</value>
        public object[] Arguments { get; }
    }
}