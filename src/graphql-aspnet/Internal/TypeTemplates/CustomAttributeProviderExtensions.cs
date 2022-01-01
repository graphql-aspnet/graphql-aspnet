// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System.Collections.Generic;
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// Helper methods for <see cref="IGraphItemTemplate"/>.
    /// </summary>
    internal static class CustomAttributeProviderExtensions
    {
        /// <summary>
        /// Retrieves the directive types declared on this item template.
        /// </summary>
        /// <param name="itemTemplate">The item template which may contain applied directives.</param>
        /// <returns>IEnumerable&lt;AppliedDirectiveTemplate&gt;.</returns>
        public static IEnumerable<IAppliedDirectiveTemplate> ExtractAppliedDirectiveTemplates(this IGraphItemTemplate itemTemplate)
        {
            return itemTemplate.AttributeProvider.ExtractAppliedDirectiveTemplates(itemTemplate);
        }

        /// <summary>
        /// Retrieves the directive types declared on this item template.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider.</param>
        /// <param name="owner">The owner of the created templates.</param>
        /// <returns>IEnumerable&lt;AppliedDirectiveTemplate&gt;.</returns>
        public static IEnumerable<IAppliedDirectiveTemplate> ExtractAppliedDirectiveTemplates(this ICustomAttributeProvider attributeProvider, INamedItem owner)
        {
            var directiveList = new List<IAppliedDirectiveTemplate>();

            var directiveAttribs = attributeProvider
                                    .AttributesOfType<ApplyDirectiveAttribute>();

            foreach (var directiveAttrib in directiveAttribs)
            {
                var template = new AppliedDirectiveTemplate(
                    owner,
                    directiveAttrib.DirectiveType,
                    directiveAttrib.Arguments);

                template.Parse();
                directiveList.Add(template);
            }

            return directiveList;
        }
    }
}