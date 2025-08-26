using System.Text.Json.Serialization;

namespace SE_Crestron_Training.Language.DataModel;

public class Address
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }
    
    [JsonPropertyName("suite")]
    public string? Suite { get; set; }
    
    [JsonPropertyName("city")]
    public string? City { get; set; }
    
    [JsonPropertyName("zipcode")]
    public string? Zipcode { get; set; }
}