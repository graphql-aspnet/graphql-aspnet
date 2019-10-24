// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A representation of data about an object graph type that can be returned from a field.
    /// </summary>
    [DebuggerDisplay("OBJECT: {Name}")]
    public class IntrospectedType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedType"/> class.
        /// </summary>
        /// <param name="ofType">The type this meta type is encapsulating.</param>
        /// <param name="metaKind">The type kind of this meta type (must be LIST or NON_NULL).</param>
        public IntrospectedType(IntrospectedType ofType, TypeKind metaKind)
        {
            Validation.ThrowIfNull(ofType, nameof(ofType));

            if (metaKind != TypeKind.LIST && metaKind != TypeKind.NON_NULL)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(metaKind),
                    $"Only {nameof(TypeKind.LIST)} and {nameof(TypeKind.NON_NULL)} are " +
                    $"acceptable meta {nameof(TypeKind)} enumerations.");
            }

            if (ofType.Kind == TypeKind.NON_NULL && metaKind == TypeKind.NON_NULL)
            {
                throw new GraphTypeDeclarationException(
                    "A non-null type cannot supply a non-null type as its contained type.");
            }

            this.OfType = ofType;
            this.Kind = metaKind;
            this.Publish = true;
            this.Name = null;
            this.Description = null;
            this.Fields = null;
            this.Interfaces = null;
            this.InputFields = null;
            this.PossibleTypes = null;
            this.EnumValues = null;
            this.InputFields = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedType" /> class.
        /// </summary>
        /// <param name="graphType">The graph type this model item will represent.</param>
        public IntrospectedType(IGraphType graphType)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            this.Name = this.GraphType.Name;
            this.Description = this.GraphType.Description;
            this.Kind = this.GraphType.Kind;
            this.Publish = this.GraphType.Publish;
            this.Fields = null;
            this.Interfaces = null;
            this.InputFields = null;
            this.PossibleTypes = null;
            this.EnumValues = null;
            this.InputFields = null;
            this.OfType = null;
        }

        /// <summary>
        /// When overridden in a child class,populates this introspected type using its parent schema to fill in any details about
        /// other references in this instance.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public void Initialize(IntrospectedSchema schema)
        {
            this.LoadFields(schema);
            this.LoadInterfaces(schema);
            this.LoadEnumValues();
            this.LoadInputValues(schema);
            this.LoadPossibleTypes(schema);
        }

        /// <summary>
        /// Loads the fields into this instance for any <see cref="IGraphType"/> that supports them.
        /// </summary>
        /// <param name="schema">The schema.</param>
        private void LoadFields(IntrospectedSchema schema)
        {
            if (!(this.GraphType is IGraphFieldContainer fieldContainer))
                return;

            var fields = new List<IntrospectedField>();
            foreach (var field in fieldContainer.Fields.Where(x => x.Publish))
            {
                IntrospectedType introspectedType = schema.FindIntrospectedType(field.TypeExpression.TypeName);
                introspectedType = Introspection.WrapBaseTypeWithModifiers(introspectedType, field.TypeExpression);
                var introField = new IntrospectedField(field, introspectedType);
                fields.Add(introField);
                introField.Initialize(schema);
            }

            this.Fields = fields;
        }

        /// <summary>
        /// Loads the possible types field for for unions or interfaces.
        /// </summary>
        /// <param name="schema">The schema.</param>
        private void LoadPossibleTypes(IntrospectedSchema schema)
        {
            var possibleTypes = new List<IntrospectedType>();
            if (this.GraphType is IUnionGraphType unionType)
            {
                // find all the graph types in the schema that implement this interface
                foreach (var typeName in unionType.PossibleGraphTypeNames)
                {
                    var foundType = schema.FindIntrospectedType(typeName);
                    if (foundType != null)
                        possibleTypes.Add(foundType);
                }

                this.PossibleTypes = possibleTypes;
            }
            else if (this.GraphType is IInterfaceGraphType interfaceType)
            {
                var graphTypes = schema.FindIntrospectedTypesByInterface(interfaceType.Name);
                foreach (var possibleType in graphTypes)
                {
                    possibleTypes.Add(possibleType);
                }

                this.PossibleTypes = possibleTypes;
            }
        }

        /// <summary>
        /// Loads the interfaces for any graph type that supports them.
        /// </summary>
        /// <param name="schema">The schema.</param>
        private void LoadInterfaces(IntrospectedSchema schema)
        {
            if (this.GraphType is IGraphInterfaceContainer interfaceContainer)
            {
                // populate the interfaces for this object type, if any are defined and exist
                var interfaceList = new List<IntrospectedType>();
                foreach (var ifaceName in interfaceContainer.InterfaceNames)
                {
                    // if the schema doesn't know of the interface, skip it.
                    var introspectedType = schema.FindIntrospectedType(ifaceName);
                    if (introspectedType == null)
                        continue;

                    interfaceList.Add(introspectedType);
                }

                this.Interfaces = interfaceList;
            }
        }

        private void LoadEnumValues()
        {
            if (!(this.GraphType is IEnumGraphType enumType))
                return;

            var list = new List<IntrospectedEnumValue>();
            foreach (var enumValue in enumType.Values.Values)
            {
                var introspectedValue = new IntrospectedEnumValue(enumValue);
                list.Add(introspectedValue);
            }

            this.EnumValues = list;
        }

        private void LoadInputValues(IntrospectedSchema schema)
        {
            if (!(this.GraphType is IInputObjectGraphType inputType))
                return;

            // populate inputFields collection
            // populate the fields for this object type
            var inputFields = new List<IntrospectedInputValueType>();
            foreach (var field in inputType.Fields)
            {
                var introspectedType = schema.FindIntrospectedType(field.TypeExpression.TypeName);
                introspectedType = Introspection.WrapBaseTypeWithModifiers(introspectedType, field.TypeExpression);
                var inputField = new IntrospectedInputValueType(field, introspectedType);
                inputField.Initialize(schema);
                inputFields.Add(inputField);
            }

            this.InputFields = inputFields;
        }

        /// <summary>
        /// Gets the type of the graph.
        /// </summary>
        /// <value>The type of the graph.</value>
        private IGraphType GraphType { get; }

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public string Description { get; }

        /// <summary>
        /// Gets the kind of graph type this instance represents (enum, scalar, object etc.).
        /// </summary>
        /// <value>The kind.</value>
        public TypeKind Kind { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IntrospectedType"/> is published
        /// on client requests or if it should be withheld.
        /// </summary>
        /// <value><c>true</c> if published; otherwise, <c>false</c>.</value>
        public bool Publish { get; }

        /// <summary>
        /// Gets the fields this type exposes if this type is an object type; otherwise null.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyList<IntrospectedField> Fields { get; private set; }

        /// <summary>
        /// Gets the interfaces this type exposes if this type is an object type; otherwise null.
        /// </summary>
        /// <value>The interfaces.</value>
        public IReadOnlyList<IntrospectedType> Interfaces { get; private set; }

        /// <summary>
        /// Gets the possible types this type could represent. Only applys to interfaces and unions; otherwise null.
        /// </summary>
        /// <value>The possible types.</value>
        public IReadOnlyList<IntrospectedType> PossibleTypes { get; private set; }

        /// <summary>
        /// Gets oall the possible enumeration valuess of this type if this is an enum type; otherwise null.
        /// </summary>
        /// <value>The enum values.</value>
        public IReadOnlyList<IntrospectedEnumValue> EnumValues { get; private set; }

        /// <summary>
        /// Gets the fields declared on this type if this type is an input type; otherwise null.
        /// </summary>
        /// <value>The input fields.</value>
        public IReadOnlyList<IntrospectedInputValueType> InputFields { get; private set; }

        /// <summary>
        /// Gets the underlying type of each element in the list type (null if not a list type).
        /// </summary>
        /// <value>The type of the of.</value>
        public IntrospectedType OfType { get; }
    }
}