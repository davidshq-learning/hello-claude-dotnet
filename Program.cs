using System.Net.Http.Headers;
using System.Text;
using dotenv.net;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // Example usage
        string prompt = "What is the capital of France?";
        
        try
        {
            var response = await CreateClaudeCompletion(prompt);
            Console.WriteLine($"Claude's response: {response}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }
    }

    public static async Task<string> CreateClaudeCompletion(string prompt, string model = "claude-3-5-sonnet-20241022", int maxTokens = 1000)
    {
        // Load environment variables from .env file
        DotEnv.Load();

        string? apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Please set the ANTHROPIC_API_KEY environment variable");
        }

        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var data = new
            {
                model = model,
                max_tokens = maxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.anthropic.com/v1/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API request failed with status code {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }

            var responseData = await response.Content.ReadAsStringAsync();
            // Parse the JSON response
            return responseData;
        }
    }
}