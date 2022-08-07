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
                    .WithRuntime(CoreRuntime.Core31)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 .NET Core 3.1"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X64)
                    .WithRuntime(MonoRuntime.Default)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 Mono"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core60)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 .NET 6"));
        }
    }
}