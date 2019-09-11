using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace QnABot.Models
{
    public class QnAService : IQnAService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public QnAService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<QnAResult[]> QueryQnAServiceAsync(string query, QnABotState qnAcontext, QnAMakerEndpoint qnAMakerEndpoint)
        {
            var options = new QnAMakerOptions
            {
                Top = 1
            };

            var hostname = qnAMakerEndpoint.Host;

            var endpoint = new QnAMakerEndpoint
            {
                KnowledgeBaseId = qnAMakerEndpoint.KnowledgeBaseId,
                EndpointKey = qnAMakerEndpoint.EndpointKey,
                Host = hostname
            };

            var requestUrl = $"{endpoint.Host}/knowledgebases/{endpoint.KnowledgeBaseId}/generateanswer";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            var jsonRequest = JsonConvert.SerializeObject(
                new
                {
                    question = query,
                    top = options.Top,
                    context = qnAcontext,
                    strictFilters = options.StrictFilters,
                    metadataBoost = options.MetadataBoost,
                    scoreThreshold = options.ScoreThreshold,
                }, Formatting.None);

            request.Headers.Add("Authorization", $"EndpointKey {endpoint.EndpointKey}");
            request.Content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();


            var contentString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<QnAResultList>(contentString);

            return result.Answers;
        }
    }
}
