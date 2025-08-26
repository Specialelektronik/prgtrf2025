using System.Text.Json.Serialization;

namespace SE_Crestron_Training.System.DataModel;

public class Room
{
    [JsonPropertyName("information")]
    public Information? Information { get; init; }

    [JsonPropertyName("sources")] 
    public List<Source> Sources { get; set; } = new();

    [JsonPropertyName("screens")] 
    public List<Screen> Screens { get; set; } = new();
    
    [JsonPropertyName("last_update")]
    public string? LastUpdate { get; set; }
}