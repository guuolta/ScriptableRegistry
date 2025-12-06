using ScriptableRegistry;
using UnityEngine;
namespace ScriptableRegistry.Sample.Block
{
    [CreateAssetMenu(menuName = "ScriptableRegistry/Sample/Block", fileName = "BlockRegisterObject")]
    public class BlockRegisterObject : ScriptableRegistryObjectBase<BlockType, BlockBehaviour>
    {
        
    }
}
