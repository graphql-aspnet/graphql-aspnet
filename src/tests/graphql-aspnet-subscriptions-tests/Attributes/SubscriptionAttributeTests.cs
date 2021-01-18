// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Attributes
{
    using GraphQL.AspNet;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionAttributeTests
    {
        [Test]
        public void SubscriptionAttribute_EmptyConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionAttribute();
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionAttribute_TemplateConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionAttribute("mySubscriptionRoute");
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("mySubscriptionRoute", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionAttribute_ReturnTypeConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionAttribute(typeof(SubscriptionAttributeTests));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionAttribute_MultiTypeConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionAttribute(typeof(SubscriptionAttributeTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionAttribute_TemplateMultiTypeConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionAttribute("myField", typeof(SubscriptionAttributeTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionAttribute_UnionConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionAttribute("myField", "myUnionType", typeof(SubscriptionAttributeTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(false, attrib.IsRootFragment);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionRootAttribute_EmptyConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionRootAttribute();
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionRootAttribute_TemplateConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionRootAttribute("mySubscriptionRootRoute");
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("mySubscriptionRootRoute", attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(0, attrib.Types.Count);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionRootAttribute_ReturnTypeConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionRootAttribute(typeof(SubscriptionAttributeTests));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(1, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionRootAttribute_MultiTypeConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionRootAttribute(typeof(SubscriptionAttributeTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual(Constants.Routing.ACTION_METHOD_META_NAME, attrib.Template);
            Assert.AreEqual(TypeExpressions.Auto, attrib.TypeExpression);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionRootAttribute_TemplateMultiTypeConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionRootAttribute("myField", typeof(SubscriptionAttributeTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual(null, attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionRootAttribute_UnionConstructor_PropertyCheck()
        {
            var attrib = new SubscriptionRootAttribute("myField", "myUnionType", typeof(SubscriptionAttributeTests), typeof(GraphFieldAttribute));
            Assert.AreEqual(GraphCollection.Subscription, attrib.FieldType);
            Assert.AreEqual(true, attrib.IsRootFragment);
            Assert.AreEqual("myUnionType", attrib.UnionTypeName);
            Assert.AreEqual("myField", attrib.Template);
            Assert.AreEqual(2, attrib.Types.Count);
            Assert.AreEqual(typeof(SubscriptionAttributeTests), attrib.Types[0]);
            Assert.AreEqual(typeof(GraphFieldAttribute), attrib.Types[1]);
            Assert.AreEqual(FieldResolutionMode.PerSourceItem, attrib.ExecutionMode);
        }

        [Test]
        public void SubscriptionAttribute_EventNameCarries()
        {
            var attrib = new SubscriptionAttribute();
            attrib.EventName = "bOb";

            Assert.AreEqual("bOb", attrib.EventName);
        }

        [Test]
        public void SubscriptionRootAttribute_EventNameCarries()
        {
            var attrib = new SubscriptionRootAttribute();
            attrib.EventName = "bOb";

            Assert.AreEqual("bOb", attrib.EventName);
        }
    }
}