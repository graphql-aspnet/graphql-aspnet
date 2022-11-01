// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.QueryOperationSteps
{
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.RulesEngine.RuleSets.DocumentValidation.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An extension on 5.2.3.1 to ensure that the virtual fields registered by controllers and routes
    /// can exist along side the first "top-level" encountered subscription field.
    /// </summary>
    internal class Rule_5_2_3_1_1_SubscriptionsRequire1EncounteredSubscriptionField
        : DocumentPartValidationRuleStep<IOperationDocumentPart>
    {
        /// <inheritdoc />
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context)
                && ((IOperationDocumentPart)context.ActivePart).OperationType == GraphOperationType.Subscription;
        }

        /// <inheritdoc />
        public override bool Execute(DocumentValidationContext context)
        {
            // due to the use of virtual fields used by controllers to make a dynamic schema,
            // this rule extends rule 5.2.3.1 to include the top-level operation and each virtual child field
            // has 1 and only 1 child field declaration up to and including a subscription action being located.
            // that is to say the nested fieldsets must not branch until AFTER a subscription field is encountered
            // as its this field that is registered as the subscription, not the virtual field paths

            /*
                -----------------------------------------------------
                Valid:
                -----------------------------------------------------
                subscription {
                    ctrlPath {
                        routePath1 {
                            routePath2 {
                                subscriptionAction { }
                            }
                        }
                    }
                }

                subscription {
                    subscriptionAction { }
                }

                -----------------------------------------------------
                Invalid:
                -----------------------------------------------------
                subscription {
                    ctrlPath {
                        routePath1 {
                            routePath2 {
                                subscriptionAction1 { }
                                subscriptionAction2 { }  // two subscription actions encountered (must be 1)
                            }
                        }
                    }
                }

                subscription {
                    ctrlPath {
                        routePath1 {
                            routePath2 {
                                queryActionField { }   // not a subscription field
                            }
                        }
                    }
                }

                subscription {
                    controller {
                        routePath1 {
                            routePath2 {
                                subscriptionActionField2 { }
                            }
                        }
                        subscriptionActionField2 { }  // split pathing allows for two possible subscription fields

                                                      //(must encounter only 1)
                    }
                }
            */

            var operation = context.ActivePart as IOperationDocumentPart;
            var fieldCollection = operation?.FieldSelectionSet;

            while (fieldCollection != null)
            {
                // if there is exactly one child then further inspection is needed
                // if there is more than one child, its an automatic failure
                var firstFew = fieldCollection.ExecutableFields.Take(2).ToList();
                if (firstFew.Count > 1)
                    break;

                // did we encounter a field collection with exactly one child that is not virtual?
                // i.e. one "top-level user action" to be called for the subscription?
                var singleExecutableField = firstFew[0];

                // if no field was found in the schema for the item on the document
                // this rule cannot continue (other rules will pick up the missing field error)
                if (singleExecutableField.GraphType == null || singleExecutableField.Field == null)
                    return true;

                if (!singleExecutableField.GraphType.IsVirtual)
                    return true;

                fieldCollection = singleExecutableField.FieldSelectionSet;
            }

            this.ValidationError(
                context,
                operation.SourceLocation,
                "Invalid Subscription. Expected exactly 1 root, non-virtual " +
                $"child field at {fieldCollection?.Path.DotString() ?? "-null-"}.");
            return false;
        }

        /// <inheritdoc />
        public override string RuleNumber => "5.2.3.1.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-Single-root-field";
    }
}