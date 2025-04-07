using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public int TeamTurn = 0;
    public bool IsPlayerTeamTurn = true;
    public int playerSkillTurn = 10;
    public int playerUseSkillTurn = 0;
    public int enemySkillTurn = 10;
    public int enemyUseSkillTurn = 0;
    public int counterTrun = 0;

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
            return;
        }
    }

    private void OnEnable()
    {
        // EventManager.Instance가 null인지 체크하여 안전하게 구독
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

    private void NextTurnEnd(bool value)
    {
        CharacterSelection.selectedCharacterIndex = -1;//캐릭 선택 초기화

        Debug.Log($"[SignalReceiver] 신호 받음: {value}");
        if (IsPlayerTeamTurn)
        {
            IsPlayerTeamTurn = false;
        }
        else
        {
            IsPlayerTeamTurn = true;
        }
        TeamTurn++;
        //Debug.Log(TeamTurn);
        //Debug.Log(IsPlayerTeamTurn);
    }
}
