using System.Text.RegularExpressions;
using ScriptableRegistry.Util;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace ScriptableRegistry.Editor.Enum
{
    /// <summary>
    /// Immutable enum entry used for both JSON persistence and code generation.
    /// </summary>
    public sealed class EnumParameter
    {
        /// <summary>Raw name from input/json (for display/debug).</summary>
        public string RawName { get; }

        /// <summary>Safe C# identifier for code generation.</summary>
        public string Name { get; }

        /// <summary>Stable numeric value assigned to the enum member.</summary>
        public int Num { get; }

        [JsonConstructor]
        public EnumParameter(string name, int num)
        {
            RawName = name ?? string.Empty;
            Name = Sanitize(RawName);
            Num = num;
        }

        // ---- Sanitizer ----

        /// <summary>
        /// Prefix to make an invalid identifier valid.
        /// </summary>
        private const string IdentifierPrefix = "_";

        /// <summary>
        /// Removes any characters that are not allowed in identifiers.
        /// </summary>
        private static readonly Regex InvalidChars =
            new Regex(@"[^a-zA-Z0-9\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}_]+",
                      RegexOptions.Compiled);

        /// <summary>
        /// Checks if the first character is valid (letter/Japanese/_).
        /// </summary>
        private static readonly Regex ValidStart =
            new Regex(@"^[a-zA-Z\p{IsHiragana}\p{IsKatakana}\p{IsCJKUnifiedIdeographs}_]",
                      RegexOptions.Compiled);

        /// <summary>
        /// Produce a safe identifier by trimming, removing invalid characters, and prefixing when necessary.
        /// </summary>
        public static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("No enum name provided.");
                return string.Empty;
            }

            name = name.Trim();

            // Remove invalid characters.
            name = InvalidChars.Replace(name, string.Empty);
            if (name.Length == 0)
            {
                Debug.LogWarning("Name became empty after sanitization.");
                return string.Empty;
            }

            // Ensure valid starting character.
            if (!ValidStart.IsMatch(name))
            {
                Debug.LogWarning("Name starts with an invalid character. Prefixing underscore.");
                name = StringUtil.Concat(IdentifierPrefix, name);
            }

            // Avoid C# keywords (prefix even if already prefixed above).
            if (IsCSharpKeyword(name))
            {
                Debug.LogWarning("Name is a C# keyword. Prefixing underscore.");
                name = StringUtil.Concat(IdentifierPrefix, name);
            }

            return name;
        }

        /// <summary>
        /// Minimal keyword check to avoid generating invalid enum identifiers.
        /// </summary>
        private static bool IsCSharpKeyword(string s)
        {
            // Minimal practical set. Extend if needed.
            return s is
                "class" or "enum" or "namespace" or "struct" or "interface" or
                "public" or "private" or "protected" or "internal" or "static" or
                "void" or "int" or "string" or "float" or "double" or "bool" or
                "new" or "return" or "using" or "null" or "true" or "false";
        }
    }
}
