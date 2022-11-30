// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common;

/// <summary>
/// A simple test console for submitting queries to the load test server
/// at a medium level volume. This console works best for small to mid level
/// traffic amounts to test memory and garbage collection pressure.
/// </summary>
/// <remarks>
/// This console cannot, and isn't designed to, meet the same throughout level
/// as something like jmeter. Use the jmeter test plans for any serious stress testing.
/// </remarks>
public static class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>System.Int32.</returns>
    public static async Task<int> Main(string[] args)
    {
        // add various script profiles from the RunnerOptions
        // class here to execute the tests contained within
        var profile = new ScriptProfileRunner(

            // session configuration information
            RunnerOptions.ProfileSession,

            // test runs to perform
            // ----------------------------
            // RunnerOptions.RestQuery,
            // RunnerOptions.GraphQLQuery,
            // RunnerOptions.GraphQLMutation,
            RunnerOptions.GraphQlSubscription
        );

        var cancelSource = new CancellationTokenSource();

        var task = profile.ExecuteProfile(cancelSource.Token);

        try
        {
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Write("Critical Failure: " + ex.Message);
            Console.WriteLine("[Halt]");
            Debugger.Break();
            Console.ReadKey();
            return -1;
        }

        if (!task.IsCompleted)
        {
            cancelSource.Cancel();
            await task;

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("[Halt]");
            Console.ReadKey();
        }

        return 0;
    }
}