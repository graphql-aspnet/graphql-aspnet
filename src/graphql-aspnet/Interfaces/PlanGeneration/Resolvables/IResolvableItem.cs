// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables
{
    /// <summary>
    /// A marker interface identifying an object as being one of several types of "resolvable items". Those items
    /// the runtime can resolve into real .NET objects from their graphql representations of fields, arrays and values.
    /// </summary>
    public interface IResolvableItem
    {
    }
}