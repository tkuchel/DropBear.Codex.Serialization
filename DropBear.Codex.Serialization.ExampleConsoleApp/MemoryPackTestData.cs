using MemoryPack;

namespace DropBear.Codex.Serialization.ConsoleApp;

[MemoryPackable]
public partial class MemoryPackTestData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime Created { get; set; }
}
