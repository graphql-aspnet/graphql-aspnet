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
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;

    /// <summary>
    /// A set of helper methods related to <see cref="DirectiveLifeCycleEventItem"/>.
    /// </summary>
    internal static class DirectiveLifeCycleEventItemExtensions
    {
        /// <summary>
        /// Validates the directive method template or throw.
        /// </summary>
        /// <param name="eventItem">The event item.</param>
        /// <param name="methodTemplate">The method template.</param>
        public static void ValidateDirectiveMethodTemplateOrThrow(
            this DirectiveLifeCycleEventItem eventItem,
            GraphDirectiveMethodTemplate methodTemplate)
        {
            if (eventItem == DirectiveLifeCycleEventItem.None)
            {
                throw new GraphTypeDeclarationException(
                    $"The directive method '{methodTemplate.InternalFullName}' has does not represent a known lifecycle event.");
            }

            if (methodTemplate.ExpectedReturnType != eventItem.ExpectedReturnType)
            {
                throw new GraphTypeDeclarationException(
                    $"The method '{methodTemplate.InternalFullName}' has an invalid signature and cannot be used as a directive " +
                    $"method. The method is expected to return {eventItem.ExpectedReturnType.FriendlyName()} " +
                    $"to be invoked properly.");
            }

            if (eventItem.HasRequiredSignature)
            {
                bool signatureisValid = methodTemplate.Arguments.Count != eventItem.Parameters.Count;
                if (signatureisValid)
                {
                    // check the argument types
                    for (var i = 0; i < methodTemplate.Arguments.Count; i++)
                    {
                        var argument = methodTemplate.Arguments[i];
                        var expectedType = eventItem.Parameters[i];

                        if (argument.DeclaredArgumentType != expectedType)
                        {
                            signatureisValid = false;
                            break;
                        }
                    }
                }

                if (!signatureisValid)
                {
                    var expectedTypes = string.Join(", ", $"'{eventItem.Parameters.Select(x => x.FriendlyName())}'");
                    throw new GraphTypeDeclarationException(
                       $"The method '{methodTemplate.InternalFullName}' has an invalid signature and cannot be used as a directive " +
                       $"method. The method must declare exactly {methodTemplate.Arguments.Count} input parameter(s) of types: {expectedTypes}.");
                }
            }
        }
    }
}