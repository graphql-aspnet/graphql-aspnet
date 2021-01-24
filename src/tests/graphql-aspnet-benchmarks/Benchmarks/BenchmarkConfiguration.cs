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
                    .WithPlatform(Platform.X86)
                    .WithRuntime(ClrRuntime.Net472)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x86 Full Framework Platform Execution"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X64)
                    .WithRuntime(ClrRuntime.Net472)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 Full Framework Platform Execution"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X64)
                    .WithRuntime(MonoRuntime.Default)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 Mono Platform Execution"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X86)
                    .WithRuntime(CoreRuntime.Core50)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x86 .NET 5 Platform Execution"));

            this.AddJob(Job.InProcess
                    .WithPlatform(Platform.X64)
                    .WithRuntime(CoreRuntime.Core50)
                    .WithUnrollFactor(unrollFactor)
                    .WithLaunchCount(launchCount)
                    .WithWarmupCount(warmupCount)
                    .WithInvocationCount(invocationCount)
                    .WithIterationTime(TimeInterval.Millisecond * 200)
                    .WithId("x64 .NET 5 Platform Execution"));
        }
    }
}