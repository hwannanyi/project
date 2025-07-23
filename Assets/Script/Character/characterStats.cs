using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Runtime.Serialization;
using System.Xml.Linq;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.VolumeComponent;

[System.Serializable]
public class CharacterStats : MonoBehaviour
{
    public static CharacterStats Instance;
    public TurnUIManager uiManager;
    public CharacterUIManager ProfileuiManager; // 캐릭터 프로필 UI 매니저

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
        wave = 1;
        uiManager.UpdateWaveCount(wave);

        if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

        // Addressable Assets를 사용하여 Character Scriptable Object 로드
        Addressables.LoadAssetsAsync<Character>("Characters", null).Completed += OnCharactersLoaded;


        playerCharacters.Add("TonTonJung");
        //playerCharacters.Add("Deus");
        //playerCharacters.Add("JuInGong");
        playerCharacters.Add("ShellLin");
        EnemieCharacters.Add("melun");
        EnemieCharacters.Add("melunDago");
        EnemieCharacters.Add("프리즘");
        EnemieCharacters.Add("PuSsiMaster");
        

        
/*        for (int i = 0; i < playerCharacters.Count; i++) // playerCharacters 리스트의 길이만큼 반복
        {
            CharacterAdd(playerCharacters[i]);
        }
        for (int j = 0; j < EnemieCharacters.Count; j++) // EnemieCharacters 리스트의 길이만큼 반복
        {
            CharacterAdd(EnemieCharacters[j]);
        }*/
        //Charactercreation();
        

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

