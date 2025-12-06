using System;
using System.Linq;
using ScriptableRegistry.Editor.Json;
using ScriptableRegistry.Editor.Script;
using UnityEngine;

namespace ScriptableRegistry.Editor.Enum
{
    /// <summary>
    /// Editor-side utilities for creating enum definitions and keeping their values stable across regenerations.
    /// </summary>
    public static class EnumUtil
    {
        /// <summary>
        /// Create enum assets (.json + .cs).
        /// </summary>
        public static EnumParameter[] CreateEnum(
            string[] parameterNames,
            string nameSpace,
            string fileName,
            string root)
        {
            if (parameterNames == null || parameterNames.Length == 0)
            {
                Debug.LogWarning("No enum parameters provided.");
                return Array.Empty<EnumParameter>();
            }

            var jsonPath = JsonUtil.GetJsonFilePath(root, fileName);
            var parameters = CreateParameters(parameterNames, jsonPath);

            JsonUtil.SaveJsonFile(parameters, jsonPath);
            SaveEnumScriptFile(parameters, nameSpace, fileName, root);
            
            return parameters;
        }

        /// <summary>
        /// Build parameters, keeping existing numbers stable.
        /// </summary>
        private static EnumParameter[] CreateParameters(string[] parameterNames, string jsonPath)
        {
            // Remove null/empty and keep order, then distinct (by raw).
            var rawNames = parameterNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .ToArray();

            var previous = JsonUtil.ReadJsonFile<EnumParameter[]>(jsonPath)
                          ?? Array.Empty<EnumParameter>();

            // Map by RawName (not SafeName) to keep stable IDs for user input.
            var prevDict = previous.ToDictionary(p => p.Name, p => p.Num);

            // Next number should start from max+1. (Fix off-by-one)
            var nextNum = previous.Length == 0
                ? 0
                : previous.Max(p => p.Num) + 1;

            var result = new EnumParameter[rawNames.Length];

            for (int i = 0; i < rawNames.Length; i++)
            {
                var raw = rawNames[i];
                if (prevDict.TryGetValue(raw, out var num))
                {
                    result[i] = new EnumParameter(raw, num);
                }
                else
                {
                    result[i] = new EnumParameter(raw, nextNum++);
                }
            }

            return result;
        }

        /// <summary>
        /// Save enum script file (.cs).
        /// </summary>
        private static void SaveEnumScriptFile(
            EnumParameter[] parameters,
            string nameSpace,
            string fileName,
            string root)
        {
            var code = new EnumCode(parameters, nameSpace, fileName);
            var script = new Script.Script(root, fileName, code);
            ScriptUtil.SaveScript(script);
        }
    }
}
