using System.Security.Principal;
using Newtonsoft.Json;

namespace SharpBird.Api.ViewModels
{
    public class GenericResultViewModel<T>
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? Elapsed { get; set; }

        public T Result { get; set; }
    }
}
