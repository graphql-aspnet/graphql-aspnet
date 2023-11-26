// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeMakers
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;

    /// <summary>
    /// A base type maker for those types that generate fields. Used to centralize common code.
    /// </summary>
    public abstract class FieldContainerGraphTypeMakerBase
    {
        private readonly ISchemaConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldContainerGraphTypeMakerBase"/> class.
        /// </summary>
        /// <param name="config">The configuration data to use when generating
        /// field items.</param>
        public FieldContainerGraphTypeMakerBase(ISchemaConfiguration config)
        {
            _config = Validation.ThrowIfNullOrReturn(config, nameof(config));
        }

        /// <summary>
        /// Creates the collection of graph fields that belong to the template.
        /// </summary>
        /// <param name="template">The template to generate fields for.</param>
        /// <returns>IEnumerable&lt;IGraphField&gt;.</returns>
        protected virtual IEnumerable<IGraphFieldTemplate> GatherFieldTemplates(IGraphTypeFieldTemplateContainer template)
        {
            // gather the fields to include in the graph type
            var requiredDeclarations = template.DeclarationRequirements ?? _config.DeclarationOptions.FieldDeclarationRequirements;

            return template.FieldTemplates.Where(x =>
            {
                if (x.IsExplicitDeclaration)
                    return true;

                switch (x.FieldSource)
                {
                    case GraphFieldSource.Method:
                        return requiredDeclarations.AllowImplicitMethods();

                    case GraphFieldSource.Property:
                        return requiredDeclarations.AllowImplicitProperties();
                }

                return false;
            });
        }
    }
}