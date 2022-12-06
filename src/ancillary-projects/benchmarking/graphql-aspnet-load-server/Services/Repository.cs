// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.Server.Services
{
    using System;
    using System.Collections.Concurrent;
    using GraphQL.AspNet.SubscriberLoadTest.Models.Models.Server;

    /// <summary>
    /// A repository of donuts.
    /// </summary>
    public class Repository
    {
        private readonly ConcurrentDictionary<string, Donut> _donuts;
        private int _idCounter = 9000;

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        public Repository()
        {
            _donuts = new ConcurrentDictionary<string, Donut>();
            _donuts.TryAdd("1", new Donut
            {
                Id = "1",
                Name = "Snowy Vanilla",
                Flavor = DonutFlavor.Vanilla,
            });

            _donuts.TryAdd("2", new Donut
            {
                Id = "2",
                Name = "Super Chocolate",
                Flavor = DonutFlavor.Chocolate,
            });
        }

        /// <summary>
        /// Adds the donut if it does not already exist, updates it if it does.
        /// </summary>
        /// <param name="donut">The donut.</param>
        /// <returns>Donut.</returns>
        public Donut AddOrUpdateDonut(Donut donut)
        {
            Donut existing;
            if (string.IsNullOrWhiteSpace(donut.Id) || !_donuts.ContainsKey(donut.Id))
            {
                existing = new Donut()
                {
                    Id = _idCounter++.ToString(),
                };

                _donuts.TryAdd(existing.Id, existing);
            }
            else
            {
                existing = _donuts[donut.Id];
            }

            lock (existing)
            {
                existing.Flavor = donut.Flavor;
                existing.Name = donut.Name;
                return existing.Clone();
            }
        }

        /// <summary>
        /// Gets the the known donuts.
        /// </summary>
        /// <value>The donuts.</value>
        public ConcurrentDictionary<string, Donut> Donuts => _donuts;
    }
}