#if USE_ZSTRING
using Cysharp.Text;
#endif

namespace ScriptableRegistry.Util
{
    /// <summary>
    /// One-line string helpers. Fast overloads for common cases,
    /// fallback to params object[] for convenience.
    /// </summary>
    public static class StringUtil
    {
        // ---- Fast overloads (no object[] allocation) ----

        public static string Concat(string a, string b)
        {
#if USE_ZSTRING
            return ZString.Concat(a, b);
#else
            return string.Concat(a, b);
#endif
        }

        public static string Concat<T1>(string a, T1 b)
        {
#if USE_ZSTRING
            return ZString.Concat(a, b);
#else
            // constrained ToString avoids boxing for structs
            return string.Concat(a, b);
#endif
        }

        public static string Concat<T1, T2>(string a, T1 b, T2 c)
        {
#if USE_ZSTRING
            return ZString.Concat(a, b, c);
#else
            return string.Concat(a, b, c);
#endif
        }

        public static string Concat<T1, T2, T3>(string a, T1 b, T2 c, T3 d)
        {
#if USE_ZSTRING
            return ZString.Concat(a, b, c, d);
#else
            return string.Concat(a, b, c, d);
#endif
        }
        
        public static string Concat<T1, T2, T3, T4>(string a, T1 b, T2 c, T3 d, T4 e)
        {
#if USE_ZSTRING
            return ZString.Concat(a, b, c, d, e);
#else
            return string.Concat(a, b, c, d, e);
#endif
        }

        // ---- Convenience fallback (slow path) ----
        public static string Concat(params object[] objects)
        {
            return string.Concat(objects);
        }

        // ---- Format (fast enough; use Build if you need max perf) ----
        public static string Format<T1>(string format, T1 a)
        {
#if USE_ZSTRING
            return ZString.Format(format, a);
#else
            return string.Format(format, a);
#endif
        }

        public static string Format<T1, T2>(string format, T1 a, T2 b)
        {
#if USE_ZSTRING
            return ZString.Format(format, a, b);
#else
            return string.Format(format, a, b);
#endif
        }

        public static string Format<T1, T2, T3>(string format, T1 a, T2 b, T3 c)
        {
#if USE_ZSTRING
            return ZString.Format(format, a, b, c);
#else
            return string.Format(format, a, b, c);
#endif
        }
        
        public static string Format<T1, T2, T3, T4>(string format, T1 a, T2 b, T3 c, T4 d)
        {
#if USE_ZSTRING
            return ZString.Format(format, a, b, c, d);
#else
            return string.Format(format, a, b, c, d);
#endif
        }

        public static string Format(string format, params object[] args)
        {
            return string.Format(format, args);
        }

        // ---- Join ----
        public static string Join<T>(string separator, params T[] parts)
        {
#if USE_ZSTRING
            return ZString.Join(separator, parts);
#else
            return string.Join(separator, parts);
#endif
        }
    }
}