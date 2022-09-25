// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An equality comaprer used for <see cref="IAppliedDirective"/> objects.
    /// </summary>
    public class AppliedDirectiveEqualityComparer : IEqualityComparer<IAppliedDirective>
    {
        /// <summary>
        /// Gets the single default instance of the comparer.
        /// </summary>
        /// <value>The instance.</value>
        public static AppliedDirectiveEqualityComparer Instance { get; } = new AppliedDirectiveEqualityComparer();

        /// <summary>
        /// Prevents a default instance of the <see cref="AppliedDirectiveEqualityComparer"/> class from being created.
        /// </summary>
        private AppliedDirectiveEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(IAppliedDirective x, IAppliedDirective y)
        {
            if (x != null && y != null)
            {
                return object.ReferenceEquals(x, y);
            }
            else if (x == null && y == null)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public int GetHashCode(IAppliedDirective obj)
        {
            return obj.GetHashCode();
        }
    }
}