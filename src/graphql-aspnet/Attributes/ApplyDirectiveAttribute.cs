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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// When applied to a class, method, property, parameter etc. of your source code,
    /// the type system directive indicated is applied to the resultant graph type, field, argument etc.
    /// generated during schema creation. Applying a directive may alter the
    /// associated <see cref="ISchemaItem"/> definition before it is added to the schema.
    /// </summary>
    /// <remarks>
    /// Consider subclassing this attribute to provide better readability
    /// </remarks>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Interface |
        AttributeTargets.Struct | AttributeTargets.Enum |
        AttributeTargets.Method | AttributeTargets.Property |
        AttributeTargets.Parameter | AttributeTargets.Field,
        AllowMultiple = true)]
    public class ApplyDirectiveAttribute : GraphAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyDirectiveAttribute" /> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive to invoke.</param>
        /// <param name="appliesTo">
        /// A set of graphql types to restrict the direct to. If the type being created
        /// does not match one of the supplied types, the directive is not applied. Pass null to indicate no type restrictions.
        /// </param>
        /// <param name="arguments">
        /// The arguments used to invoke the directive. The argument type and order
        /// must match the signature of the directive being applied or an exception will be thrown.
        /// </param>
        public ApplyDirectiveAttribute(Type directiveType, TypeKind[] appliesTo, params object[] arguments)
        {
            this.DirectiveType = directiveType;
            this.Arguments = arguments;
            this.AppliesToTypes = appliesTo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyDirectiveAttribute" /> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive to invoke.</param>
        /// <param name="arguments">The arguments used to invoke the directive. The argument type and order
        /// must match the signature of the directive being applied or an exception will be thrown.</param>
        public ApplyDirectiveAttribute(Type directiveType, params object[] arguments)
            : this(directiveType, null as TypeKind[], arguments)
        {
            this.DirectiveType = directiveType;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyDirectiveAttribute" /> class.
        /// </summary>
        /// <param name="directiveName">Name of the directive as it will exist in the target schema (e.g. "@skip").</param>
        /// <param name="appliesTo">
        /// A set of graphql types to restrict the direct to. If the type being created
        /// does not match one of the supplied types, the directive is not applied. Pass null to indicate no type restrictions.
        /// </param>
        /// <param name="arguments">
        /// The arguments used to invoke the directive. The argument type and order
        /// must match the signature of the directive being applied or an exception will be thrown.
        /// </param>
        public ApplyDirectiveAttribute(string directiveName, TypeKind[] appliesTo, params object[] arguments)
        {
            this.DirectiveName = directiveName;
            this.Arguments = arguments;
            this.AppliesToTypes = appliesTo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyDirectiveAttribute" /> class.
        /// </summary>
        /// <param name="directiveName">Name of the directive as it will exist in the target schema (e.g. "@skip").</param>
        /// <param name="arguments">The arguments used to invoke the directive. The argument type and order
        /// must match the signature of the directive being applied or an exception will be thrown.</param>
        public ApplyDirectiveAttribute(string directiveName, params object[] arguments)
            : this(directiveName, null as TypeKind[], arguments)
        {
            this.DirectiveName = directiveName;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Gets the directive, by declared type, to be applied to the target schema item.
        /// </summary>
        /// <value>The directive types.</value>
        public Type DirectiveType { get; }

        /// <summary>
        /// Gets the directive, by name, to be applied to the target schema item.
        /// </summary>
        /// <value>The name of the directive.</value>
        public string DirectiveName { get; }

        /// <summary>
        /// Gets the arguments values passed to the <see cref="DirectiveType"/>
        /// when its invoked.
        /// </summary>
        /// <value>The arguments.</value>
        public object[] Arguments { get; }

        /// <summary>
        /// Gets the set of types this directive will be applied to. If a graphql type is created
        /// from a related template it must match one of the applicable type kinds in this set for the directive to be applied.
        /// (Default: null).
        /// </summary>
        /// <remarks>
        /// When null, indicates that no restrictions are applied, matching all types.
        /// </remarks>
        public TypeKind[] AppliesToTypes { get; }
    }
}