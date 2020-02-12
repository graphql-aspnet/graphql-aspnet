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
            // extend rule 5.2.3.1 to mean the top-level operation and each virtual child field has 1 and only 1 child field declaration
            // up to and including a subscription action being located

            /*
                Valid:
                --------
                subscription {
                    controllerPath {
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


                Invalid:
                -------
                subscription {
                    controllerPath {
                        routePath1 {
                            routePath2 {
                                subscriptionAction1 { }
                                subscriptionAction2 { }  // two subscription actions encountered (must be 1)
                            }
                        }
                    }
                }

                subscription {
                    controller {
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
                        subscriptionActionField2 { }  // split pathing allows for two possible subscription fields (must be 1)
                    }
                }
            */

            var operation = (QueryOperation)context.ActivePart;
            if (fieldCollection == null || fieldCollection.Children.Count != 1)
            {
                this.ValidationError(
                    context,
                    $"Invalid Subscription. Expected exactly 1 root child field, recieved {fieldCollection?.Children.Count ?? 0} child fields.");
                return false;
            }
            var n = fieldCollection.Children[0];

            while (_field != null)
            {
                // when pointing at a subscription field we're done
                if (currentField is ISubscriptionGraphField)
                {
                    _field = currentField as ISubscriptionGraphField;
                    break;
                }

                // when not pointing at a subscription field
                // we must be pointing at a virtual field or error
                // this allows us to walk down a controller custom route path to find
                // our subscription field while preserving the user's expected graph structure
                if (!currentField.IsVirtual)
                {
                    this.Messages.Add(
                      GraphMessageSeverity.Critical,
                      $"The first non-virtual field found in the subscription operation is not a valid subscription field (Path: {currentField.Route.Path})",
                      Constants.ErrorCodes.BAD_REQUEST);
                    break;
                }

                // when looking at a controller level field (or an intermediary)
                // it must have child fields for us to continue searching
                if (!(currentField is IGraphFieldContainer fieldSet) || fieldSet.Fields.Count != 1)
                {
                    this.Messages.Add(
                      GraphMessageSeverity.Critical,
                      $"The virtual field, {currentField.Route.Path}, contains no children found in the subscription operation is not a valid subscription field (Path: {currentField.Route.Path})",
                      Constants.ErrorCodes.BAD_REQUEST);
                    break;
                    break;
                }

                // also, said field must have exactly 1 child
                // in order to keep the data in tact. Otherwise its possible
                // that we have two "top level subscription fields" (indicating two seperate events)
                // in cased in one subscription.
                if (fieldSet.Fields.Count != 1)
                {
                }
            }

            if (_field == null && !fieldErrorRecorded)
            {
                // theoretically not possible but just in case
                // the user swaps out some DI components incorrectly or by mistake...
                this.Messages.Add(
                  GraphMessageSeverity.Critical,
                  $"An eventable field could not found in the subscription operation. Ensure you include a field declared " +
                  $"as a subscription field.",
                  Constants.ErrorCodes.BAD_REQUEST);
            }

            return true;
        }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z").
        /// </summary>
        /// <value>The rule number.</value>
        public override string RuleNumber => "5.2.3.1.1";

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification.
        /// </summary>
        /// <value>The rule URL.</value>
        public override string ReferenceUrl => "https://graphql.github.io/graphql-spec/June2018/#sec-Single-root-field";
    }
}