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
    public BattelManager battelManager; // 배틀 매니저 인스턴스

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
        // 현재 적이 살아있는 수
        var timingList = CurrentStage.storyTiming;
        var timingList1 = CurrentStage.storyTiming;
        if (timingList.Count == 0) return;
        // 살아있는 적이 없으면 조건을 만족하는 첫 StoryTiming 실행
        if (battelManager.boss_hp <= 0)
        {

            // 조건을 만족하는 StoryTiming를 추출
            var timing = timingList
                .Where(t => t.storyTimingType == StoryTimingType.hp && t.isPopUp) // kill 타입만 필터링
                .FirstOrDefault(t => !storyManager.readpopupStoryID.Contains(t.ID));

            // 조건을 만족하는 StoryTiming를 추출
            var bartiming = timingList1
                .Where(talk => talk.storyTimingType == StoryTimingType.turnCount && !talk.isPopUp)
                .FirstOrDefault(talk => !storyManager.readStoryID.Contains(talk.ID));

            float hp = battelManager.boss_hp;
            float hpMx = battelManager.boss_maxhp;
            bool check = hp / hpMx <= timing.value;

            if(!check)
                return;
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
        // 살아있는 적이 없으면 조건을 만족하는 첫 StoryTiming 실행
        if (battelManager.boss_hp <= 0)
        {

            // 조건을 만족하는 StoryTiming를 추출
            var timing = timingList
                .Where(t => t.storyTimingType == StoryTimingType.kill && t.isPopUp) // kill 타입만 필터링
                .FirstOrDefault(t => !storyManager.readpopupStoryID.Contains(t.ID));

            // 조건을 만족하는 StoryTiming를 추출
            var bartiming = timingList1
                .Where(talk => talk.storyTimingType == StoryTimingType.kill && !talk.isPopUp)
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
            .Where(t => t.storyTimingType == StoryTimingType.turnCount && t.isPopUp) // turn 타입과 speech_bubble 타입만 필터링
            .FirstOrDefault(t => turn >= t.value && !storyManager.readpopupStoryID.Contains(t.ID));

        // 조건을 만족하는 StoryTiming를 추출
        var bartiming = timingList1
            .Where(talk => talk.storyTimingType == StoryTimingType.turnCount && !talk.isPopUp) 
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


    public void CheckmainTurn()
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
        Time.timeScale = 1f; // 게임 시간 재개
        SceneManager.LoadScene("Stage_Selection"); // 게임 씬으로 전환
    }
}
