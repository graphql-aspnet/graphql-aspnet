// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    /// <summary>
    /// A set of options to instruct how a field clone operation should handle a default value
    /// applied to a field argument or input object field.
    /// </summary>
    public enum DefaultValueCloneOptions
    {
        /// <summary>
        /// No change to the requirability of the item is made.
        /// </summary>
        None = 0,

        /// <summary>
        /// The item is marked as required and any assigned default value is removed.
        /// </summary>
        MakeRequired = 1,

        /// <summary>
        /// The item is marked as not required and the supplied default value is set as the
        /// value for the field.
        /// </summary>
        UpdateDefaultValue = 2,
    }
}
