using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Security;
using System.Security.Claims;
using System;

namespace WatchPortalFunction
{
    public  class WatchInfo
    {
        private readonly IAccessTokenProvider tokenProvider;

        public WatchInfo(IAccessTokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;
        }
        
        [FunctionName("WatchInfo")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            ClaimsPrincipal user;

            user = await this.tokenProvider.ValidateToken(req);

            if (user == null)
            {
                return new UnauthorizedResult();
            }
            
            log.LogInformation($"Request received for {user.GetUserId()} from tenant {user.GetTenantId()}");

            string model = req.Query["model"];

            if (model == null)
            {
                return new BadRequestObjectResult("Please provide a watch model in the query string");
            }
            
            // Use dummy data for this example
            dynamic watchinfo = new { Manufacturer = "Abc", CaseType = "Solid", Bezel = "Titanium", Dial = "Roman", CaseFinish = "Silver", Jewels = 15 };
            return (ActionResult)new OkObjectResult($"Watch Details: {watchinfo.Manufacturer}, {watchinfo.CaseType}, {watchinfo.Bezel}, {watchinfo.Dial}, {watchinfo.CaseFinish}, {watchinfo.Jewels}");
        }
    }
}
