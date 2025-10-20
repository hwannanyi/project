using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class AIPattern
{
    public List<List<Pattern>> skillCondition;
    public List<List<Pattern>> patterns_turn_alone; //패턴
    public List<List<Pattern>> patterns_turn_cooperation; //패턴
    public List<List<Pattern>> patterns_turn_cooperation_end; //패턴
    public bool isRandomPattern; //패턴 랜덤실행


    public AIPattern(BossPattern data)
    {
        patterns_turn_alone = new List<List<Pattern>>();
        if (data == null || data.patterns_turn_alone == null)
            return;
        for (int i = 0; i < data.patterns_turn_alone.Count; i++)
        {
            // null 체크 추가
            var doubleRow = data.patterns_turn_alone[i];
            if (doubleRow == null || doubleRow.pattern == null)
                continue;


            var row = data.patterns_turn_alone[i].pattern; // DoubleList_SkillQueue의 skillQueue 사용
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
                    count_repeat_Random = src.count_repeat_Random,
                    isRandom_repeat = src.isRandom_repeat,
                    isRandom_index = src.isRandom_index,
                    isindex_mix = src.isindex_mix,
                    Random_index = src.Random_index,
                    at_once = src.at_once,
                    at_onces = new List<int>(src.at_onces), // 리스트 복사
                    useCond = src.useCond,
                    condition = src.condition,
                    statusType = src.statusType
                });
            }
            patterns_turn_alone.Add(skillList);
        }



        patterns_turn_cooperation = new List<List<Pattern>>();
        if (data == null || data.patterns_turn_cooperation == null)
            return;
        for (int i = 0; i < data.patterns_turn_cooperation.Count; i++)
        {
            // null 체크 추가
            var doubleRow = data.patterns_turn_cooperation[i];
            if (doubleRow == null || doubleRow.pattern == null)
                continue;


            var row = data.patterns_turn_cooperation[i].pattern; // DoubleList_SkillQueue의 skillQueue 사용
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
                    count_repeat_Random = src.count_repeat_Random,
                    isRandom_repeat = src.isRandom_repeat,
                    isRandom_index = src.isRandom_index,
                    isindex_mix = src.isindex_mix,
                    Random_index = src.Random_index,
                    at_once = src.at_once,
                    at_onces = new List<int>(src.at_onces), // 리스트 복사
                    useCond = src.useCond,
                    condition = src.condition,
                    statusType = src.statusType
                });
            }
            patterns_turn_cooperation.Add(skillList);
        }

        patterns_turn_cooperation_end = new List<List<Pattern>>();
        if (data == null || data.patterns_turn_cooperation_end == null)
            return;
        for (int i = 0; i < data.patterns_turn_cooperation_end.Count; i++)
        {
            // null 체크 추가
            var doubleRow = data.patterns_turn_cooperation_end[i];
            if (doubleRow == null || doubleRow.pattern == null)
                continue;


            var row = data.patterns_turn_cooperation_end[i].pattern; // DoubleList_SkillQueue의 skillQueue 사용
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
                    count_repeat_Random = src.count_repeat_Random,
                    isRandom_repeat = src.isRandom_repeat,
                    isRandom_index = src.isRandom_index,
                    isindex_mix = src.isindex_mix,
                    Random_index = src.Random_index,
                    at_once = src.at_once,
                    at_onces = new List<int>(src.at_onces), // 리스트 복사
                    useCond = src.useCond,
                    condition = src.condition,
                    statusType = src.statusType
                });
            }
            patterns_turn_cooperation_end.Add(skillList);
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
                    count_repeat_Random = src.count_repeat_Random,
                    isRandom_repeat = src.isRandom_repeat,
                    isRandom_index = src.isRandom_index,
                    isindex_mix = src.isindex_mix,
                    Random_index = src.Random_index,
                    at_once = src.at_once,
                    at_onces = new List<int>(src.at_onces), // 리스트 복사
                    useCond = src.useCond,
                    condition = src.condition,
                    statusType = src.statusType
                });
            }
            skillCondition.Add(skillList);
        }
    }
}