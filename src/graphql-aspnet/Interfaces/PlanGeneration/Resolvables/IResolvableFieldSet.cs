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
    /// Represents a resolvable item that is a collection of key/value pairs. This item type is
    /// typically resolved into an object with its KVPs representing properties of the object to be created.
    /// </summary>
    public interface IResolvableFieldSet : IResolvableValueItem
    {
        bool TryGetField(string fieldName, out IResolvableValueItem foundField);

        /// <summary>
        /// Gets the collection of fields defined on this instance.
        /// </summary>
        /// <value>The fields.</value>
        IEnumerable<KeyValuePair<string, IResolvableValueItem>> Fields { get; }
    }
}