// --------------------------------------------------
// RgssDecrypter - OptionAction.cs
// --------------------------------------------------

namespace RgssDecrypter.Options
{
    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}
