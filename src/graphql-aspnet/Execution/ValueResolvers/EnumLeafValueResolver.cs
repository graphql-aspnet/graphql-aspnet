// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.ValueResolvers
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A leaf resolver that can create a valid <see cref="Enum"/> of a given
    /// type from a set of characters parsed from a document.
    /// </summary>
    public class EnumLeafValueResolver : ILeafValueResolver
    {
        private readonly Type _enumType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumLeafValueResolver"/> class.
        /// </summary>
        /// <param name="enumType">Type of the enum that will be parsed
        /// by this resolver.</param>
        public EnumLeafValueResolver(Type enumType)
        {
            _enumType = Validation.ThrowIfNullOrReturn(enumType, nameof(enumType));

            if (!_enumType.IsEnum)
                throw new ArgumentException($"The type '{_enumType.Name}' is not a valid enum");
        }

        /// <inheritdoc />
        public object Resolve(ReadOnlySpan<char> data)
        {
            try
            {
                return Enum.Parse(_enumType, data.ToString(), true);
            }
            catch
            {
                return null;
            }
        }
    }
}