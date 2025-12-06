using UnityEngine;

namespace ScriptableRegistry.Sample.Music
{
    [CreateAssetMenu(menuName = "ScriptableRegistry/Sample/Music", fileName = "MusicRegisterObject")]
    public class MusicRegisterObject: ScriptableRegistryObjectBase<MusicType, AudioClip>
    {
        
    }
}