using ScriptableRegistry.Editor.Enum;
using ScriptableRegistry.Util;
using UnityEngine;

namespace ScriptableRegistry.Editor.Script
{
    /// <summary>
    /// Script code for enum declarations.
    /// </summary>
    public sealed class EnumCode : ScriptCode
    {
        /// <summary>
        /// Enum declaration format.
        /// <para>0: enum name</para>
        /// </summary>
        private const string EnumNameFormat = "public enum {0}";
        /// <summary>
        /// Enum member format.
        /// <para>0: member name</para>
        /// <para>1: value</para>
        /// </summary>
        private const string EnumParameterFormat = "\t\t{0} = {1},\n";

        /// <summary>
        /// Build enum script code.
        /// </summary>
        /// <param name="parameters">Enum members.</param>
        /// <param name="nameSpace">Namespace (dot-separated for nesting).</param>
        /// <param name="fileName">Enum name / file name.</param>
        public EnumCode(EnumParameter[] parameters, string nameSpace, string fileName)
        {
            var enumName = StringUtil.Format(EnumNameFormat, fileName);
            var contents = CreateContents(parameters);

            if (string.IsNullOrEmpty(nameSpace) || nameSpace == Empty)
            {
                Contents = StringUtil.Format(ScriptFormat, Empty, enumName, contents);
            }
            else
            {
                Contents = StringUtil.Format(NamespaceScriptFormat, nameSpace, Empty, enumName, contents);
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        public EnumCode(EnumCode code) : base(code) { }

        /// <summary>
        /// Build enum body from parameters.
        /// </summary>
        private static string CreateContents(EnumParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                Debug.LogWarning("No enum parameters provided.");
                return "\n";
            }

            var builder = StringBuilderFactory.Create();

            foreach (var parameter in parameters)
            {
                builder.Append(FormatEnumParameter(parameter));
            }

            var result = builder.ToString();
            StringBuilderFactory.Release(builder);
            
            return result;
        }

        /// <summary>
        /// Format one enum line using already-sanitized parameter.Name.
        /// </summary>
        private static string FormatEnumParameter(EnumParameter parameter)
        {
            return StringUtil.Format(EnumParameterFormat, parameter.Name, parameter.Num);
        }
    }
}