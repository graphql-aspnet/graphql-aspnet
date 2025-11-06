// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using GraphQL.AspNet.Directives.Global;

    /// <summary>
    /// A marker interface that instructs the type system to treat any INPUT OBJECT type created from this class
    /// as always applying the '@OneOf' directive in the type system.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class OneOfAttribute : ApplyDirectiveAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OneOfAttribute" /> class.
        /// </summary>
        public OneOfAttribute()
            : base(typeof(OneOfDirective))
        {
        }
    }
}