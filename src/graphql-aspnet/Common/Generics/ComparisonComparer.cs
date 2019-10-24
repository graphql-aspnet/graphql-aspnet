// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Generics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An implementation of <see cref="Comparer{T}"/> that utilizes a <see cref="Comparison{T}"/>
    /// object.
    /// </summary>
    /// <typeparam name="T">The type to be compared.</typeparam>
    public class ComparisonComparer<T> : Comparer<T>
    {
        private readonly Comparison<T> _compareFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComparisonComparer{T}" /> class.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        public ComparisonComparer(Comparison<T> comparison)
        {
            _compareFunction = comparison ?? throw new ArgumentNullException(nameof(comparison));
        }

        /// <summary>
        /// Compares two items for to determine order.
        /// </summary>
        /// <param name="arg1">First argument to compare.</param>
        /// <param name="arg2">Second argument to compare.</param>
        /// <returns>System.Int32.</returns>
        public override int Compare(T arg1, T arg2)
        {
            return _compareFunction(arg1, arg2);
        }
    }
}