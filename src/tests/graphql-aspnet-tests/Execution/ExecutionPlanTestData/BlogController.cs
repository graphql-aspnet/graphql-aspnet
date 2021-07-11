// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System.Collections.Generic;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class BlogController : GraphController
    {
        [QueryRoot("blogs", typeof(IEnumerable<Blog>))]
        public IGraphActionResult RetrieveBlogs()
        {
            var list = new List<Blog>();

            var blog = new BlogProxy()
            {
                BlogId = 1,
                Url = "http://blog.com",
                Posts = new List<BlogPost>(),
            };
            blog.Posts.Add(new BlogPostProxy()
            {
                BlogId = 1,
                PostId = 1,
                Content = "Content 1",
                Title = "Title 1",
                Blog = blog,
                Comments = new List<BlogPostComment>()
                {
                    new BlogPostCommentProxy()
                    {
                        CommentId = 30,
                        Comment = "Comment 30",
                    },
                    new BlogPostCommentProxy()
                    {
                        CommentId = 31,
                        Comment = "Comment 31",
                    },
                },
            });

            blog.Posts.Add(new BlogPostProxy()
            {
                BlogId = 1,
                PostId = 2,
                Content = "Content 2",
                Title = "Title 2",
                Blog = blog,
                Comments = new List<BlogPostComment>()
                {
                    new BlogPostCommentProxy()
                    {
                        CommentId = 32,
                        Comment = "Comment 32",
                    },
                    new BlogPostCommentProxy()
                    {
                        CommentId = 33,
                        Comment = "Comment 33",
                    },
                },
            });

            list.Add(blog);

            return this.Ok(list);
        }
    }
}