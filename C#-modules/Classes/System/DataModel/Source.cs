using System.Text.Json.Serialization;

namespace SE_Crestron_Training.System.DataModel;

public class Source
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("icon")]
    public int? Icon { get; set; }
        
    [JsonPropertyName("input")]
    public int Input { get; set; }
    
    [JsonPropertyName("rtsp_url")]
    public string? RtspUrl { get; set; }
    
    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }
    
    [JsonPropertyName("ipid")]
    public uint IpId { get; set; }
}