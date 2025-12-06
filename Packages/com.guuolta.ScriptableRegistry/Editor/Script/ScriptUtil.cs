using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ScriptableRegistry.Util;
using UnityEditor;
using UnityEngine;

namespace ScriptableRegistry.Editor.Script
{
    /// <summary>
    /// Utility helpers for generating, saving, and locating scripts inside the Unity editor.
    /// </summary>
    public static class ScriptUtil
    {
        /// <summary>
        /// Search filter format.
        /// <para>0: file name</para>
        /// </summary>
        private const string SearchFilterFormat = "{0} t:Script";
        
        /// <summary>
        /// Type full name format.
        /// </summary>
        private const string TypeFormat = "{0}, {1}";

        /// <summary>
        /// Build a search filter for AssetDatabase.FindAssets.
        /// </summary>
        private static string GetSearchFilter(string fileName)
            => StringUtil.Format(SearchFilterFormat, fileName);

        /// <summary>
        /// Find a script asset path by exact file name (without extension).
        /// </summary>
        /// <param name="fileName">File name without extension.</param>
        /// <returns>Unity asset path, or empty if not found.</returns>
        public static string FindScriptPath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            var guids = AssetDatabase.FindAssets(GetSearchFilter(fileName));
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == fileName)
                    return path;
            }

            Debug.LogWarning($"{fileName} was not found.");
            return string.Empty;
        }

        /// <summary>
        /// Write script contents to disk (overwrite if exists) and refresh assets.
        /// </summary>
        public static void SaveScript(Script script)
        {
            if (script == null)
            {
                Debug.LogError("No script provided.");
                return;
            }
            
            var dir = Path.GetDirectoryName(script.FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            System.IO.File.WriteAllText(script.FilePath, script.Code.Contents);
            Debug.Log("Create Script: " + script.FilePath);
        }

        /// <summary>
        /// Write script only if the file does not already exist.
        /// </summary>
        public static void SaveScriptIfNotExists(Script script)
        {
            if (script == null)
            {
                Debug.LogError("No script provided.");
                return;
            }
            
            if (System.IO.File.Exists(script.FilePath))
            {
                Debug.Log("Already Create Script: " + script.FilePath);
                return;
            }

            SaveScript(script);
        }

        /// <summary>
        /// Delete a folder and all script files inside it.
        /// </summary>
        public static void DeleteScripts(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                return;

            if (!Directory.Exists(directoryPath))
            {
                Debug.Log("Directory does not exist: " + directoryPath);
                return;
            }

            // Delete scripts in the directory.
            var scriptPaths = Directory.GetFiles(
                directoryPath,
                Script.CsExtension.ToPattern(),
                SearchOption.AllDirectories);

            foreach (var scriptPath in scriptPaths)
            {
                var unityPath = ToUnityPath(scriptPath);

                if (AssetDatabase.DeleteAsset(unityPath))
                    Debug.Log($"Delete Success: {unityPath}");
                else
                    Debug.LogError($"Delete Failed: {unityPath}");
            }

            // Delete the directory itself.
            if (AssetDatabase.DeleteAsset(ToUnityPath(directoryPath)))
                Debug.Log($"Delete Success: {directoryPath}");
            else
                Debug.LogError($"Delete Failed: {directoryPath}");

            AssetDatabase.Refresh();
        }
        
        public static bool TryGetTypeFromScriptName(string name, out Type type, string nameSpace = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                Debug.LogWarning("No script name provided.");
                type = null;
                return false;
            }

            try
            {
                type = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch // ignore broken assemblies
                        {
                            Debug.LogWarning($"Failed to get types from assembly.");
                            return Array.Empty<Type>();
                        }
                    })
                    .FirstOrDefault(t =>
                        t.Name == name && 
                        (string.IsNullOrEmpty(nameSpace) || t.Namespace == nameSpace));

                return type != null;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                type = null;
                return false;
            }
        }

        /// <summary>
        /// Convert OS path to Unity asset path format.
        /// </summary>
        private static string ToUnityPath(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
