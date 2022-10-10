using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Newtonsoft.Json.Linq;

namespace FunctionApp6
{
    public static class FunctionHttpTrigger
    {
        [FunctionName("FunctionHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Table("Sample", Connection = "SampleTableConnection")] TableClient client,
            ILogger log)
        {
            var body = await req.ReadAsStringAsync().ConfigureAwait(false);

            var data = JObject.Parse(body);

            var entity = new TableEntity(data["Country"].Value<string>(), data["Company"].Value<string>())
            {
                ["Description"] = data["Description"].Value<string>()
            };

            var result = await client.AddEntityAsync(entity).ConfigureAwait(false);
            log.LogInformation($"Add - added");

            return new NoContentResult();
        }
    }
}
