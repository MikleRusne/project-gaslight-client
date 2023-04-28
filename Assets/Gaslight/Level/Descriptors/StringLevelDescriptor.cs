
using UnityEngine;
[CreateAssetMenu(fileName = "default level descriptor", menuName = "Level/StringLevelDescriptor")]
public class StringLevelDescriptor: LevelDescriptorSO
{
    public string levelJson;
    public string charactersJson;
    
    void OnValidate()
    {

        var LevelDescriptorsJSON = levelJson;
        LevelDescriptor =JsonUtility.FromJson<LevelDescriptor>(LevelDescriptorsJSON);
        
        var CharacterDescriptorsJSON = PlayerPrefs.GetString("character", "");
        if (CharacterDescriptorsJSON == "")
        {
            Debug.LogWarning("Could not find saved level in PlayerPrefs, loading from scene string");
        }

        CharacterDescriptors = JsonUtility.FromJson<CharacterLevelDescriptorArray>(charactersJson);
    }
}