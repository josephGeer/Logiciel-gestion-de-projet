using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace app_test.Collection
{
    public class CollectionObject
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("collection_id")]
        public int CollectionId { get; set; }

        [JsonPropertyName("image")]
        public string ImageUrl { get; set; }
    }
}
