using System.Text.Json.Serialization;

namespace Vecc.TriggerAdoBuild
{
    public class BlobObject
    {
        [JsonPropertyName("pat")]
        public string Pat { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

    }
}
