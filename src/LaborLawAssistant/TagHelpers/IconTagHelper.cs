using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LaborLawAssistant.TagHelpers;

[HtmlTargetElement("icon", TagStructure = TagStructure.WithoutEndTag)]
public class IconTagHelper : TagHelper
{
    private static readonly Dictionary<string, string> Shapes = new()
    {
        ["scale"] = "<path d=\"M12 4v16\"/><path d=\"M8 20h8\"/><path d=\"M5 7h14\"/><path d=\"M2 14l3-7 3 7\"/><path d=\"M2 14a3 3 0 0 0 6 0\"/><path d=\"M16 14l3-7 3 7\"/><path d=\"M16 14a3 3 0 0 0 6 0\"/>",
        ["message"] = "<path d=\"M21 12a8 8 0 0 1-11.6 7.1L4 20l1-4.6A8 8 0 1 1 21 12z\"/>",
        ["file"] = "<path d=\"M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z\"/><path d=\"M14 3v5h5\"/><path d=\"M9 13h6\"/><path d=\"M9 17h6\"/>",
        ["clock"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3 2\"/>",
        ["moon"] = "<path d=\"M20 14.5A8.5 8.5 0 1 1 9.5 4a6.5 6.5 0 0 0 10.5 10.5z\"/>",
        ["sun"] = "<circle cx=\"12\" cy=\"12\" r=\"4\"/><path d=\"M12 2v2\"/><path d=\"M12 20v2\"/><path d=\"M4.9 4.9l1.4 1.4\"/><path d=\"M17.7 17.7l1.4 1.4\"/><path d=\"M2 12h2\"/><path d=\"M20 12h2\"/><path d=\"M4.9 19.1l1.4-1.4\"/><path d=\"M17.7 6.3l1.4-1.4\"/>",
        ["globe"] = "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M3 12h18\"/><path d=\"M12 3a14 14 0 0 1 0 18a14 14 0 0 1 0-18z\"/>",
        ["upload"] = "<path d=\"M12 16V4\"/><path d=\"M7 9l5-5 5 5\"/><path d=\"M4 20h16\"/>",
        ["send"] = "<path d=\"M21 3L10 14\"/><path d=\"M21 3l-6.5 18-4.5-7-7-4.5z\"/>",
        ["check"] = "<path d=\"M5 12.5l4.5 4.5L19 7.5\"/>",
        ["alert"] = "<path d=\"M12 3.5l9.5 16.5h-19z\"/><path d=\"M12 10v4\"/><path d=\"M12 17h.01\"/>",
        ["book"] = "<path d=\"M4 5.5A2.5 2.5 0 0 1 6.5 3H20v15H6.5A2.5 2.5 0 0 0 4 20.5z\"/><path d=\"M4 20.5A2.5 2.5 0 0 1 6.5 18H20v3H6.5\"/>",
        ["arrow-left"] = "<path d=\"M19 12H5\"/><path d=\"M11 18l-6-6 6-6\"/>",
        ["sparkles"] = "<path d=\"M12 3l1.9 4.6L18.5 9.5l-4.6 1.9L12 16l-1.9-4.6L5.5 9.5l4.6-1.9z\"/><path d=\"M19 15l.8 2 2 .8-2 .8-.8 2-.8-2-2-.8 2-.8z\"/>",
        ["search"] = "<circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"M20 20l-4-4\"/>",
        ["layers"] = "<path d=\"M12 3l9 5-9 5-9-5z\"/><path d=\"M3 13l9 5 9-5\"/>",
        ["coins"] = "<ellipse cx=\"12\" cy=\"6\" rx=\"7\" ry=\"3\"/><path d=\"M5 6v6c0 1.7 3.1 3 7 3s7-1.3 7-3V6\"/><path d=\"M5 12v6c0 1.7 3.1 3 7 3s7-1.3 7-3v-6\"/>",
        ["chevron-down"] = "<path d=\"M6 9l6 6 6-6\"/>",
    };

    public string Name { get; set; } = "";

    public int Size { get; set; } = 18;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "svg";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
        output.Attributes.SetAttribute("width", Size);
        output.Attributes.SetAttribute("height", Size);
        output.Attributes.SetAttribute("viewBox", "0 0 24 24");
        output.Attributes.SetAttribute("fill", "none");
        output.Attributes.SetAttribute("stroke", "currentColor");
        output.Attributes.SetAttribute("stroke-width", "1.8");
        output.Attributes.SetAttribute("stroke-linecap", "round");
        output.Attributes.SetAttribute("stroke-linejoin", "round");
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute("focusable", "false");
        output.Content.SetHtmlContent(Shapes.GetValueOrDefault(Name, ""));
    }
}
