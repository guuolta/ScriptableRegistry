using ScriptableRegistry.Editor;
using UnityEditor;
using UnityEngine;

namespace ScriptableRegistry.Sample.Music.Editor
{
    [CustomEditor(typeof(MusicRegisterObject))]
    public class MusicRegisterObjectEditor: ScriptableRegistryObjectBaseEditor<MusicRegisterObject, MusicType, AudioClip, AudioClip>
    {
        protected override AudioClip CreateValue(AudioClip file)
        {
            return file;
        }
    }
}