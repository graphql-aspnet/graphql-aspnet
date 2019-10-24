// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.InterfaceExtensionTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class InterfaceExtensionController : GraphController
    {
        [QueryRoot("multiObjects", typeof(IInterfaceA), TypeExpression = TypeExpressions.IsList)]
        public IGraphActionResult RetrieveObjects()
        {
            var list = new List<IInterfaceA>();
            list.Add(new ConcreteObjectA()
            {
                FirstName = "0A_prop1",
                LastName = "0A_prop2",
                MiddleName = "0A_prop3",
            });
            list.Add(new ConcreteObjectA()
            {
                FirstName = "1A_prop1",
                LastName = "1A_prop2",
                MiddleName = "1A_prop3",
            });
            list.Add(new ConcreteObjectB()
            {
                FirstName = "0B_prop1",
                LastName = "0B_prop2",
                Title = "0B_prop3",
            });
            list.Add(new ConcreteObjectB()
            {
                FirstName = "1B_prop1",
                LastName = "1B_prop2",
                Title = "1B_prop3",
            });

            return this.Ok(list);
        }

        [QueryRoot("multiObjectsWithC", typeof(IInterfaceA), TypeExpression = TypeExpressions.IsList)]
        public IGraphActionResult InvalidReturn()
        {
            var list = new List<IInterfaceA>();
            list.Add(new ConcreteObjectA()
            {
                FirstName = "0A_prop1",
                LastName = "0A_prop2",
                MiddleName = "0A_prop3",
            });
            list.Add(new ConcreteObjectB()
            {
                FirstName = "0B_prop1",
                LastName = "0B_prop2",
                Title = "0B_prop3",
            });
            list.Add(new ConcreteObjectC()
            {
                FirstName = "0C_prop1",
                LastName = "0C_prop2",
                MiddleName = "0C_prop3",
            });

            return this.Ok(list);
        }

        [TypeExtension(typeof(IInterfaceA), "FullName", typeof(string))]
        public IGraphActionResult FullName(IInterfaceA person)
        {
            return this.Ok($"{person.FirstName} {person.LastName}");
        }
    }
}