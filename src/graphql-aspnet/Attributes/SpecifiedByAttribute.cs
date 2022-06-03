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
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An attribute that applies the @specifiedBy directive to the target scalar.
    /// </summary>
    /// <remarks>
    /// This attribute must be applied to a class that implements <see cref="IScalarGraphType"/>
    /// or a runtime exception will occur when the schema is built.</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class SpecifiedByAttribute : ApplyDirectiveAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecifiedByAttribute"/> class.
        /// </summary>
        /// <param name="url">The url pointing to the specification for this
        /// scalar.</param>
        public SpecifiedByAttribute(string url)
            : base(typeof(SpecifiedByDirective), url)
        {
        }
    }
}