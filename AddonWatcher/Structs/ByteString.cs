using System;
using System.Linq;
using System.Text;

namespace AddonWatcher.Structs;

public readonly struct ByteString
{
    private readonly byte[] _data;

    public ByteString(params byte[] data)
        => _data = data;

    public ByteString(params char[] data)
        => _data = Encoding.UTF8.GetBytes(data);

    public ByteString(string data)
        => _data = Encoding.UTF8.GetBytes(data);

    public unsafe bool Equals(byte* ptr)
    {
        if (ptr == null)
            return false;

        var i = 0;
        for (; i < _data.Length; ++i, ++ptr)
        {
            var c1 = _data[i];
            var c2 = *ptr;
            if (c1 != c2)
                return false;

            if (c2 == 0)
                break;
        }

        return i == _data.Length;
    }

    public unsafe bool Equals(byte* ptr, int length)
    {
        if (_data.Length != length)
            return false;

        for (var i = 0; i < length; ++i, ++ptr)
        {
            var c1 = _data[i];
            var c2 = *ptr;
            if (c1 != c2)
                return false;
        }

        return true;
    }

    public bool Equals(ReadOnlySpan<byte> data)
        => data.SequenceEqual(_data);

    public unsafe bool Equals(IntPtr ptr)
        => Equals((byte*)ptr);

    public unsafe bool Equals(IntPtr ptr, int length)
        => Equals((byte*)ptr, length);
}
