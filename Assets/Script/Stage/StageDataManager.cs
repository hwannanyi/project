using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class StageDataManager : MonoBehaviour
{
    public static StageDataManager Instance; // 싱글턴 인스턴스
    public StageManager stageManager; // 스테이지 매니저 인스턴스
    public StoryManager storyManager; // 스토리 매니저 인스턴스

    public Stage CurrentStage; // 현재 선택된 스테이지

    public delegate void StoryCondition(List<StoryTiming> timing);//조건부 스토리 실행
    // 델리게이트 변수 선언
    public StoryCondition storyConditionHandler;


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
/*            if (timing.talkType == TalkType.speech_bubble)
                storyManager.PopUpStoryReStart(timing.ID);
            else if (timing.talkType == TalkType.bare)
                storyManager.StoryReStart(CurrentStage.ID);
            else if (timing.talkType == TalkType.all)
            {
                storyManager.PopUpStoryReStart(timing.ID);
                storyManager.StoryReStart(CurrentStage.ID);
            }
*/

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
        var timingList1 = CurrentStage.storyTiming;
        if (timingList.Count == 0) return;
        int aliveEnemyCount = characterStats.characterList.Count(stats => stats.team == Team.enemy && stats.isdie == false);

        // 살아있는 적이 없으면 조건을 만족하는 첫 StoryTiming 실행
        if (aliveEnemyCount == 0)
        {

            // 조건을 만족하는 StoryTiming를 추출
            var timing = timingList
                .Where(t => t.storyTimingType == StoryTimingType.kill && t.isPopUp) // kill 타입만 필터링
                .FirstOrDefault(t => !storyManager.readpopupStoryID.Contains(t.ID));

            // 조건을 만족하는 StoryTiming를 추출
            var bartiming = timingList1
                .Where(talk => talk.storyTimingType == StoryTimingType.turn && !talk.isPopUp)
                .FirstOrDefault(talk => !storyManager.readStoryID.Contains(talk.ID));



            try
            {
                if (timing.isPopUp)
                {
                    // 말풍선 스토리 실행
                    storyManager.PopUpStoryReStart(timing.ID);
                }
            }
            catch
            {
            }
            try
            {
                if (!bartiming.isPopUp)
                {
                    // 말풍선 스토리 실행
                    storyManager.StoryReStart(bartiming.ID);
                }
            }
            catch
            {
            }
        }
    }

    public void CheckTurn()
    {
        var timingList = CurrentStage.storyTiming;
        var timingList1 = CurrentStage.storyTiming;
        if (timingList.Count == 0) return;
        float turn = (float)turnManager.Turn;

        // 조건을 만족하는 StoryTiming를 추출
        var timing = timingList
            .Where(t => t.storyTimingType == StoryTimingType.turn && t.isPopUp) // turn 타입과 speech_bubble 타입만 필터링
            .FirstOrDefault(t => turn >= t.value && !storyManager.readpopupStoryID.Contains(t.ID));

        // 조건을 만족하는 StoryTiming를 추출
        var bartiming = timingList1
            .Where(talk => talk.storyTimingType == StoryTimingType.turn && !talk.isPopUp) 
            .FirstOrDefault(talk => turn >= talk.value && !storyManager.readStoryID.Contains(talk.ID));

        try
        {
            if (timing.isPopUp)
            {
                // 말풍선 스토리 실행
                storyManager.PopUpStoryReStart(timing.ID);
            }
        }
        catch
        {
        }
        try
        {
            if (!bartiming.isPopUp)
            {
                // 말풍선 스토리 실행
                storyManager.StoryReStart(bartiming.ID);
            }
        }
        catch
        {
        }
        
    }


    public void GoStage_Selection()
    {
        SceneManager.LoadScene("Stage_Selection"); // 게임 씬으로 전환
    }
}
