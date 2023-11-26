// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.PipelineContextBuilders
{
    using System;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;

    /// <summary>
    /// A mocked controller template that will selectively parse actions instead of the whole template.
    /// </summary>
    public class SingleMethodGraphControllerTemplate : GraphControllerTemplate
    {
        private readonly string _methodName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleMethodGraphControllerTemplate" /> class.
        /// </summary>
        /// <param name="controllerType">Type of the controller.</param>
        /// <param name="methodName">Name of the single action method to parse. When not
        /// provided (e.g. <c>null</c>) this template will function the same as <see cref="GraphControllerTemplate" />
        /// and all methods will be parsed.</param>
        public SingleMethodGraphControllerTemplate(Type controllerType, string methodName = null)
             : base(controllerType)
        {
            _methodName = methodName;
        }

        /// <summary>
        /// Determines whether the given container could be used as a graph field either because it is
        /// explictly declared as such or that it conformed to the required parameters of being
        /// a field.
        /// </summary>
        /// <param name="memberInfo">The member information to check.</param>
        /// <returns><c>true</c> if the info represents a possible graph field; otherwise, <c>false</c>.</returns>
        protected override bool CouldBeGraphField(IMemberInfoProvider memberInfo)
        {
            if (_methodName != null && memberInfo.MemberInfo.Name != _methodName)
                return false;

            return base.CouldBeGraphField(memberInfo);
        }
    }
}