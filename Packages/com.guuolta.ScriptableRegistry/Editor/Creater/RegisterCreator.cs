using System;
using System.Linq;
using ScriptableRegistry.Editor.Enum;
using ScriptableRegistry.Editor.Script;
using ScriptableRegistry.Util;
using UnityEngine;

namespace ScriptableRegistry.Editor.Creator
{
    /// <summary>
    /// Generates registry-related scripts (registry asset, editor, placeholder types) based on user input.
    /// </summary>
    public static class RegisterCreator
    {
        private static readonly UsingModule ScriptableRegistryUsingModule = new(new[]
        {
            "ScriptableRegistry",
            "UnityEngine",
        });
        private static readonly UsingModule EditorUsingModule = new(new[]
        {
            "UnityEditor",
            "ScriptableRegistry.Editor",
        });
        
        private const string CreateAssetMenuName = "CreateAssetMenu";
        private const string MenuNameFormat = "menuName = \"{0}\"";
        private const string FileNameFormat = "fileName = \"{0}\"";

        private static readonly string EditorAttributeFormat = "CustomEditor(typeof({0}))";
        
        private const string RegisterObjectBaseTypeNameFormat = "ScriptableRegistryObjectBase<{0}, {1}>";
        private const string EditorTypeNameFormat = "ScriptableRegistryObjectBaseEditor<{0}, {1}, {2}, {3}>";

        private const string SameEditorContentsFormat = @"protected override {0} CreateValue({0} file)
        {{
            return file;
        }}";
        private const string DiffetentEditorContentsFormat = @"protected override {0} CreateValue({1} file)
        {{
            return new {0}();
        }}";
        
        /// <summary>
        /// Create registry ScriptableObject, its editor, and any missing key/value/target type stubs.
        /// </summary>
        public static void CreateRegisterObjectScript(string scriptName, string root, string nameSpace, 
            string editorName, string editorPath, string editorNameSpace,
            string menuName, string fileName,
            string keyType, string keyTypeNameSpace, 
            string valueType, string valueTypeNameSpace,
            string targetFileName)
        {
            if (!TryGetFileNameSpace(targetFileName, out var targetFileNameSpace))
            {
                Debug.LogError($"{targetFileName} is not a valid target file type.");
                return;
            }
            
            CreateKeyEnumScript(keyType, root, keyTypeNameSpace);
            CreateValueClassScript(valueType, root, valueTypeNameSpace);
            
            var commonUsingModule = CreateUsingModule(keyTypeNameSpace, valueTypeNameSpace, targetFileNameSpace);
            
            var scriptableRegistryUsingModule = ScriptableRegistryUsingModule.Add(commonUsingModule);
            var scriptableRegistryAttribute = CreateAssetMenuAttribute(menuName, fileName);
            var scriptableRegistryBaseTypeName = StringUtil.Format(RegisterObjectBaseTypeNameFormat, keyType, valueType);
            CreateRegisterObjectClassScript(scriptName, root, nameSpace, scriptableRegistryUsingModule, scriptableRegistryAttribute, scriptableRegistryBaseTypeName);
            
            var editorUsingModule = EditorUsingModule.Add(commonUsingModule);
            var editorAttribute = new ScriptAttribute(StringUtil.Format(EditorAttributeFormat, scriptName));
            var editorBaseTypeName = StringUtil.Format(EditorTypeNameFormat, scriptName, keyType, valueType, targetFileName);
            var editorContents = valueType == targetFileName
                ? StringUtil.Format(SameEditorContentsFormat, valueType)
                : StringUtil.Format(DiffetentEditorContentsFormat,valueType, targetFileName);
            CreateEditorClassScript(editorName, editorPath, editorNameSpace, editorUsingModule, editorAttribute, editorBaseTypeName, editorContents);
        }

