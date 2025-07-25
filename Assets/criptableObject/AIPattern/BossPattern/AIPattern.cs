using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class AIPattern
{
    public List<List<SkillQueue>> skillQueueList; // 2차원 리스트
    public List<List<SkillCondition>> skillConditionList; // 2차원 리스트

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
                    condition = src.condition,
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
                    target = src.target
                });
            }
            skillQueueList.Add(skillList);
        }

        skillConditionList = new List<List<SkillCondition>>();

        if (data == null || data.skillCondition == null)
            return;

        for (int i = 0; i < data.skillCondition.Count; i++)
        {
            var row = data.skillCondition[i].skillCondition; // DoubleList_SkillQueue의 skillQueue 사용
            var skillList = new List<SkillCondition>();

            for (int j = 0; j < row.Count; j++)
            {
                SkillCondition src = row[j];
                skillList.Add(new SkillCondition
                {
                    skill = src.skill,
                    condition = src.condition,
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
                    target = src.target
                });
            }
            skillConditionList.Add(skillList);
        }
    }
}