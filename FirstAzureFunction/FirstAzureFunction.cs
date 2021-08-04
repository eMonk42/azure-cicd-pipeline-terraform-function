using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FirstAzureFunction
{
    public static class FirstAzureFunction
    {
        [FunctionName("FirstAzureFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            try
            {
                if (!requestBody.Any())
                {
                    return new OkObjectResult("Please provide a body in the JSON format.");
                }
                var data = JsonConvert.DeserializeObject<Request>(requestBody);

                var result = data.GetSumOfIntegers();

                return new OkObjectResult($"The result is {result}. Thx and bye.");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
    }
}
