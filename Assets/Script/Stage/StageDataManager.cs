using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

public class StageDataManager : MonoBehaviour
{
    public StageManager stageManager;

    public Stage CurrentStage; // 현재 선택된 스테이지

    public delegate bool StoryCondition(StoryTiming timing);//조건

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
    }


    public void CheckHP(StoryTiming timing)
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
    public void CheckKill(StoryTiming timing)
    {
        int kill = characterStats.characterList.Count(stats => stats.team == Team.enemy && stats.isdie == false);
        bool check = kill >= timing.value;
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

    public void CheckTurn(StoryTiming timing)
    {
        float turn = (float)turnManager.Turn;
        bool check = turn == timing.value;
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
