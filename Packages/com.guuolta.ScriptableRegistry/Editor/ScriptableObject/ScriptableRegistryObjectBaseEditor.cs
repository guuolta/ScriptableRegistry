using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScriptableRegistry.Editor.Enum;
using UnityEditor;
using UnityEngine;

namespace ScriptableRegistry.Editor
{
    /// <summary>
    /// Base editor for ScriptableRegistryObjectBase.
    /// Two-step workflow:
    /// 1) Generate Enum / Json (triggers recompile)
    /// 2) Register dictionary from existing enum
    /// </summary>
    public abstract class ScriptableRegistryObjectBaseEditor<TSO, TKey, TValue, TFile> : UnityEditor.Editor
        where TSO   : ScriptableRegistryObjectBase<TKey, TValue>
        where TKey  : System.Enum
        where TFile : UnityEngine.Object
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate Enum / Json"))
                GenerateEnumAndJson(target as TSO);

            if (GUILayout.Button("Register Dictionary From Existing Enum"))
                RegisterDictionary(target as TSO);

            if (GUILayout.Button("Reset"))
                Reset(target as TSO);

            EditorGUILayout.Space(4);
            base.OnInspectorGUI();
        }

        // ----------------------------------------------------------------------
        // Step 1: Generate Enum / Json
        // ----------------------------------------------------------------------

        /// <summary>
        /// Generate Enum + Json only.
        /// This step will trigger script recompilation.
        /// </summary>
        private void GenerateEnumAndJson(TSO targetSO)
        {
            if (targetSO == null)
            {
                Debug.LogError("[ScriptableRegistry] Target object is null.");
                return;
            }

            var filePaths = GetPaths(targetSO);
            if (filePaths.Length == 0)
            {
                Debug.LogError("[ScriptableRegistry] No target files found. Check folder path or extensions.");
                return;
            }

            var enumStrParameters = GetFileNames(filePaths);

            CreateEnumFile(targetSO, enumStrParameters);
        }

        // ----------------------------------------------------------------------
        // Step 2: Register Dictionary
        // ----------------------------------------------------------------------

        /// <summary>
        /// Register dictionary using already generated enum.
        /// Assumes scripts are compiled.
        /// </summary>
        private void RegisterDictionary(TSO targetSO)
        {
            if (targetSO == null)
            {
                Debug.LogError("[ScriptableRegistry] Target object is null.");
                return;
            }

            var filePaths = GetPaths(targetSO);
            if (filePaths.Length == 0)
            {
                Debug.LogError("[ScriptableRegistry] No target files found. Check folder path or extensions.");
                return;
            }

            var files = LoadFiles(filePaths);
            if (files.Length == 0)
            {
                Debug.LogError("[ScriptableRegistry] No assets loaded. Check file types or paths.");
                return;
            }

            var keys = GetKeys(filePaths);
            if (keys.Length == 0)
            {
                Debug.LogError("[ScriptableRegistry] No enum keys resolved. Generate Enum first.");
                return;
            }

            CreateDictionary(targetSO, keys, files);
            Save(targetSO);

            Debug.Log($"[ScriptableRegistry] Dictionary registered: {files.Length} items.");
        }

        // ----------------------------------------------------------------------
        // Reset
        // ----------------------------------------------------------------------

        /// <summary>
        /// Clears dictionary and saves the asset.
        /// </summary>
        private void Reset(TSO targetSO)
        {
            if (targetSO == null)
            {
                Debug.LogError($"[ScriptableRegistry] Target object is null.");
                return;
            }

            targetSO.GetDictionary().Clear();
            Save(targetSO);

            Debug.Log("[ScriptableRegistry] Dictionary reset.");
        }

        // ----------------------------------------------------------------------
        // Path / Load helpers
        // ----------------------------------------------------------------------

        /// <summary>
        /// Collect all matching file paths under target folder,
        /// excluding ignored folders.
        /// </summary>
        private string[] GetPaths(TSO targetSO)
        {
            var folderPath = targetSO.FolderPath;
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                Debug.LogError($"[ScriptableRegistry] Folder path invalid: {folderPath}");
                return Array.Empty<string>();
            }

            var ignoreFolderNames = targetSO.IgnoreFolderNames?.ToArray()
                                   ?? Array.Empty<string>();

            var paths = new List<string>(256);

            foreach (var ext in targetSO.FileExtensions)
            {
                var found = Directory.GetFiles(
                    folderPath,
                    ext.ToPattern(),
                    SearchOption.AllDirectories);

                foreach (var path in found)
                {
                    if (ContainsFolder(path, ignoreFolderNames))
                        continue;

                    paths.Add(path);
                }
            }

            return paths.ToArray();
        }

        /// <summary>
        /// Returns true if the path contains any ignored folder name.
        /// </summary>
        private bool ContainsFolder(string path, params string[] folderNames)
        {
            if (folderNames == null || folderNames.Length == 0)
                return false;

            var directories = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (var folderName in folderNames)
            {
                if (string.IsNullOrWhiteSpace(folderName))
                {
                    Debug.LogError($"[ScriptableRegistry] Folder name invalid: {folderName}");
                    continue;
                }

                if (directories.Contains(folderName, StringComparer.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Extract file names without extension from paths.
        /// </summary>
        private string[] GetFileNames(string[] paths)
        {
            var fileNames = new List<string>();
            foreach (var path in paths)
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(path));
            }

            return fileNames.ToArray();
        }

        /// <summary>
        /// Load assets by paths.
        /// Returned array is sorted by asset name for stable order.
        /// </summary>
        private TFile[] LoadFiles(string[] paths)
        {
            var files = new List<TFile>();
            foreach (var path in paths)
            {
                var file = AssetDatabase.LoadAssetAtPath<TFile>(path);
                if (file == null)
                {
                    Debug.LogError($"[ScriptableRegistry] Failed to load asset: {path}");
                    continue;
                }

                files.Add(file);
            }

            files.Sort((f1, f2) => string.Compare(f1.name, f2.name, StringComparison.Ordinal));
            return files.ToArray();
        }

        // ----------------------------------------------------------------------
        // Enum / Dictionary helpers
        // ----------------------------------------------------------------------

        /// <summary>
        /// Resolve TKey array from file names (sanitized before parsing).
        /// </summary>
        private TKey[] GetKeys(string[] paths)
        {
            return GetFileNames(paths)
                .Where(fileName => !string.IsNullOrEmpty(fileName))
                .Select(fileName =>
                    (TKey)System.Enum.Parse(typeof(TKey), EnumParameter.Sanitize(fileName)))
                .ToArray();
        }

        /// <summary>
        /// Create Enum and Json from candidates.
        /// </summary>
        private void CreateEnumFile(TSO targetSO, string[] enumParams)
        {
            var defaultParams = GetDefaultParams();
            if (defaultParams.Length > 0)
            {
                enumParams = defaultParams.Concat(enumParams).ToArray();
            }
            
            EnumUtil.CreateEnum(
                enumParams,
                targetSO.EnumNameSpace,
                targetSO.EnumFileName,
                targetSO.EnumPath);
        }

        /// <summary>
        /// default enum parameters to prepend.
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetDefaultParams()
        {
            return Array.Empty<string>();
        }

        /// <summary>
        /// Populate dictionary with resolved keys and loaded files.
        /// </summary>
        private void CreateDictionary(
            TSO targetSO,
            TKey[] keys,
            TFile[] files)
        {
            var dict = targetSO.GetDictionary();
            dict.Clear();

            for (int i = 0; i < files.Length; i++)
            {
                dict[keys[i]] = CreateValue(files[i]);
            }
        }

        /// <summary>
        /// Create TValue from TFile. Implement in derived editors.
        /// </summary>
        protected abstract TValue CreateValue(TFile file);

        /// <summary>
        /// Save ScriptableObject to disk and refresh editor view.
        /// </summary>
        private void Save(TSO targetSO)
        {
            EditorUtility.SetDirty(targetSO);
            AssetDatabase.SaveAssets();
            Repaint();
        }
    }
}
