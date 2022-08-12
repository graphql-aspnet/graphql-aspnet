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
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Converts supplied objects to strings representing how they would
    /// be defined if supplied in a query document or type system definition document.
    /// </summary>
    public class QueryLanguageGenerator
    {
        /// <summary>
        /// Converts the supplied .NET object to a string representing the object as defined
        /// by the <paramref name="schema"/>.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <param name="schema">The schema to use when determining various values to be
        /// converted.</param>
        /// <returns>A string representation of the object in graphql query language.</returns>
        public static string SerializeObject(object obj, ISchema schema)
        {
            return SerializeObject(obj, schema, null);
        }

        private static string SerializeObject(object obj, ISchema schema, HashSet<object> recursionStack)
        {
            if (obj == null)
                return Constants.QueryLanguage.NULL;

            Validation.ThrowIfNull(schema, nameof(schema));

            var graphType = schema.KnownTypes.FindGraphType(obj.GetType(), TypeKind.INPUT_OBJECT);

            switch (graphType)
            {
                case IInputObjectGraphType _:
                case IScalarGraphType _:
                case IEnumGraphType _:
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unable to locate a valid graph type for an object of type '{obj.GetType().FriendlyName()}' on the schema '{schema.Name}'. " +
                        $"Only {TypeKind.INPUT_OBJECT}, {TypeKind.SCALAR} and {TypeKind.ENUM} types can be converted.");
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
            }
            else if (graphType is IInputObjectGraphType iogt)
            {
                return QueryLanguageGenerator.SerializeInputObject(obj, iogt, schema, recursionStack);
            }

            return Constants.QueryLanguage.NULL;
        }

        private static string SerializeInputObject(object obj, IInputObjectGraphType inputObjectGraphType, ISchema schema, HashSet<object> recursionStack = null)
        {
            if (obj == null || inputObjectGraphType == null)
                return Constants.QueryLanguage.NULL;

            recursionStack = recursionStack ?? new HashSet<object>();

            if (recursionStack.Contains(obj))
            {
                throw new GraphExecutionException(
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
                builder.Append(SerializeObject(getter.Invoke(ref obj), schema, recursionStack));
                builder.Append(" ");
            }

            builder.Append("}");

            recursionStack.Remove(obj);
            return builder.ToString();
        }
    }
}