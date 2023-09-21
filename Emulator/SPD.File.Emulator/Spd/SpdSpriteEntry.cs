using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SPD.File.Emulator.Spd;

public struct SpdSpriteEntry
{
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
    int _spriteId;
    int _spriteTextureId;
    int _unk08;
    int _unk0c;
    int _unk10;
    int _unk14;
    int _unk18;
    int _unk1c;
    int _spriteXPosition;
    int _spriteYPosition;
    int _spriteXLength;
    int _spriteYLength;
    int _unk30;
    int _unk34;
    int _spriteXScale;
    int _spriteYScale;
    int _unk40;
    int _unk44;
    int _unk48;
    int _unk4c;
    int _unk50;
    int _unk54;
    int _unk58;
    int _unk5c;
    int _unk60;
    int _unk64;
    int _unk68;
    int _unk6c;
    unsafe fixed byte _spriteName[48];

    public readonly int GetSpriteId() => _spriteId;
    public readonly int GetSpriteTextureId() => _spriteTextureId;
    public void SetTextureId(int id) => _spriteTextureId = id;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
}

public class SpdSpriteDictionary : IDictionary<int, SpdSpriteEntry>
{
    private readonly Dictionary<int, SpdSpriteEntry> SpriteDictionary;


    public SpdSpriteDictionary()
    {
        SpriteDictionary = new Dictionary<int, SpdSpriteEntry>();
    }

    public SpdSpriteEntry this[int key]
    {
        get { return SpriteDictionary[key]; }
        set { SpriteDictionary[key] = value;}
    }

    public void Add(int key, SpdSpriteEntry value)
    {
        ((IDictionary<int, SpdSpriteEntry>)SpriteDictionary).Add(key, value);
    }

    public bool ContainsKey(int key)
    {
        return ((IDictionary<int, SpdSpriteEntry>)SpriteDictionary).ContainsKey(key);
    }

    public bool Remove(int key)
    {
        return ((IDictionary<int, SpdSpriteEntry>)SpriteDictionary).Remove(key);
    }

    public bool TryGetValue(int key, [MaybeNullWhen(false)] out SpdSpriteEntry value)
    {
        return ((IDictionary<int, SpdSpriteEntry>)SpriteDictionary).TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<int, SpdSpriteEntry> item)
    {
        ((ICollection<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).Add(item);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).Clear();
    }

    public bool Contains(KeyValuePair<int, SpdSpriteEntry> item)
    {
        return ((ICollection<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).Contains(item);
    }

    public void CopyTo(KeyValuePair<int, SpdSpriteEntry>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<int, SpdSpriteEntry> item)
    {
        return ((ICollection<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).Remove(item);
    }

    public IEnumerator<KeyValuePair<int, SpdSpriteEntry>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)SpriteDictionary).GetEnumerator();
    }
    public ICollection<int> Keys => ((IDictionary<int, SpdSpriteEntry>)SpriteDictionary).Keys;

    public ICollection<SpdSpriteEntry> Values => ((IDictionary<int, SpdSpriteEntry>)SpriteDictionary).Values;

    public int Count => ((ICollection<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<int, SpdSpriteEntry>>)SpriteDictionary).IsReadOnly;
}