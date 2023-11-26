// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    /// <summary>
    /// The available rules that dictate how the runtime will handle a missing or unresolved parameter
    /// during the execution of a field or directive resolver.
    /// </summary>
    public enum ResolverParameterResolutionRules
    {
        ThrowException = 0,
        UseNullorDefault = 1,
    }
}