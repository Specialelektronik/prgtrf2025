using System.Text.Json.Serialization;

namespace SE_Crestron_Training.System.DataModel;

public class Information
{
    [JsonPropertyName("guid")]
    public string? Guid { get; set; }
    
    [JsonPropertyName("building")]
    public string? BuildingName { get; set; }
    
    [JsonPropertyName("floor")]
    public string? Floor { get; set; }
    
    [JsonPropertyName("support")]
    public string? SupportNumber { get; set; }
}