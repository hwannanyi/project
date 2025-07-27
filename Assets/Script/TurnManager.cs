using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

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

    public UnityEvent<string> OnStoryStart = new UnityEvent<string>();
    public UnityEvent<string> OnStoryStop = new UnityEvent<string>();
    public UnityEvent<string> PopUpOnStoryStart = new UnityEvent<string>();
    public UnityEvent<string> PopUpOnStoryStop = new UnityEvent<string>();

    public StageManager stageManager;
    public TurnUIManager uiManager;
    public CharacterUIManager characterUIManager; // 캐릭터 프로필 UI 매니저

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

    private void Awake()
    {
        characterSelection = GetComponent<CharacterSelection>();
        characterStats = GetComponent<CharacterStats>();
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
        return CharacterStats.Instance.playerCharacters.Contains(character.name);
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
        CharacterSelection.selectedCharacterIndex = -1;
        characterUIManager.UpdateProfileUIBySelection();

        Reacttrun = ReactTurnPhase.None;
        // 턴 복귀
        Debug.Log($"[TurnManager] 대응단계 종료: 턴 복귀");
    }

    //  턴 전환 (일반 턴 순환: 플레이어 <-> 적)
    public void NextTurn()
    {

        
        isPlayerTurn = !isPlayerTurn; // 플레이어 턴 여부 토글
        EnterReactPhase();
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
            if (character.mp < 10)
            {
                character.mp += 1;
            }
            if (character.NowMoveCount < 8)
            {
                character.NowMoveCount += 1;
            }
            character.isPatternEnd = false; // AI 패턴 초기화
        }

        Debug.Log($"[TurnManager] 턴 전환됨");
        // Team.enemy인 캐릭터만 enemies 리스트에 저장
        var enemies = characterStats.characterList
            .Where(c => c.team == Team.enemy)
            .ToList();
        // 모든 적이 죽었는지 확인 (예시: enemies 리스트 사용)
        bool allEnemiesDead = enemies.All(e => e.isdie);

        // 스토리 종료 상태 확인
        if (allEnemiesDead && StoryManager.instance != null && StoryManager.instance.isStoryEnd)
        {
            Debug.Log("스테이지 클리어!");
            SceneManager.LoadScene("Stage_Selection"); // 게임 씬으로 전환
            return;
            // 필요하다면 UI에 메시지 표시 등 추가 작업
        }
        Tutorial();
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

    ////////////////////////////////
    
    public void Tutorial()
    {
        if (stageManager.CurrentStage.stagenumber == 0)
        {
            if (Turn == 1 || Turn == 3 || Turn == 5 || Turn == 7)
            {
                // 스토리 시작 이벤트 발생
                PopUpOnStoryStart.Invoke("1");
            }
        }
    }
}
