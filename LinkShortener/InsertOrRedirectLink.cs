using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LinkShortener
{
    using LinkShortener.Model;
    using Microsoft.Azure.Cosmos.Table;
    public static class InsertOrRedirectLink
    {
        [FunctionName("RedirectLink")]
        public static async Task<IActionResult> RedirectLink(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{shortLink}")] HttpRequest req,
            string shortLink,
            ILogger log)
        {                
            log.LogInformation("C# HTTP trigger function processed an insert link request.");
            log.LogInformation("shortLink " + shortLink);
            string tableName = "link";

            // Create or reference an existing table
            CloudTable table = await Common.CreateOrGetCloudTableAsync(tableName);

            LinkMap linkMap =  await Common.RetrieveEntityUsingPointQueryAsync(table, "Link", shortLink);

            if (linkMap == null)
            {
                return new RedirectResult("http://error");
            }
            return new RedirectResult("http://" + linkMap.LongLink);
        }

        [FunctionName("InsertLink")]
        public static async Task<IActionResult> InsertLink(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addLink")] HttpRequest req,
            ILogger log)
        {
            string shortLink = req.Query["shortLink"];
            string longLink = req.Query["longLink"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            shortLink = shortLink ?? data?.shortLink;
            longLink = longLink ?? data?.longLink;

            if (shortLink == null || longLink == null)
            {
                return new BadRequestObjectResult("Wrong Input");
            }

            LinkMap linkMap = new LinkMap(shortLink)
            {
                LongLink = longLink
            };

            string tableName = "link";

            // Create or reference an existing table
            CloudTable table = await Common.CreateOrGetCloudTableAsync(tableName);

            // Demonstrate how to insert the entity
            Console.WriteLine("Insert an Entity.");
            linkMap = await Common.InsertOrMergeEntityAsync(table, linkMap);

            return new OkObjectResult(linkMap.LongLink);
        }
    }
}
