using UnityEngine;

public class EventManager : MonoBehaviour
// TurnManager 보다 먼저 실행되어야함
{

    // 턴을 넘기는 매니저
    public static EventManager Instance;

    public StoryManager storyManager; // 스토리 매니저 인스턴스
    private void Awake()
    {
        storyManager = GetComponent<StoryManager>();

            Instance = this;

    }

    public delegate void SignalEvent(bool value);
    public event SignalEvent TurnEnd;

    public delegate void SignalUseSkill(bool value);
    //public event SignalUseSkill Useskill;

    public void FinishTurn(bool value)
    {
        try
        {
            if (storyManager.isStoryActive || storyManager.turnLock)
                return; // 모든 입력 무시
        }
        catch
        {

            return; // StoryManager를 못불려와도 모든입력무시
        }

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
