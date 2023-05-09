using Newtonsoft.Json;

namespace Interpretarr.Clients.FlareSolver.Model
{
    public class FlareSolverHeaders
    {
        public string Status;
        public string Date;
        [JsonProperty(PropertyName = "content-type")]
        public string ContentType;
    }
}
