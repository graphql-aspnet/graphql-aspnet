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
    /// A delgate describing the invocation of an object activator to fast-create an instance
    /// of an object.
    /// </summary>
    /// <param name="args">The constructor arguments for the object being created.</param>
    /// <returns>The created object.</returns>
    public delegate object ObjectActivator(object[] args);
}