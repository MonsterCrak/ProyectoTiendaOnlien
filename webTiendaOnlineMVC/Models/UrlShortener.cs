using Newtonsoft.Json;
using System.Text;

namespace webTiendaOnlineMVC.Models
{
    public class UrlShortener
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "https://api-ssl.bitly.com/v4/shorten"; // Endpoint de Bitly
        private const string AccessToken = "955f4b7353cd8802d60b7e56714e60eec446de8e"; // Reemplaza con tu token de acceso de Bitly

        public UrlShortener(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ShortenUrl(string originalUrl)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
                request.Headers.Add("Authorization", $"Bearer {AccessToken}");

                var requestData = new { long_url = originalUrl };
                request.Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var shortenedUrl = responseData.link;

                    return shortenedUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al acortar la URL: " + ex.Message);
            }

            return originalUrl; // Devuelve la URL original en caso de error
        }
    }

}
