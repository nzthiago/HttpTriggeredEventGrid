using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;

namespace Microsoft.Sample
{
    public static class HandleBlobCreateEvent
    {
        [FunctionName("HandleBlobCreateEvent")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            EventGridEvent[] events = new EventGridSubscriber().DeserializeEventGridEvents(requestBody);

            var sw = new StringWriter(); 
            foreach (EventGridEvent eventGridEvent in events)
            {
                if (eventGridEvent.Data is SubscriptionValidationEventData)
                {
                    //todo: move to separate method
                    var eventData = (SubscriptionValidationEventData)eventGridEvent.Data;
                    log.LogInformation($"Got SubscriptionValidation event data, validation code: {eventData.ValidationCode}, topic: {eventGridEvent.Topic}");
                    // Do any additional validation (as required) and then return back the below response

                    var validationResponseData = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = eventData.ValidationCode
                    };

                    return new OkObjectResult(validationResponseData);
                }
                if (eventGridEvent.Data is StorageBlobCreatedEventData)
                {
                    //todo: move to separate method
                    var blobEventData = (StorageBlobCreatedEventData)eventGridEvent.Data;
                    //do whatever with the blob event info
                    await sw.WriteLineAsync($"Url: {blobEventData.Url}, Length: {blobEventData.ContentLength}");
                    await sw.WriteLineAsync();
                }
            } 
            return new OkObjectResult(sw.ToString());
        }
    }
}
