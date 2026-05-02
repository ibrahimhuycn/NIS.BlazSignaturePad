using NIS.BlazSignaturePad;
using System.Text.Json;
using Xunit;

namespace NIS.BlazSignaturePad.Tests;

public class SignaturePadOptionsTests
{
    [Fact]
    public void SignaturePadOptions_Default_BackgroundColor_Is_Transparent()
    {
        var options = new SignaturePadOptions();
        Assert.Equal("rgba(0,0,0,0)", options.BackgroundColor);
    }

    [Fact]
    public void SignaturePadOptions_Serializes_CamelCase()
    {
        var options = new SignaturePadOptions
        {
            PenColor = "black",
            MinWidth = 0.5f
        };
        var json = JsonSerializer.Serialize(options);
        Assert.Contains("\"penColor\":\"black\"", json);
        Assert.Contains("\"minWidth\":0.5", json);
    }

    [Fact]
    public void SignaturePadOptions_Null_Values_Are_Not_Serialized_With_WhenWritingNull()
    {
        var options = new SignaturePadOptions
        {
            PenColor = "black",
            MaxWidth = null
        };
        var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        Assert.Contains("\"penColor\":\"black\"", json);
        Assert.DoesNotContain("maxWidth", json);
    }
}
