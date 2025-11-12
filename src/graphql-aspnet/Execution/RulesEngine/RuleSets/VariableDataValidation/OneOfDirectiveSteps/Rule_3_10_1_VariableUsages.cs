// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation.OneOfDirectiveSteps
{
    using System.Collections;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.VariableDataValidation.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A validation step that inspects resolved variable usage and ensures the coerced variable value match
    /// the rules for @oneOf directives when applied against a corrisponding INPUT OBJECT graph type.
    /// </summary>
    internal class Rule_3_10_1_VariableUsages : VariableValidationRuleStep<IVariableUsageDocumentPart>
    {
        /// <inheritdoc />
        public override bool Execute(VariableDataValidationContext context)
        {
            var varUsageDocPart = context.ActivePart as IVariableUsageDocumentPart;
            var owner = varUsageDocPart.Parent;

            // if the variable isn't resolved we can't check with this rule
            // as the document is not valid (this is a theoretically impossible situation unless something got messed up).
            if (!context.VariableData.TryGetValue(varUsageDocPart.VariableName, out var resolvedVariable))
                return true;

            // if the owner of the usage is an input argument we have to validate both
            // non-nullability and single supplied value which was deferred from document validation.
            if (owner is IInputArgumentDocumentPart argument)
            {
                return this.ValidateVariableUsageAgainstResolvedVariable(
                    context,
                    varUsageDocPart,
                    argument,
                    argument.GraphType,
                    argument.TypeExpression,
                    resolvedVariable,
                    resolvedVariable.ResovableItem,
                    resolvedVariable.Value);
            }

            // if the owner of the usage is a single field on an input argument
            // then we need validate that the field value is not null.
            // Determining if its the only field on the input object would be handled during doc validation.
            if (owner is IInputObjectFieldDocumentPart fieldDocumentPart)
            {
                // when the variable usage is as a value to an input object
                // then the only check we're doing here is that its non-null
                // since the structure of the input object would have already been validated
                return this.ValidateVariableUsageAgainstFieldOfInputObject(
                    context,
                    varUsageDocPart,
                    fieldDocumentPart,
                    resolvedVariable);
            }

            // theoretically a variable must be used as a refernce for an input argument or a field
            // on an input object so this should never get hit
            return true;
        }

        private bool ValidateVariableUsageAgainstResolvedVariable(
            VariableDataValidationContext context,
            IVariableUsageDocumentPart owner,
            IInputValueDocumentPart inputValue,
            IGraphType graphType,
            GraphTypeExpression typeExpression,
            IResolvedVariable resolvedVariable, // the original variable that was resovled
            IResolvableValueItem resolvedMetadata, // the metadata of the part of the variable being inspected
            object resolvedValue, // the actual resolved value for the target item
            int? listIndex = null)
        {
            // if the target of the usage is not an input union, skip validation requirements
            if (this.IsApplicableGraphType(graphType))
            {
                // the variable is an INPUT OBJECT
                // so the resolved metadata for the usage should be a field set
                if (resolvedMetadata is IResolvableFieldSet resolvedFieldSet)
                {
                    // 2. The resolved input argument value is a real value so we need to inspect the data and ensure that:
                    //    A: only one field was supplied to it
                    //    B: The supplied value is not null
                    var allFields = resolvedFieldSet.ResolvableFields?.ToList() ?? [];
                    if (allFields.Count != 1)
                    {
                        var indexCounter = !listIndex.HasValue
                            ? string.Empty
                            : $" at index {listIndex.Value}";

                        this.ValidationError(
                            context,
                            owner.SourceLocation,
                            $"Invalid variable usage. The variable '{resolvedVariable.Name}' supplied for the input object supplied argument '{inputValue.Name}' " +
                            $"of type '{graphType.Name}' could not be coerced correctly. '{graphType.Name}' is declared as an input union (i.e. '@oneOf') and exactly one field must " +
                            $"be supplied. Received {allFields.Count} fields{indexCounter}. ({string.Join(", ", allFields.Select(x => x.Key))})");

                        return false;
                    }

                    // we can assume that the field metadata is accurate, if the value of the singular field was parsed as null
                    // that's invalid
                    if (allFields[0].Value is IResolvableNullValue)
                    {
                        // json object was supplied with a null field value:  {  "prop1": null }
                        this.ValidationError(
                            context,
                            owner.SourceLocation,
                            $"Invalid variable usage. The variable '{resolvedVariable.Name}' supplied for the input argument '{inputValue.Name}' could " +
                            $"not be coerced correctly. The argument's graph type, '{owner.GraphType.Name}', is declared as an input union (i.e. '@oneOf') and the supplied " +
                            $"field must be non-null. Received field {allFields[0].Key} as null.");

                        return false;
                    }
                }
            }

            // if the resolved value is null we can't recursively check into it
            // just exit
            if (resolvedValue is null)
                return true;

            // type expression might be a non-nullable, don't really care about that here
            // since we enforce nullabiltiy as a rule of the directive (type expression rules have already been handled)
            if (typeExpression.IsNonNullable)
                typeExpression = typeExpression.UnWrapExpression();

            // recursive check (each item in a list)
            // ---------------------
            if (typeExpression.IsListOfItems)
            {
                return this.ValidateListofItems(
                    context,
                    owner,
                    inputValue,
                    graphType,
                    typeExpression,
                    resolvedVariable,
                    resolvedMetadata,
                    resolvedValue);
            }

            // recursive check (fields)
            // ----------------------------
            // if hte provided object was an input object
            // we need to check each field own its own if hte field type is an input union it needs to be checked
            // recursively on down etc.
            if (graphType is IInputObjectGraphType inputGraphType
                && resolvedMetadata is IResolvableFieldSet fieldSet)
            {
                var allFields = fieldSet.ResolvableFields?.ToList() ?? [];
                var allFieldsValid = true;
                foreach (var resolvedField in allFields)
                {
                    var targetField = inputGraphType.Fields[resolvedField.Key];
                    var fieldGraphType = context.Schema.KnownTypes.FindGraphType(targetField.TypeExpression.TypeName);
                    var resolvedFieldValue = this.ExtractPropertyValueFromObject(resolvedValue, targetField);
                    allFieldsValid = this.ValidateVariableUsageAgainstResolvedVariable(
                                         context,
                                         owner,
                                         inputValue,
                                         fieldGraphType,
                                         targetField.TypeExpression,
                                         resolvedVariable,
                                         resolvedField.Value,
                                         resolvedFieldValue)
                                     && allFieldsValid;
                }

                return allFieldsValid;
            }

            // not a list, not an input ojbject, value must be a leaf type
            // we can safely exit
            return true;
        }

        private bool ValidateListofItems(
            VariableDataValidationContext context,
            IVariableUsageDocumentPart owner,
            IInputValueDocumentPart inputValue,
            IGraphType graphType,
            GraphTypeExpression typeExpression,
            IResolvedVariable resolvedVariable,
            IResolvableValueItem resolvedMetadata,
            object resolvedValue)
        {
            var allValid = true;

            if (resolvedMetadata is IInputListVariable ilv)
            {
                // the variable should be a list
                if (resolvedValue is IEnumerable listValues)
                {
                    var i = 0;

                    // pop the list wrapper off the type expression
                    // to access the expected type expression of each item
                    var expression = typeExpression;
                    expression = expression.UnWrapExpression();

                    foreach (var itemValue in listValues)
                    {
                        if (ilv.Items.Count <= i)
                            continue;

                        var singleItemFieldMetadata = ilv.Items[i] as IResolvableFieldSet;
                        allValid = this.ValidateVariableUsageAgainstResolvedVariable(
                            context,
                            owner,
                            inputValue,
                            graphType,
                            expression,
                            resolvedVariable,
                            singleItemFieldMetadata,
                            itemValue,
                            i) && allValid;

                        i++;
                    }
                }
            }

            return allValid;
        }

        private bool ValidateVariableUsageAgainstFieldOfInputObject(
            VariableDataValidationContext context,
            IVariableUsageDocumentPart owner,
            IInputObjectFieldDocumentPart fieldDocumentPart,
            IResolvedVariable resolvedVariable)
        {
            // fields must belong to an input object (e.g. complex supplied value)
            var inputObjectGraphType = (fieldDocumentPart.Parent as IComplexSuppliedValueDocumentPart)?.GraphType;

            // if the owner of hte field (the input object graph type)
            // is an input union, then this supplied field value must be non-null
            // we dont check for field counts here, since the owner was an object literal
            // we can assume other checks validated the object's field count
            if (this.IsApplicableGraphType(inputObjectGraphType))
            {
                if (resolvedVariable.Value is null)
                {
                    // json object was supplied with a null field value:  {  "prop1": null }
                    this.ValidationError(
                        context,
                        owner.SourceLocation,
                        $"Invalid variable usage. The variable '{resolvedVariable.Name}' supplied for the input field '{fieldDocumentPart.Name}' could " +
                        $"not be coerced correctly. The graph type that owns the field, '{owner.GraphType.Name}', is declared as an input union (i.e. '@oneOf') and the supplied " +
                        $"field must be non-null.");

                    return false;
                }
            }

            // ok so if we have a non-null field value that was resolved
            // we need to check it on its own merits so we can recurise down into it looking for
            // other potential input unions (childs of childs etec.)
            // this must happen even if the original usage was not an input union, maybe a supplied child value is.
            if (resolvedVariable.Value is not null)
            {
                return this.ValidateVariableUsageAgainstResolvedVariable(
                    context,
                    owner,
                    fieldDocumentPart,
                    fieldDocumentPart.GraphType,
                    fieldDocumentPart.TypeExpression,
                    resolvedVariable,
                    resolvedVariable.ResovableItem,
                    resolvedVariable.Value);
            }

            return true;
        }

        private object ExtractPropertyValueFromObject(object obj, IInputGraphField field)
        {
            if (obj is null)
                return null;

            var propAccessors = InstanceFactory.CreatePropertyGetterInvokerCollection(obj.GetType());
            if (propAccessors is null)
            {
                // this should not be possible, but stop the query dead if so
                throw new GraphExecutionException(
                    $"Unable to inspect variable contents for variable object of type '{obj.GetType().FriendlyName()}'" +
                    $"supplied to field '{field.Name}'.");
            }

            // extract a value
            if (propAccessors.TryGetValue(field.InternalName, out var propGetter))
                return propGetter.Invoke(ref obj);

            return null;
        }

        private bool IsApplicableGraphType(IGraphType graphType)
        {
            return graphType is not null
                   && graphType.Kind == TypeKind.INPUT_OBJECT
                   && (graphType.AppliedDirectives?.Includes<OneOfDirective>() ?? false);
        }

        /// <inheritdoc />
        public override string RuleNumber => "3.10.1";

        /// <inheritdoc />
        protected override string RuleAnchorTag => "#sec-OneOf-Input-Objects";
    }
}