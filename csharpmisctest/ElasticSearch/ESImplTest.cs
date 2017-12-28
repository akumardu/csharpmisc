using System;
using System.Linq;
using System.Threading.Tasks;
using csharpmisc.ElasticSearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace csharpmisctest.ElasticSearch
{
    [Ignore]
    [TestClass]
    public class ESImplTest
    {
        private readonly string ESUri = "http://localhost:9200";

        [TestMethod]
        public void TestSetup()
        {
            var es = new ESImpl(ESUri);
            var clusterHealth = es.ClusterHealth();
            Console.WriteLine(clusterHealth);
            Assert.IsTrue(clusterHealth.IsValid);
        }

        [TestMethod]
        public async Task TestIndexCreation()
        {
            var es = new ESImpl(ESUri);
            var initialClusterHealth = es.GetIndex("blog_post");
            Assert.IsTrue(initialClusterHealth == null || initialClusterHealth.Count() == 0, "Initial cluster health is invalid");
            var createResult = es.CreateIndex("blog_post");
            Assert.IsTrue(createResult, "Index wasn't created");
            var finalClusterHealth = es.GetIndex("blog_post");
            await Task.Delay(1000);
            Assert.IsTrue(finalClusterHealth != null && finalClusterHealth.Count() == 1, "Final cluster health is invalid");
            var deleteResult = es.DeleteIndex("blog_post");
            Assert.IsTrue(deleteResult, "Delete didn't work");
        }

        [TestMethod]
        public async Task TestPostAndSearchMessage()
        {
            var es = new ESImpl(ESUri);
            var createResult = es.CreateIndex("blog_post");
            Assert.IsTrue(createResult, "Index wasn't created");
            var searchResults = es.SearchAllDocuments();
            var postResult = es.PostDocuments(new Post()
            {
                UserId = 1,
                PostDate = DateTime.UtcNow,
                PostText = "This is an awesome post"
            });
            Assert.IsTrue(postResult);
            await Task.Delay(1000);
            searchResults = es.SearchAllDocuments();
            Assert.IsTrue(searchResults != null && searchResults.Count() == 1, "Search failed");
            var deleteResult = es.DeleteIndex("blog_post");
            Assert.IsTrue(deleteResult, "Delete didn't work");
        }
    }
}
