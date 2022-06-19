// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
using System.Runtime.CompilerServices;

// support for mocking internal interface members in test suite
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

[assembly: InternalsVisibleTo("graphql-aspnet-testframework")]
[assembly: InternalsVisibleTo("graphql-aspnet-tests")]
[assembly: InternalsVisibleTo("graphql-aspnet-subscriptions-tests")]
