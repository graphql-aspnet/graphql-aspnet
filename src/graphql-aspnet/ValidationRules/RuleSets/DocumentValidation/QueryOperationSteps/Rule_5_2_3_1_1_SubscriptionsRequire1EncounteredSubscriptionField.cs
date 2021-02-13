// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.QueryOperationSteps
{
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.PlanGeneration.Contexts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.ValidationRules.RuleSets.DocumentValidation.Common;

    /// <summary>
    /// An extension on 5.2.3.1 to ensure that the virtual fields registered by controllers and routes
    /// can exist along side the first "top-level" encountered subscription field.
    /// </summary>
    internal class Rule_5_2_3_1_1_SubscriptionsRequire1EncounteredSubscriptionField : DocumentPartValidationRuleStep<QueryOperation>
    {
        /// <summary>
        /// Determines whether this instance can process the given context. The rule will have no effect on the node if it cannot
        /// process it.
        /// </summary>
        /// <param name="context">The context that may be acted upon.</param>
        /// <returns><c>true</c> if this instance can validate the specified node; otherwise, <c>false</c>.</returns>
        public override bool ShouldExecute(DocumentValidationContext context)
        {
            return base.ShouldExecute(context) &&
                   context.ActivePart is QueryOperation operation && operation.OperationType == GraphCollection.Subscription;
        }

        /// <summary>
        /// Validates the completed document context to ensure it is "correct" against the specification before generating
        /// the final document.
        /// </summary>
        /// <param name="context">The context containing the parsed sections of a query document..</param>
        /// <returns><c>true</c> if the rule passes, <c>false</c> otherwise.</returns>
        public override bool Execute(DocumentValidationContext context)
        {
            // due to the use of virtual fields used by controllers to make a dynamic schema,
            // this rule extend rule 5.2.3.1 to include the top-level operation and each virtual child field
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

            var operation = context.ActivePart as QueryOperation;
            var fieldCollection = operation?.FieldSelectionSet;

            while (fieldCollection != null && fieldCollection.Count == 1)
            {
                // did we encounter a field collection with exactly one child that is not virtual?
                // i.e. one "top-level user action" to be called for the subscription?
                var childField = fieldCollection[0];
                if (!childField.GraphType.IsVirtual)
                    return true;

                fieldCollection = childField.FieldSelectionSet;
            }

            this.ValidationError(
                context,
                operation.Node,
                "Invalid Subscription. Expected exactly 1 child field, " +
                $"recieved {fieldCollection?.Count ?? 0} child fields at {fieldCollection?.RootPath.DotString() ?? "-null-"}.");
            return false;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.2.3.1.1";

        /// <summary>
        /// Gets an anchor tag, pointing to a specific location on the webpage identified
        /// as the specification supported by this library. If ReferenceUrl is overriden
        /// this value is ignored.
        /// </summary>
        /// <value>The rule anchor tag.</value>
        protected override string RuleAnchorTag => "#sec-Single-root-field";
    }
}