using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace csharpmisc.ElasticSearch
{
    public class ESImpl
    {
        private Uri node;
        private ConnectionSettings settings;
        private ElasticClient client;

        public ESImpl(string uri)
        {
            node = new Uri(uri);

            // We can setup default index in ConnectionSettings
            settings = new ConnectionSettings(node)
                       .DefaultIndex("defaultindex");

            // Setup exact indices for known data types 
            settings.MapDefaultTypeIndices(m => m
                                                .Add(typeof(Post), "blog_post"));

            client = new ElasticClient(settings);
        }

        public bool CreateIndex(string index, int numberOfReplicas=1, int numberOfShards=1)
        {
            var response = client.CreateIndex(index, c => c
                                                        .Settings(s => s
                                                                        .NumberOfReplicas(numberOfReplicas)
                                                                        .NumberOfShards(numberOfShards)
                                                                  )
                                                         .Mappings(m => m
                                                                        .Map<Post>(d => d.AutoMap()))
                                               );
                                                                        
            if(!response.IsValid)
            {
                Console.WriteLine($"CreateIndex failed with {response.OriginalException}");
                return false;
            }

            return true;
        }

        public bool DeleteIndex(string index)
        {
            var deleteResponse = client.DeleteIndex(index);
            return deleteResponse.IsValid;
        }

        public IEnumerable<string> GetIndex(string index)
        {
            var getIndex = client.GetIndex(index);
            return getIndex?.Indices?.Keys;
        }

        public IClusterHealthResponse ClusterHealth()
        {
            var clusterHealth = client.ClusterHealth();
            return clusterHealth;
        }

        public bool PostDocuments(Post doc)
        {
            var result = client.Index<Post>(doc);
            return result.IsValid;
        }

        public bool DeleteDocuments(Post doc)
        {
            var result = client.Delete<Post>(doc);
            return result.IsValid;
        }

        public IEnumerable<Post> SearchAllDocuments()
        {
            var result = client.Search<Post>(s => s
                                                   .Query(q => q.MatchAll())
                                                    );
            var posts = new List<Post>();
            if (result.IsValid)
            {
                foreach(var doc in result.Documents)
                {
                    posts.Add(doc);
                }
                return posts;
            }

            return null;
        }
    }
}
