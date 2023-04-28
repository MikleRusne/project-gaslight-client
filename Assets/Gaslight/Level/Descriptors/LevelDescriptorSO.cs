
using UnityEngine;
[CreateAssetMenu(fileName = "default level descriptor", menuName = "Level/LevelDescriptor")]
public class LevelDescriptorSO: ScriptableObject
{
    public LevelDescriptor LevelDescriptor;
    public CharacterLevelDescriptorArray CharacterDescriptors;
}