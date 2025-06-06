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
    public TurnUIManager uiManager;

    public Character[] ALLcharacterList;
    public List<string> playerCharacters = new List<string>();
    public List<string> EnemieCharacters = new List<string>();
    public List<Stats> characterList = new List<Stats>();
    public List<GameObject> characters = new List<GameObject>();
    public int wave = 1; // 현재 웨이브

    public Dictionary<GameObject, Stats> characterMap = new();
    public GameObject validMainTarget = null;


    // 캐릭터 생성 시 등록
    void RegisterCharacter(GameObject obj, Stats stats)
    {
        if (!characterMap.ContainsKey(obj))
            characterMap.Add(obj, stats);
    }

    void Awake()
    {
        wave = 2;
        uiManager.UpdateWaveCount(wave);

        if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

        playerCharacters.Add("TonTonJung");
        playerCharacters.Add("Deus");
        playerCharacters.Add("JuInGong");
        playerCharacters.Add("ShellLin");
        EnemieCharacters.Add("melun");
        EnemieCharacters.Add("melunDago");
        EnemieCharacters.Add("MelunMelun");
        EnemieCharacters.Add("PuSsiMaster");
        

        
        for (int i = 0; i < playerCharacters.Count; i++) // playerCharacters 리스트의 길이만큼 반복
        {
            CharacterAdd(playerCharacters[i]);
        }
        for (int j = 0; j < EnemieCharacters.Count; j++) // EnemieCharacters 리스트의 길이만큼 반복
        {
            CharacterAdd(EnemieCharacters[j]);
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

    public void Update()
    {
        if (characterList != null ) {
            UEmenyCount(); }
        if(Input.GetKeyDown(KeyCode.K))
        {
            WaveUpdate();
        }
    }
    public void CharacterAdd(string chname)
    {
        
            if (ALLcharacterList.Any(Character => Character.charactername == chname))
            {
                int index = Array.FindIndex(ALLcharacterList, Character => Character.charactername == chname);
                //캐릭터 생성
                characterList.Add(new Stats(ALLcharacterList[index], Vector3.zero, Quaternion.identity, false, new()));
            }
            else
            {
                Debug.Log("에러 캐릭터를 찾을수 없음");
            return;
            }
        // 캐릭터 리스트에 있는 스킬 리스트를 순회하며 각 캐릭터의 스킬을 저장

        int index1 = characterList.FindIndex(Character => Character.name == chname);
        
        for (int i = 0; i < characterList[index1].useSkill.Count; i++)
        {
            // 캐릭터 이름과 스킬을 SkillData로 만들어 리스트에 추가
            if (characterList[index1].useSkill[i] == null)
            {
                characterList[index1].usingSkill.Add(new SkillData(null, characterList[index1].name));
            }
            else
            {
                characterList[index1].usingSkill.Add(new SkillData(characterList[index1].useSkill[i], characterList[index1].name));
            }
        }

    }

    public void Charactercreation()
    {
        for (int i = 0; i < characterList.Count; i++)
        {
            GameObject CharacterObject = Instantiate(characterList[i].characterPrefab);
            CharacterObject.name = characterList[i].name; // 캐릭터 이름 설정
            characters.Add(CharacterObject);
            CharacterObject.transform.position = transform.position;

            // 수정: 프리팹 원본 대신 씬 인스턴스를 Stats에 저장
            characterList[i].characterPrefab = CharacterObject; // <-- 추가됨

            // 수정: 캐릭터 리스트에 인스턴스 등록
            CharacterStats.Instance.RegisterCharacter(CharacterObject, characterList[i]);
            /*if (!characters.Contains(CharacterObject))
            {
                characterMap.Add(CharacterObject, characterList[i]); // <-- 수정 또는 추가됨
            }*/

            // 수정: CharacterStats의 characters 리스트에 인스턴스 등록
            if (!CharacterStats.Instance.characters.Contains(CharacterObject))
            {
                CharacterStats.Instance.characters.Add(CharacterObject); // <-- 추가됨
            }
        }

    }

    public Stats GetStats(GameObject obj)
    {
        foreach (var stats in characterList)
        {
            if (stats.characterPrefab == obj)
                return stats;
        }
        return null;
    }

    public void UEmenyCount()
    {
        int enemyAliveCount = characterList
    .Count(stats => stats.team == Team.enemy && stats.isdie == false);
        uiManager.Updatenemytcount(enemyAliveCount);
    }

    public void WaveUpdate()
    {
        wave--;
        uiManager.UpdateWaveCount(wave);
    }
}
