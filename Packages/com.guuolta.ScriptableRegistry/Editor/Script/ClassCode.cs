using ScriptableRegistry.Util;

namespace ScriptableRegistry.Editor.Script
{
    /// <summary>
    /// Script code for a class declaration.
    /// </summary>
    public sealed class ClassCode : ScriptCode
    {
        private const string BaseClassFormat = " : {0}";

        /// <summary>
        /// Build class script code.
        /// </summary>
        /// <param name="name">Class/file name.</param>
        /// <param name="usingModule">Using directives.</param>
        /// <param name="nameSpace">Namespace (empty for none).</param>
        /// <param name="attribute">Class attributes.</param>
        /// <param name="contents">Class body.</param>
        /// <param name="baseClassNames">Base class / interface names.</param>
        public ClassCode(
            string name,
            UsingModule usingModule = null,
            string nameSpace = Empty,
            ScriptAttribute attribute = null,
            string contents = Empty,
            params string[] baseClassNames)
        {
            var builder = StringBuilderFactory.Create();

            // using section
            if (usingModule != null)
            {
                builder.Append(usingModule.Contents);
            }

            // attributes
            var attributeContents = attribute?.Contents ?? string.Empty;

            // declaration line
            var scriptName = BuildScriptName(name, baseClassNames);

            // body
            if (string.IsNullOrEmpty(nameSpace) || nameSpace == Empty)
            {
                builder.AppendFormat(ScriptFormat, attributeContents, scriptName, contents);
            }
            else
            {
                builder.AppendFormat(NamespaceScriptFormat, nameSpace, attributeContents, scriptName, contents);
            }

            Contents = builder.ToString();
            StringBuilderFactory.Release(builder);
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public ClassCode(ClassCode code) : base(code) { }

        /// <summary>
        /// Build the declaration line (e.g., "public class Foo : Bar, IBaz").
        /// </summary>
        private static string BuildScriptName(string name, params string[] baseClassNames)
        {
            var builder = StringBuilderFactory.Create();
            builder.AppendFormat(ScriptNameFormat, "class", name);

            if (baseClassNames != null && baseClassNames.Length > 0)
            {
                var baseClassNamesString = string.Join(", ", baseClassNames);
                builder.AppendFormat(BaseClassFormat, baseClassNamesString);
            }

            var result = builder.ToString();
            StringBuilderFactory.Release(builder);
            
            return result;
        }
    }

    /// <summary>
    /// Represents attribute list for generated scripts.
    /// (Renamed to avoid conflict with System.Attribute)
    /// </summary>
    public sealed class ScriptAttribute
    {
        public string Contents { get; }

        // Common attribute names
        public const string Serializable = "Serializable";

        /// <summary>
        /// CreateAssetMenu attribute format.
        /// <para>0: menuName</para>
        /// <para>1: fileName</para>
        /// </summary>
        public const string CreateAssetMenuFormat =
            "CreateAssetMenu(menuName = \"{0}\", fileName = \"{1}\")";

        public ScriptAttribute(params string[] attributeNames)
        {
            if (attributeNames == null || attributeNames.Length == 0)
            {
                Contents = string.Empty;
                return;
            }

            Contents = StringUtil.Concat(
                "[",
                StringUtil.Join(", ", attributeNames),
                "]"
            );
        }
    }
}