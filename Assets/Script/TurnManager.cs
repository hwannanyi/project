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

    public TurnPhase currentPhase = TurnPhase.PlayerTurn;
    public TurnPhase previousPhase;

    public int Turn = 0;
    public int nextTrunCount = 4;
    public int nextTrunCounting = 0;

    public int playerSkillTurn = 10;
    public int enemySkillTurn = 10;

    public int playerUseSkillTurn = 0;
    public int enemyUseSkillTurn = 0;


    public int playerReactTrun = 3;
    public int enemyReactTrun = 3;

    public int playerUseSkillReactTrun = 0;
    public int enemyUseSkillReactTrun = 0;

    private void Awake()
    {
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
    public void EnterReactPhase(Stats reactingTeam)
    {
        playerUseSkillReactTrun = 0;
        enemyUseSkillReactTrun = 0;
        previousPhase = currentPhase;

        if (IsPlayerTeam(reactingTeam))
            currentPhase = TurnPhase.ReactPhase_EnemyResponding;
        else
            currentPhase = TurnPhase.ReactPhase_PlayerResponding;

        Debug.Log($"[TurnManager] 대응단계 진입: 현재 대응팀 = {currentPhase}");
    }

    //  대응단계 종료 → 이전 턴 복원
    public void ExitReactPhase()
    {
        CharacterSelection.selectedCharacterIndex = -1;
        currentPhase = previousPhase;
        Debug.Log($"[TurnManager] 대응단계 종료: 턴 복귀 = {currentPhase}");
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
        CharacterSelection.selectedCharacterIndex = -1;
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
}