    private void OnCharactersLoaded(AsyncOperationHandle<IList<Character>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            ALLcharacterList = handle.Result.ToArray();

            for (int i = 0; i < playerCharacters.Count; i++) // playerCharacters 리스트의 길이만큼 반복
            {
                CharacterAdd(playerCharacters[i]);
            }
            for (int j = 0; j < EnemieCharacters.Count; j++) // EnemieCharacters 리스트의 길이만큼 반복
            {
                CharacterAdd(EnemieCharacters[j]);
            }
            Charactercreation();
        }
        else
        {
            Debug.LogError("Addressable Assets에서 Character Scriptable Object를 로드하는 데 실패했습니다.");
        }
    }

    public void CharacterAdd(string chname)
    {
        // 이미 같은 이름의 캐릭터가 있는지 확인
        string uniqueName = chname;
        int duplicateCount = 1;
        while (characterList.Any(c => c.name == uniqueName))
        {
            uniqueName = $"{chname}({duplicateCount})";
            duplicateCount++;
        }


        if (ALLcharacterList.Any(Character => Character.charactername == chname))
            {
                int index = Array.FindIndex(ALLcharacterList, Character => Character.charactername == chname);
                //캐릭터 생성
                characterList.Add(new Stats(ALLcharacterList[index],false, new()));
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
                characterList[index1].usingSkill.Add(new SkillData(null, characterList[index1].name, false));
            }
            else
            {
                var character = characterList[index1];
                characterList[index1].usingSkill.Add(new SkillData(characterList[index1].useSkill[i], characterList[index1].name, false));

                if (!string.IsNullOrEmpty(character.useSkill[i].AdditionalSkills.skillName))
                {
                    character.usingSkill[i].AdditionalSkillData.skill =
                    GetSkillDataByName(character.useSkill[i].AdditionalSkills.skillName, character);
                    Debug.Log("추가 스킬 이름: " + character.usingSkill[i].AdditionalSkillData.skill.skillName);
                }

                if(!string.IsNullOrEmpty(character.useSkill[i].StartAddSkills.skillName))
                {
                    character.usingSkill[i].StartAddSkills.skill =
                    GetSkillDataByName(character.useSkill[i].StartAddSkills.skillName, character);
                    Debug.Log("추가 스킬 이름: " + character.usingSkill[i].StartAddSkills.skill.skillName);
                }

                if (!string.IsNullOrEmpty(character.useSkill[i].EndAddSkills.skillName))
                {
                    character.usingSkill[i].EndAddSkills.skill =
                    GetSkillDataByName(character.useSkill[i].EndAddSkills.skillName, character);
                    Debug.Log("스킬" + character.usingSkill[i].skillName);
                    Debug.Log("추가 스킬 이름: " + character.usingSkill[i].EndAddSkills.skill.skillName);
                }
                //AdditionalSkills
            }


/*
            if (characterList[index1].skillQueue == null)
            {
                characterList[index1].usingSkill.Add(new SkillData(null, characterList[index1].name, false));
            }
            else
            {
                characterList[index1].usingSkill.Add(new SkillData(characterList[index1].useSkill[i], characterList[index1].name, false));
            }*/
        }

    }

    public SkillData GetSkillDataByName(string name, Stats character)
    {
        if (character == null || character.useSkill == null)
            return null;
        
        // skillName이 name과 일치하는 skill 찾음
        return new SkillData(
            (character.useSkill.FirstOrDefault(skill => skill != null && skill.skillName == name)),
            character.name,
            false);
    }

    public void Charactercreation()
    {
        //임시
        Vector3 playerStartPos = new Vector3(1, 0, 0); // 아군 시작 위치 (오른쪽)
        Vector3 enemyStartPos = new Vector3(-1, 0, 0); // 적군 시작 위치 (왼쪽)
        float yOffset = 1f; // 캐릭터 간 세로 간격

        int playerIndex = 0;
        int enemyIndex = 0;

        //암사
        for (int i = 0; i < characterList.Count; i++)
        {
            GameObject CharacterObject = Instantiate(characterList[i].characterPrefab);

            CharacterObject.name = characterList[i].name;
            characters.Add(CharacterObject);

            // 팀에 따라 위치 지정
            if (characterList[i].team == Team.team)
            {
                CharacterObject.transform.position = playerStartPos + new Vector3(0, 0, playerIndex * yOffset);
                //characterList[i].charPosition = CharacterObject.transform.position; // 캐릭터 위치 저장
                playerIndex++;
            }
            else if (characterList[i].team == Team.enemy)
            {
                CharacterObject.transform.position = enemyStartPos + new Vector3(0, 0, enemyIndex * yOffset);
                //characterList[i].charPosition = CharacterObject.transform.position; // 캐릭터 위치 저장
                enemyIndex++;
            }
            else
            {
                //characterList[i].charPosition = CharacterObject.transform.position; // 캐릭터 위치 저장
                CharacterObject.transform.position = transform.position;
            }

            characterList[i].characterPrefab = CharacterObject;

            Transform highlight = CharacterObject.transform.Find("HighlightEffect");
            if (highlight != null)
                characterList[i].highlightEffect = highlight.gameObject;
            else
                characterList[i].highlightEffect = null;

            CharacterStats.Instance.RegisterCharacter(CharacterObject, characterList[i]);

            if (!CharacterStats.Instance.characters.Contains(CharacterObject))
            {
                CharacterStats.Instance.characters.Add(CharacterObject);
            }
        }
        ProfileuiManager.AssignMiniprofileTargets();
        ProfileuiManager.AssignMiniprofileTargets2P();

        /* for (int i = 0; i < characterList.Count; i++)
         {
             GameObject CharacterObject = Instantiate(characterList[i].characterPrefab);

             CharacterObject.name = characterList[i].name; // 캐릭터 이름 설정
             characters.Add(CharacterObject);
             CharacterObject.transform.position = transform.position;

             // 프리팹 원본 대신 씬 인스턴스를 Stats에 저장
             characterList[i].characterPrefab = CharacterObject; // <-- 추가됨


             // 하이라이트 오브젝트 연결 (자식 오브젝트 이름이 "HighlightEffect"라고 가정)
             Transform highlight = CharacterObject.transform.Find("HighlightEffect");
             if (highlight != null)
                 characterList[i].highlightEffect = highlight.gameObject;
             else
                 characterList[i].highlightEffect = null; // 없으면 null

             // 캐릭터 리스트에 인스턴스 등록
             CharacterStats.Instance.RegisterCharacter(CharacterObject, characterList[i]);
             *//*if (!characters.Contains(CharacterObject))
             {
                 characterMap.Add(CharacterObject, characterList[i]); // <-- 수정 또는 추가됨
             }*//*

             // 수정: CharacterStats의 characters 리스트에 인스턴스 등록
             if (!CharacterStats.Instance.characters.Contains(CharacterObject))
             {
                 CharacterStats.Instance.characters.Add(CharacterObject); // <-- 추가됨
             }
         }
         ProfileuiManager.AssignMiniprofileTargets();
         ProfileuiManager.AssignMiniprofileTargets2P();*/
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
