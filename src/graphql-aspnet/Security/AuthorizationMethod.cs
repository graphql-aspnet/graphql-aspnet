// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Security
{
    /// <summary>
    /// An enumeration of the different types of field authorization methods available.
    /// </summary>
    public enum AuthorizationMethod
    {
        /// <summary>
        /// No graphql field authorization security is enforced. All defined authorization attributes are ignored on all types
        /// and fields.
        /// </summary>
        None = -1,

        /// <summary>
        /// Each field is authorized individually. One field failing authorization does not prevent a query from executing other
        /// fields. If a user fails to authorize to a given a field an "access denied" message is added to the errors collection
        /// and "null" is set at the resolved field value.
        /// </summary>
        PerField = 0,

        /// <summary>
        /// The request is authenticated against every field on the selected operation BEFORE any action is taken. If the
        /// request fails a single field, the entire request is rejected and no fields are invoked.
        /// </summary>
        PerRequest = 1,
    }
}