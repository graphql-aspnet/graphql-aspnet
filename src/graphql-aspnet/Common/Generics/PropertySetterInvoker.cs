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
    /// A representation of a compiled lamda that can set a value to a property.
    /// </summary>
    /// <param name="inputObject">The input object.</param>
    /// <param name="valueToSet">The value to set.</param>
    public delegate void PropertySetterInvoker(object inputObject, object valueToSet);
}