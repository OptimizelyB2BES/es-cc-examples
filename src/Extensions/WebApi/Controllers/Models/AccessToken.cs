using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Extensions.WebApi.Controllers.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccessToken
    {
        [JsonProperty("expires_in")]
        public int Expiration { get; set; }

        [JsonProperty("access_token")]
        public string AccessTokenResult { get; set; }
    }
}
