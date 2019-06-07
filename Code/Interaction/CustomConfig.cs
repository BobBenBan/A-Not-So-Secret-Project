using System.Collections;
using System.Collections.Generic;
using Godot;

namespace MusicMachine.Interaction
{
public abstract class CustomConfig : Resource, IEnumerable<KeyValuePair<string, object>>
{
    public string TypeId { get; protected set; }

    protected CustomConfig()
    {
    }

    public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public abstract object this[string key] { get; set; }
}
}