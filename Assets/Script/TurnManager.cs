using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using static EventManager;

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

    public event Action<Stats> OnTurnChanged;
    public static event Action<Stats> OnTurnChanged_condition;

    public BattelManager battelManager;
    public StageManager stageManager;
    public TurnUIManager uiManager;
    public SkillManager skillmanager; // 스킬 매니저 인스턴스
    public CharacterUIManager characterUIManager; // 캐릭터 프로필 UI 매니저
    public StoryManager storyManager; // 스토리 매니저 인스턴스


    public bool isTurn_cooperation = true; // 플레이어 턴 여부

    /*    public TurnPhase nowtrun = TurnPhase.PlayerTurn;
        public TurnPhase currentPhase = TurnPhase.PlayerTurn;
        public TurnPhase previousPhase;*/

    public ReactTurnPhase Reacttrun = ReactTurnPhase.EnemyTurn;

    public int Turn = 0;

    public int turn_alone = 0;
    public int turn_cooperation = 0;


    public int turnCount = 0; // 행동횟수


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

    public static event Action TurnEnd; // 턴 종료 이벤트 

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
        Turn = 0;
        turn_alone = 0;
        turn_cooperation = 0;
        stageDataManager.CheckTurn();//스토리활성화
        stageDataManager.CheckmainTurn();
        EventManager.Instance.TurnEnd += NextTurnEnd;
        EventManager.Instance.isMove += TurnCount;
        SkillManager.SkillCast += TurnCount;
        EventManager.Instance.FinishTurn();
    }
    //  EventManager 연동 유지 (턴 종료 이벤트로 외부에서 턴 넘기기 가능)
    private void OnEnable()
    {

            
    }

    private void OnDisable()
    {
            EventManager.Instance.TurnEnd -= NextTurnEnd;
            EventManager.Instance.isMove -= TurnCount;
            SkillManager.SkillCast -= TurnCount;

    }

public void OnDestroy()
    {
        EventManager.Instance.TurnEnd -= NextTurnEnd;
        EventManager.Instance.isMove -= TurnCount;
        SkillManager.SkillCast -= TurnCount;
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
        return IsPlayerReactPhase() || isTurn_cooperation;
    }

    public bool IsEnemyActive()
    {
        return IsEnemyReactPhase() || !isTurn_cooperation;
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
        
        Reacttrun = isTurn_cooperation ? ReactTurnPhase.PlayerTurn : ReactTurnPhase.EnemyTurn;
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
        if (Turn == 1)
            return;
        
        Stats selec = characterSelection.selectedCharacter;
        Stats chrt1 = characterStats.PlayerCharacter1;
        Stats chrt2 = characterStats.PlayerCharacter2;
        if (isTurn_cooperation)
        {
            // 공격턴일 때는 모든 캐릭터 복귀
            if (!characterStats.PlayerCharacter1.isdie)
            {
                characterStats.PlayerCharacter1.Rest(3);
                characterStats.PlayerCharacter1.characterPrefab.SetActive(true);
            }
            if (!characterStats.PlayerCharacter2.isdie)
            {
                characterStats.PlayerCharacter2.Rest(3);
                characterStats.PlayerCharacter2.characterPrefab.SetActive(true);
            }



            // HP바 활성화
            if (!chrt1.mainCh && !chrt1.isdie)
                characterStats.PlayerCharacter1.HPbar.gameObject.SetActive(true);
            if (!chrt2.mainCh && !chrt2.isdie)
                characterStats.PlayerCharacter2.HPbar.gameObject.SetActive(true);

        }
        else
        {
            

            // 수비턴일 때는 선택된 캐릭터만 활성화

            if (!characterStats.PlayerCharacter1.isdie)
                chrt1.characterPrefab.SetActive(characterStats.PlayerCharacter1.HasStatus(StatusType.main));
            if (!characterStats.PlayerCharacter2.isdie)
                chrt2.characterPrefab.SetActive(characterStats.PlayerCharacter2.HasStatus(StatusType.main));

            OnTurnChanged?.Invoke(selec);

            // 선택된 캐릭만 HP바 활성화
            if (!chrt1.mainCh && !chrt1.isdie)
                characterStats.PlayerCharacter1.HPbar.gameObject.SetActive(characterStats.PlayerCharacter1.HasStatus(StatusType.main));

            if (!chrt2.mainCh && !chrt2.isdie)
                characterStats.PlayerCharacter2.HPbar.gameObject.SetActive(characterStats.PlayerCharacter2.HasStatus(StatusType.main));
        }
        OnTurnChanged_condition?.Invoke(selec);
        OnTurnChanged_condition?.Invoke(characterStats.PlayerCharacter1);
        OnTurnChanged_condition?.Invoke(characterStats.PlayerCharacter2);
    }

    //  턴 전환 (일반 턴 순환: 플레이어 <-> 적)
    public void NextTurn()
    {
        skillmanager.Skillcancel(); // 스킬 쿨타임 초기화


        //턴 업데이트
        List<bool> ServeTurn = stageDataManager.CurrentStage.isServeTurn;

        Turn++;

        //턴 카운트
        isTurn_cooperation = ServeTurn[(Turn - 1) % ServeTurn.Count];
        if (isTurn_cooperation) turn_cooperation++;
        else turn_alone++;

        turnCount = stageDataManager.CurrentStage.turnOrder[(Turn - 1) % ServeTurn.Count];
        uiManager.UpdateTrunCount2(turnCount);

        EnterReactPhase();
        //CharacterSelection.selectedCharacterIndex = -1;
        characterUIManager.UpdateProfileUIBySelection();
        
        UITrunCount(Turn);//  턴 UI 업데이트
        ChrtTag();

        try
        {
            if(CharacterSelection.selectedCharacterIndex != -1)
            characterUIManager.ProfileUpdate(characterSelection.PickcharNumber(CharacterSelection.selectedCharacterIndex),
                isTurn_cooperation);
        }
        catch
        {
            Debug.LogError($"[TurnManager] 캐릭터 프로필 업데이트 실패:");
        }
        // 모든 캐릭터의 스킬 쿨타임 감소
        foreach (var character in characterStats.characterList)
        {
/*            foreach (var skill in character.usingSkill)
            {
                if (skill.colldownTime > 0)
                {
                    skill.ReduceCooldown(Turn % 2);
                }
            }*/
            character.isPatternEnd = character.isdie; // AI 패턴 초기화
        }

        Debug.Log($"[TurnManager] 턴 전환됨");

        stageDataManager.CheckTurn();//스토리활성화
        stageDataManager.CheckmainTurn();
    }

    //  EventManager 이벤트용
    public void NextTurnEnd()
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
        uiManager.UpdateReactTurn(isTurn_cooperation);
    }

    public void TurnCount()
    {
        if(!isTurn_cooperation) return;
        turnCount--;
        uiManager.UpdateTrunCount2(turnCount);
        if (turnCount <= 0)
        {

            StartCoroutine(TurnCountZero());
        }

    }

    public IEnumerator TurnCountZero()
    {
        yield return new WaitForSeconds(1f);
        while (skillmanager.isCastingSkill) yield return null;
        TurnEnd?.Invoke();

        while (skillmanager.isCastingSkill 
            || !CharacterStats.Instance.characterList
            .Where(stats => stats.team == Team.enemy)
            .All(stats => stats.isPatternEnd)) yield return null;

        yield return new WaitForSeconds(3f);
        EventManager.Instance.FinishTurn();
    }
}

