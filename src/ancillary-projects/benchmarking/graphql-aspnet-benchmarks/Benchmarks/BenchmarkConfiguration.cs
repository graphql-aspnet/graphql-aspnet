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
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Jobs;
    using Perfolizer.Horology;

    public class BenchmarkConfiguration : ManualConfig
    {
        public BenchmarkConfiguration()
        {
            var launchCount = 5;
            var warmupCount = 2;
            var unrollFactor = 18;
            var invocationCount = 5 * unrollFactor;

            this.AddJob(Job.InProcess
                .WithPlatform(Platform.X64)
                .WithRuntime(CoreRuntime.Core80)
                .WithUnrollFactor(unrollFactor)
                .WithLaunchCount(launchCount)
                .WithWarmupCount(warmupCount)
                .WithInvocationCount(invocationCount)
                .WithIterationTime(TimeInterval.Millisecond * 200)
                .WithId("x64 .NET 8"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core90)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 .NET 9"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core10_0)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 .NET 10"));
        }
    }
}