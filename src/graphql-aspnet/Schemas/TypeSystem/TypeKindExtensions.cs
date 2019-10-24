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
        /// Determines whether this <see cref="TypeKind"/> is allowed to be transformed into the provided kind.
        /// </summary>
        /// <param name="kind">The kind.</param>
        /// <param name="kindToBecome">The kind to become.</param>
        /// <returns><c>true</c> if this instance [can be come] the specified kind to become; otherwise, <c>false</c>.</returns>
        public static bool CanBecome(this TypeKind kind, TypeKind kindToBecome)
        {
            switch (kind)
            {
                case TypeKind.INTERFACE:
                    return kindToBecome == TypeKind.ENUM || kindToBecome == TypeKind.SCALAR || kindToBecome == TypeKind.OBJECT;

                case TypeKind.OBJECT:
                    return kindToBecome == TypeKind.ENUM || kindToBecome == TypeKind.SCALAR || kindToBecome == TypeKind.INTERFACE;

                case TypeKind.INPUT_OBJECT:
                    return kindToBecome == TypeKind.ENUM || kindToBecome == TypeKind.SCALAR;

                case TypeKind.NONE:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the given kind of graph type is a valid leaf.
        /// </summary>
        /// <param name="kind">The kind.</param>
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
    }
}