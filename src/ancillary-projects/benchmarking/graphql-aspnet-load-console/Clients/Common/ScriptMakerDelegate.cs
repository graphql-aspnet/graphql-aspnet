// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common
{
    /// <summary>
    /// A delegate that can generate a new client script.
    /// </summary>
    /// <param name="scriptNumber">The global number assigned during the profile run.
    /// (e.g. 'the 20th script created').</param>
    /// <returns>IClientScript.</returns>
    public delegate IClientScript ScriptMakerDelgate(int scriptNumber);
}