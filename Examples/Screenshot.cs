namespace PuppeteerAzureFunc.Examples
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using PuppeteerSharp;
    using System.IO;
    using System;

    public static class Screenshot
    {
        [FunctionName("Examples-Screenshot")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Examples/Screenshot")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var url = req.Query["url"];

            if (string.IsNullOrWhiteSpace(url))
                return new BadRequestObjectResult("Please pass a url on the query string or in the request body");

            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Args = new[] { "--no-sandbox" },
                Headless = true
            });
            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);

            var fileName = $"{DateTime.Now.Ticks}.png";
            await page.ScreenshotAsync(fileName);
            await browser.CloseAsync();

            var content = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            File.Delete(fileName);

            return new FileStreamResult(content, "application/octet-stream");
        }
    }
}
