using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AIPattern
{
    public SkillQueue[] skillQueue; // 복사된 스킬 큐
    public List<SkillQueue> skillQueueList; // 복사된 스킬 큐

    public AIPattern(BossPattern data)
    {
        if (data == null || data.skillQueue == null)
        {
            skillQueue = new SkillQueue[0];
            return;
        }

        skillQueue = new SkillQueue[data.skillQueue.Length];
        for (int i = 0; i < data.skillQueue.Length; i++)
        {
            SkillQueue src = data.skillQueue[i];
            skillQueue[i] = new SkillQueue
            {
                skill = src.skill, // Skill이 ScriptableObject라면 참조 복사(문제 없음)
                currentIndex = src.currentIndex,
                delay = src.delay,
                coordinate = src.coordinate,
                Rotation = src.Rotation,
                targetTypeX = src.targetTypeX,
                targetTypeY = src.targetTypeY,
                RotationType = src.RotationType,
                Designation = src.Designation,
                reverse_order = src.reverse_order
            };
        }
        skillQueueList = GetSkillQueueList();
    }

    // SkillQueue 배열을 List로 반환
    public List<SkillQueue> GetSkillQueueList()
    {
        if (skillQueue == null)
            return new List<SkillQueue>();
        return new List<SkillQueue>(skillQueue);
    }
}