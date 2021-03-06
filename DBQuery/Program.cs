﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


#pragma warning disable 169

namespace Samples
{
    public class Program
    {
        private static void Main()
        {
           // SetupDatabase();

            using (var db = new BloggingContext())
            {
                #region Query
                var postCounts = db.BlogPostCounts.ToList();
                var param = new SqlParameter("@FirstName", "Bill");

                //lst = await this.Query<SpGetProductByPriceGreaterThan1000>().FromSql(sqlQuery).ToListAsync();
                string sqlQuery = "EXEC BlogPostsCountTest6 @FirstName";
                var x = db.Query<BlogPostsCountTest6>().FromSql(sqlQuery, param);

                foreach (var postCount1 in x)
                {
                    Console.WriteLine($"{postCount1.Name} has {postCount1.PostCount} posts.{postCount1.FirstName}");
                    Console.WriteLine();
                }


                foreach (var postCount in postCounts)
                {
                    Console.WriteLine($"{postCount.BlogName} has {postCount.PostCount} posts.");
                    Console.WriteLine();
                }
                #endregion
            }
        }

        private static void SetupDatabase()
        {
            using (var db = new BloggingContext())
            {
                if (db.Database.EnsureCreated())
                {
                    db.Blogs.Add(
                        new Blog
                        {
                            Name = "Fish Blog",
                            Url = "http://sample.com/blogs/fish",
                            Posts = new List<Post>
                            {
                                new Post { Title = "Fish care 101" },
                                new Post { Title = "Caring for tropical fish" },
                                new Post { Title = "Types of ornamental fish" }
                            }
                        });

                    db.Blogs.Add(
                        new Blog
                        {
                            Name = "Cats Blog",
                            Url = "http://sample.com/blogs/cats",
                            Posts = new List<Post>
                            {
                                new Post { Title = "Cat care 101" },
                                new Post { Title = "Caring for tropical cats" },
                                new Post { Title = "Types of ornamental cats" }
                            }
                        });

                    db.Blogs.Add(
                        new Blog
                        {
                            Name = "Catfish Blog",
                            Url = "http://sample.com/blogs/catfish",
                            Posts = new List<Post>
                            {
                                new Post { Title = "Catfish care 101" },
                                new Post { Title = "History of the catfish name" }
                            }
                        });

                    db.SaveChanges();

                    #region View
                    db.Database.ExecuteSqlCommand(
                        @"CREATE VIEW View_BlogPostCounts AS 
                        SELECT b.Name, Count(p.PostId) as PostCount 
                        FROM Blogs b
                        JOIN Posts p on p.BlogId = b.BlogId
                        GROUP BY b.Name");


                    //db.Database.ExecuteSqlCommand(
                    //   @"CREATE procedure BlogPostsCountTest AS 
                    //        SELECT b.Name, Count(p.PostId) as PostCount 
                    //        FROM Blogs b
                    //        JOIN Posts p on p.BlogId = b.BlogId
                    //        GROUP BY b.Name");

                    //db.Database.ExecuteSqlCommand(
                    //      @"CREATE procedure BlogPostsCountTest3 AS 
                    //            SELECT b.Name as BlogName
                    //            FROM Blogs b
                    //           ");

                    //db.Database.ExecuteSqlCommand(
                    //   @"CREATE procedure BlogPostsCountTest2 AS 
                    //        SELECT b.Name, Count(p.PostId) as PostCount 
                    //        FROM Blogs b
                    //        JOIN Posts p on p.BlogId = b.BlogId
                    //        GROUP BY b.Name");

                    //db.Database.ExecuteSqlCommand(
                    //      @"CREATE procedure BlogPostsCountTest5 AS 
                    //            SELECT b.Name as BlogName, b.Url
                    //            FROM Blogs b
                    //           ");

                    db.Database.ExecuteSqlCommand(
                   @"CREATE procedure BlogPostsCountTest6 
                        @FirstName varchar(50)
                        AS
                        SELECT b.Name, Count(p.PostId) as PostCount,@FirstName as FirstName  
                        FROM Blogs b
                        JOIN Posts p on p.BlogId = b.BlogId
                        GROUP BY b.Name");

                #endregion
                Console.WriteLine("test");
                }
            }
        }
    }

    public class BloggingContext : DbContext
    {
        private static readonly ILoggerFactory _loggerFactory
            = new LoggerFactory();//.AddConsole((s, l) => l == LogLevel.Information && !s.EndsWith("Connection"));

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public DbQuery<BlogPostsCount> BlogPostCounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder
            //    .UseSqlServer(
            //        @"Server=(localdb)\mssqllocaldb;Database=Sample.QueryTypes;Trusted_Connection=True;ConnectRetryCount=0;")
            //    .UseLoggerFactory(_loggerFactory);

            //populate the connection string
            optionsBuilder
                .UseSqlServer(
                    @"Server=?\SQLEXPRESS;Database=Sample.QueryTypes;Trusted_Connection=True;ConnectRetryCount=0;")
                .UseLoggerFactory(_loggerFactory);
        }

        #region Configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Query<BlogPostsCount>().ToView("View_BlogPostCounts")
                .Property(v => v.BlogName).HasColumnName("Name");
            modelBuilder
                .Query<BlogPostsCountTest6>();

           



        }
        #endregion
    }

    #region Entities
    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public ICollection<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int BlogId { get; set; }
    }
    #endregion

    #region QueryType
    public class BlogPostsCount
    {
        public string BlogName { get; set; }
        public int PostCount { get; set; }
    }

    public class BlogPostsCountTest2
    {
        public string Name { get; set; }
        public int PostCount { get; set; }
    }

    public class BlogPostsCountTest3
    {
        public string BlogName { get; set; }
        //public int PostCount { get; set; }
    }

    public class BlogPostsCountTest4
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class BlogPostsCountTest5
    {
        public string BlogName { get; set; }
        public string Url { get; set; }
    }

    public class BlogPostsCountTest6
    {
        public string Name { get; set; }
        public int PostCount { get; set; }
        public string FirstName { get; set; }
    }

    #endregion
}