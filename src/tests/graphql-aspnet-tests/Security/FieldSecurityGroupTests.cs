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
    using System.Linq;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Security.SecurtyGroupData;
    using NUnit.Framework;

    [TestFixture]
    public class FieldSecurityGroupTests
    {
        [Test]
        public void PolicyName_YieldsPolicyNameInRule()
        {
            var group = FieldSecurityGroup.FromAttributeCollection(typeof(PolicyNameObject));

            Assert.IsNotNull(group);
            Assert.AreEqual(1, group.Count);
            Assert.IsFalse(group.AllowAnonymous);

            var policy = group.First();
            Assert.AreEqual("TestPolicy", policy.PolicyName);
            Assert.IsEmpty(policy.AllowedRoles);
        }

        [Test]
        public void Roles_YieldsRoleInRule()
        {
            var group = FieldSecurityGroup.FromAttributeCollection(typeof(RoleListObject));

            Assert.IsNotNull(group);
            Assert.AreEqual(1, group.Count);
            Assert.IsFalse(group.AllowAnonymous);

            var policy = group.First();

            Assert.AreEqual(null, policy.PolicyName);
            Assert.AreEqual(3, policy.AllowedRoles.Count);
            Assert.IsTrue(policy.AllowedRoles.Contains("role1"));
            Assert.IsTrue(policy.AllowedRoles.Contains("role2"));
            Assert.IsTrue(policy.AllowedRoles.Contains("role3"));
        }

        [Test]
        public void RolePolicyCombined_YieldsSingleRule()
        {
            var group = FieldSecurityGroup.FromAttributeCollection(typeof(PolicyNameWithRolesObject));

            Assert.IsNotNull(group);
            Assert.AreEqual(1, group.Count);
            Assert.IsFalse(group.AllowAnonymous);

            var policy = group.First();

            Assert.AreEqual("TestPolicWithRoleList", policy.PolicyName);
            Assert.AreEqual(2, policy.AllowedRoles.Count);
            Assert.IsTrue(policy.AllowedRoles.Contains("roleQ"));
            Assert.IsTrue(policy.AllowedRoles.Contains("roleW"));
        }

        [Test]
        public void MultiPolicy_HasAnonymousAttribute_IndicatesAll()
        {
            var group = FieldSecurityGroup.FromAttributeCollection(typeof(PolicyNameWithAnonymousObject));

            Assert.IsNotNull(group);
            Assert.AreEqual(2, group.Count);
            Assert.IsTrue(group.AllowAnonymous);

            var policy = group.First();
            Assert.AreEqual("TestPolicyWithAnonSupport", policy.PolicyName);
            Assert.AreEqual(0, policy.AllowedRoles.Count);

            var rolePolicy = group.Skip(1).First();
            Assert.AreEqual(3, rolePolicy.AllowedRoles.Count);
            Assert.IsTrue(rolePolicy.AllowedRoles.Contains("roleA"));
            Assert.IsTrue(rolePolicy.AllowedRoles.Contains("roleB"));
            Assert.IsTrue(rolePolicy.AllowedRoles.Contains("roleC"));
        }
    }
}