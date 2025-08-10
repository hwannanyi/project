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

    public bool characterCreat = false; // 캐릭터 생성 여부

    // 체력바
    public GameObject HpBar;
    public Canvas canvas;
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

            Instance = this;
        characterCreat = false; // 캐릭터 생성 여부 초기화

        // Addressable Assets를 사용하여 Character Scriptable Object 로드
        Addressables.LoadAssetsAsync<Character>("Characters", null).Completed += OnCharactersLoaded;


        LoadStageCharacters();
        /* playerCharacters.Add("TonTonJung");
         //playerCharacters.Add("Deus");
         //playerCharacters.Add("JuInGong");
         playerCharacters.Add("ShellLin");
         EnemieCharacters.Add("melun");
         EnemieCharacters.Add("melunDago");
         EnemieCharacters.Add("프리즘");
         EnemieCharacters.Add("PuSsiMaster");*/

    }

    public void Update()
    {
        if (characterList != null) {
            UEmenyCount(); }
    }

    /// <summary>
    /// 캐릭터 로드
    /// </summary>
    public void LoadStageCharacters()
    {
        // StageManager에서 현재 스테이지 데이터 가져오기
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.CurrentStage == null)
        {
            Debug.LogWarning("StageManager 또는 CurrentStage가 없습니다.");
            return;
        }

        // 플레이어 캐릭터 리스트 초기화 및 추가
        playerCharacters.Clear();
        if (stageManager.character != null)
        {
            for (int i = 1; i <= stageManager.CurrentStage.participants; i++)
            {
                if (!string.IsNullOrEmpty(stageManager.character[i - 1]))
                    playerCharacters.Add(stageManager.character[i - 1]);
            }
        }

        // 적 캐릭터 리스트 초기화 및 추가
        EnemieCharacters.Clear();
        if (stageManager.CurrentStage.enemyDatalist != null)
        {
            foreach (var enemy in stageManager.CurrentStage.enemyDatalist)
            {
                if (!string.IsNullOrEmpty(enemy.enemyName))
                    EnemieCharacters.Add(enemy.enemyName);
            }
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
            var newStats = new Stats(ALLcharacterList[index], false, new());
            newStats.name = uniqueName; // 이름 덮어쓰기
            characterList.Add(newStats);
        }
        else
        {
            Debug.Log("에러 캐릭터를 찾을수 없음");
            return;
        }
        // 캐릭터 리스트에 있는 스킬 리스트를 순회하며 각 캐릭터의 스킬을 저장

        int index1 = characterList.FindIndex(Character => Character.name == uniqueName);

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

                if (!string.IsNullOrEmpty(character.useSkill[i].StartAddSkills.skillName))
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
        //암사
        for (int i = 0; i < characterList.Count; i++)
        {
            Stats chter = characterList[i];
            GameObject chterObj = Instantiate(chter.characterPrefab);
            var stageManager = StageManager.Instance;
            chterObj.name = chter.name;
            characters.Add(chterObj);

            // 팀에 따라 위치 지정
            if (chter.team == Team.team)
            {
                Vector2 postion = stageManager.CurrentStage.startPositions[i];
                Vector3 startpostion = new Vector3(postion.x, 0, postion.y);
                chterObj.transform.position = startpostion;

            }
            else if (chter.team == Team.enemy)
            {
                Vector2 postion = stageManager.CurrentStage.enemyDatalist[i - playerCharacters.Count].position;
                Vector3 startpostion = new Vector3(postion.x, 0, postion.y);
                chterObj.transform.position = startpostion;

            }
            else
            {
                //characterList[i].charPosition = CharacterObject.transform.position; // 캐릭터 위치 저장
                chterObj.transform.position = transform.position;
            }

            chter.characterPrefab = chterObj;

            Transform highlight = chterObj.transform.Find("HighlightEffect");
            if (highlight != null)
                chter.highlightEffect = highlight.gameObject;
            else
                chter.highlightEffect = null;

            CharacterStats.Instance.RegisterCharacter(chterObj, chter);

            if (!CharacterStats.Instance.characters.Contains(chterObj))
            {
                CharacterStats.Instance.characters.Add(chterObj);
            }
            WorldHPBar.Create(HpBar, chterObj.transform, canvas, chter);
        }
        ProfileuiManager.AssignMiniprofileTargets();
        characterCreat = true; // 캐릭터 생성 완료
    }

    public void Charactercreat(Stats character,Vector3 startpostion)
    {

            GameObject chterObj = Instantiate(character.characterPrefab);
            var stageManager = StageManager.Instance;
            chterObj.name = character.name;
            characters.Add(chterObj);


            chterObj.transform.position = startpostion;

            character.characterPrefab = chterObj;

            Transform highlight = chterObj.transform.Find("HighlightEffect");
            if (highlight != null)
            character.highlightEffect = highlight.gameObject;
            else
            character.highlightEffect = null;

            CharacterStats.Instance.RegisterCharacter(chterObj, character);

            if (!CharacterStats.Instance.characters.Contains(chterObj))
            {
                CharacterStats.Instance.characters.Add(chterObj);
            }

        WorldHPBar.Create(HpBar, chterObj.transform, canvas, character);

        ProfileuiManager.AssignMiniprofileTargets();
    }

    public Stats CharacterAddStats(string chname)
    {
        // 이미 같은 이름의 캐릭터가 있는지 확인
        Stats newStats = null;
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
            newStats = new Stats(ALLcharacterList[index], false, new());
            newStats.name = uniqueName; // 이름 덮어쓰기
            characterList.Add(newStats);
        }
        else
        {
            Debug.Log("에러 캐릭터를 찾을수 없음");
            return null;
        }
        // 캐릭터 리스트에 있는 스킬 리스트를 순회하며 각 캐릭터의 스킬을 저장

        int index1 = characterList.FindIndex(Character => Character.name == uniqueName);

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

                if (!string.IsNullOrEmpty(character.useSkill[i].StartAddSkills.skillName))
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
        }

        return newStats;
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

    public bool IsClear(VictoryRule rule)
    {
        bool isClear = false;
        switch (rule)
        {
            case VictoryRule.killAll:
                isClear = characterList.Count(stats => stats.team == Team.enemy && stats.isdie == false) == 0;
                break;
            case VictoryRule.story:
                isClear = true;
                break;

        }
        return isClear;
    }
}
