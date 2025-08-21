#nullable enable
using System.Runtime.CompilerServices;

namespace Findx.Common;

/// <summary>
///     ValueStringBuilder
/// </summary>
public ref struct ValueStringBuilder
{
    private int _index;
    private char[]? _array;
    private Span<char> _chars;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="buffer"></param>
    public ValueStringBuilder(Span<char> buffer)
    {
        _index = 0;
        _array = null;
        _chars = buffer;
    }

    /// <summary>
    ///     添加 char
    /// </summary>
    /// <param name="value"></param> 
    public void Append(char value)
    {
        var newSize = _index + 1;
        if (newSize > _chars.Length)
        {
            Grow(newSize);
        }

        _chars[_index..][0] = value;
        _index = newSize;
    }

    /// <summary>
    ///     添加 chars
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public void Append(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            return;
        }

        var newSize = _index + value.Length;
        if (newSize > _chars.Length)
        {
            Grow(newSize);
        }

        value.CopyTo(_chars[_index..]);
        _index = newSize;
    }

    /// <summary>
    ///     添加 chars 并换行
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public void AppendLine(ReadOnlySpan<char> value)
    {
        Append(value);
        Append(Environment.NewLine);
    }

    /// <summary>
    ///     扩容
    /// </summary>
    /// <param name="newSize"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int newSize)
    {
        var size = Math.Max(newSize, _chars.Length * 2);
        if (_array == null)
        {
            _array = new char[size];
            _chars.CopyTo(_array);
        }
        else
        {
            Array.Resize(ref _array, size);
        }

        _chars = _array.AsSpan();
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns></returns>
    public readonly override string ToString()
    {
        return _chars[.._index].ToString();
    }
}
