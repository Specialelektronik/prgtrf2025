using System.Text.Json.Serialization;

namespace SE_Crestron_Training.System.DataModel;

public class Screen
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("output")]
    public int Output { get; set; }
    
    [JsonPropertyName("ipid")]
    public uint IpId { get; set; }
}