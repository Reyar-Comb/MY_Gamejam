using Godot;

[GlobalClass]
public partial class AudioKeyValuePair : Resource
{
    [Export] public string Key { get; set; }
    [Export] public AudioStream Value { get; set; }
}