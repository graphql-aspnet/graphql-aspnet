﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Tests.Common
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using NUnit.Framework;

    [TestFixture]
    public class TimerAsyncTests
    {
        [Test]
        public void NoInitialDelayThrowsException()
        {
            var total = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using var timer = new TimerAsync(
                    (token) =>
                    {
                        total++;
                        return Task.CompletedTask;
                    },
                    TimeSpan.FromMilliseconds(-1),
                    TimeSpan.FromMilliseconds(1));
            });
        }

        [Test]
        public void NoIntervalThrowsException()
        {
            var total = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using var timer = new TimerAsync(
                    (token) =>
                    {
                        total++;
                        return Task.CompletedTask;
                    },
                    TimeSpan.FromMilliseconds(1),
                    TimeSpan.FromMilliseconds(-1));
            });
        }

        [Test]
        public async Task TimerCallBackIsCalledAfterInterval()
        {
            var total = 0;
            using var timer = new TimerAsync(
                (token) =>
                {
                    total++;
                    return Task.CompletedTask;
                },
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(1));

            timer.Start();
            while (total < 3)
            {
                await Task.Yield();
            }

            await timer.Stop();
            Assert.AreEqual(3, total);
        }

        [Test]
        public async Task ActionThrowsException_ExceptionIsbubbled()
        {
            var exceptionToThrow = new Exception("Test Exception");
            using var timer = new TimerAsync(
                (token) =>
                {
                    throw exceptionToThrow;
                },
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(1));

            var errorThrown = false;
            timer.Error += (o, e) =>
            {
                Assert.AreSame(exceptionToThrow, e);
                errorThrown = true;
            };

            timer.Start();
            await Task.Delay(3);
            await timer.Stop();
            if (!errorThrown)
                Assert.Fail("Error was expected, but did not occur");
        }
    }
}