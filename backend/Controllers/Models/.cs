namespace WebPushApiDemo.Controllers.Models
{
    public class ReqSub
    {
        public string Client { get; set; }

        public string Endpoint { get; set; }

        public string P256dh { get; set; }

        public string Auth { get; set; }
    }
}
