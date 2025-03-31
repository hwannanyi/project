using UnityEngine;

public class EventManager : MonoBehaviour
// TurnManager 보다 먼저 실행되어야함
{
    public static EventManager Instance;

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

    public delegate void SignalEvent(bool value);
    public event SignalEvent TurnEnd;

    public delegate void SignalUseSkill(bool value);
    public event SignalUseSkill Useskill;

    public void FinishTurn(bool value)
    {
        Debug.Log($"[EventManager] 신호 보냄: {value}");
        TurnEnd?.Invoke(value);
    }
}
