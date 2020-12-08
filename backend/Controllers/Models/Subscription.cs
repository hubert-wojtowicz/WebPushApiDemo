using MongoDB.Bson.Serialization.Attributes;
using WebPush;

namespace WebPushApiDemo.Controllers.Models
{
    public class Subscription
    {
        [BsonId]
        public string Client { get; set; }

        public PushSubscription PushSubscription { get; set; }
    }
}
