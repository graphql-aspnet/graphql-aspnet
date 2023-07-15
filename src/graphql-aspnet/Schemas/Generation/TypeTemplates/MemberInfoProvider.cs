// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Internal;

    /// <summary>
    /// A <see cref="IMemberInfoProvider"/> for to abstract out appropriate pieces
    /// of <see cref="MemberInfo"/> that are exposed to the templating system.
    /// </summary>
    public class MemberInfoProvider : IMemberInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfoProvider"/> class.
        /// </summary>
        /// <param name="memberInfo">The primary member information used in this instance.
        /// This object will be used as the primary member info as well as the attribute provider for this instance.</param>
        public MemberInfoProvider(MemberInfo memberInfo)
        {
            this.MemberInfo = Validation.ThrowIfNullOrReturn(memberInfo, nameof(memberInfo));
            this.AttributeProvider = this.MemberInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInfoProvider" /> class.
        /// </summary>
        /// <param name="memberInfo">The primary member information used in this instance.</param>
        /// <param name="attributeProvider">The an alternate attribute source
        /// to use for determining configuration and control attributes for this instance.</param>
        public MemberInfoProvider(MemberInfo memberInfo, ICustomAttributeProvider attributeProvider)
        {
            this.MemberInfo = Validation.ThrowIfNullOrReturn(memberInfo, nameof(memberInfo));
            this.AttributeProvider = Validation.ThrowIfNullOrReturn(attributeProvider, nameof(attributeProvider));
        }

        /// <inheritdoc />
        public MemberInfo MemberInfo { get; }

        /// <inheritdoc />
        public ICustomAttributeProvider AttributeProvider { get; }
    }
}