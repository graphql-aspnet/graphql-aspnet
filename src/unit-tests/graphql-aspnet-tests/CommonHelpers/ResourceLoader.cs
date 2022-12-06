// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.CommonHelpers
{
    using System.IO;

    /// <summary>
    /// A helper to load resources from disk used in tests.
    /// </summary>
    public static class ResourceLoader
    {
        /// <summary>
        /// Reads all lines from the given file at location '[AssemblyDirectory]\TestFiles\{key}\file'.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="file">The file.</param>
        /// <returns>System.String.</returns>
        public static string ReadAllLines(string key, string file)
        {
            var directory = new FileInfo(typeof(ResourceLoader).Assembly.Location).Directory?.FullName;
            var path = Path.Combine(directory, "TestFiles", key, file);
            return File.ReadAllText(path);
        }
    }
}