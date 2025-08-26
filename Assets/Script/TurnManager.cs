using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/*public enum TurnPhase
{
    PlayerTurn,
    EnemyTurn,
    ReactPhase_PlayerResponding,
    ReactPhase_EnemyResponding
}*/


public enum ReactTurnPhase
{
    None,
    PlayerTurn,
    EnemyTurn,
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    /*    public UnityEvent<string> OnStoryStart = new UnityEvent<string>();
        public UnityEvent<string> OnStoryStop = new UnityEvent<string>();*/



    public StageManager stageManager;
    public TurnUIManager uiManager;
    public SkillManager skillmanager; // 스킬 매니저 인스턴스
    public CharacterUIManager characterUIManager; // 캐릭터 프로필 UI 매니저
    public StoryManager storyManager; // 스토리 매니저 인스턴스


    public bool isPlayerTurn = true; // 플레이어 턴 여부

    /*    public TurnPhase nowtrun = TurnPhase.PlayerTurn;
        public TurnPhase currentPhase = TurnPhase.PlayerTurn;
        public TurnPhase previousPhase;*/

    public ReactTurnPhase Reacttrun = ReactTurnPhase.EnemyTurn;

    public int Turn = 1;

    public int playerSkillTurn = 10;
    public int enemySkillTurn = 10;

    public int playerUseSkillTurn = 0;
    public int enemyUseSkillTurn = 0;


    public int playerReactTrun = 3;
    public int enemyReactTrun = 3;

    public int playerUseSkillReactTrun = 0;
    public int enemyUseSkillReactTrun = 0;

    public CharacterSelection characterSelection;
    public CharacterStats characterStats;
    public StageDataManager stageDataManager;

    private void Awake()
    {
        skillmanager = GetComponent<SkillManager>();
        characterSelection = GetComponent<CharacterSelection>();
        characterStats = GetComponent<CharacterStats>();
        stageDataManager = GetComponent<StageDataManager>();
        storyManager = GetComponent<StoryManager>();
        try
        {
            stageManager = StageManager.Instance;
        }
        catch
        {
            Debug.LogError("StageManager instance not found");
        }

        Turn = 1;
    UITrunCount(Turn);//  턴 UI 업데이트

            Instance = this;

    }

    public void Start()
    {
        stageDataManager.CheckTurn();//스토리활성화
    }
    //  EventManager 연동 유지 (턴 종료 이벤트로 외부에서 턴 넘기기 가능)
    private void OnEnable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TurnEnd += NextTurnEnd;
        }
        else
        {
            Debug.LogError("[TurnManager] EventManager.Instance가 null입니다! 실행 순서를 확인하세요.");
        }
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.TurnEnd -= NextTurnEnd;
        }
    }

    //  현재 턴 판별 함수들

