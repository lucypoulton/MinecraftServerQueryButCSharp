using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace MinecraftQuery.Util;

public static class ComponentUtil
{
    private static readonly Regex Modifiers = new("§\\w");

    private static void Concat(JsonNode component, StringBuilder builder)
    {
        try
        {
            builder.Append(component["text"] ?? component["translate"]);
            var extra = component["extra"]?.AsArray();
            if (extra != null)
                foreach (var node in extra)
                {
                    Concat(node, builder);
                }
        }
        catch (InvalidOperationException)
        {
            builder.Append(component.AsValue());
        }
    }

    public static string ToPlainString(JsonNode component)
    {
        var builder = new StringBuilder();
        Concat(component, builder);
        return Modifiers.Replace(builder.ToString(), "");
    }
}