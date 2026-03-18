using NUnit.Framework;
using NUnit.Framework.Legacy;

using DocsGenerator.Models;
using Microsoft.EntityFrameworkCore;


using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace TestProject1
{
    using DocsGenerator.Controllers;
    using DocsGenerator.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using NUnit.Framework.Legacy;

    public static class TestDbContextFactory
    {
        public static AppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new AppDbContext(options);
        }
    }

    public class Tests
    {

                     
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task GetVersions_NeverReturnsNull()
        {
            var context = TestDbContextFactory.CreateContext("db_null");
            var controller = new DocsController(context);

            var result = await controller.GetVersions();

            Assert.NotNull(result);
        }


        [Test]
        public async Task GetVersions_ShouldReturnCorrectCount()
        {
            var context = TestDbContextFactory.CreateContext("db_count2");

            ProjectVersion project1 = new ProjectVersion()
            {
                Branch = "sdfsfds",
                CommitName = "sdfsfsfdf",
                DocsPath = "/v1",
                CommitHash = "xyz1",
                CreatedAt = DateTime.Now
            };

            ProjectVersion project2 = new ProjectVersion()
            {
                Branch = "sdfsfds",
                CommitName = "sdfsfsfdf",
                DocsPath = "/v1",
                CommitHash = "xyz2",
                CreatedAt = DateTime.Now
            };

            ProjectVersion project3 = new ProjectVersion()
            {
                Branch = "sdfsfds",
                CommitName = "sdfsfsfdf",
                DocsPath = "/v1",
                CommitHash = "xyz3",
                CreatedAt = DateTime.Now
            };

            context.ProjectVersions.AddRange(
                project1,
                project2,
                project3
            );

            await context.SaveChangesAsync();

            var controller = new DocsController(context);
            var result = (await controller.GetVersions()).ToList();

            CollectionAssert.AllItemsAreNotNull(result);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public async Task GetLatest_ShouldReturnCorrectUrl()
        {
            var context = TestDbContextFactory.CreateContext("db_url");

            context.ProjectVersions.Add(new ProjectVersion
            {
                Branch = "sdfsfds",
                CommitName = "sdfsfsfdf",
                DocsPath = "/v1",
                CommitHash = "xyz789",
                CreatedAt = DateTime.Now
            });

            await context.SaveChangesAsync();

            var controller = new DocsController(context);

            var result = await controller.GetLatest();

            var redirect = result as RedirectResult;

            StringAssert.Contains("/v1/html/index.html", redirect.Url);
            StringAssert.StartsWith("http://localhost:8080", redirect.Url);
        }

        [Test]
        public async Task GetVersions_ShouldUseFallbackPath()
        {
            var context = TestDbContextFactory.CreateContext("db_fallback2");

            context.ProjectVersions.Add(new ProjectVersion
            {
                Branch = "sdfsfds",
                CommitName = "sdfsfsfdf",
                DocsPath = null,
                CommitHash = "xyz789",
                CreatedAt = DateTime.Now
            });

            await context.SaveChangesAsync();

            var controller = new DocsController(context);

            var result = (await controller.GetVersions()).ToList();

            var path = result[0].GetType().GetProperty("path")?.GetValue(result[0])?.ToString();

            StringAssert.AreEqualIgnoringCase("/docs/xyz789/", path);
        }
    }
}
