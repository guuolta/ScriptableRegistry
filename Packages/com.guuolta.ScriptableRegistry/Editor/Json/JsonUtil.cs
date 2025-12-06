using System;
using System.IO;
using ScriptableRegistry.File;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace ScriptableRegistry.Editor.Json
{
    /// <summary>
    /// JSON file utilities for editor tools.
    /// </summary>
    public static class JsonUtil
    {
        private static readonly Extension JsonExtension = new("json");

        /// <summary>
        /// Reads a JSON file and deserializes it.
        /// Returns default(T) if the file does not exist or deserialization fails.
        /// </summary>
        public static T ReadJsonFile<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
            {
                Debug.LogWarning($"JSON file not found: {path}");
                return default;
            }

            try
            {
                var json = System.IO.File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                Debug.LogWarning($"Failed to read or deserialize JSON file: {path}");
                return default;
            }
        }

        /// <summary>
        /// Serializes an object to JSON and writes it to a file.
        /// Creates the directory if it does not exist.
        /// </summary>
        public static void SaveJsonFile<T>(T value, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is null or empty.", nameof(path));

            path = EnsureExtension(path);

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Combines root and file name and ensures ".json" extension.
        /// </summary>
        public static string GetJsonFilePath(string root, string fileName)
        {
            return Path.Combine(root, JsonExtension.ToFileName(fileName));
        }

        // Ensure ".json" extension is appended if missing.
        private static string EnsureExtension(string path)
        {
            var ext = Path.GetExtension(path);
            return string.IsNullOrEmpty(ext) ? JsonExtension.ToFileName(path) : path;
        }
    }
}
