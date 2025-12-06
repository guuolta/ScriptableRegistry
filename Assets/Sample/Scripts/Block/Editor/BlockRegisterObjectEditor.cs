using UnityEditor;
using ScriptableRegistry.Editor;
using UnityEngine;
namespace ScriptableRegistry.Sample.Block.Editor
{
    [CustomEditor(typeof(BlockRegisterObject))]
    public class BlockRegisterObjectEditor : ScriptableRegistryObjectBaseEditor<BlockRegisterObject, BlockType, BlockBehaviour, GameObject>
    {
        protected override string[] GetDefaultParams()
        {
            return new string[] { "None" };
        }

        protected override BlockBehaviour CreateValue(GameObject file)
        {
            return file.GetComponent<BlockBehaviour>();
        }
    }
}
