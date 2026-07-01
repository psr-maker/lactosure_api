using System.Text.Json;

public class FaceService
{
    private readonly HttpClient _http;

    public FaceService(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<List<float>> GetEmbedding(byte[] imageBytes)
    {
        using var content = new MultipartFormDataContent();

        var file = new ByteArrayContent(imageBytes);
        file.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        content.Add(file, "file", "face.jpg");
        Console.WriteLine("Calling Python...");

        var response = await _http.PostAsync("http://localhost:8000/embedding", content);
        Console.WriteLine("Python replied.");

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);
        Console.WriteLine("------------------");

        var result = JsonSerializer.Deserialize<EmbeddingResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

        if (result == null || result.Embedding == null)
            throw new Exception("Embedding is NULL from Python API");

        return result.Embedding;
    }
}

public class EmbeddingResponse
{
    public List<float> Embedding { get; set; }
}