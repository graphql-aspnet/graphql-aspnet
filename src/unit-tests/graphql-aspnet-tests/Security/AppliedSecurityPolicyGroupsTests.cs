// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Security
{
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Security;
    using Microsoft.AspNetCore.Authorization;
    using NUnit.Framework;

    [TestFixture]
    public class AppliedSecurityPolicyGroupsTests
    {
        private class TestClass
        {
            [Authorize]
            public bool HasAuthorize() => true;

            public bool DoesNotHaveAuthorize() => true;

            [AllowAnonymous]
            public bool AnonymousMethod() => true;
        }

        [Test]
        public void SingleGroup_WithAuthorize_SecurityChecksAreCorrectlyIdentified()
        {
            var secureGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(
                typeof(TestClass).GetMethod(nameof(TestClass.HasAuthorize)));

            var groups = new AppliedSecurityPolicyGroups(new List<AppliedSecurityPolicyGroup>()
            {
                secureGroup,
            });

            Assert.IsTrue(groups.HasSecurityChecks);
            Assert.AreEqual(1, groups.Count);
        }

        [Test]
        public void MultipleGroup_WithAuthorize_SecurityChecksAreCorrectlyIdentified()
        {
            var secureGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(
                typeof(TestClass).GetMethod(nameof(TestClass.HasAuthorize)));

            var unsecureGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(
                typeof(TestClass).GetMethod(nameof(TestClass.DoesNotHaveAuthorize)));

            var groups = new AppliedSecurityPolicyGroups(new List<AppliedSecurityPolicyGroup>()
            {
                secureGroup,
                unsecureGroup,
            });

            Assert.IsTrue(groups.HasSecurityChecks);
            Assert.AreEqual(2, groups.Count);
        }

        [Test]
        public void NoSecurityChecksAreCorrectlyIdentified()
        {
            var unsecureGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(
                typeof(TestClass).GetMethod(nameof(TestClass.DoesNotHaveAuthorize)));

            var groups = new AppliedSecurityPolicyGroups(new List<AppliedSecurityPolicyGroup>()
            {
                unsecureGroup,
            });

            Assert.IsFalse(groups.HasSecurityChecks);
            Assert.AreEqual(1, groups.Count);
        }

        [Test]
        public void AllowAnonymous_StillFlagsForSecurityChecks()
        {
            var secureGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(
                typeof(TestClass).GetMethod(nameof(TestClass.HasAuthorize)));

            var unsecureGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(
                typeof(TestClass).GetMethod(nameof(TestClass.DoesNotHaveAuthorize)));

            var anonGroup = AppliedSecurityPolicyGroup.FromAttributeCollection(
                typeof(TestClass).GetMethod(nameof(TestClass.AnonymousMethod)));

            var groups = new AppliedSecurityPolicyGroups(new List<AppliedSecurityPolicyGroup>()
            {
                secureGroup,
                unsecureGroup,
                anonGroup,
            });

            Assert.IsTrue(groups.HasSecurityChecks);
            Assert.AreEqual(3, groups.Count);
        }

        [Test]
        public void NoPoliciesSupplied_NoSecurityChecksDiscovered()
        {
            var group = new AppliedSecurityPolicyGroups();
            Assert.IsFalse(group.HasSecurityChecks);
            Assert.AreEqual(0, group.Count);
        }

        [Test]
        public void NullPoliciesSupplied_NoSecurityChecksDiscovered()
        {
            var group = new AppliedSecurityPolicyGroups(null);
            Assert.IsFalse(group.HasSecurityChecks);
            Assert.AreEqual(0, group.Count);
        }

        [Test]
        public void NoSuppliedPoliciesSupplied_NoSecurityChecksDiscovered()
        {
            var group = new AppliedSecurityPolicyGroups(Enumerable.Empty<AppliedSecurityPolicyGroup>());
            Assert.IsFalse(group.HasSecurityChecks);
            Assert.AreEqual(0, group.Count);
        }
    }
}