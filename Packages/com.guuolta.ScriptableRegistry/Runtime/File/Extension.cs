using System;
using ScriptableRegistry.Util;
using UnityEngine;

namespace ScriptableRegistry.File
{
    /// <summary>
    /// Represents a file extension (e.g., "png", "mp3").
    /// Value can be set only via constructor or Inspector.
    /// It is normalized automatically.
    /// </summary>
    [Serializable]
    public sealed class Extension : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Dot prefix used when rendering an extension.
        /// </summary>
        private const string Dot = ".";

        [SerializeField, Header("Value (e.g., png, mp3)")]
        private string _value;
        
        /// <summary>
        /// Creates a new Extension and normalizes the input.
        /// </summary>
        public Extension(string value)
        {
            _value = Normalize(value);
        }

        /// <summary>
        /// Returns the extension with a leading dot (e.g., ".png").
        /// If Value is empty, returns an empty string.
        /// </summary>
        public override string ToString()
        {
            return string.IsNullOrEmpty(_value)
                ? string.Empty
                : StringUtil.Concat(Dot, _value);
        }

        /// <summary>
        /// Appends this extension to a file name.
        /// </summary>
        public string ToFileName(string fileName)
        {
            return WithDot(fileName);
        }

        /// <summary>
        /// Converts to a pattern string for wildcard searches (e.g., "*.png").
        /// </summary>
        public string ToPattern(string fileName = "*")
        {
            return WithDot(fileName);
        }

        /// <summary>
        /// Case-insensitive equality based on the normalized Value.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Extension other &&
                   string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Case-insensitive hash code based on the normalized Value.
        /// </summary>
        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(_value);
        }

        // Keep serialized value normalized.
        public void OnBeforeSerialize()
        {
            _value = Normalize(_value);
        }

        // Keep serialized value normalized.
        public void OnAfterDeserialize()
        {
            _value = Normalize(_value);
        }

        /// <summary>
        /// Normalizes an extension string:
        /// trims whitespace and removes a leading dot if present.
        /// Returns empty string for null/whitespace.
        /// </summary>
        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            value = value.Trim();
            return value.StartsWith(Dot, StringComparison.Ordinal)
                ? value.Substring(1)
                : value;
        }

        /// <summary>
        /// Helper to append this extension to a base string.
        /// If Value is empty, returns the base string unchanged.
        /// </summary>
        private string WithDot(string name)
        {
            if (string.IsNullOrEmpty(_value)) return name;
            return StringUtil.Concat(name, Dot, _value);
        }
    }
}