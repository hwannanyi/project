using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class StageDataManager : MonoBehaviour
{
    public static StageDataManager Instance; // 싱글턴 인스턴스
    public StageManager stageManager; // 스테이지 매니저 인스턴스
    public StoryManager storyManager; // 스토리 매니저 인스턴스

    public Stage CurrentStage; // 현재 선택된 스테이지

    public delegate void StoryCondition(List<StoryTiming> timing);//조건부 스토리 실행
    // 델리게이트 변수 선언
    public StoryCondition storyConditionHandler;

    private UnityEvent<string> PopUpOnStoryReStart = new UnityEvent<string>();
    private UnityEvent<string> OnStoryReStop = new UnityEvent<string>();
    public UnityEvent PopUpOnStoryEnd = new UnityEvent();

    public TurnManager turnManager;
    public CharacterStats characterStats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this; // 싱글턴 인스턴스 할당
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

        PopUpOnStoryReStart.AddListener(storyManager.PopUpStoryReStart);// 팝업 스토리 재시작 이벤트
        OnStoryReStop.AddListener(storyManager.StoryReStart);// 일반 스토리 재시작 이벤트

    }

    public void CheckHP()
    {
        var timingList = CurrentStage.storyTiming;
        // 아직 읽지 않은 스토리 중 value(비율)가 가장 높은 것 우선
        var timing = timingList
            .Where(t => !storyManager.readpopupStoryID.Contains(t.ID))
            .OrderByDescending(t => t.value)
            .FirstOrDefault();

        if (timing == null) return;

        // 타겟 캐릭터의 체력 비율이 timing.value 이하인지 확인 (0~1 기준)
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
    /*    public void CheckKill(List<StoryTiming> timingList)
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
        }*/

    public void CheckKill()
    {
        // 현재 적이 살아있는 수
        var timingList = CurrentStage.storyTiming;
        if (timingList.Count == 0) return;
        int aliveEnemyCount = characterStats.characterList.Count(stats => stats.team == Team.enemy && stats.isdie == false);

        // 살아있는 적이 없으면 조건을 만족하는 첫 StoryTiming 실행
        if (aliveEnemyCount == 0)
        {

            // 조건을 만족하는 StoryTiming를 추출
            var timing = timingList
                .Where(t => t.storyTimingType == StoryTimingType.kill) // kill 타입만 필터링
                .FirstOrDefault(t => !storyManager.readpopupStoryID.Contains(t.ID));

            if (timing != null && !string.IsNullOrEmpty(timing.ID))
            {
                if (timing.isPopUp)
                    PopUpOnStoryReStart.Invoke(timing.ID);
                else
                    OnStoryReStop.Invoke(timing.ID);
            }
            return;
        }

/*        // 살아있는 적이 있을 때 기존 조건대로 실행
        var killTiming = timingList
            .FirstOrDefault(t => aliveEnemyCount >= t.value && !storyManager.readpopupStoryID.Contains(t.ID));

        if (killTiming != null && !string.IsNullOrEmpty(killTiming.ID))
        {
            if (killTiming.isPopUp)
                PopUpOnStoryReStart.Invoke(killTiming.ID);
            else
                OnStoryReStop.Invoke(killTiming.ID);
        }*/
    }

    public void CheckTurn()
    {
        var timingList = CurrentStage.storyTiming;
        if (timingList.Count == 0) return;
        float turn = (float)turnManager.Turn;

        // 조건을 만족하는 StoryTiming를 추출
        var timing = timingList
            .Where(t => t.storyTimingType == StoryTimingType.turn) // turn 타입만 필터링
            .FirstOrDefault(t => turn >= t.value && !storyManager.readpopupStoryID.Contains(t.ID));
        if (timing == null) return;
        bool check = !string.IsNullOrEmpty(timing.ID);
        
        string ID = check ? timing.ID : null;
        if (timing.isPopUp)
        { //스토리바, 팝업창 구분
            Debug.Log($"[StageDataManager] CheckTurn: {ID} : {timing.ID}");
            Startpopstory(ID);
        }
        else
            Startstory(ID);
    }


    public void Startpopstory(string ID)
    {
        PopUpOnStoryReStart.Invoke(ID);
    }

    public void Startstory(string ID)
    {
        OnStoryReStop.Invoke(ID);
    }
}
