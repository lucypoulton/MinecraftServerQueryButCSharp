using System.Text.Json.Nodes;

namespace MinecraftQuery;

public struct ServerStatus
{
    public JsonNode Description { get; set; }

    public struct PlayerCount
    {
        public uint Max { get; set; }
        public uint Online { get; set; }
    }
    
    public PlayerCount Players { get; set; }

    public struct VersionInfo
    {
        public string Name { get; set; }
        public int Protocol { get; set; }
    }
    
    public VersionInfo Version { get; set; }
    
    public string? Favicon { get; set; }
    
    public bool? PreviewsChat { get; set; }
}