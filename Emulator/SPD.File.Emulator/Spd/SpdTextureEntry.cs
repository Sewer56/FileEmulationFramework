using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SPD.File.Emulator.Spd;

public struct SpdTextureEntry
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
    int _textureId;
    int _unk04;
    int _textureDataOffset;
    int _textureDataSize;
    int _textureWidth;
    int _textureHeight;
    int _unk18;
    int _unk1c;
    unsafe fixed byte _textureName[16];

    public readonly int GetTextureId() => _textureId;
    public readonly (int, int) GetTextureOffsetAndSize() => (_textureDataOffset, _textureDataSize);
    public void SetTextureOffset(int newOffset) => _textureDataOffset = newOffset;
    public void SetTextureId(int id) => _textureId = id;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}

public class SpdTextureDictionary : IDictionary<int, SpdTextureEntry>
{
    private readonly Dictionary<int, SpdTextureEntry> textureDictionary;


    public SpdTextureDictionary()
    {
        textureDictionary = new Dictionary<int, SpdTextureEntry>();
    }

    public SpdTextureEntry this[int key]
    {
        get { return textureDictionary[key]; }
        set { textureDictionary[key] = value; }
    }

    public void Add(int key, SpdTextureEntry value)
    {
        ((IDictionary<int, SpdTextureEntry>)textureDictionary).Add(key, value);
    }

    public bool ContainsKey(int key)
    {
        return ((IDictionary<int, SpdTextureEntry>)textureDictionary).ContainsKey(key);
    }

    public bool Remove(int key)
    {
        return ((IDictionary<int, SpdTextureEntry>)textureDictionary).Remove(key);
    }

    public bool TryGetValue(int key, [MaybeNullWhen(false)] out SpdTextureEntry value)
    {
        return ((IDictionary<int, SpdTextureEntry>)textureDictionary).TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<int, SpdTextureEntry> item)
    {
        ((ICollection<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).Add(item);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).Clear();
    }

    public bool Contains(KeyValuePair<int, SpdTextureEntry> item)
    {
        return ((ICollection<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).Contains(item);
    }

    public void CopyTo(KeyValuePair<int, SpdTextureEntry>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<int, SpdTextureEntry> item)
    {
        return ((ICollection<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).Remove(item);
    }

    public IEnumerator<KeyValuePair<int, SpdTextureEntry>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)textureDictionary).GetEnumerator();
    }
    public ICollection<int> Keys => ((IDictionary<int, SpdTextureEntry>)textureDictionary).Keys;

    public ICollection<SpdTextureEntry> Values => ((IDictionary<int, SpdTextureEntry>)textureDictionary).Values;

    public int Count => ((ICollection<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<int, SpdTextureEntry>>)textureDictionary).IsReadOnly;
}