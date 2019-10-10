using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace QnABot.Models
{
    public class Incident
    {

        private readonly IConfiguration _configuration;
        private readonly HttpClient _http;

        public Incident(IConfiguration configuration,
            IHttpClientFactory httpFactory)
        {
            _configuration = configuration;
            _http = httpFactory.CreateClient();
        }

        public async Task<string> Create(SupportTicketInformation supportInformation)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{Url}/{IncidentEndPoint}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", AuthKey);

            request.Content = new StringContent(
                JsonConvert.SerializeObject(new {
                    short_description = supportInformation.Question,
                    comments = supportInformation.Comments
                }),
                Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return response.Headers.GetValues("Location").First();
            }
            else
            {
                return string.Empty;
            }
        }

        private string Url
        {
            get
            {
                return _configuration["TicketSystemDomainAPI"];
            }
        }

        private string AuthKey
        {
            get
            {
                return _configuration["TicketSystemAuthKey"];
            }
        }

        private string IncidentEndPoint
        {
            get
            {
                return _configuration["TicketSystemIncidentEndPoint"];
            }
        }

    }
}