        private static void CreateKeyEnumScript(string scriptName, string root, string nameSpace)
        {
            if (ScriptUtil.TryGetTypeFromScriptName(scriptName, out var type, nameSpace))
            {
                if (!type.IsEnum)
                {
                    throw new ArgumentException($"The type {type.FullName} is not an enum: {type.FullName}");
                }
                return;
            }

            var keyCode = new EnumCode(Array.Empty<EnumParameter>(), nameSpace, scriptName);
            var keyScript = new Script.Script(root, scriptName, keyCode);
            ScriptUtil.SaveScriptIfNotExists(keyScript);
        }
        
        private static void CreateValueClassScript(string scriptName, string root, string nameSpace)
        {
            if (ScriptUtil.TryGetTypeFromScriptName(scriptName, out var type, nameSpace))
            {
                if((!type.IsSerializable && !typeof(UnityEngine.Object).IsAssignableFrom(type)) || (type.IsSerializable && type.IsEnum))
                {
                    throw new ArgumentException($"The type {type.FullName} is not a serializable class or struct: {type.FullName}");
                }
                
                return;
            }

            var valueCode = new ClassCode(scriptName, nameSpace: nameSpace,  attribute: new ScriptAttribute("System.Serializable"));
            var valueScript = new Script.Script(root, scriptName, valueCode);
            ScriptUtil.SaveScriptIfNotExists(valueScript);
        }

        private static ScriptAttribute CreateAssetMenuAttribute(string menuName, string fileName)
        {
            var builder = StringBuilderFactory.Create();
            
            builder.Append(CreateAssetMenuName);
            
            var hasMenuName = !string.IsNullOrEmpty(menuName);
            var hasFileName = !string.IsNullOrEmpty(fileName);
            if(!hasMenuName && !hasFileName)
            {
                return new ScriptAttribute(builder.ToString());
            }
            
            builder.Append("(");
            if(hasMenuName)
            {
                builder.AppendFormat(MenuNameFormat, menuName);
            }
            
            if(hasMenuName && hasFileName)
            {
                builder.Append(", ");
                builder.AppendFormat(FileNameFormat, fileName);
            }
            else if (hasFileName)
            {
                builder.AppendFormat(FileNameFormat, fileName);
            }
            builder.Append(")");
            
            var result = builder.ToString();
            StringBuilderFactory.Release(builder);
            
            return new ScriptAttribute(result);
        }
        
        private static UsingModule CreateUsingModule(string rootNameSpace, params string[] nameSpaces)
        {
            if (nameSpaces == null || nameSpaces.Length == 0)
            {
                Debug.LogError("No namespace provided for using module.");
                return new UsingModule();
            }
            
            var allNameSpaces = nameSpaces
                .Where(ns => !string.IsNullOrEmpty(ns) && ns != rootNameSpace)
                .Distinct()
                .ToArray();
            
            return new UsingModule(allNameSpaces);
        }

        private static void CreateRegisterObjectClassScript(string scriptName, string root, string nameSpace, 
            UsingModule usingModule, ScriptAttribute attribute, string baseTypeName)
        {
            var registerObjectCode = new ClassCode(scriptName, usingModule, nameSpace, attribute, baseClassNames: baseTypeName);
            var registerObjectScript = new Script.Script(root, scriptName, registerObjectCode);
            ScriptUtil.SaveScript(registerObjectScript);
        }
        
        private static void CreateEditorClassScript(string scriptName, string root, string nameSpace, 
            UsingModule usingModule, ScriptAttribute attribute, string baseTypeName, string editorContents)
        {
            var editorCode = new ClassCode(scriptName, usingModule, nameSpace, attribute, editorContents, baseTypeName);
            var editorScript = new Script.Script(root, scriptName, editorCode);
            ScriptUtil.SaveScript(editorScript);
        }
        
        private static bool TryGetFileNameSpace(string typeName, out string typeNamespace)
        {
            typeNamespace = string.Empty;

            if (ScriptUtil.TryGetTypeFromScriptName(typeName, out var type))
            {
                // Namespace can be null for global namespace
                typeNamespace = type.Namespace ?? string.Empty;
                return typeof(UnityEngine.Object).IsAssignableFrom(type);
            }

            return false;
        }
    }
}
