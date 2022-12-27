// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#if DEBUG

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("graphql-aspnet-testframework")]
[assembly: InternalsVisibleTo("graphql-aspnet-subscriptions-tests")]

#endif