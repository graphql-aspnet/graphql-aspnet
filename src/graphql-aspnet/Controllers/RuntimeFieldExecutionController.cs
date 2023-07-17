// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A special controller instance for executing runtime configured controller
    /// actions (e.g. minimal api defined fields and type exensions).
    /// </summary>
    [GraphRoot]
    internal sealed class RuntimeFieldExecutionController : GraphController
    {
        /// <inheritdoc />
        protected override object CreateAndInvokeAction(IGraphFieldResolverMetaData resolverMetaData, object[] invocationArguments)
        {
            // minimal api resolvers are allowed to be static since there is no
            // extra context to setup or make available such as 'this.User' etc.
            if (resolverMetaData.Method.IsStatic)
            {
                var invoker = InstanceFactory.CreateStaticMethodInvoker(resolverMetaData.Method);
                return invoker(invocationArguments);
            }
            else
            {
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(resolverMetaData.Method);
                var instance = InstanceFactory.CreateInstance(resolverMetaData.Method.DeclaringType);
                return invoker(ref instance, invocationArguments);
            }
        }
    }
}