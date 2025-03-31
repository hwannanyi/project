using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public static int Turn;
    public static int skillTurn;

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
            EventManager.Instance.TurnEnd += ReceiveTurnEnd;
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
            EventManager.Instance.TurnEnd -= ReceiveTurnEnd;
        }
    }

    private void ReceiveTurnEnd(bool value)
    {
        Debug.Log($"[SignalReceiver] 신호 받음: {value}");
        Turn++;
        Debug.Log(Turn);
    }
}
