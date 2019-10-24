// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Benchmarks.Benchmarks
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.ConsoleArguments;
    using BenchmarkDotNet.Engines;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Horology;
    using BenchmarkDotNet.Jobs;

    public class BenchmarkConfiguration : ManualConfig
    {
        public BenchmarkConfiguration()
        {
            var launchCount = 5;
            var warmupCount = 2;

            var unrollFactor = 18;
            var invocationCount = 5 * unrollFactor;

            Add(Job.InProcess
                    .With(Platform.X64)
                    .With(Runtime.Core)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 .NET Core Platform Execution"));

            Add(Job.InProcess
                    .With(Platform.X86)
                    .With(Runtime.Clr)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x86 Full Framework Platform Execution"));

            Add(Job.InProcess
                    .With(Platform.X64)
                    .With(Runtime.Clr)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 Full Framework Platform Execution"));

            Add(Job.InProcess
                    .With(Platform.X64)
                    .With(Runtime.Mono)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 Mono Platform Execution"));
        }
    }
}