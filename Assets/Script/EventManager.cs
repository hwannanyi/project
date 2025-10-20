using System;

using UnityEngine;

public class EventManager : MonoBehaviour
// TurnManager 보다 먼저 실행되어야함
{

    // 턴을 넘기는 매니저
    public static EventManager Instance;

    public StoryManager storyManager; // 스토리 매니저 컴포넌트
    public TurnManager turnManager; // 턴 매니저 컴포넌트
    private void Awake()
    {
        storyManager = GetComponent<StoryManager>();
        turnManager = GetComponent<TurnManager>();
            Instance = this;

    }

    public delegate void SignalEvent();
    public event SignalEvent TurnEnd;

    public event Action isMove; // 누군가이동

    public delegate void SignalUseSkill(bool value);
    //public event SignalUseSkill Useskill;

    public void FinishTurn()
    {
        Debug.Log("OnTurnEnd");
        /*        try
                {
                    if (storyManager.isStoryActive || storyManager.turnLock)
                        return; // 모든 입력 무시
                }
                catch
                {

                    return; // StoryManager를 못불려와도 모든입력무시
                }*/

        TurnEnd?.Invoke();
    }


    public void Ismove()
    {
        isMove?.Invoke();
    }

/*    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && turnManager.isTurn_cooperation) 
        {
            FinishTurn();
        }
    }*/
}
