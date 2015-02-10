using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterInformation
{
	public string label;
	
	public string aiAgentPath; // this should be added under the root node of character.
	
	public string modelPath;

}

// global character configuration.
public class CharacterTable : ScriptableObject
{
    public List<CharacterInformation> characterTableList = new List<CharacterInformation>();

    public CharacterInformation FindCharacterByLabel(string label)
    {
        foreach (CharacterInformation ci in characterTableList)
        {
            if (ci.label == label)
            {
                return ci;
            }
        }
        return null;
    }
}
