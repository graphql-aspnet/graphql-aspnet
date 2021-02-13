// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A set of constants known to the graphql library.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Initializes static members of the <see cref="Constants"/> class.
        /// </summary>
        static Constants()
        {
        }

        /// <summary>
        /// Constants related to the graphql logging framework.
        /// </summary>
        public static class Logging
        {
            /// <summary>
            /// A category of log entries generated during the execution of a user's operation/request.
            /// </summary>
            public const string LOG_CATEGORY = "GraphQL.AspNet";
        }

        /// <summary>
        /// Constants related to messaging and message serialziation.
        /// </summary>
        public static class Messaging
        {
            /// <summary>
            /// The key value used when writing a url containing a link to a violated specification rule
            /// to a message's metadata collection.
            /// </summary>
            public const string REFERENCE_RULE_URL = "RuleReference";

            /// <summary>
            /// The key value used when writing a value containing the name or number of a violated specification
            /// rule to a message's metadata collection.
            /// </summary>
            public const string REFERENCE_RULE_NUMBER = "Rule";
        }

        /// <summary>
        /// Gets a collection of types that, when scanning an assembly to load <see cref="IGraphType" /> items, the base types in this
        /// collection will be searched for. (e.g. <see cref="GraphController"/>, <see cref="GraphDirective"/> etc.)
        /// </summary>
        /// <value>The assembly scan types.</value>
        public static Type[] AssemblyScanTypes { get; } =
        {
            typeof(GraphController),
            typeof(GraphDirective),
            typeof(BaseGraphAttribute),
        };

        /// <summary>
        /// Gets a collection of restricted types that cannot be returned from any graph field...ever.
        /// </summary>
        /// <value>The invalid template types.</value>
        public static Type[] InvalidFieldTemplateTypes { get; } =
        {
            typeof(IGraphUnionProxy),
            typeof(Attribute),
            typeof(GraphController),
            typeof(GraphDirective),
        };

        /// <summary>
        /// Gets a collection of globally known directives that will be added to all schema's by default.
        /// This is the @skip and @include directive's required by graphql.
        /// </summary>
        /// <value>The global directives.</value>
        public static IReadOnlyList<Type> GlobalDirectives { get; } = new List<Type>()
        {
            typeof(SkipDirective),
            typeof(IncludeDirective),
        };

        /// <summary>
        /// A collection of common suffixes that are semantically handled or removed from naming.
        /// </summary>
        public static class CommonSuffix
        {
            /// <summary>
            /// A phrase that if found in a controller group name is automatically removed.
            /// </summary>
            public const string CONTROLLER_SUFFIX = "Controller";

            /// <summary>
            /// A phrase that, if found on a directive, is automatically removed.
            /// </summary>
            public const string DIRECTIVE_SUFFIX = "Directive";
        }

        /// <summary>
        /// Common error codes used in graph resolution errors.
        /// </summary>
        public static class ErrorCodes
        {
            public const string INVALID_BATCH_RESULT = "INVALID_BATCH_RESULT";
            public const string FIELD_REQUEST_ABORTED = "FIELD_REQUEST_ABORTED";
            public const string REQUEST_ABORTED = "REQUEST_ABORTED";
            public const string INVALID_ROUTE = "INVALID_ROUTE";
            public const string MODEL_VALIDATION_ERROR = "MODEL_VALIDATION_ERROR";
            public const string BAD_REQUEST = "BAD_REQUEST";
            public const string EXECUTION_ERROR = "EXECUTION_ERROR";
            public const string SYNTAX_ERROR = "SYNTAX_ERROR";
            public const string UNHANDLED_EXCEPTION = "UNHANDLED_EXCEPTION";
            public const string ACCESS_DENIED = "ACCESS_DENIED";
            public const string OPERATION_CANCELED = "OPERATION_CANCELED";
            public const string INVALID_DOCUMENT = "INVALID_DOCUMENT";
            public const string INVALID_OBJECT = "INVALID_OBJECT";
            public const string DEFAULT = "UNKNOWN";
            public const string GENERAL_ERROR = "GENERAL_ERROR";
            public const string INVALID_ARGUMENT = "INVALID_ARGUMENT";
        }

        /// <summary>
        /// The graph type names for all natively supported scalar types.
        /// </summary>
        public static class ScalarNames
        {
            public const string INT = "Int";
            public const string LONG = "Long";
            public const string UINT = "UInt";
            public const string ULONG = "ULong";
            public const string FLOAT = "Float";
            public const string DOUBLE = "Double";
            public const string DECIMAL = "Decimal";
            public const string BOOLEAN = "Boolean";
            public const string STRING = "String";
            public const string DATETIME = "DateTime";
            public const string DATETIMEOFFSET = "DateTimeOffset";
            public const string BYTE = "Byte";
            public const string SIGNED_BYTE = "SignedByte";
            public const string GUID = "Guid";
            public const string URI = "Uri";
            public const string ID = "ID";
        }

        /// <summary>
        /// The reserved names for various introspection entities in GraphQL.
        /// </summary>
        public static class ReservedNames
        {
            /// <summary>
            /// Gets a collection of reserved names, defined by the graphql schema, that
            /// may appear as route path segments in an introspection query but are otherwise
            /// not allowed by user created controllers or types.
            /// </summary>
            /// <value>A read only hashset of all the known reserved names.</value>
            public static IImmutableSet<string> IntrospectableRouteNames { get; }

            // directives
            public const string SKIP_DIRECTIVE = "skip";
            public const string INCLUDE_DIRECTIVE = "include";
            public const string DIRECTIVE_BEFORE_RESOLUTION_METHOD_NAME = "BeforeFieldResolution";
            public const string DIRECTIVE_AFTER_RESOLUTION_METHOD_NAME = "AfterFieldResolution";

            // type names for top level operation types
            public const string QUERY_TYPE_NAME = "Query";
            public const string MUTATION_TYPE_NAME = "Mutation";
            public const string SUBSCRIPTION_TYPE_NAME = "Subscription";

            // introspection type and input value names
            public const string DEPRECATED_ARGUMENT_NAME = "includeDeprecated";
            public const string DIRECTIVE_LOCATION_ENUM = "__DirectiveLocation";
            public const string TYPE_KIND_ENUM = "__TypeKind";
            public const string DIRECTIVE_TYPE = "__Directive";
            public const string ENUM_VALUE_TYPE = "__EnumValue";
            public const string INPUT_VALUE_TYPE = "__InputValue";
            public const string FIELD_TYPE = "__Field";
            public const string TYPE_TYPE = "__Type";
            public const string SCHEMA_TYPE = "__Schema";

            public const string SCHEMA_FIELD = "__schema";
            public const string TYPE_FIELD = "__type";
            public const string TYPENAME_FIELD = "__typename";

            private static readonly IReadOnlyDictionary<GraphCollection, string> GRAPH_OPERATION_TYPE_NAME_BY_TYPE;
            private static readonly IReadOnlyDictionary<string, GraphCollection> GRAPH_OPERATION_TYPE_BY_TYPE_NAME;
            private static readonly IReadOnlyDictionary<string, GraphCollection> GRAPH_OPERATION_TYPE_BY_KEYWORD;

            /// <summary>
            /// Inspects the known operation graph type for a name matching the provided value returning it when found.
            /// </summary>
            /// <param name="operationTypeName">Name of the operation graph type as it exists in a schema (e.g. Query, Mutation).</param>
            /// <returns>GraphCollection.</returns>
            public static GraphCollection FindOperationTypeByTypeName(string operationTypeName)
            {
                if (GRAPH_OPERATION_TYPE_BY_TYPE_NAME.ContainsKey(operationTypeName))
                    return GRAPH_OPERATION_TYPE_BY_TYPE_NAME[operationTypeName];

                return GraphCollection.Unknown;
            }

            /// <summary>
            /// Inspects the known operation types for a name matching the provided value returning it when found.
            /// </summary>
            /// <param name="operationType">Type of the operation to retrieve the graph type name for.</param>
            /// <returns>GraphCollection.</returns>
            public static string FindOperationTypeNameByType(GraphCollection operationType)
            {
                if (GRAPH_OPERATION_TYPE_NAME_BY_TYPE.ContainsKey(operationType))
                    return GRAPH_OPERATION_TYPE_NAME_BY_TYPE[operationType];

                return string.Empty;
            }

            /// <summary>
            /// Inspects the known operation types for a name matching the provided value returning it when found.
            /// </summary>
            /// <param name="operationKeyword">The operation keyword as it exists in a query document (e.g. query, mutation).</param>
            /// <returns>GraphCollection.</returns>
            public static GraphCollection FindOperationTypeByKeyword(string operationKeyword)
            {
                if (string.IsNullOrEmpty(operationKeyword))
                    return GraphCollection.Query;

                if (GRAPH_OPERATION_TYPE_BY_KEYWORD.ContainsKey(operationKeyword))
                    return GRAPH_OPERATION_TYPE_BY_KEYWORD[operationKeyword];

                return GraphCollection.Unknown;
            }

            /// <summary>
            /// Initializes static members of the <see cref="ReservedNames"/> class.
            /// </summary>
            static ReservedNames()
            {
                var dicOperationType = new Dictionary<string, GraphCollection>();
                dicOperationType.Add(ParserConstants.Keywords.Query.ToString(), GraphCollection.Query);
                dicOperationType.Add(ParserConstants.Keywords.Mutation.ToString(), GraphCollection.Mutation);
                dicOperationType.Add(ParserConstants.Keywords.Subscription.ToString(), GraphCollection.Subscription);
                GRAPH_OPERATION_TYPE_BY_KEYWORD = dicOperationType;

                var dicTypeToTypeName = new Dictionary<GraphCollection, string>();
                dicTypeToTypeName.Add(GraphCollection.Query, QUERY_TYPE_NAME);
                dicTypeToTypeName.Add(GraphCollection.Mutation, MUTATION_TYPE_NAME);
                dicTypeToTypeName.Add(GraphCollection.Subscription, SUBSCRIPTION_TYPE_NAME);
                GRAPH_OPERATION_TYPE_NAME_BY_TYPE = dicTypeToTypeName;

                var dicTypeNameToType = new Dictionary<string, GraphCollection>();
                foreach (var entry in dicTypeToTypeName)
                    dicTypeNameToType.Add(entry.Value, entry.Key);

                GRAPH_OPERATION_TYPE_BY_TYPE_NAME = dicTypeNameToType;

                IntrospectableRouteNames = ImmutableHashSet.Create(
                            QUERY_TYPE_NAME,
                            MUTATION_TYPE_NAME,
                            SUBSCRIPTION_TYPE_NAME,
                            DEPRECATED_ARGUMENT_NAME,
                            DIRECTIVE_LOCATION_ENUM,
                            TYPE_KIND_ENUM,
                            DIRECTIVE_TYPE,
                            ENUM_VALUE_TYPE,
                            INPUT_VALUE_TYPE,
                            FIELD_TYPE,
                            TYPE_TYPE,
                            SCHEMA_TYPE,
                            SCHEMA_FIELD,
                            TYPE_FIELD,
                            TYPENAME_FIELD);
            }
        }

        /// <summary>
        /// A set of known, custom headers used by this library.
        /// </summary>
        public static class ServerInformation
        {
            /// <summary>
            /// The header value for the server information header.
            /// </summary>
            public const string SERVER_INFORMATION_HEADER = "X-GraphQL-AspNet-Server";

            /// <summary>
            /// Initializes static members of the <see cref="ServerInformation"/> class.
            /// </summary>
            static ServerInformation()
            {
                var assembly = Assembly.GetAssembly(typeof(Constants));
                ServerData = $"v{assembly.GetName().Version}";
            }

            /// <summary>
            /// Gets the server information string identifying this graphql server.
            /// </summary>
            /// <value>The server information.</value>
            public static string ServerData { get; }
        }

        /// <summary>
        /// Constants pertaining to the setup, creation, usage or iteration of routes within the graph structure.
        /// </summary>
        public static class Routing
        {
            /// <summary>
            /// The endpoint to which graphql will register its default schema.
            /// </summary>
            public const string DEFAULT_HTTP_ROUTE = "/graphql";

            /// <summary>
            /// A phrase that will be subsituted to the actual class name of the controller at run time.
            /// </summary>
            public const string CONTOLLER_META_NAME = "[controller]";

            /// <summary>
            /// A phrase that will be subsituted to the actual class name of the object at run time.
            /// </summary>
            public const string CLASS_META_NAME = "[class]";

            /// <summary>
            /// A phrase that will be subsituted with the actual name of the action method at run time.
            /// </summary>
            public const string ACTION_METHOD_META_NAME = "[action]";

            /// <summary>
            /// A phrase that will be subsituted with the actual enum value name at run time.
            /// </summary>
            public const string ENUM_VALUE_META_NAME = "[value]";

            /// <summary>
            /// a phrase that will be subsituted with the actual parameter name at run time.
            /// </summary>
            public const string PARAMETER_META_NAME = "[parameter]";

            /// <summary>
            /// A phrase, used at the start of a route string, to indicate the route should not map
            /// into the graph.
            /// </summary>
            public const string NOOP_ROOT = DELIMITER_ROOT_START + "noop" + DELIMITER_ROOT_END;

            /// <summary>
            /// A phrase, used at the start of a route string, to indicate its part of the query root.
            /// </summary>
            public const string QUERY_ROOT = DELIMITER_ROOT_START + "query" + DELIMITER_ROOT_END;

            /// <summary>
            /// A phrase, used at the start of a route string, to indicate its part of the mutation root.
            /// </summary>
            public const string MUTATION_ROOT = DELIMITER_ROOT_START + "mutation" + DELIMITER_ROOT_END;

            /// <summary>
            /// A phrase, used at the start of a route string, to indicate its part of the object type tree.
            /// </summary>
            public const string TYPE_ROOT = DELIMITER_ROOT_START + "type" + DELIMITER_ROOT_END;

            /// <summary>
            /// A phrase, used at the start of a route string, to indicate its part of the subscription root.
            /// </summary>
            public const string SUBSCRIPTION_ROOT = DELIMITER_ROOT_START + "subscription" + DELIMITER_ROOT_END;

            /// <summary>
            /// A phrase, used at the start of a route string, to indicate its part of the enum type tree.
            /// </summary>
            public const string ENUM_ROOT = DELIMITER_ROOT_START + "enum" + DELIMITER_ROOT_END;

            /// <summary>
            /// A phrase, used at the start of a route string, to indicate its part of the directive tree.
            /// </summary>
            public const string DIRECTIVE_ROOT = DELIMITER_ROOT_START + "directive" + DELIMITER_ROOT_END;

            /// <summary>
            /// The phrase used to seperate individual elements of a route fragement.
            /// </summary>
            public const string PATH_SEPERATOR = "/";

            /// <summary>
            /// A potentially, misused alternate path seperator. Will be automatically replaced with the correct seperator
            /// at runtime.
            /// </summary>
            public const string ALT_PATH_SEPERATOR = "\\";

            /// <summary>
            /// The character used to denote the start of a root path phrase.
            /// </summary>
            public const string DELIMITER_ROOT_START = "[";

            /// <summary>
            /// The character used to denote the end of a root path phrase.
            /// </summary>
            public const string DELIMITER_ROOT_END = "]";

            /// <summary>
            /// A single string containing all used special characters in <see cref="GraphFieldPath"/> objects.
            /// </summary>
            public const string DELIMITERS_ALL = PATH_SEPERATOR + DELIMITER_ROOT_START + DELIMITER_ROOT_END;

            /// <summary>
            /// The path seperator phrase, doubled.
            /// </summary>
            public const string DOUBLE_PATH_SEPERATOR = PATH_SEPERATOR + PATH_SEPERATOR;
        }

        /// <summary>
        /// A collection common, often used regex objects.
        /// </summary>
        public static class RegExPatterns
        {
            /// <summary>
            /// A regex containing the rules for parsing a graphql name.
            /// Spec: https://graphql.github.io/graphql-spec/June2018/#sec-Appendix-Grammar-Summary.Lexical-Tokens .
            /// </summary>
            public static readonly Regex NameRegex = new Regex(@"^([_A-Za-z][0-9A-Za-z]|[0-9A-Za-z]+)[_0-9A-Za-z]*$");
        }

        /// <summary>
        /// A collection of media typws used in this application.
        /// </summary>
        public static class MediaTypes
        {
            /// <summary>
            /// A mediatype/mimetype name for a json text document.
            /// </summary>
            public const string JSON = "application/json";
        }

        /// <summary>
        /// The default names given to each of the 3 pipelines registered per schema.
        /// </summary>
        public static class Pipelines
        {
            /// <summary>
            /// The primary query pipeline invoked to process an HTTP Request through the graphql runtime.
            /// </summary>
            public const string QUERY_PIPELINE = "Query Execution Pipeline";

            /// <summary>
            /// The pipeline that is executed to resolve a request for a specific field of data in a
            /// larger query operation.
            /// </summary>
            public const string FIELD_EXECUTION_PIPELINE = "Field Execution Pipeline";

            /// <summary>
            /// The pipeline, invoked as a child of the main query or each field (depending on confiuration) that will
            /// authorize a request to a field for the given context.
            /// </summary>
            public const string FIELD_AUTHORIZATION_PIPELINE = "Field Authorization Pipeline";
        }

        /// <summary>
        /// Gets a URL pointing to the page of the graphql specification this library
        /// targets. This value is used as a base url for most validation rules to generate
        /// a link pointing to a violated rule.
        /// </summary>
        public const string SPECIFICATION_URL = "http://spec.graphql.org/June2018/";
    }
}