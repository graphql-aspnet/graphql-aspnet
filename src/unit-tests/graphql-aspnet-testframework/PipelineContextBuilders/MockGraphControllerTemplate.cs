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
    using System.Reflection;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Internal.TypeTemplates;

    /// <summary>
    /// A mocked controller template that will selectively parse actions instead of the whole template.
    /// </summary>
    /// <typeparam name="TControllerType">The type of the controller to templatize.</typeparam>
    public class MockGraphControllerTemplate<TControllerType> : GraphControllerTemplate
        where TControllerType : GraphController
    {
        private readonly string _methodName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockGraphControllerTemplate{TControllerType}"/> class.
        /// </summary>
        /// <param name="methodName">Name of the single action method to parse.</param>
        public MockGraphControllerTemplate(string methodName = null)
             : base(typeof(TControllerType))
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
        protected override bool CouldBeGraphField(MemberInfo memberInfo)
        {
            if (_methodName != null && memberInfo.Name != _methodName)
                return false;

            return base.CouldBeGraphField(memberInfo);
        }
    }
}