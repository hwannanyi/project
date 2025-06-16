using UnityEngine;
using System;

public enum TurnPhase
{
    PlayerTurn,
    EnemyTurn,
    ReactPhase_PlayerResponding,
    ReactPhase_EnemyResponding
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnUIManager uiManager;
    public CharacterUIManager characterUIManager; // 캐릭터 프로필 UI 매니저
    public TurnPhase nowtrun = TurnPhase.PlayerTurn;
    public TurnPhase currentPhase = TurnPhase.PlayerTurn;
    public TurnPhase previousPhase;

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

    private void Awake()
    {
        characterSelection = GetComponent<CharacterSelection>();
        characterStats = GetComponent<CharacterStats>();

    Turn = 1;
    UITrunCount(Turn);//  턴 UI 업데이트
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {

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

    public bool NowPlayerTurn()
    {
        return nowtrun == TurnPhase.PlayerTurn;
    }

    public bool IsPlayerTurn()
    {
        return currentPhase == TurnPhase.PlayerTurn;
    }

    public bool IsEnemyTurn()
    {
        return currentPhase == TurnPhase.EnemyTurn;
    }

    public bool IsInReactPhase()
    {
        return currentPhase == TurnPhase.ReactPhase_PlayerResponding ||
               currentPhase == TurnPhase.ReactPhase_EnemyResponding;
    }

    public bool IsPlayerReactPhase()
    {
        return currentPhase == TurnPhase.ReactPhase_PlayerResponding;
    }

    public bool IsEnemyReactPhase()
    {
        return currentPhase == TurnPhase.ReactPhase_EnemyResponding;
    }


    //  캐릭터가 플레이어 팀인지 판별
    public bool IsPlayerTeam(Stats character)
    {
        return CharacterStats.Instance.playerCharacters.Contains(character.name);
    }

    //  대응단계 진입 → 대응하는 팀에게 턴을 넘김
    public void EnterReactPhase()
    {
        playerUseSkillReactTrun = 0;
        enemyUseSkillReactTrun = 0;
        previousPhase = currentPhase;

        if (currentPhase == TurnPhase.PlayerTurn)
            currentPhase = TurnPhase.ReactPhase_EnemyResponding;
        else if (currentPhase == TurnPhase.EnemyTurn)
            currentPhase = TurnPhase.ReactPhase_PlayerResponding;
        else
            Debug.LogWarning("[TurnManager] 잘못된 대응단계 진입 시도: 현재 턴이 유효하지 않음");
    }

    //  대응단계 종료 → 이전 턴 복원
    /*public void ExitReactPhase()
    {
        CharacterSelection.selectedCharacterIndex = -1;

        if (currentPhase == TurnPhase.ReactPhase_PlayerResponding)
            currentPhase = TurnPhase.EnemyTurn;
        else if (currentPhase == TurnPhase.ReactPhase_EnemyResponding)
            currentPhase = TurnPhase.PlayerTurn;
        else
            Debug.LogWarning("유효하지 못한 턴");
        //currentPhase = previousPhase;
        Debug.Log($"[TurnManager] 대응단계 종료: 턴 복귀 = {currentPhase}");
    }*/

    public void ExitReactPhase()
    {
        //CharacterSelection.prevSelectedIndex = CharacterSelection.selectedCharacterIndex;
        //characterStats.characterList[CharacterSelection.selectedCharacterIndex].SetHighlight(false);
        CharacterSelection.selectedCharacterIndex = -1;
        characterUIManager.UpdateProfileUIBySelection();

        if (currentPhase == TurnPhase.ReactPhase_PlayerResponding)
        { currentPhase = TurnPhase.EnemyTurn;}
        else if (currentPhase == TurnPhase.ReactPhase_EnemyResponding)
        { currentPhase = TurnPhase.PlayerTurn;}
        else
            Debug.LogWarning("유효하지 못한 턴");
        // 턴 복귀
        Debug.Log($"[TurnManager] 대응단계 종료: 턴 복귀");
    }

    //  턴 전환 (일반 턴 순환: 플레이어 <-> 적)
    public void NextTurn()
    {
        if (currentPhase == TurnPhase.PlayerTurn)
        {
            currentPhase = TurnPhase.EnemyTurn;
        }
        else if (currentPhase == TurnPhase.EnemyTurn)
        {
            currentPhase = TurnPhase.PlayerTurn;
        }
        nowtrun = currentPhase;
        CharacterSelection.selectedCharacterIndex = -1;
        characterUIManager.UpdateProfileUIBySelection();
        Turn++;
        UITrunCount(Turn);//  턴 UI 업데이트
                          // 모든 캐릭터의 스킬 쿨타임 감소
        foreach (var character in CharacterStats.Instance.characterList)
        {
            foreach (var skill in character.usingSkill)
            {
                if (skill.colldownTime > 0)
                {
                    skill.ReduceCooldown(1);
                }
            }
        }
        foreach (var character in CharacterStats.Instance.characterList)
        {
            if (character.NowMoveCount < 8)
            {
                character.NowMoveCount += 1;
            }
        }
        foreach (var character in CharacterStats.Instance.characterList)
        {
            if (character.mp < 10)
            {
                character.mp += 1;
            }
        }
        Debug.Log($"[TurnManager] 턴 전환됨: 현재 턴 = {currentPhase}");
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
        uiManager.UpdateReactTurn(NowPlayerTurn());
    }
}
