using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class AIPattern
{
    public List<List<SkillQueue>> skillQueueList; // 2차원 리스트
    public List<List<Pattern>> skillCondition; // 2차원 리스트
    public List<List<Pattern>> patterns; //패턴
    public bool isRandomPattern; //패턴 랜덤실행

    public AIPattern(BossPattern data)
    {

        skillQueueList = new List<List<SkillQueue>>();
        if (data == null || data.skillQueue == null)
            return;
        isRandomPattern = data.isRandomPattern;
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

        patterns = new List<List<Pattern>>();
        if (data == null || data.patterns == null)
            return;
        for (int i = 0; i < data.patterns.Count; i++)
        {
            // null 체크 추가
            var doubleRow = data.patterns[i];
            if (doubleRow == null || doubleRow.pattern == null)
                continue;


            var row = data.patterns[i].pattern; // DoubleList_SkillQueue의 skillQueue 사용
            var skillList = new List<Pattern>();

            for (int j = 0; j < row.Count; j++)
            {
                Pattern src = row[j];
                // null 체크 추가
                if (src == null)
                    continue;
                skillList.Add(new Pattern
                {
                    skill = src.skill,
                    ammo_circle = src.ammo_circle,
                    radius_circle = src.radius_circle,
                    isRadius_tracking_circle = src.isRadius_tracking_circle,
                    delayTime_circle = src.delayTime_circle,
                    position_circle = src.position_circle,
                    isPosition_tracking_circle = src.isPosition_tracking_circle,
                    count_circle = src.count_circle,
                    isRandom_circle = src.isRandom_circle,
                    ammo_straight = src.ammo_straight,
                    interval_straight = src.interval_straight,
                    direction_straight = src.direction_straight,
                    radius_straight = src.radius_straight,
                    isRadius_tracking_straight = src.isRadius_tracking_straight,
                    delayTime_straight = src.delayTime_straight,
                    position_straight = src.position_straight,
                    isPosition_tracking_straight = src.isPosition_tracking_straight,
                    count_straight = src.count_straight,
                    isRandom_straight = src.isRandom_straight,
                    skill_repeat = src.skill_repeat,
                    count_repeat = src.count_repeat,
                    isRandom_repeat = src.isRandom_repeat,
                    useCond = src.useCond,
                    condition = src.condition,
                    statusType = src.statusType
                });
            }
            patterns.Add(skillList);
        }


        skillCondition = new List<List<Pattern>>();
        if (data == null || data.skillCondition == null)
            return;
        for (int i = 0; i < data.skillCondition.Count; i++)
        {
            // null 체크 추가
            var doubleRow = data.skillCondition[i];
            if (doubleRow == null || doubleRow.pattern == null)
                continue;


            var row = data.skillCondition[i].pattern; // DoubleList_SkillQueue의 skillQueue 사용
            var skillList = new List<Pattern>();

            for (int j = 0; j < row.Count; j++)
            {
                Pattern src = row[j];
                // null 체크 추가
                if (src == null)
                    continue;
                skillList.Add(new Pattern
                {
                    skill = src.skill,
                    ammo_circle = src.ammo_circle,
                    radius_circle = src.radius_circle,
                    isRadius_tracking_circle = src.isRadius_tracking_circle,
                    delayTime_circle = src.delayTime_circle,
                    position_circle = src.position_circle,
                    isPosition_tracking_circle = src.isPosition_tracking_circle,
                    count_circle = src.count_circle,
                    isRandom_circle = src.isRandom_circle,
                    ammo_straight = src.ammo_straight,
                    interval_straight = src.interval_straight,
                    direction_straight = src.direction_straight,
                    radius_straight = src.radius_straight,
                    isRadius_tracking_straight = src.isRadius_tracking_straight,
                    delayTime_straight = src.delayTime_straight,
                    position_straight = src.position_straight,
                    isPosition_tracking_straight = src.isPosition_tracking_straight,
                    count_straight = src.count_straight,
                    isRandom_straight = src.isRandom_straight,
                    skill_repeat = src.skill_repeat,
                    count_repeat = src.count_repeat,
                    isRandom_repeat = src.isRandom_repeat,
                    useCond = src.useCond,
                    condition = src.condition,
                    statusType = src.statusType
                });
            }
            skillCondition.Add(skillList);
        }
    }
}