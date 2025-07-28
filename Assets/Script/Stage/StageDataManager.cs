using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

public class StageDataManager : MonoBehaviour
{
    public StageManager stageManager;

    public StoryManager storyManager; // 스토리 매니저 인스턴스

    public Stage CurrentStage; // 현재 선택된 스테이지

    public delegate void StoryCondition(List<StoryTiming> timing);//조건부 스토리 실행
    // 델리게이트 변수 선언
    public StoryCondition storyConditionHandler;

    public UnityEvent<string> PopUpOnStoryReStart = new UnityEvent<string>();
    public UnityEvent<string> OnStoryReStop = new UnityEvent<string>();
    public UnityEvent PopUpOnStoryEnd = new UnityEvent();

    public TurnManager turnManager;
    public CharacterStats characterStats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        try
        {
            stageManager = StageManager.Instance;
            CurrentStage = stageManager.CurrentStage;
        } catch {
            Debug.LogError("StageManager instance not found");
        }
        turnManager = GetComponent<TurnManager>();
        characterStats = GetComponent<CharacterStats>();
        storyManager = GetComponent<StoryManager>();
        storyConditionHandler = CheckStoryTiming;
    }


    public void CheckStoryTiming(List<StoryTiming> timing)
    {
        if (timing == null)
        {
            Debug.LogError("StoryTiming is null");
            return;
        }
/*        switch (timing.storyTimingType)
        {
            case StoryTimingType.hp:
                CheckHP(timing);
                break;
            case StoryTimingType.kill:
                CheckKill(timing);
                break;
            case StoryTimingType.turn:
                CheckTurn(timing);
                break;
            default:
                Debug.LogWarning("에러끼얏호우");
                break;
        }*/
    }

    public void CheckHP(List<StoryTiming> timing)
    {

        // (stats.hp / (float)stats.maxhp) : 0~1 사이의 비율
        bool check = characterStats.characterList
            .Any(stats => stats.name == timing.Target
                          && !stats.isdie
                          && (stats.hp / (float)stats.maxhp) <= timing.value);
        if (check)
        {
            if (timing.isPopUp)
            {
                PopUpOnStoryReStart.Invoke(timing.ID);
            }
            else
            {
                OnStoryReStop.Invoke(timing.ID);
            }

        }
    }
    public void CheckKill(List<StoryTiming> timingList)
    {
        // 현재 적이 살아있는 수
        int kill = characterStats.characterList.Count(stats => stats.team == Team.enemy && stats.isdie == false);

        // 조건을 만족하는 StoryTiming 객체 추출
        var timing = timingList
            .FirstOrDefault(t => kill >= t.value && !storyManager.readpopupStoryID.Contains(t.ID));

        if (!string.IsNullOrEmpty(timingId))
        {
            var timing = timingList.First(t => t.ID == timingId);
            if (timing.isPopUp)
                PopUpOnStoryReStart.Invoke(timingId);
            else
                OnStoryReStop.Invoke(timingId);
        }
    }

    public void CheckTurn(List<StoryTiming> timingList)
    {
        float turn = (float)turnManager.Turn;

        // 조건을 만족하는 StoryTiming를 추출
        var timing = timingList
        .FirstOrDefault(t => turn >= t.value && !storyManager.readpopupStoryID.Contains(t.ID));
        bool check = !string.IsNullOrEmpty(timing.ID);
        if (check) 
        {
            if (timing.isPopUp)
            {
                PopUpOnStoryReStart.Invoke(timing.ID);
            }
            else
            {
                OnStoryReStop.Invoke(timing.ID);
            }

        }
    }

}
