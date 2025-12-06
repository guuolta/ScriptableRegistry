#if USE_ZSTRING
using Cysharp.Text;
#endif

namespace ScriptableRegistry.Util
{
    /// <summary>
    /// Simple factory that swaps between ZString and System.Text.StringBuilder depending on defines.
    /// </summary>
    public static class StringBuilderFactory
    {
#if USE_ZSTRING
        public static Utf16ValueStringBuilder Create() => ZString.CreateStringBuilder();
#else
        public static System.Text.StringBuilder Create() => new();
#endif

#if USE_ZSTRING
        public static void Release(Utf16ValueStringBuilder sb) => sb.Dispose();
#else
        public static void Release(System.Text.StringBuilder sb) {}
#endif
    }

}
