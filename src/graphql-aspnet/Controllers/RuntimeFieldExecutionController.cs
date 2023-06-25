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
    /// actions (e.g. minimal api defined fields).
    /// </summary>
    [GraphRoot]
    internal sealed class RuntimeFieldExecutionController : GraphController
    {
        /// <inheritdoc />
        protected override object CreateAndInvokeAction(IGraphFieldResolverMethod resolver, object[] invocationArguments)
        {
            // for minimal api resolvers create an instance of the
            // "runtime declared owner type" and invoke using it
            var instance = InstanceFactory.CreateInstance(resolver.Method.DeclaringType);
            var invoker = InstanceFactory.CreateInstanceMethodInvoker(resolver.Method);

            return invoker(ref instance, invocationArguments);
        }
    }
}