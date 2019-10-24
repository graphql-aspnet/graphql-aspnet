// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using GraphQL.AspNet.Configuration;

    /// <summary>
    /// A marker interface that can be applied to any class to indicate that the class is a graphql object
    /// and should be included in a schema. This attribute is optional depending on your schema configuration and
    /// naming preferences.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum)]
    public class GraphTypeAttribute : BaseGraphAttribute
    {
        private TemplateDeclarationRequirements _templateDeclarationRequirements = TemplateDeclarationRequirements.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeAttribute"/> class.
        /// </summary>
        public GraphTypeAttribute()
        {
            this.Name = null;
            this.InputName = null;
            this.Publish = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeAttribute"/> class.
        /// </summary>
        /// <param name="name">The name to apply to the primary graph type created from this class.</param>
        public GraphTypeAttribute(string name)
        {
            this.Name = name?.Trim();
            this.Publish = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphTypeAttribute" /> class.
        /// </summary>
        /// <param name="name">The name to apply to the primary graph type created from this class.</param>
        /// <param name="inputName">The name of the apply to the INPUT_OBJECT graph type created from this class.</param>
        public GraphTypeAttribute(string name, string inputName)
        {
            this.Name = name?.Trim();
            this.InputName = inputName?.Trim();
            this.Publish = true;
        }

        /// <summary>
        /// Gets or sets the name to apply to the primary graph type created from this class.
        /// </summary>
        /// <value>The name to use for the OBJECT graph type.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the apply to the INPUT_OBJECT graph type created from this class.
        /// </summary>
        /// <value>The name to use for the INPUT_OBJECT graph type.</value>
        public string InputName { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="FieldDeclarationRequirements"/> property was
        /// updated at some point in this attributes creation.
        /// </summary>
        /// <value><c>true</c> if the requirements were altered; otherwise, <c>false</c>.</value>
        public bool RequirementsWereDeclared { get; private set; }

        /// <summary>
        /// <para>Gets or sets a value indicating which fields in this template the templating engine will attempt
        /// to include in graph types created from your source code. Setting this value will override any schema
        /// configuration settings for this type.
        /// </para>
        /// </summary>
        /// <value>The declaration requirements for this Type.</value>
        public TemplateDeclarationRequirements FieldDeclarationRequirements
        {
            get => _templateDeclarationRequirements;
            set
            {
                // can't use Nullable<T> in attribute declarations so set a secondary value to
                // know if the user's code altered this value.
                _templateDeclarationRequirements = value;
                this.RequirementsWereDeclared = true;
            }
        }

        /// <summary>
        /// <para>Gets or sets a value indicating whether this graph type will not be auto included into a given schema. When true, any assembly scans or
        /// undeclared type additions to schemas will ignore this type. This is useful for any "input only" object types (TypeKind: INPUT_OBJECT) to prevent
        /// them from being mistakenly added as a normal object. Setting this value to true will override all other inclusion configuration options.</para>
        /// <para>If this type is referenced in a field added to the schema it will still be added
        /// as a valid INPUT_OBJECT.</para>
        /// </summary>
        /// <value><c>true</c> to prevent the engine from attempting to automatically include this type in a schema; otherwise, <c>false</c>.</value>
        public bool PreventAutoInclusion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the graph types generated
        /// from this template are to be published in an introspected schema query (Default: true).
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public bool Publish { get; set; }
    }
}