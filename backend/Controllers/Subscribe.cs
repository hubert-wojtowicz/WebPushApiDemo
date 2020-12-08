using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;
using WebPushApiDemo.Controllers.Models;
using WebPushApiDemo.Services;

namespace WebPushApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebPush : ControllerBase
    {
        private readonly ILogger<WebPush> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMongoCrud _mongo;

        private const string SubsciptionTable = "Subscriptions";

        public WebPush(
            ILogger<WebPush> logger,
            IConfiguration configuration,
            IMongoCrud mongo)
        {
            _logger = logger;
            _configuration = configuration;
            _mongo = mongo;
        }

        [HttpPost("key")]
        public Task<string> PubKey()
        {
            return Task.FromResult(_configuration["Vapid:publicKey"]);
        }

        [HttpPost("/subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] ReqSub request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Client))
            {
                return BadRequest("No Client Name parsed.");
            }

            var allSubs = await _mongo.LoadRecords<Subscription>(SubsciptionTable);
            if (allSubs.Any(x => x.Client == request.Client))
            {
                return BadRequest("Client Name already used.");
            }

            var pushSubscription = new PushSubscription(request.Endpoint, request.P256dh, request.Auth);
            await _mongo.InsertRecord(SubsciptionTable, new Subscription { Client = request.Client, PushSubscription = pushSubscription });
            return Ok(allSubs);
        }

        [HttpPost("/notify")]
        public async Task<IActionResult> Notify([FromBody] ReqNotif request)
        {
            if (request == null||string.IsNullOrWhiteSpace(request.Client))
            {
                return BadRequest("No Client Name parsed.");
            }

            PushSubscription subscription = (await _mongo.LoadRecords<Subscription>(SubsciptionTable)).SingleOrDefault(x => x.Client == request.Client)?.PushSubscription;
            if (subscription == null)
            {
                return BadRequest("Client was not found");
            }

            var subject = _configuration["VAPID:subject"];
            var publicKey = _configuration["VAPID:publicKey"];
            var privateKey = _configuration["VAPID:privateKey"];

            var vapidDetails = new VapidDetails(subject, publicKey, privateKey);
            var webPushClient = new WebPushClient();
            try
            {
                webPushClient.SendNotification(subscription, request.Message, vapidDetails);
            }
            catch (Exception exception)
            {
                // user unsubscribed
                // or SW was updated withour resubscribing
                _logger.LogError(exception, exception.Message);
            }

            return Ok();
        }
    }
}
