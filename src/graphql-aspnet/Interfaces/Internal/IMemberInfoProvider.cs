// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System.Reflection;

    /// <summary>
    /// A common interface that exposes the various attributes of a
    /// <see cref="MethodInfo"/> and <see cref="PropertyInfo"/> to the templating system.
    /// </summary>
    public interface IMemberInfoProvider
    {
        /// <summary>
        /// Gets the member info object that defines a field.
        /// </summary>
        /// <value>The member information.</value>
        public MemberInfo MemberInfo { get; }

        /// <summary>
        /// Gets the attribute provider that serves up the various control and
        /// configuration attributes for generating a graph field from the <see cref="MemberInfo"/>.
        /// </summary>
        /// <value>The attribute provider.</value>
        public ICustomAttributeProvider AttributeProvider { get; }
    }
}