using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class AIPattern
{
    public List<List<SkillQueue>> skillQueueList; // 2차원 리스트

    public AIPattern(BossPattern data)
    {
        skillQueueList = new List<List<SkillQueue>>();

        if (data == null || data.skillQueue == null)
            return;

        for (int i = 0; i < data.skillQueue.Count; i++)
        {
            var row = data.skillQueue[i].skillQueue; // DoubleList_SkillQueue의 skillQueue 사용
            var skillList = new List<SkillQueue>();

            for (int j = 0; j < row.Count; j++)
            {
                SkillQueue src = row[j];
                skillList.Add(new SkillQueue
                {
                    skill = src.skill,
                    currentIndex = src.currentIndex,
                    delay = src.delay,
                    isCastingNotCast = src.isCastingNotCast,
                    coordinate = src.coordinate,
                    Rotation = src.Rotation,
                    targetTypeX = src.targetTypeX,
                    targetTypeY = src.targetTypeY,
                    RotationType = src.RotationType,
                    Designation = src.Designation,
                    reverse_order = src.reverse_order,
                    index = src.index,
                });
            }
            skillQueueList.Add(skillList);
        }
    }
}