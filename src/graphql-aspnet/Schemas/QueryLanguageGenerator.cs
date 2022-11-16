// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Converts supplied objects to strings representing how they would
    /// be defined if supplied in a query document or type system definition document.
    /// </summary>
    public class QueryLanguageGenerator
    {
        private ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryLanguageGenerator"/> class.
        /// </summary>
        /// <param name="schema">The schema to use when determining defined fields, serializers and other name
        /// formatters used during the serialization.</param>
        public QueryLanguageGenerator(ISchema schema)
        {
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
        }

        /// <summary>
        /// Converts the supplied .NET object to a string representing the object as defined
        /// by the schema.
        /// </summary>
        /// <remarks>
        /// The object must represent a valid SCALAR, ENUM or INPUT_OBJECT else an <see cref="InvalidOperationException"/> will be thrown.
        /// </remarks>
        /// <param name="obj">The object to convert.</param>
        /// <returns>A string representation of the object in graphql query language.</returns>
        public string SerializeObject(object obj)
        {
            return SerializeObject(obj, null);
        }

        private string SerializeObject(object obj, HashSet<object> recursionStack)
        {
            if (obj == null)
                return Constants.QueryLanguage.NULL;

            var objType = obj.GetType();

            if (GraphValidation.IsValidListType(obj.GetType()))
                return this.SerailizeList(obj as IEnumerable);

            if (!GraphValidation.IsValidGraphType(objType))
            {
                throw new InvalidOperationException(
                    $"The supplied object type '{objType.FriendlyName()}' does not represent an object that could ever be a graph type. " +
                    $"Only object types that represent an {TypeKind.INPUT_OBJECT}, {TypeKind.SCALAR} or {TypeKind.ENUM} can be converted.");
            }

            var graphType = _schema.KnownTypes.FindGraphType(objType, TypeKind.INPUT_OBJECT);
            switch (graphType)
            {
                case IInputObjectGraphType _:
                case IScalarGraphType _:
                case IEnumGraphType _:
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unable to locate a valid graph type for an entity of type '{obj.GetType().FriendlyName()}' on the schema '{_schema.Name}'. " +
                        $"Only known {TypeKind.INPUT_OBJECT}, {TypeKind.SCALAR} and {TypeKind.ENUM} types can be converted.");
            }

            if (!graphType.ValidateObject(obj))
                return Constants.QueryLanguage.NULL;

            if (graphType is IScalarGraphType sgt)
                return sgt.SerializeToQueryLanguage(obj);

            if (graphType is IEnumGraphType egt)
            {
                var enumValue = egt.Values.FindByEnumValue(obj);
                if (enumValue != null)
                    return enumValue.Name;

                throw new InvalidOperationException(
                      $"The enum graph type does not declare a label for the supplied value of '{obj}'. " +
                      $"Only declared values on the enum graph type can be serialized.");
            }
            else if (graphType is IInputObjectGraphType iogt)
            {
                return this.SerializeInputObject(obj, iogt, recursionStack);
            }

            return Constants.QueryLanguage.NULL;
        }

        private string SerailizeList(IEnumerable list)
        {
            var builder = new StringBuilder();
            builder.Append("[");

            if (list != null)
            {
                var listStarted = false;
                foreach (var item in list)
                {
                    if (listStarted)
                        builder.Append(", ");

                    builder.Append(this.SerializeObject(item));
                    listStarted = true;
                }
            }

            builder.Append("]");
            return builder.ToString();
        }

        private string SerializeInputObject(object obj, IInputObjectGraphType inputObjectGraphType, HashSet<object> recursionStack = null)
        {
            if (obj == null || inputObjectGraphType == null)
                return Constants.QueryLanguage.NULL;

            recursionStack = recursionStack ?? new HashSet<object>();

            if (recursionStack.Contains(obj))
            {
                throw new InvalidOperationException(
                    $"Circular reference detected. Unable to convert an object of graph type " +
                    $"'{inputObjectGraphType.Name}'(Kind: {inputObjectGraphType.Kind}) to query language syntax due to a self referencing child.");
            }

            recursionStack.Add(obj);

            var builder = new StringBuilder("{ ");
            var getters = InstanceFactory.CreatePropertyGetterInvokerCollection(obj.GetType());
            foreach (var field in inputObjectGraphType.Fields)
            {
                if (!getters.ContainsKey(field.InternalName))
                    continue;

                var getter = getters[field.InternalName];
                builder.Append(field.Name);
                builder.Append(Constants.QueryLanguage.FieldValueSeperator);
                builder.Append(" ");
                builder.Append(this.SerializeObject(getter.Invoke(ref obj), recursionStack));
                builder.Append(" ");
            }

            builder.Append("}");

            recursionStack.Remove(obj);
            return builder.ToString();
        }
    }
}