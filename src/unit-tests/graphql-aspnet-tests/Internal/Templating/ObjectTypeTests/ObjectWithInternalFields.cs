// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    public class ObjectWithInternalFields
    {
        [GraphField]
        internal int Method1()
        {
            return 0;
        }

        internal int Method2()
        {
            return 0;
        }

        // not explicitly declared so shouldnt be added to a schema (with default config)
        // but still should be templated
        public int Method3()
        {
            return 0;
        }

        private int Method4()
        {
            return 0;
        }

        public static int Method5()
        {
            return 0;
        }

        public static int Method6()
        {
            return 0;
        }

        public int Field1 { get; set; }

        // not explicitly decalred but internal, should be skipped
        internal string Field2 { get; set; }

        // explicitly declared but still internal, should be skipped
        [GraphField]
        internal string Field3 { get; set; }

        private int Field4 { get; set; }

        public static int Field5 { get; set; }

        internal static int Field6 { get; set; }

        private static int Field7 { get; set; }
    }
}