using System;
using System.IO;
using System.Linq;
using ScriptableRegistry.File;
using ScriptableRegistry.Util;

namespace ScriptableRegistry.Editor.Script
{
    /// <summary>
    /// Holds data required to generate a script file.
    /// </summary>
    public sealed class Script
    {
        public static readonly Extension CsExtension = new(".cs");

        public readonly string Name;
        public readonly string FilePath;
        public readonly ScriptCode Code;

        public Script(string root, string fileName, ScriptCode code)
        {
            Name = fileName;
            FilePath = Path.Combine(root, CsExtension.ToFileName(fileName));
            Code = new ScriptCode(code);
        }
    }

    /// <summary>
    /// Base class for script code.
    /// </summary>
    public class ScriptCode
    {
        /// <summary>Script body text.</summary>
        public string Contents { get; protected set; }

        /// <summary>Empty string constant.</summary>
        protected const string Empty = "";

        // Script name format.
        protected const string ScriptNameFormat = "public {0} {1}";

        // Code format without namespace.
        protected const string ScriptFormat = @"{0}
{1}
{{
    {2}
}}
";

        // Code format with namespace.
        protected const string NamespaceScriptFormat = @"namespace {0}
{{
    {1}
    {2}
    {{
        {3}
    }}
}}
";

        public ScriptCode() { }

        /// <summary>Copy constructor.</summary>
        public ScriptCode(ScriptCode code)
        {
            Contents = code?.Contents ?? Empty;
        }
    }

    /// <summary>
    /// Represents a set of using directives.
    /// </summary>
    public sealed class UsingModule
    {
        private const string UsingFormat = "using {0};\n";

        public string[] UsingModules { get; }
        public string Contents { get; }

        /// <summary>
        /// Builds using directives from module names.
        /// </summary>
        public UsingModule(params string[] usingModules)
        {
            UsingModules = usingModules ?? Array.Empty<string>();

            var builder = StringBuilderFactory.Create();
            foreach (var module in UsingModules)
            {
                builder.AppendFormat(UsingFormat, module);
            }

            Contents = builder.ToString();
            StringBuilderFactory.Release(builder);
        }

        /// <summary>Copy constructor.</summary>
        public UsingModule(UsingModule usingModule)
        {
            UsingModules = usingModule?.UsingModules ?? Array.Empty<string>();
            Contents = usingModule?.Contents ?? string.Empty;
        }

        /// <summary>
        /// Returns a new UsingModule with additional directives.
        /// </summary>
        public UsingModule Add(UsingModule other)
        {
            var added = other?.UsingModules ?? Array.Empty<string>();

            // Keep order: existing first, then added. Remove duplicates.
            var modules = UsingModules
                .Concat(added)
                .Distinct()
                .ToArray();

            return new UsingModule(modules);
        }
    }
}