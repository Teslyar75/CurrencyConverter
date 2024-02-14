using Newtonsoft.Json;

public class Currency
{
    [JsonProperty("cc")]
    public string Cc { get; set; }

    [JsonProperty("rate")]
    public decimal Rate { get; set; }
}
