// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    /// <summary>
    /// An object representing the data and processes surrounding the
    /// execution of a query request, not the query itself.
    /// </summary>
    /// <remarks>
    /// In general, this object represents data related to the execution of the query
    /// but not specifically about the query itself. Items such as auxiallary data,
    /// control and configuration keys, pipeline metadata etc. belongs in the query session.
    /// </remarks>
    public interface IQuerySession : IMetaDataContainer
    {
    }
}