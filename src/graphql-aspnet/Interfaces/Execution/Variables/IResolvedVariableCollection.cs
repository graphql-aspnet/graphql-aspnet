// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Variables
{
    using System.Collections.Generic;

    /// <summary>
    /// A collection of variables that have been resolved and contextualized in terms
    /// of a specific query operation they are targeting.
    /// </summary>
    public interface IResolvedVariableCollection : IReadOnlyDictionary<string, IResolvedVariable>
    {
    }
}