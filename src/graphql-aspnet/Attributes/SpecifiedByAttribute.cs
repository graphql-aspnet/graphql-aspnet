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
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A specialized attribute that applies the built in @specifiedBy directive to
    /// the target scalar.
    /// </summary>
    /// <remarks>
    /// This attribute must be applied to a class that implements <see cref="IScalarGraphType"/>
    /// or an exception will be thrown when the schema is built.
    /// </remarks>
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