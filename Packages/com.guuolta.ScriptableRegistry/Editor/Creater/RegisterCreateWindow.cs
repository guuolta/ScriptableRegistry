using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableRegistry.Editor.Creator
{
    /// <summary>
    /// Editor window that collects registry generation settings and triggers script creation.
    /// </summary>
    public class RegisterCreateWindow : EditorWindow
    {
        // ---------------- Inputs ----------------

        private string _scriptName = "RegisterObject";
        private string _scriptSavePath = "Assets/";
        private string _scriptNamespace = "";

        private string _editorScriptName = "";
        private string _editorSavePath = "";
        private string _editorNamespace = "";

        private string _createMenuName = "ScriptableRegistry/";
        private string _createFileName = "";

        // ---- Key / Value types ----
        private string _keyEnumNamespace = "";     // ★ added
        private string _keyEnumName = "";

        private string _valueClassNamespace = ""; // ★ added
        private string _valueClassName = "";

        private string _targetFileClassName = "";

        // ---------------- Auto-default flags ----------------
        private bool _editorScriptNameAuto = true;
        private bool _editorSavePathAuto = true;
        private bool _editorNamespaceAuto = true;
        private bool _createFileNameAuto = true;

        private bool _hasSubmitted;
        private string _summary;

        [MenuItem("Window/ScriptableRegistry/CreateWindow")]
        private static void Open()
        {
            var window = GetWindow<RegisterCreateWindow>();
            window.titleContent = new GUIContent("ScriptableRegistry Create Window");
            window.minSize = new Vector2(520, 460);
            window.Show();
        }

        private void OnEnable()
        {
            ApplyAutoDefaults(force: true);
        }

        private void OnGUI()
        {
            ApplyAutoDefaults(force: false);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Script Settings", EditorStyles.boldLabel);

                _scriptName = EditorGUILayout.TextField(
                    new GUIContent("Script Name", "Default: RegisterObject"),
                    _scriptName);

                _scriptSavePath = EditorGUILayout.TextField(
                    new GUIContent("Script Save Path", "Default: Assets/"),
                    _scriptSavePath);

                _scriptNamespace = EditorGUILayout.TextField(
                    new GUIContent("Script Namespace", "Optional"),
                    _scriptNamespace);
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Editor Script Settings", EditorStyles.boldLabel);

                DrawAutoField(
                    label: "Editor Script Name",
                    tooltip: "Default: {ScriptName}Editor",
                    value: _editorScriptName,
                    autoEnabled: _editorScriptNameAuto,
                    onValueChanged: v => { _editorScriptName = v; _editorScriptNameAuto = false; },
                    setAutoEnabled: b => _editorScriptNameAuto = b
                );

                DrawAutoField(
                    label: "Editor Save Path",
                    tooltip: "Default: {ScriptSavePath}/Editor",
                    value: _editorSavePath,
                    autoEnabled: _editorSavePathAuto,
                    onValueChanged: v => { _editorSavePath = v; _editorSavePathAuto = false; },
                    setAutoEnabled: b => _editorSavePathAuto = b
                );

                DrawAutoField(
                    label: "Editor Namespace",
                    tooltip: "Default: {ScriptNamespace}.Editor",
                    value: _editorNamespace,
                    autoEnabled: _editorNamespaceAuto,
                    onValueChanged: v => { _editorNamespace = v; _editorNamespaceAuto = false; },
                    setAutoEnabled: b => _editorNamespaceAuto = b
                );
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("CreateAssetMenu", EditorStyles.boldLabel);

                _createMenuName = EditorGUILayout.TextField(
                    new GUIContent("Menu Name", "Default: ScriptableRegistry/"),
                    _createMenuName);

                DrawAutoField(
                    label: "File Name",
                    tooltip: "Default: {ScriptName}",
                    value: _createFileName,
                    autoEnabled: _createFileNameAuto,
                    onValueChanged: v =>
                    {
                        _createFileName = v;
                        _createFileNameAuto = false;
                    },
                    setAutoEnabled: b => _createFileNameAuto = b
                );
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Registry Types", EditorStyles.boldLabel);

                // ---- Key Enum ----
                _keyEnumName = EditorGUILayout.TextField(
                    new GUIContent(
                        "Key Enum Name",
                        "Enum type used as key.\n" +
                        "If the enum does not exist, an empty enum script will be generated\n" +
                        "next to the main script (same folder) using this name."
                    ),
                    _keyEnumName);
                
                _keyEnumNamespace = EditorGUILayout.TextField(
                    new GUIContent(
                        "Key Enum Namespace",
                        "Namespace of the key enum.\n" +
                        "Optional. Leave empty if enum is in global namespace."
                    ),
                    _keyEnumNamespace);

                // ---- Value Class ----
                _valueClassName = EditorGUILayout.TextField(
                    new GUIContent(
                        "Value Class Name",
                        "Class type stored as value.\n" +
                        "If the class does not exist, an empty class script will be generated\n" +
                        "next to the main script (same folder) using this name."
                    ),
                    _valueClassName);
                
                _valueClassNamespace = EditorGUILayout.TextField(
                    new GUIContent(
                        "Value Class Namespace",
                        "Namespace of the value class.\n" +
                        "Optional. Leave empty if class is in global namespace."
                    ),
                    _valueClassNamespace);

                // ---- Target file type ----
                _targetFileClassName = EditorGUILayout.TextField(
                    new GUIContent(
                        "Target File Class Name",
                        "Class name of files to scan/load (e.g., AudioClip, Sprite).\n" +
                        "If the class does not exist, an empty class script will be generated\n" +
                        "next to the main script (same folder) using this name."
                    ),
                    _targetFileClassName);

                EditorGUILayout.HelpBox(
                    "About missing types:\n" +
                    "- If \"Key Enum Name\" does not exist, an empty enum will be created.\n" +
                    "- If \"Value Class Name\" does not exist, an empty class will be created.\n" +
                    "- If \"Target File Class Name\" does not exist, an empty class will be created.\n\n" +
                    "All missing scripts are generated in the same folder as the main script.",
                    MessageType.Info
                );
            }

            GUILayout.Space(8);

            using (new EditorGUI.DisabledScope(!CanSubmit()))
            {
                if (GUILayout.Button("Submit"))
                {
                    RegisterCreator.CreateRegisterObjectScript(_scriptName, _scriptSavePath, _scriptNamespace,
                        _editorScriptName, _editorSavePath, _editorNamespace,
                        _createMenuName, _createFileName,
                        _keyEnumName, _keyEnumNamespace,
                         _valueClassName, _valueClassNamespace,
                        _targetFileClassName);
                    _summary = BuildSummary();
                    _hasSubmitted = true;
                    Repaint();
                }
            }

            GUILayout.Space(8);

            if (_hasSubmitted)
                EditorGUILayout.HelpBox(_summary, MessageType.Info);
            else
                EditorGUILayout.HelpBox("Fill fields and press Submit.", MessageType.None);
        }

        // ---------------- UI helpers ----------------

        private void DrawAutoField(
            string label,
            string tooltip,
            string value,
            bool autoEnabled,
            Action<string> onValueChanged,
            Action<bool> setAutoEnabled
        )
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(160));

                using (new EditorGUI.DisabledScope(autoEnabled))
                {
                    var newVal = EditorGUILayout.TextField(value);
                    if (newVal != value)
                        onValueChanged(newVal);
                }

                var buttonLabel = autoEnabled ? "Auto" : "Manual";
                if (GUILayout.Button(buttonLabel, GUILayout.Width(70)))
                {
                    var nextAuto = !autoEnabled;
                    setAutoEnabled(nextAuto);

                    if (nextAuto)
                        ApplyAutoDefaults(force: true);

                    GUI.FocusControl(null);
                    Repaint();
                }
            }
        }

        // ---------------- Default logic ----------------

        private void ApplyAutoDefaults(bool force)
        {
            var scriptNameSafe = string.IsNullOrWhiteSpace(_scriptName) ? "RegisterObject" : _scriptName.Trim();
            var scriptPathSafe = NormalizeFolder(_scriptSavePath, "Assets/");
            var scriptNsSafe = _scriptNamespace?.Trim() ?? "";

            if (force || _editorScriptNameAuto)
                _editorScriptName = $"{scriptNameSafe}Editor";

            if (force || _editorSavePathAuto)
                _editorSavePath = CombineFolder(scriptPathSafe, "Editor");

            if (force || _editorNamespaceAuto)
                _editorNamespace = string.IsNullOrEmpty(scriptNsSafe) ? "Editor" : $"{scriptNsSafe}.Editor";

            if (force || _createFileNameAuto)
                _createFileName = scriptNameSafe;

            _scriptSavePath = scriptPathSafe;
        }

        private static string NormalizeFolder(string path, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(path)) return defaultValue;
            path = path.Trim().Replace("\\", "/");
            while (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);
            return path;
        }

        private static string CombineFolder(string basePath, string child)
        {
            basePath = NormalizeFolder(basePath, "Assets");
            child = NormalizeFolder(child, "Editor");
            return $"{basePath}/{child}";
        }

        // ---------------- Validation & summary ----------------

        private bool CanSubmit()
        {
            if (string.IsNullOrWhiteSpace(_scriptName)) return false;
            if (string.IsNullOrWhiteSpace(_scriptSavePath)) return false;
            if (string.IsNullOrWhiteSpace(_keyEnumName)) return false;
            if (string.IsNullOrWhiteSpace(_valueClassName)) return false;
            if (string.IsNullOrWhiteSpace(_targetFileClassName)) return false;
            return true;
        }

        private string BuildSummary()
        {
            return
$@"[ScriptableRegistry] Settings
--- Script ---
Name: {_scriptName}
Save Path: {_scriptSavePath}
Namespace: {_scriptNamespace}

--- Editor Script ---
Name: {_editorScriptName}
Save Path: {_editorSavePath}
Namespace: {_editorNamespace}

--- CreateAssetMenu ---
Menu Name: {_createMenuName}
File Name: {_createFileName}

--- Types ---
Key Enum: {_keyEnumNamespace}.{_keyEnumName}
Value Class: {_valueClassNamespace}.{_valueClassName}
Target File Class: {_targetFileClassName}";
        }
    }
}
