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
    /// <summary>
    /// Helper methods to cut down on noise in user code due to the long name of the enum. Also improves code readability
    /// with bool negations.
    /// </summary>
    public static class TemplateDeclarationRequirementsExtensions
    {
        /// <summary>
        /// Determines, on this set of requirements, if methods need not be declared to be included.
        /// </summary>
        /// <param name="requirementSetting">The requirement setting.</param>
        /// <returns><c>true</c> if methods do not need to be explicitly declared, <c>false</c> otherwise.</returns>
        public static bool AllowImplicitMethods(this TemplateDeclarationRequirements requirementSetting)
        {
            return !requirementSetting.HasFlag(TemplateDeclarationRequirements.Method);
        }

        /// <summary>
        /// Determines, on this set of requirements, if properties need not be declared to be included.
        /// </summary>
        /// <param name="requirementSetting">The requirement setting.</param>
        /// <returns><c>true</c> if properties do not need to be explicitly declared, <c>false</c> otherwise.</returns>
        public static bool AllowImplicitProperties(this TemplateDeclarationRequirements requirementSetting)
        {
            return !requirementSetting.HasFlag(TemplateDeclarationRequirements.Property);
        }

        /// <summary>
        /// Determines, on this set of requirements, if enum values need not be declared to be included.
        /// </summary>
        /// <param name="requirementSetting">The requirement setting.</param>
        /// <returns><c>true</c> if enum values do not need to be explicitly declared, <c>false</c> otherwise.</returns>
        public static bool AllowImplicitEnumValues(this TemplateDeclarationRequirements requirementSetting)
        {
            return !requirementSetting.HasFlag(TemplateDeclarationRequirements.EnumValue);
        }
    }
}