/*/   public bool NowPlayerTurn()
    {
        return nowtrun == TurnPhase.PlayerTurn;
    }
*/


    public bool IsInReactPhase()
    {
        return Reacttrun != ReactTurnPhase.None; 
    }

    public bool IsPlayerReactPhase()
    {
        return Reacttrun == ReactTurnPhase.PlayerTurn;
    }

    public bool IsEnemyReactPhase()
    {
        return Reacttrun == ReactTurnPhase.EnemyTurn;
    }

    public bool IsPlayerActive()
    {
        return IsPlayerReactPhase() || isPlayerTurn;
    }

    public bool IsEnemyActive()
    {
        return IsEnemyReactPhase() || !isPlayerTurn;
    }

    //  캐릭터가 플레이어 팀인지 판별
    public bool IsPlayerTeam(Stats character)
    {
        return characterStats.playerCharacters.Contains(character.name);
    }

    //  대응단계 진입 → 대응하는 팀에게 턴을 넘김
    public void EnterReactPhase()
    {
        playerUseSkillReactTrun = 0;
        enemyUseSkillReactTrun = 0;
        
        Reacttrun = isPlayerTurn ? ReactTurnPhase.PlayerTurn : ReactTurnPhase.EnemyTurn;
    }

    public void ExitReactPhase()
    {
        //CharacterSelection.prevSelectedIndex = CharacterSelection.selectedCharacterIndex;
        //characterStats.characterList[CharacterSelection.selectedCharacterIndex].SetHighlight(false);
        //CharacterSelection.selectedCharacterIndex = -1;
        characterUIManager.UpdateProfileUIBySelection();

        Reacttrun = ReactTurnPhase.None;
        // 턴 복귀
        Debug.Log($"[TurnManager] 대응단계 종료: 턴 복귀");
    }

    public void ChrtTag()//수비턴일시 비선택된 캐릭은 퇴각, 공격턴에 복귀
    {
        if (isPlayerTurn)
        {
            // 공격턴일 때는 모든 캐릭터 복귀
            if (characterStats.PlayerCharacter1.characterPrefab)
                characterStats.PlayerCharacter1.characterPrefab.SetActive(true);
            if (characterStats.PlayerCharacter2.characterPrefab)
                characterStats.PlayerCharacter2.characterPrefab.SetActive(true);

            // HP바 활성화
            if (characterStats.PlayerCharacter1.HPbar.gameObject)
                characterStats.PlayerCharacter1.HPbar.gameObject.SetActive(true);
            if (characterStats.PlayerCharacter2.HPbar.gameObject)
                characterStats.PlayerCharacter2.HPbar.gameObject.SetActive(true);

        }
        else
        {
            GameObject chrtObj = skillmanager.defendingCharacter?.characterPrefab;
            Stats chrt1 = characterStats.PlayerCharacter1;
            Stats chrt2 = characterStats.PlayerCharacter2;

            // 수비턴일 때는 선택된 캐릭터만 활성화

            if (characterStats.PlayerCharacter1.characterPrefab)
                chrt1.characterPrefab.SetActive(!chrt1.isdie && chrt1.characterPrefab == chrtObj);
            if (characterStats.PlayerCharacter2.characterPrefab)
                chrt2.characterPrefab.SetActive(!chrt2.isdie && chrt2.characterPrefab == chrtObj);

            // 선택된 캐릭만 HP바 활성화
            if (characterStats.PlayerCharacter1.HPbar.gameObject)
                characterStats.PlayerCharacter1.HPbar.gameObject.SetActive(characterStats.PlayerCharacter1.characterPrefab == chrtObj);

            if (characterStats.PlayerCharacter2.HPbar.gameObject)
                characterStats.PlayerCharacter2.HPbar.gameObject.SetActive(characterStats.PlayerCharacter2.characterPrefab == chrtObj);
        }
        
    }

    //  턴 전환 (일반 턴 순환: 플레이어 <-> 적)
    public void NextTurn()
    {
        if (isPlayerTurn &&
            !(skillmanager.defendingCharacter == characterStats.PlayerCharacter1 ||
            skillmanager.defendingCharacter == characterStats.PlayerCharacter2) &&
            skillmanager.defendingCharacter.isdie)
            return; //공격턴에서 선택된 캐릭이 없으면 턴 전환 불가
        

        skillmanager.Skillcancel(); // 스킬 쿨타임 초기화
        isPlayerTurn = !isPlayerTurn; // 플레이어 턴 여부 토글
        EnterReactPhase();
        //CharacterSelection.selectedCharacterIndex = -1;
        characterUIManager.UpdateProfileUIBySelection();
        Turn++;
        UITrunCount(Turn);//  턴 UI 업데이트
        ChrtTag();
        try
        {
            if(CharacterSelection.selectedCharacterIndex != -1)
            characterUIManager.ProfileUpdate(characterSelection.PickcharNumber(CharacterSelection.selectedCharacterIndex),
                isPlayerTurn);
        }
        catch (Exception e)
        {
            Debug.LogError($"[TurnManager] 캐릭터 프로필 업데이트 실패: {e.Message}");
        }
        // 모든 캐릭터의 스킬 쿨타임 감소
        foreach (var character in characterStats.characterList)
        {
            foreach (var skill in character.usingSkill)
            {
                if (skill.colldownTime > 0)
                {
                    skill.ReduceCooldown(1);
                }
            }

                character.mp = character.mp < 10 ? character.mp + 1 : character.mp;

            /*            if (character.NowMoveCount < 8)
                        {
                            character.NowMoveCount += 1;
                        }*/
            character.isPatternEnd = character.isdie; // AI 패턴 초기화
        }

        Debug.Log($"[TurnManager] 턴 전환됨");

        stageDataManager.CheckTurn();//스토리활성화
    }

    //  EventManager 이벤트용
    public void NextTurnEnd(bool value)
    {
        Debug.Log("[TurnManager] EventManager를 통한 턴 종료 요청 감지");
        NextTurn();
    }

    //  턴 내 행동횟수 초기화 (시간 경과 등)
    public void ResetTurn()
    {
        playerUseSkillTurn = 0;
        enemyUseSkillTurn = 0;
    }

    public void UITrunCount(int turnCount)
    {
        uiManager.UpdateTrunCount(turnCount);
        uiManager.UpdateReactTurn(isPlayerTurn);
    }
}

