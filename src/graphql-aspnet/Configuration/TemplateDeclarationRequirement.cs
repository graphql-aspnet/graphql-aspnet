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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A set of potential requirements enforcing template field declarations
    /// on various objects that will be included in the object graph.
    /// </summary>
    [Flags]
    [GraphSkip]
    public enum TemplateDeclarationRequirements
    {
        /// <summary>
        /// Indicates that no items require explicit declaration across the board. This option cannot be combined with any
        /// other requirement settings.
        /// </summary>
        None = 0,

        /// <summary>
        /// <para>When set, indicates that public properties on classes added to a
        /// <see cref="ISchema"/> are REQUIRED to have <see cref="GraphFieldAttribute"/> to be included in the type system.</para>
        /// <para>When not set, properties that conform to the requirements of being a graphql field
        /// are automatically added as fields to the type system.</para>
        /// </summary>
        Property = 1,

        /// <summary>
        /// <para>When set, indicates that public methods on classes added to a
        /// <see cref="ISchema"/> are REQUIRED to have <see cref="GraphFieldAttribute"/> to be included in the type system.</para>
        /// <para>When not set, methods that conform to the requirements of being a graphql field are
        /// automatically added as fields to the type system.</para>
        ///
        /// <para>Note: this only applies to standard common model classes. Controller actions, by definition, are required to decalre a <see cref="GraphFieldAttribute"/> to be included.</para>
        /// </summary>
        Method = 2,

        /// <summary>
        /// <para>When set, inidcates that values of enumerations added to a
        /// <see cref="ISchema"/> are REQUIRED to have an <see cref="GraphEnumValueAttribute"/> to be included in the type system.</para>
        /// <para>When not set,all declared values of the enumeration will be added and available to the type system.</para>
        /// </summary>
        EnumValue = 4,

        /// <summary>
        /// The system default field declaration requirement options.
        /// <para>DO REQUIRE Method Declaration</para>
        /// <para>DO NOT REQUIRE Property Declaration</para>
        /// <para>DO NOT REQUIRE Enum Value Declaration</para>
        /// </summary>
        Default = Method,

        /// <summary>
        /// <para>DO REQUIRE Method Declaration</para>
        /// <para>DO REQUIRE Property Declaration</para>
        /// <para>DO NOT REQUIRE Enum Value Declaration</para>
        /// </summary>
        RequireMethodAndProperties = Method | Property,

        /// <summary>
        /// When set, requires explicit declaration across the board on all items. The templating engine
        /// will make no assumptions about what it should include. This is the most restrictive and most
        /// secure option.
        /// </summary>
        RequireAll = Property | Method | EnumValue,
    }
}