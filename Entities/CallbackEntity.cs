namespace TrackBot4.Entities
{
    using Newtonsoft.Json;

    public partial class Callback
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("sender")]
        public User Sender { get; set; }
    }

    public partial class Message
    {
        [JsonProperty("text")]
        public string textMessage { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}