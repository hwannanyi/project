using UnityEngine;

public class EventManager : MonoBehaviour
// TurnManager 보다 먼저 실행되어야함
{
    public static EventManager Instance;

    private void Awake()
    {

            Instance = this;

    }

    public delegate void SignalEvent(bool value);
    public event SignalEvent TurnEnd;

    public delegate void SignalUseSkill(bool value);
    //public event SignalUseSkill Useskill;

    public void FinishTurn(bool value)
    {
        TurnEnd?.Invoke(value);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            FinishTurn(true);
        }
    }
}
