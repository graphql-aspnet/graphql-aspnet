// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests.TestData
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class MultiPartFileController : GraphController
    {
        [QueryRoot("simple")]
        public TwoPropertyObject SimpleMethod()
        {
            return new TwoPropertyObject()
            {
                Property1 = "sample",
                Property2 = 33,
            };
        }

        [QueryRoot("providedValue")]
        public TwoPropertyObject Provided(int value)
        {
            return new TwoPropertyObject()
            {
                Property1 = "sample",
                Property2 = value,
            };
        }

        [QueryRoot("fileUpload")]
        public async Task<TwoPropertyObject> SimpleMethod(FileUpload singleFile)
        {
            var stream = await singleFile.OpenFileAsync();
            var reader = new StreamReader(stream);
            var data = reader.ReadToEnd();
            return new TwoPropertyObject()
            {
                Property1 = singleFile.MapKey + " - " + singleFile.FileName + " - " + data,
                Property2 = Convert.ToInt32(stream.Length),
            };
        }
    }
}