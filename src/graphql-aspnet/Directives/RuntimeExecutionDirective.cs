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
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A special directive instance for executing runtime configured directive
    /// actions (e.g. minimal api defined directives).
    /// </summary>
    internal sealed class RuntimeExecutionDirective : GraphDirective
    {
        /// <inheritdoc />
        protected override object CreateAndInvokeAction(IGraphFieldResolverMetaData metadata, object[] invocationArguments)
        {
            // minimal api resolvers are allowed to be static since there is no
            // extra context to setup or make available such as 'this.User' etc.
            if (metadata.Method.IsStatic)
            {
                var invoker = InstanceFactory.CreateStaticMethodInvoker(metadata.Method);
                return invoker(invocationArguments);
            }
            else
            {
                var invoker = InstanceFactory.CreateInstanceMethodInvoker(metadata.Method);
                var instance = InstanceFactory.CreateInstance(metadata.Method.DeclaringType);
                return invoker(ref instance, invocationArguments);
            }
        }
    }
}