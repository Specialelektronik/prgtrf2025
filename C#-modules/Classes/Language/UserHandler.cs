using System.Text.Json;
using SE_Crestron_Training.Language.DataModel;
using SE_Crestron_Training.Logging;

namespace SE_Crestron_Training.Language;

public class UserHandler
{
    HttpClient _client = new HttpClient();
    public UserHandler()
    {
        
    }

    public async Task GetUserInformation()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://jsonplaceholder.typicode.com/users");
        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var users = await JsonSerializer.DeserializeAsync<List<User>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (users is not null)
        {
            foreach (var user in users)
            {
                SeriLog.Log?.Debug($"UserID: {user.id}, Name: {user.Name}, Email: {user.Email}");
            }
        }
    }
}