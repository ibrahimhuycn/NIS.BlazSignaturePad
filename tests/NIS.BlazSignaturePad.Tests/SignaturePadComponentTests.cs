using Bunit;
using NIS.BlazSignaturePad;
using Xunit;

namespace NIS.BlazSignaturePad.Tests;

public class SignaturePadComponentTests : TestContext
{
    public SignaturePadComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void SignaturePad_Renders_Canvas_With_Id()
    {
        JSInterop.SetupModule("./_content/NIS.BlazSignaturePad/sigpad.interop.js?ver=5.1.3").SetupVoid("init", _ => true);
        var cut = RenderComponent<SignaturePad>(parameters => parameters
            .Add(p => p.Id, "test-pad"));

        Assert.Equal("sig-test-pad", cut.Find("canvas").GetAttribute("id"));
    }

    [Fact]
    public void SignaturePad_Renders_Canvas_With_Class()
    {
        JSInterop.SetupModule("./_content/NIS.BlazSignaturePad/sigpad.interop.js?ver=5.1.3").SetupVoid("init", _ => true);
        var cut = RenderComponent<SignaturePad>(parameters => parameters
            .Add(p => p.Class, "my-sig-class"));

        Assert.Contains("my-sig-class", cut.Find("canvas").ClassList);
    }

    [Fact]
    public void SignaturePad_Renders_Canvas_With_Attributes()
    {
        JSInterop.SetupModule("./_content/NIS.BlazSignaturePad/sigpad.interop.js?ver=5.1.3").SetupVoid("init", _ => true);
        var cut = RenderComponent<SignaturePad>(parameters => parameters
            .Add(p => p.Attributes, new Dictionary<string, object> { { "data-testid", "sig-canvas" } }));

        Assert.Equal("sig-canvas", cut.Find("canvas").GetAttribute("data-testid"));
    }
}
