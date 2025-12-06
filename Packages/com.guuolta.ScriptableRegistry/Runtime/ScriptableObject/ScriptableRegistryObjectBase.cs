using System;
using System.Collections.Generic;
using ScriptableRegistry.Collections;
using ScriptableRegistry.File;
using UnityEngine;

namespace ScriptableRegistry
{
    /// <summary>
    /// ScriptableObject that auto-generates an enum and a data set.
    /// </summary>
    /// <typeparam name="TKey">Enum key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    public abstract class ScriptableRegistryObjectBase<TKey, TValue> : ScriptableObject
        where TKey : Enum
    {
        // -------- Paths --------

        [Header("Paths")]
        [Tooltip("Folder path that contains target files. No trailing slash needed.")]
        [SerializeField]
        private string _folderPath = "Assets";

        [Tooltip("Folder path to save generated enum. No trailing slash needed.")]
        [SerializeField]
        private string _enumPath = "Assets";

        // -------- Enum Settings --------

        [Header("Enum Settings")]
        [Tooltip("Enum namespace. If empty, no namespace block is generated.")]
        [SerializeField]
        private string _enumNameSpace = "";

        [Tooltip("Enum file name (also used as enum type name).")]
        [SerializeField]
        private string _enumFileName = "";

        // -------- Filters --------

        [Header("Filters")]
        [Tooltip("File extensions to include (e.g., png, mp3).")]
        [SerializeField]
        private List<Extension> _fileExtensions = new();

        [Tooltip("Folder names to ignore during search.")]
        [SerializeField]
        private List<string> _ignoreFolderNames = new();

        // -------- Dictionary --------

        [Header("Data")]
        [SerializeField]
        private SerializableDictionary<TKey, TValue> _dictionary = new();

        /// <summary>
        /// Read-only view of serialized dictionary (no allocation).
        /// </summary>
        public IReadOnlyDictionary<TKey, TValue> Dictionary => _dictionary;

        /// <summary>
        /// Returns a copy as a normal Dictionary.
        /// </summary>
        public Dictionary<TKey, TValue> ToDictionary() => _dictionary.ToDictionary();

#if UNITY_EDITOR
        // -------- Inspector-friendly read-only accessors --------
        public string FolderPath => _folderPath;
        public string EnumPath => _enumPath;
        public string EnumNameSpace => _enumNameSpace;
        public string EnumFileName => _enumFileName;
        public IReadOnlyList<Extension> FileExtensions => _fileExtensions;
        public IReadOnlyList<string> IgnoreFolderNames => _ignoreFolderNames;

        public SerializableDictionary<TKey, TValue> GetDictionary()
        {
            return _dictionary;
        }
        
        // Normalize inputs edited in Inspector.
        private void OnValidate()
        {
            _folderPath = NormalizePath(_folderPath, defaultValue: "Assets");
            _enumPath   = NormalizePath(_enumPath, defaultValue: "Assets");
            _enumFileName = _enumFileName?.Trim() ?? "";
            _enumNameSpace = _enumNameSpace?.Trim() ?? "";
        }

        private static string NormalizePath(string path, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(path))
                return defaultValue;

            path = path.Trim();

            // Remove trailing slash/backslash.
            while (path.EndsWith("/") || path.EndsWith("\\"))
                path = path.Substring(0, path.Length - 1);

            return path;
        }
#endif
    }
}
