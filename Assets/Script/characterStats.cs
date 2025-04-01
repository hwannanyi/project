using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml.Linq;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class CharacterStats : MonoBehaviour
{
    public static CharacterStats Instance;

    public Character[] ALLcharacterList;
    public List<string> playerCharacters = new List<string>();
    public List<string> EnemieCharacters = new List<string>();
    public List<Stats> characterList = new List<Stats>();
    public List<GameObject> characters = new List<GameObject>();

    void Awake()
    {

        
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        
        playerCharacters.Add("melun");
        playerCharacters.Add("melunDago");
        EnemieCharacters.Add("MelunMelun");
  

        for (int i = 0; i < playerCharacters.Count; i++) // playerCharacters 리스트의 길이만큼 반복
        {
            CharacterAdd(playerCharacters[i]);
            Debug.Log(ALLcharacterList[i].charactername);
        }
        for (int i = 0; i < EnemieCharacters.Count; i++) // EnemieCharacters 리스트의 길이만큼 반복
        {
            CharacterAdd(EnemieCharacters[i]);
            Debug.Log(ALLcharacterList[i].charactername);
        }
        Charactercreation();
        /*characterStats["melun"] = new Dictionary<string, int>
        {
            { "MaxHealth", 100 },
            { "mana", 30 },
            { "attack", 15 },
            { "defense", 10 },
            { "speed", 9 }
        };

        characterStats["melunDago"] = new Dictionary<string, int>
        {
            { "MaxHealth", 150 },
            { "mana", 50 },
            { "attack", 20 },
            { "defense", 10 },
            { "speed", 7 }
        };

        characterStats["MelunMelun"] = new Dictionary<string, int>
        {
            { "MaxHealth", 110 },
            { "mana", 40 },
            { "attack", 12 },
            { "defense", 10 },
            { "speed", 8 }
        };

        characterSkill["MelunMelun"] = new Dictionary<string, int>
        {
            { "MaxHealth", 110 },
            { "mana", 40 },
            { "attack", 12 },
            { "defense", 10 },
            { "speed", 8 }
        };*/
    }

    public void CharacterAdd(string chname)
    {
        
            if (ALLcharacterList.Any(Character => Character.charactername == chname))
            {
                int index = Array.FindIndex(ALLcharacterList, Character => Character.charactername == chname);
                //캐릭터 생성
                characterList.Add(new Stats(ALLcharacterList[index], Vector3.zero, Quaternion.identity, false));
            }
            else
            {
                Debug.Log("에러 캐릭터를 찾을수 없음");
            }
        
    }

    public void Charactercreation()
    {
        for (int i = 0; i < characterList.Count; i++)
        {
            GameObject CharacterObject = Instantiate(characterList[i].characterPrefab);
            characters.Add(CharacterObject);
            CharacterObject.transform.position = transform.position;
        }

    }

}
