using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Socket.Newtonsoft.Json;

namespace ElysiaInteractMenu.Discord
{
    public class DiscordWebhookSender
    {
        private readonly HttpClient _httpClient;
        public string WebhookUrl;
        private readonly EmbedType _type;

        public DiscordWebhookSender(string webhookUrl,EmbedType type)
        {
            _httpClient = new HttpClient();
            WebhookUrl = webhookUrl;
            _type = type;
        }

        public async Task SendMessageAsync(string message, string username = null, string avatarUrl = null, bool embed = false)
        {
            DateTime now = DateTime.Now;
            string formattedDate = now.ToString("dd/MM/yyyy HH:mm:ss");
            formattedDate = $"[{formattedDate}]";
            if (embed)
            {
                var embedPayload = new
                {
                    title = _type.ToString(),
                    description = $"{formattedDate} {message}",
                    color = EmbedTypeExtensions.GetColor(_type)
                };

                var payload = new
                {
                    username = username,
                    avatar_url = avatarUrl,
                    embeds = new[] { embedPayload }
                };

                await SendPayloadAsync(payload);
            }
            else
            {
                var payload = new
                {
                    content =  $"{formattedDate} {message}",
                    username = username,
                    avatar_url = avatarUrl
                };

                await SendPayloadAsync(payload);
            }
        }


        private async Task SendPayloadAsync(object payload)
        {
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync(WebhookUrl, httpContent);
        }
    }
}