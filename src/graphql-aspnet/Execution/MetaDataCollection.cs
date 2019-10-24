// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Collections.Concurrent;

    /// <summary>
    /// An arbitrary dictionary of items.
    /// </summary>
    public class MetaDataCollection : ConcurrentDictionary<string, object>
    {
        /// <summary>
        /// Merges the provided collection into this one. Any existing keys in this instance are updated with their new values
        /// and non-existant keys are added to this instance.
        /// </summary>
        /// <param name="otherCollection">The other collection.</param>
        public void Merge(MetaDataCollection otherCollection)
        {
            if (otherCollection == null)
                return;

            foreach (var kvp in otherCollection)
            {
                this[kvp.Key] = kvp.Value;
            }
        }
    }
}