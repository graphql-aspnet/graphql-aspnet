// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Helpful filters for navgiating the <see cref="ISchemaItem"/> selection
    /// set when late binding directives.
    /// </summary>
    public static class SchemaItemExtensions
    {
        /// <summary>
        /// Determines whether the specified item is one related to introspection objects.
        /// </summary>
        /// <param name="item">The item to inspect.</param>
        /// <returns><c>true</c> if the specified item is an introspection related schema item; otherwise, <c>false</c>.</returns>
        public static bool IsIntrospectionItem(this ISchemaItem item)
        {
            return item is IIntrospectionSchemaItem;
        }

        /// <summary>
        /// Determines whether the specified item is one owned by graphql.
        /// (i.e. an item that starts with '__').
        /// </summary>
        /// <param name="item">The item to inspect.</param>
        /// <returns><c>true</c> if the specified item is system level data; otherwise, <c>false</c>.</returns>
        public static bool IsSystemItem(this ISchemaItem item)
        {
            // name regex matches on valid "user supplied names" any schema items
            // made that don't match this are made internally by the system
            return item != null && !Constants.RegExPatterns.NameRegex.IsMatch(item.Name);
        }

        /// <summary>
        /// Determines whether the specified item is an <see cref="IDirective"/>.
        /// </summary>
        /// <param name="item">The item to inspect.</param>
        /// <returns><c>true</c> if the specified item is a directive; otherwise, <c>false</c>.</returns>
        public static bool IsDirective(this ISchemaItem item)
        {
            return item is IDirective;
        }

        /// <summary>
        /// Determines whether the given schema item is a field on the graph type
        /// declared by the <typeparamref name="TType"/> parameter.
        /// </summary>
        /// <typeparam name="TType">The model type that declares the target field.</typeparam>
        /// <param name="schemaItem">The schema item to inspect.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldNameIsCaseSensitive">if set to <c>true</c> then <paramref name="fieldName"/> is
        /// matched on the exact name as its declared in the schema.</param>
        /// <param name="graphTypeKind">The expected <see cref="TypeKind"/> of the graph type
        /// that owns this field.</param>
        /// <returns><c>true</c> if the schema item is a matching field; otherwise, <c>false</c>.</returns>
        public static bool IsField<TType>(
            this ISchemaItem schemaItem,
            string fieldName,
            bool fieldNameIsCaseSensitive = true,
            TypeKind graphTypeKind = TypeKind.OBJECT)
        {
            return schemaItem != null
                && schemaItem is IGraphField gf
                && string.Compare(gf.Name, fieldName, !fieldNameIsCaseSensitive) == 0
                && gf.Parent is IGraphType gt
                && gt.Kind == graphTypeKind
                && gt is ITypedSchemaItem tsi
                && tsi.ObjectType == typeof(TType);
        }

        /// <summary>
        /// Determines whether the given schema item is a field on a graph type.
        /// </summary>
        /// <param name="schemaItem">The schema item to inspect.</param>
        /// <param name="graphTypeName">the name of the graph type that owns the field.</param>
        /// <param name="fieldName">The name of the field to match on.</param>
        /// <param name="fieldNameIsCaseSensitive">if set to <c>true</c> then <paramref name="fieldName" /> is
        /// matched on the exact name as its declared in the schema.</param>
        /// <param name="graphTypeNameIsCaseSensitive">if set to <c>true</c> then <paramref name="graphTypeName" /> is
        /// matched on the exact name as its declared in the schema.</param>
        /// <returns><c>true</c> if the schema item is a field; otherwise, <c>false</c>.</returns>
        public static bool IsField(
            this ISchemaItem schemaItem,
            string graphTypeName,
            string fieldName,
            bool fieldNameIsCaseSensitive = true,
            bool graphTypeNameIsCaseSensitive = true)
        {
            return schemaItem != null
                && schemaItem is IGraphField gf
                && string.Compare(gf.Name, fieldName, !fieldNameIsCaseSensitive) == 0
                && gf.Parent is IGraphType gt
                && string.Compare(gt.Name, graphTypeName, !graphTypeNameIsCaseSensitive) == 0;
        }

        /// <summary>
        /// Determines whether the given schema item is an OBJECT graph type
        /// that is created from the type specified by <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">The model type to match on.</typeparam>
        /// <param name="schemaItem">The schema item.</param>
        /// <returns><c>true</c> if the schema item is an OBJECT type declared from the given <typeparamref name="TType"/>; otherwise, <c>false</c>.</returns>
        public static bool IsObjectGraphType<TType>(
           this ISchemaItem schemaItem)
        {
            return schemaItem != null
                && schemaItem is IGraphType gt
                && gt.Kind == TypeKind.OBJECT
                && gt is IObjectGraphType ogt
                && ogt.ObjectType == typeof(TType);
        }

        /// <summary>
        /// Determines whether the given schema item is an OBJECT graph type
        /// with the given name.
        /// </summary>
        /// <param name="schemaItem">The schema item.</param>
        /// <param name="graphTypeName">Name of the graph type as its declared in the schema.</param>
        /// <param name="graphTypeNameIsCaseSensitive">if set to <c>true</c> then <paramref name="graphTypeName" /> is
        /// matched on the exact name as its declared in the schema.</param>
        /// <returns><c>true</c> if the schema item is an OBJECT graph type with the given name; otherwise, <c>false</c>.</returns>
        public static bool IsObjectGraphType(
           this ISchemaItem schemaItem,
           string graphTypeName,
           bool graphTypeNameIsCaseSensitive = true)
        {
            return schemaItem != null
                && schemaItem is IObjectGraphType gt
                && gt.Kind == TypeKind.OBJECT
                && string.Compare(gt.Name, graphTypeName, !graphTypeNameIsCaseSensitive) == 0;
        }

        /// <summary>
        /// Determines whether the given schema item is a graph type of the
        /// specific <paramref name="typeKind"/> that is created from the .NET type
        /// specified by <typeparamref name="TType" />.
        /// </summary>
        /// <typeparam name="TType">The model type to match on.</typeparam>
        /// <param name="schemaItem">The schema item.</param>
        /// <param name="typeKind">The expected type kind of the graph type.</param>
        /// <returns><c>true</c> if the schema item is a graph type of the
        /// given <paramref name="typeKind"/> and created from the .NET Type <typeparamref name="TType"/>; otherwise, <c>false</c>.</returns>
        public static bool IsGraphType<TType>(
           this ISchemaItem schemaItem,
           TypeKind typeKind)
        {
            return schemaItem != null
                && schemaItem is IGraphType gt
                && gt.Kind == typeKind
                && gt is ITypedSchemaItem tsi
                && tsi.ObjectType == typeof(TType);
        }

        /// <summary>
        /// Determines whether the given schema item is a graph type with the given name.
        /// </summary>
        /// <param name="schemaItem">The schema item.</param>
        /// <param name="graphTypeName">Name of the graph type to match on.</param>
        /// <param name="graphTypeNameIsCaseSensitive">if set to <c>true</c> then <paramref name="graphTypeName"/> must
        /// match exactly to the name of the type in the schema.</param>
        /// <returns><c>true</c> if the schema item is a graph type with the supplied <paramref name="graphTypeName"/>; otherwise, <c>false</c>.</returns>
        public static bool IsGraphType(
           this ISchemaItem schemaItem,
           string graphTypeName,
           bool graphTypeNameIsCaseSensitive = true)
        {
            return schemaItem != null
                && schemaItem is IGraphType gt
                && string.Compare(gt.Name, graphTypeName, !graphTypeNameIsCaseSensitive) == 0;
        }

        /// <summary>
        /// Determines whether the given <paramref name="schemaItem" /> is
        /// the schema item representing the supplied <paramref name="enumValue" />.
        /// </summary>
        /// <typeparam name="TEnum">The type of the t enum.</typeparam>
        /// <param name="schemaItem">The schema item.</param>
        /// <param name="enumValue">The enum value.</param>
        /// <returns><c>true</c> if the schema item represents the enum value; otherwise, <c>false</c>.</returns>
        public static bool IsEnumValue<TEnum>(this ISchemaItem schemaItem, TEnum enumValue)
            where TEnum : Enum
        {
            return schemaItem != null
                && schemaItem is IEnumValue ev
                && ev.Parent.ObjectType == typeof(TEnum)
                && Enum.Equals(ev.InternalValue, enumValue);
        }

        /// <summary>
        /// Determines whether the schema item is an argument,
        /// on a field, declared on <typeparamref name="TType"/>.
        /// </summary>
        /// <typeparam name="TType">The .NET class that represents
        /// the owning object of this argument.</typeparam>
        /// <param name="schemaItem">The schema item to inspect. </param>
        /// <param name="fieldName">The name of the field on <typeparamref name="TType"/>
        /// where the argument is declared.</param>
        /// <param name="argumentName">The name of the argument as
        /// its declared in the graph.</param>
        /// <param name="fieldNameIsCaseSensitive">
        /// if set to <c>true</c> the field name must match the name in the graph exactly.</param>
        /// <param name="argumentNameIsCaseSensitive">if set to <c>true</c>
        /// the argument name must match the name in the graph exactly.</param>
        /// <param name="parentTypeKind">The kind of object represented by <typeparamref name="TType"/>.</param>
        /// <returns><c>true</c> if the specified schema item is a matching argument; otherwise, <c>false</c>.</returns>
        public static bool IsArgument<TType>(
            this ISchemaItem schemaItem,
            string fieldName,
            string argumentName,
            bool fieldNameIsCaseSensitive = true,
            bool argumentNameIsCaseSensitive = true,
            TypeKind parentTypeKind = TypeKind.OBJECT)
        {
            return schemaItem != null
                && schemaItem is IGraphArgument ga
                && string.Compare(ga.Name, argumentName, !argumentNameIsCaseSensitive) == 0
                && ga.Parent is IGraphField gf
                && string.Compare(gf.Name, fieldName, !fieldNameIsCaseSensitive) == 0
                && gf.Parent is IGraphType gt
                && gt.Kind == parentTypeKind
                && gt is ITypedSchemaItem tsi
                && tsi.ObjectType == typeof(TType);
        }

        /// <summary>
        /// Determines whether the given graph item is a virtual item, or belongs to a vritual item,
        /// created by the library and not by the developer.
        /// </summary>
        /// <param name="item">The item to inspect.</param>
        /// <returns><c>true</c> if this item is virtual, not tied to a real object; otherwise, <c>false</c>.</returns>
        public static bool IsVirtualItem(this ISchemaItem item)
        {
            if (item == null)
                return true;

            if (item is IGraphType gt)
                return gt.IsVirtual;

            if (item is IGraphField gf)
                return gf.Parent.IsVirtualItem();

            if (item is IEnumValue ev)
                return ev.Parent.IsVirtualItem();

            if (item is IGraphArgument ga)
                return ga.Parent.IsVirtualItem();

            return false;
        }
    }
}