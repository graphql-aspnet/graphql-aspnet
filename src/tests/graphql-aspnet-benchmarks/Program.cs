// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Benchmarks
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Running;
    using GraphQL.AspNet.Benchmarks.Benchmarks;

    /// <summary>
    /// Startup class fro the benchmark runner.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.</returns>
        public static int Main(string[] args)
        {
            // quick tetsers to ensure processing before
            // unleasing the beast. keep commented out when executing benchmark run
            // ****************************************
#if DEBUG
            var item = new ExecuteQueries();
            item.InitializeEnvironment();
            item.SingleObjectQuery();
            item.TypeExtensionQuery();
            item.MultiActionMethodQuery();
            Console.WriteLine("All actions complete. Execute in release mode, without the debugger attached to run benchmarks.");
            Console.Read();
            return 0;
#endif

            // results written to the bin directory of this project
            // be sure to execute in release mode without the debugger attached
            // ****************************************
#if !DEBUG
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Benchmarks should be executed without the debugger attached.");
                Console.Read();
            }
            else
            {
                var summary = BenchmarkRunner.Run<ExecuteQueries>();
            }
            return 0;
#endif
        }
    }
}