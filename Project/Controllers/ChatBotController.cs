using Application.DTOs.AI;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Serialization;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatBotController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _apiKey;

        public ChatBotController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClient = httpClientFactory.CreateClient();

          
            _apiUrl = config["Gemini:ApiUrl"];

         
            _apiKey = config["Gemini:ApiKey"];
        }

        [HttpPost("chat")]
        public async Task<IActionResult> GetGeminiResponse([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { reply = "I didn't catch that. Could you please type a message?" });
            }

    
            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"You are a helpful customer support assistant for an E-commerce Football Jersey store. Be friendly and concise. User asks: {request.Prompt}" }
                        }
                    }
                }
            };

            try
            {
               
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}?key={_apiKey}", payload);

                // --- 3. Handle Rate Limits (429) ---
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Ok(new { reply = "I'm processing too many requests right now. Please wait a moment and try again!" });
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, new { reply = "I'm having trouble connecting to my brain. Try again later." });
                }

                // --- 4. Parse the Multi-layered JSON Response ---
                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                // Extract the specific text from the Gemini response structure
                string botReply = result?.Candidates?[0]?.Content?.Parts?[0]?.Text
                                  ?? "I'm sorry, I couldn't generate a response to that.";

                return Ok(new { reply = botReply });
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logger
                return StatusCode(500, new { reply = "Internal server error. Please try again later." });
            }
        }
    }

    // --- 5. Helper Classes for Clean Deserialization ---
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}