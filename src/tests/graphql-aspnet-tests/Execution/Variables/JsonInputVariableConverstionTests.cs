// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Variables
{
    using System.Linq;
    using System.Text.Json;
    using GraphQL.AspNet.Interfaces.Variables;
    using GraphQL.AspNet.Variables;
    using NUnit.Framework;

    [TestFixture]
    public class JsonInputVariableConverstionTests
    {
        public class ObjectToTestInputCollectionDeserialization
        {
            public InputVariableCollection Variables { get; set; }
        }

        private ObjectToTestInputCollectionDeserialization DeserializeJson(string jsonText)
        {
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.AllowTrailingCommas = true;
            options.ReadCommentHandling = JsonCommentHandling.Skip;

            jsonText = "{ \"variables\": " + jsonText + "}";
            return JsonSerializer.Deserialize<ObjectToTestInputCollectionDeserialization>(jsonText, options);
        }

        [Test]
        public void NullValue_ConvertsToSingleValueVariable()
        {
            var jsonText = "{ \"var1\" : null }";
            var obj = this.DeserializeJson(jsonText);
            var result = obj.Variables;

            Assert.AreEqual(1, result.Count);
            var found = result.TryGetVariable("var1", out var variable);
            Assert.IsTrue(found);
            Assert.AreEqual("var1", variable.Name);

            var castVariable = variable as IInputSingleValueVariable;
            Assert.AreEqual(null, castVariable.Value);
        }

        [Test]
        public void NoValues_ConvertsToEmptyCollection()
        {
            var jsonText = "{}";
            var obj = this.DeserializeJson(jsonText);
            var result = obj.Variables;

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void NumberValue_ConvertsToSingleValueVariable()
        {
            var jsonText = "{ \"var1\" : 55 }";
            var obj = this.DeserializeJson(jsonText);
            var result = obj.Variables;

            Assert.AreEqual(1, result.Count);
            var found = result.TryGetVariable("var1", out var variable);
            Assert.IsTrue(found);
            Assert.AreEqual("var1", variable.Name);

            var castVariable = variable as IInputSingleValueVariable;
            Assert.AreEqual("55", castVariable.Value);
        }

        [Test]
        public void StringValue_ConvertsToSingleValueVariable_AndIsEscaped()
        {
            var jsonText = "{ \"var1\" : \"value1\" }";
            var obj = this.DeserializeJson(jsonText);
            var result = obj.Variables;

            Assert.AreEqual(1, result.Count);
            var found = result.TryGetVariable("var1", out var variable);
            Assert.IsTrue(found);
            Assert.AreEqual("var1", variable.Name);

            var castVariable = variable as IInputSingleValueVariable;
            Assert.AreEqual("\"value1\"", castVariable.Value);
        }

        [Test]
        public void BoolValue_ConvertsToSingleValueVariable()
        {
            var jsonText = "{ \"var1\" : false }";
            var obj = this.DeserializeJson(jsonText);
            var result = obj.Variables;

            Assert.AreEqual(1, result.Count);
            var found = result.TryGetVariable("var1", out var variable);
            Assert.IsTrue(found);
            Assert.AreEqual("var1", variable.Name);

            var castVariable = variable as IInputSingleValueVariable;
            Assert.AreEqual("false", castVariable.Value);
        }

        [Test]
        public void ArrayValue_ConvertsToArrayOfVariables()
        {
            var jsonText = "{ \"var1\" : [55, 18, 22] }";
            var obj = this.DeserializeJson(jsonText);
            var result = obj.Variables;

            Assert.AreEqual(1, result.Count);
            var found = result.TryGetVariable("var1", out var variable);
            Assert.IsTrue(found);
            Assert.AreEqual("var1", variable.Name);

            var castVariable = variable as IInputListVariable;
            Assert.AreEqual(3, castVariable.Items.Count);
            Assert.IsTrue(castVariable.Items.OfType<IInputSingleValueVariable>().Any(x => x.Value == "55"));
            Assert.IsTrue(castVariable.Items.OfType<IInputSingleValueVariable>().Any(x => x.Value == "18"));
            Assert.IsTrue(castVariable.Items.OfType<IInputSingleValueVariable>().Any(x => x.Value == "22"));
        }

        [Test]
        public void ComplexValue_ConvertsToArrayOfVariables()
        {
            var jsonText = "{ \"var1\" : {\"childVar1\" : 55, \"childVar2\" : \"value1\" } }";
            var obj = this.DeserializeJson(jsonText);
            var result = obj.Variables;

            Assert.AreEqual(1, result.Count);
            var found = result.TryGetVariable("var1", out var variable);
            Assert.IsTrue(found);
            Assert.AreEqual("var1", variable.Name);

            var castVariable = variable as IInputFieldSetVariable;
            Assert.AreEqual(2, castVariable.Fields.Count);

            var childVar1 = castVariable.Fields["childVar1"] as IInputSingleValueVariable;
            var childVar2 = castVariable.Fields["childVar2"] as IInputSingleValueVariable;

            Assert.AreEqual("55", childVar1.Value);
            Assert.AreEqual("\"value1\"", childVar2.Value);
        }
    }
}