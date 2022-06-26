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
    using System.Collections.Generic;

    /// <summary>
    /// Represents a list of "things" that can be resolved into an of objects.
    /// </summary>
    public interface IResolvableList : IEnumerable<IResolvableKeyedItem>, IResolvableValueItem
    {
    }
}