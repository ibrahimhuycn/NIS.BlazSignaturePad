using System.Text.Json.Serialization;

namespace NIS.BlazSignaturePad;

public class SignaturePadOptions
{
    [JsonPropertyName("minWidth")]
    public float? MinWidth { get; set; }

    [JsonPropertyName("maxWidth")]
    public float? MaxWidth { get; set; }

    [JsonPropertyName("penColor")]
    public string? PenColor { get; set; }

    [JsonPropertyName("backgroundColor")]
    public string? BackgroundColor { get; set; } = "rgba(0,0,0,0)";

    [JsonPropertyName("velocityFilterWeight")]
    public float? VelocityFilterWeight { get; set; }

    [JsonPropertyName("dotSize")]
    public float? DotSize { get; set; }

    [JsonPropertyName("minDistance")]
    public float? MinDistance { get; set; }

    [JsonPropertyName("throttle")]
    public int? Throttle { get; set; }

    [JsonPropertyName("compositeOperation")]
    public string? CompositeOperation { get; set; }
}
