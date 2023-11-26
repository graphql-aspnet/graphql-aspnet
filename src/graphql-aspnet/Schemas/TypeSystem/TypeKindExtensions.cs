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
    /// <summary>
    /// Helper methods for dealing with <see cref="TypeKind"/>.
    /// </summary>
    public static class TypeKindExtensions
    {
        /// <summary>
        /// Determines whether the given kind of graph type is a valid leaf.
        /// </summary>
        /// <param name="kind">The kind to check.</param>
        /// <returns><c>true</c> if the type kind is a leaf type; otherwise, <c>false</c>.</returns>
        public static bool IsLeafKind(this TypeKind kind)
        {
            switch (kind)
            {
                case TypeKind.SCALAR:
                case TypeKind.ENUM:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the given type kind is one that can be used as an input argument value.
        /// </summary>
        /// <param name="kind">The kind to check.</param>
        /// <returns><c>true</c> if kind is a valid input type; otherwise, <c>false</c>.</returns>
        public static bool IsValidInputKind(this TypeKind kind)
        {
            switch (kind)
            {
                case TypeKind.SCALAR:
                case TypeKind.ENUM:
                case TypeKind.INPUT_OBJECT:
                    return true;

                default:
                    return false;
            }
        }
    }
}