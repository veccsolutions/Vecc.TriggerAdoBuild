using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vecc.TriggerAdoBuild.Models
{
    public class Blob
    {
        [JsonPropertyName("pat")]
        public string Pat { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
