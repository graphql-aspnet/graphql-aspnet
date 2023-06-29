﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration.Templates
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An abstract class containing all the common elements across minimal field builders and
    /// their supporting classes.
    /// </summary>
    internal abstract class BaseRuntimeControllerActionDefinition : IGraphQLRuntimeSchemaItemDefinition
    {
        private readonly IGraphQLRuntimeFieldDefinition _parentField;

        /// <summary>
        /// Prevents a default instance of the <see cref="BaseRuntimeControllerActionDefinition"/> class from being created.
        /// </summary>
        private BaseRuntimeControllerActionDefinition()
        {
            this.AppendedAttributes = new List<Attribute>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRuntimeControllerActionDefinition" /> class.
        /// </summary>
        /// <param name="options">The schema options where this field item
        /// is being defined.</param>
        /// <param name="route">The full route to use for this schema item.</param>
        protected BaseRuntimeControllerActionDefinition(
            SchemaOptions options,
            SchemaItemPath route)
            : this()
        {
            this.Options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRuntimeControllerActionDefinition" /> class.
        /// </summary>
        /// <param name="options">The schema options where this field item
        /// is being defined.</param>
        /// <param name="collection">The schema collection this item will belong to.</param>
        /// <param name="pathTemplate">The path template identifying this item.</param>
        protected BaseRuntimeControllerActionDefinition(
            SchemaOptions options,
            SchemaItemCollections collection,
            string pathTemplate)
            : this()
        {
            this.Options = Validation.ThrowIfNullOrReturn(options, nameof(options));
            pathTemplate = pathTemplate?.Trim() ?? string.Empty;

            this.Route = new SchemaItemPath(collection, pathTemplate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRuntimeControllerActionDefinition"/> class.
        /// </summary>
        /// <param name="parentField">The field from which this entity is being added.</param>
        /// <param name="partialPathTemplate">The partial path template defined for this
        /// individual entity.</param>
        protected BaseRuntimeControllerActionDefinition(
            IGraphQLRuntimeFieldDefinition parentField,
            string partialPathTemplate)
            : this()
        {
            _parentField = Validation.ThrowIfNullOrReturn(parentField, nameof(parentField));
            this.Options = Validation.ThrowIfNullOrReturn(parentField?.Options, nameof(parentField.Options));

            partialPathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(partialPathTemplate, nameof(partialPathTemplate));

            this.Route = _parentField.Route.CreateChild(partialPathTemplate);
        }

        /// <inheritdoc />
        public virtual void AddAttribute(Attribute attrib)
        {
            Validation.ThrowIfNull(attrib, nameof(attrib));
            this.AppendedAttributes.Add(attrib);
        }

        /// <summary>
        /// Creates the primary attribute that would identify this instance if it was defined on
        /// a controller.
        /// </summary>
        /// <returns>The primary attribute.</returns>
        protected abstract Attribute CreatePrimaryAttribute();

        /// <inheritdoc />
        public virtual IEnumerable<Attribute> Attributes
        {
            get
            {
                var combinedAttribs = new List<Attribute>(1 + this.AppendedAttributes.Count);
                var definedTypes = new HashSet<Type>();

                // apply the attributes defined on the parent (and parent's parents)
                // FIRST to mimic controller level attribs being encountered before action method params.
                if (_parentField != null)
                {
                    foreach (var attrib in _parentField.Attributes)
                    {
                        if (!definedTypes.Contains(attrib.GetType()) || attrib.CanBeAppliedMultipleTimes())
                        {
                            combinedAttribs.Add(attrib);
                            definedTypes.Add(attrib.GetType());
                        }
                    }
                }

                // apply the primary attribute first as its defined by this
                // exact instance
                var topAttrib = this.CreatePrimaryAttribute();
                if (topAttrib != null)
                {
                    combinedAttribs.Add(topAttrib);
                    definedTypes.Add(topAttrib.GetType());
                }

                // apply all the secondary attributes defined directly on this instance
                foreach (var attrib in this.AppendedAttributes)
                {
                    if (!definedTypes.Contains(attrib.GetType()) || attrib.CanBeAppliedMultipleTimes())
                    {
                        combinedAttribs.Add(attrib);
                        definedTypes.Add(attrib.GetType());
                    }
                }

                return combinedAttribs;
            }
        }

        /// <summary>
        /// Gets a list of attributes that were directly appended to this instance.
        /// </summary>
        /// <value>The appended attributes.</value>
        protected List<Attribute> AppendedAttributes { get; }

        /// <inheritdoc />
        public virtual SchemaOptions Options { get; protected set; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; protected set; }
    }
}