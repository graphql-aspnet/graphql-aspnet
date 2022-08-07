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
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// Helper methods for <see cref="IGraphItemTemplate"/>.
    /// </summary>
    internal static class ApplyDirectiveAttributeExtensions
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
        public static IEnumerable<IAppliedDirectiveTemplate> ExtractAppliedDirectiveTemplates(this ICustomAttributeProvider attributeProvider, object owner)
        {
            var directiveList = new List<IAppliedDirectiveTemplate>();

            var directiveAttribs = attributeProvider
                                    .AttributesOfType<ApplyDirectiveAttribute>(true);

            foreach (var directiveAttrib in directiveAttribs)
            {
                AppliedDirectiveTemplate template;

                if (directiveAttrib.DirectiveType != null)
                {
                    template = new AppliedDirectiveTemplate(
                        owner,
                        directiveAttrib.DirectiveType,
                        directiveAttrib.Arguments);
                }
                else
                {
                    template = new AppliedDirectiveTemplate(
                        owner,
                        directiveAttrib.DirectiveName,
                        directiveAttrib.Arguments);
                }

                template.Parse();
                directiveList.Add(template);
            }

            return directiveList;
        }

        /// <summary>
        /// Inspects the provider for <see cref="ApplyDirectiveAttribute"/> items and creates an applied
        /// directive object from any found attributes.
        /// </summary>
        /// <param name="attributeProvider">The attribute provider to inspect.</param>
        /// <returns>IEnumerable&lt;IAppliedDirective&gt;.</returns>
        public static IEnumerable<IAppliedDirective> ExtractAppliedDirectives(this ICustomAttributeProvider attributeProvider)
        {
            var directiveList = new List<IAppliedDirective>();

            var directiveAttribs = attributeProvider
                                    .AttributesOfType<ApplyDirectiveAttribute>(true);

            foreach (var directiveAttrib in directiveAttribs)
            {
                AppliedDirective dir;

                if (directiveAttrib.DirectiveType != null)
                {
                    dir = new AppliedDirective(
                        directiveAttrib.DirectiveType,
                        directiveAttrib.Arguments);
                }
                else
                {
                    dir = new AppliedDirective(
                        directiveAttrib.DirectiveName,
                        directiveAttrib.Arguments);
                }

                directiveList.Add(dir);
            }

            return directiveList;
        }
    }
}