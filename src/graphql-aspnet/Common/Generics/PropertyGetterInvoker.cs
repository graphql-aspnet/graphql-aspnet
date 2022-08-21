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
    /// A representation of a compiled lamda that can get a value to a property.
    /// </summary>
    /// <param name="inputObject">The input object.</param>
    /// <returns>The value of the property on the supplied object.</returns>
    public delegate object PropertyGetterInvoker(ref object inputObject);
}