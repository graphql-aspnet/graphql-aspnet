// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Configuration
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An interface defining the various configuration options available for setting up your <see cref="ISchema"/>.
    /// </summary>
    public interface ISchemaDeclarationConfiguration
    {
        /// <summary>
        /// Gets a value indicating whether to disable introspection queries. When disabled,
        /// queries against "__schema" and "__type" will generate a server error. In addition to the obfuscation, this
        /// may provide a performance boost for some large schemas as the initialization will not attempt to
        /// extract and configure metadata from each scalar, object, method and property added to the schema.
        /// (Default: false).
        /// </summary>
        /// <value><c>true</c> if introspection meta fields should NOT be included in the schema; otherwise, <c>false</c>.</value>
        bool DisableIntrospection { get; }

        /// <summary>
        /// <para>Gets a value indicating which fields the templating engine will attempt to include in graph types created from your source code.
        /// You can alter how methods, properties, enum values etc. are injected into your schema by changing the declaration requirements. Setting
        /// this property will affect all graph types for your schema unless they declare an explicit override with the <see cref="GraphTypeAttribute"/>.
        /// </para>
        /// <para>(Default: Require Method Declaration).</para>
        /// </summary>
        /// <value>The declaration requirements for this schema.</value>
        TemplateDeclarationRequirements FieldDeclarationRequirements { get; }

        /// <summary>
        /// <para>Gets an object used to format the declared names in your C# code as various items in the type system
        /// for this <see cref="ISchema" />.
        /// </para>
        ///
        /// <para>
        /// Defaults:
        /// Graph Type Names : ProperCase.
        /// Field Names: camelCase.
        /// Enum Values: UPPERCASE.
        /// </para>
        /// </summary>
        /// <value>The graph naming formatter.</value>
        GraphNameFormatter GraphNamingFormatter { get; }

        /// <summary>
        /// Gets the set of operation types that can be registered to this schema.
        /// </summary>
        /// <value>The allowed operations.</value>
        HashSet<GraphCollection> AllowedOperations { get; }
    }
}