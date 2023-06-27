// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Generics
{
    /// <summary>
    /// A representation of a compiled lamda to invoke a method on an instance of an object.
    /// </summary>
    /// <param name="instanceToInvokeOn">The object instance to invoke on.</param>
    /// <param name="methodParameters">The parameters to pass the method call.</param>
    /// <returns>The result of the call.</returns>
    internal delegate object InstanceMethodInvoker(ref object instanceToInvokeOn, params object[] methodParameters);

    /// <summary>
    /// A representation of a compiled lamda to invoke a static method not attached to a specific
    /// object instance.
    /// </summary>
    /// <param name="methodParameters">The parameters to pass the method call.</param>
    /// <returns>The result of the call.</returns>
    internal delegate object StaticMethodInvoker(params object[] methodParameters);
}