using UnityEngine;
using System.Linq;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Rendering;
using static UnityEngine.EventSystems.EventTrigger;
using System.Collections.Generic;
using static Stats;
using System.Xml.Schema;


public class SkillHitEffects : MonoBehaviour
{
    public int ValueCalculation(HitEffects effectType, Target target, Stats caster, SkillData skillData, int index)
    {

        int value = Mathf.RoundToInt(GetDamageBaseValueAt(effectType, skillData, target, index));

        int valueAdd = 0;
        int valueDefault = 0;

        // 지정된 대상(target)에 맞는 HitEffectEntry 찾기
        var targetEntry = skillData.hitEffects.FirstOrDefault(h => h.target == target);


        // 효과 타입에 따라 분기
        switch (effectType)
        {
            case HitEffects.DamageEffect:
                // 없거나 효과가 없다면 종료
                if (targetEntry == null || targetEntry.effects.DamageEffect.Count == 0)
                {
                    value = 0;
                    return value;
                }
                break;
            case HitEffects.BuffEffects:
                // 없거나 효과가 없다면 종료
                if (targetEntry == null || targetEntry.effects.BuffEffects.Count == 0)
                {
                    value = 0;
                    return value;
                };
                break;
            case HitEffects.DebuffEffects:
                // 없거나 효과가 없다면 종료
                if (targetEntry == null || targetEntry.effects.DebuffEffects.Count == 0)
                {
                    value = 0;
                    return value;
                }
                break;
            case HitEffects.CCEffects:
                // 없거나 효과가 없다면 종료
                if (targetEntry == null || targetEntry.effects.CCEffects.Count == 0)
                {
                    value = 0;
                    return value;
                }
                break;
            default:
                value = 0;
                return value;
                // 새로운 효과 타입이 추가되면 여기에 case 추가
        }

        // 효과 타입에 따라 해당 리스트에서 index번째 효과를 가져옴
        object effect = null;
        switch (effectType)
        {
            case HitEffects.DamageEffect:
                effect = targetEntry.effects.DamageEffect[index];
                break;
            case HitEffects.BuffEffects:
                effect = targetEntry.effects.BuffEffects[index];
                break;
            case HitEffects.DebuffEffects:
                effect = targetEntry.effects.DebuffEffects[index];
                break;
            case HitEffects.CCEffects:
                effect = targetEntry.effects.CCEffects[index];
                break;
        }

        int baseDmg = 0;
        UDictionary<IncreaseType, float> increaseDict = null;

        // 각 효과 타입별로 동일한 계산 로직 적용
        if (effectType == HitEffects.DamageEffect && effect is DamageEffect dmgEffect)
        {
            // DamageEffect의 baseValue와 increase 사용
            baseDmg = Mathf.RoundToInt(dmgEffect.baseValue);
            increaseDict = dmgEffect.increase;
        }
        else if (effectType == HitEffects.BuffEffects && effect is BuffEffect buffEffect)
        {
            // BuffEffect의 baseValue와 increase 사용
            baseDmg = Mathf.RoundToInt(buffEffect.baseValue);
            increaseDict = buffEffect.increase;
        }
        else if (effectType == HitEffects.DebuffEffects && effect is DebuffEffect debuffEffect)
        {
            // DebuffEffect의 baseValue와 increase 사용
            baseDmg = Mathf.RoundToInt(debuffEffect.baseValue);
            increaseDict = debuffEffect.increase;
        }
        else if (effectType == HitEffects.CCEffects && effect is CCEffect ccEffect)
        {
            // CCEffect의 baseValue와 increase 사용
            baseDmg = Mathf.RoundToInt(ccEffect.baseValue);
            increaseDict = ccEffect.increase;
        }

        valueDefault += baseDmg;

        // increase 딕셔너리 값에 따라 추가 계산
        if (increaseDict != null)
        {
            // ad 비율 적용
            if (increaseDict.TryGetValue(IncreaseType.ad, out float adRatio))
                valueAdd += Mathf.RoundToInt(caster.atk * adRatio);

            // ap 비율 적용
            if (increaseDict.TryGetValue(IncreaseType.ap, out float apRatio))
                valueAdd += Mathf.RoundToInt(caster.atk * apRatio);

            // hp 비율 적용
            if (increaseDict.TryGetValue(IncreaseType.hp, out float hpRatio))
                valueAdd += Mathf.RoundToInt(caster.maxhp * hpRatio);
        }

        // 최종 값 반환
        value = valueDefault + valueAdd;
        return value;

        /*        int baseDmg = 0;
                if (effectType == HitEffects.DamageEffect)
                {

                }
                else if(effectType == HitEffects.BuffEffects)
                {

                }
                else if (effectType == HitEffects.DebuffEffects)
                {

                }
                else if (effectType == HitEffects.CCEffects)
                {

                }

                // index번째 DamageEffect의 데미지에 적용
                var dmgEffect = targetEntry.effects.DamageEffect[index];
                baseDmg = Mathf.RoundToInt(dmgEffect.baseValue);
                valueDefault += baseDmg;
                var increaseDict = dmgEffect.increase;

                if (increaseDict != null)
                {
                    if (increaseDict.TryGetValue(IncreaseType.ad, out float adRatio))
                        valueAdd += Mathf.RoundToInt(caster.atk * adRatio);

                    if (increaseDict.TryGetValue(IncreaseType.ap, out float apRatio))
                        valueAdd += Mathf.RoundToInt(caster.atk * apRatio);

                    if (increaseDict.TryGetValue(IncreaseType.hp, out float hpRatio))
                        valueAdd += Mathf.RoundToInt(caster.maxhp * hpRatio);
                }

                value = valueDefault + valueAdd;
                return value;*/
    }

    public void TargetOnHit(GameObject targetObj, GameObject selfObj, SkillData skillData, Target target)
    {
        var manager = CharacterStats.Instance;

        var targetStats = manager.GetStats(targetObj);
        var casterStats = manager.GetStats(selfObj);

        if (IsTargetType(targetStats, casterStats, skillData, target, 0))
        {
/*            //조건부
            if (!IsEffectConditionNull(skillData, 0))
            {
                if (IsEffectCondition(targetStats, casterStats, skillData, 0))
                    return;
            }
*/
            int damageValue = ValueCalculation(HitEffects.DamageEffect, target, casterStats, skillData, 0);
            if (HasSkillHitEffect_Damge_ByTarget(skillData, target, 0, SkillhitEffect.damage))
            {
                int finalDamage = DamageFormula(damageValue, targetStats, casterStats, skillData);
                int targetShlields = targetStats.shields; // 현재 대상의 보호막 값
                if (targetShlields > 0)
                {

                    targetShlields = targetShlields >= finalDamage ? targetShlields - finalDamage : 0;

                    int damageExceeded = targetShlields < finalDamage ? -(targetShlields - finalDamage) : 0;
                    targetStats.hp -= -damageExceeded;
                    DamageText.Instance.ShowDamage(targetObj.transform.position + Vector3.up * 1.5f, finalDamage - damageExceeded, false);
                    if (damageExceeded > 0) { DamageText.Instance.ShowDamage(targetObj.transform.position + Vector3.up * 2f, damageExceeded, false); }
                    
                    Debug.Log($"[Hit] {targetStats.name}이(가) {casterStats.name}에게 {damageExceeded} 피해를 입음. 남은 HP: {targetStats.hp}");

                }
                else
                {
                    targetStats.hp -= DamageFormula(damageValue, targetStats, casterStats, skillData)

;
                    DamageText.Instance.ShowDamage(targetObj.transform.position + Vector3.up * 1.5f, damageValue, true);
                    Debug.Log($"[Hit] {targetStats.name}이(가) {casterStats.name}에게 {damageValue} 피해를 입음. 남은 HP: {targetStats.hp}");
                }
                
            }
            if (HasSkillHitEffect_Damge_ByTarget(skillData, target, 0, SkillhitEffect.heal))
            {
                targetStats.hp += damageValue;
                DamageText.Instance.ShowDamage(targetObj.transform.position + Vector3.up * 1.5f, damageValue, true);
                Debug.Log($"[Hit] {targetStats.name}이(가) {casterStats.name}에게 {damageValue} 회복을 함. 남은 HP: {targetStats.hp}");
            }
            if (HasSkillHitEffect_Damge_ByTarget(skillData, target, 0, SkillhitEffect.Shields))
            {
                targetStats.shields += damageValue;
                DamageText.Instance.ShowDamage(targetObj.transform.position + Vector3.up * 1.5f, damageValue, true);
                Debug.Log($"[Hit] {targetStats.name}이(가) {casterStats.name}에게 {damageValue} 보호박을 획득함. 남은 HP: {targetStats.hp}");
            }

            int buffValue = ValueCalculation(HitEffects.BuffEffects, target, casterStats, skillData, 0);
            int buffValue_Time = Time_ValueCalculation(HitEffects.BuffEffects, target, casterStats, skillData, 0);
            if (HasSkillHitEffect_Buff_ByTarget(skillData, target, 0, Buffs.solid))
            {
                // 버프 효과 적용
                BuffEffect buffEffect = skillData.hitEffects.FirstOrDefault(h => h.target == target).effects.BuffEffects[0];
                if (buffEffect != null)
                {
                    targetStats.buffEffects.Add(new Buffa
                    {
                        effect = buffEffect.Buff,
                        Value = buffValue,
                        trun = buffValue_Time
                    });
                    Debug.Log($"[Hit] {targetStats.name}이(가) {casterStats.name}에게 {buffValue} 버프를 획득함. 남은 턴: {buffValue_Time}");
                }
            }

            int ccValue = ValueCalculation(HitEffects.CCEffects, target, casterStats, skillData, 0);
            int ccValue_Time = Time_ValueCalculation(HitEffects.CCEffects, target, casterStats, skillData, 0);
            if (HasSkillHitEffect_CC_ByTarget(skillData, target, 0, CCs.stun))
            {
                // 버프 효과 적용
                CCEffect CCEffect = skillData.hitEffects.FirstOrDefault(h => h.target == target).effects.CCEffects[0];
                if (CCEffect != null)
                {
                    targetStats.ccEffects.Add(new CC
                    {
                        effect = CCEffect.CC,
                        Value = ccValue,
                        trun = ccValue_Time
                    });
                    Debug.Log($"[Hit] {targetStats.name}이(가) {casterStats.name}에게 {ccValue} 기절를 획득함. 남은 턴: {ccValue_Time}");
                }



            }
        }
    
    }

    public enum HitEffects
    {
        DamageEffect,
        BuffEffects,
        DebuffEffects,
        CCEffects
    }
    public float GetDamageBaseValueAt(HitEffects effectType, SkillData skillData, Target target, int index)
    {
        // 1. 해당 target에 맞는 hitEffect 엔트리 찾기
        var entry = skillData.hitEffects.FirstOrDefault(h => h.target == target);
        if (entry == null) return 0f; // 없으면 0 반환

        // 2. 효과 타입에 따라 분기
        switch (effectType)
        {
            case HitEffects.DamageEffect:
                // DamageEffect 리스트에서 index번째 baseValue 반환
                if (entry.effects.DamageEffect != null && entry.effects.DamageEffect.Count > index && index >= 0)
                    return entry.effects.DamageEffect[index].baseValue;
                break;
            case HitEffects.BuffEffects:
                // BuffEffects 리스트에서 index번째 baseValue 반환
                if (entry.effects.BuffEffects != null && entry.effects.BuffEffects.Count > index && index >= 0)
                    return entry.effects.BuffEffects[index].baseValue;
                break;
            case HitEffects.DebuffEffects:
                // DebuffEffects 리스트에서 index번째 baseValue 반환
                if (entry.effects.DebuffEffects != null && entry.effects.DebuffEffects.Count > index && index >= 0)
                    return entry.effects.DebuffEffects[index].baseValue;
                break;
            case HitEffects.CCEffects:
                // CCEffects 리스트에서 index번째 baseValue 반환
                if (entry.effects.CCEffects != null && entry.effects.CCEffects.Count > index && index >= 0)
                    return entry.effects.CCEffects[index].baseValue;
                break;
                // 새로운 효과 타입이 추가되면 여기에 case 추가
        }
        // 조건에 맞는 값이 없으면 0 반환
        return 0f;
    }
    public bool IsTargetType(Stats targetStats, Stats casterStats, SkillData skillData, Target target, int hitEffectEntryIndex)
    {
        if (skillData.hitEffects == null ||
            hitEffectEntryIndex < 0 ||
            hitEffectEntryIndex >= skillData.hitEffects.Count)
            return false;

        if (target == Target.self)
        {
            return skillData.hitEffects[hitEffectEntryIndex].target == target && targetStats == casterStats;
        }
        else if (target == Target.team)
        {
            return skillData.hitEffects[hitEffectEntryIndex].target == target && targetStats.team == casterStats.team && targetStats != casterStats;
        }
        else if (target == Target.enemy)
        {
            return skillData.hitEffects[hitEffectEntryIndex].target == target && targetStats.team != casterStats.team;
        }
        else
        {
            return false;
        }
    }

    //데미지 유형 찾기
    public bool HasSkillHitEffect_Damge_ByTarget(SkillData skillData, Target target, int damageEffectIndex, SkillhitEffect effectType)
    {
        // 1. target에 맞는 HitEffectEntry 찾기
        var entry = skillData.hitEffects.FirstOrDefault(h => h.target == target);

        // 2. entry와 DamageEffect 리스트, 인덱스 유효성 검사
        if (entry != null &&
            entry.effects.DamageEffect != null &&
            damageEffectIndex >= 0 &&
            damageEffectIndex < entry.effects.DamageEffect.Count)
        {
            // 3. 해당 인덱스의 skillhitEffect가 원하는 값과 같은지 확인
            return entry.effects.DamageEffect[damageEffectIndex].skillhitEffect == effectType;
        }

        // 4. 없으면 false 반환
        return false;
    }

    //디버프 유형 찾기
    public bool HasSkillHitEffect_Debuff_ByTarget(SkillData skillData, Target target, int debuffEffectIndex, Debuffs effectType)
    {
        // 1. target에 맞는 HitEffectEntry 찾기
        var entry = skillData.hitEffects.FirstOrDefault(h => h.target == target);

        // 2. entry와 DamageEffect 리스트, 인덱스 유효성 검사
        if (entry != null &&
            entry.effects.DebuffEffects != null &&
            debuffEffectIndex >= 0 &&
            debuffEffectIndex < entry.effects.DebuffEffects.Count)
        {
            // 3. 해당 인덱스의 skillhitEffect가 원하는 값과 같은지 확인
            return entry.effects.DebuffEffects[debuffEffectIndex].Debuff == effectType;
        }

        // 4. 없으면 false 반환
        return false;
    }

    //버프 유형 찾기
    public bool HasSkillHitEffect_Buff_ByTarget(SkillData skillData, Target target, int buffEffectIndex, Buffs effectType)
    {
        // 1. target에 맞는 HitEffectEntry 찾기
        var entry = skillData.hitEffects.FirstOrDefault(h => h.target == target);

        // 2. entry와 DamageEffect 리스트, 인덱스 유효성 검사
        if (entry != null &&
            entry.effects.BuffEffects != null &&
            buffEffectIndex >= 0 &&
            buffEffectIndex < entry.effects.BuffEffects.Count)
        {
            // 3. 해당 인덱스의 skillhitEffect가 원하는 값과 같은지 확인
            return entry.effects.BuffEffects[buffEffectIndex].Buff == effectType;
        }

        // 4. 없으면 false 반환
        return false;
    }

    public bool HasSkillHitEffect_CC_ByTarget(SkillData skillData, Target target, int CCEffectIndex, CCs effectType)
    {
        // 1. target에 맞는 HitEffectEntry 찾기
        var entry = skillData.hitEffects.FirstOrDefault(h => h.target == target);

        // 2. entry와 DamageEffect 리스트, 인덱스 유효성 검사
        if (entry != null &&
            entry.effects.CCEffects != null &&
            CCEffectIndex >= 0 &&
            CCEffectIndex < entry.effects.CCEffects.Count)
        {
            // 3. 해당 인덱스의 skillhitEffect가 원하는 값과 같은지 확인
            return entry.effects.CCEffects[CCEffectIndex].CC == effectType;
        }

        // 4. 없으면 false 반환
        return false;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //데미지 외

    public float Time_GetEffectBaseValueAt(HitEffects effectType, SkillData skillData, Target target, int index)
    {
        // 1. 해당 target에 맞는 hitEffect 엔트리 찾기
        var entry = skillData.hitEffects.FirstOrDefault(h => h.target == target);
        if (entry == null) return 0f; // 없으면 0 반환

        // 2. 효과 타입에 따라 분기
        switch (effectType)
        {
            case HitEffects.DamageEffect:
                Debug.LogWarning("TumeGetEffectBaseValueAt: DamageEffect는 시간 계산에 사용되지 않습니다.");
                break;
            case HitEffects.BuffEffects:
                // BuffEffects 리스트에서 index번째 trunDuration 반환
                if (entry.effects.BuffEffects != null && entry.effects.BuffEffects.Count > index && index >= 0)
                    return entry.effects.BuffEffects[index].trunDuration;
                break;
            case HitEffects.DebuffEffects:
                // DebuffEffects 리스트에서 index번째 trunDuration 반환
                if (entry.effects.DebuffEffects != null && entry.effects.DebuffEffects.Count > index && index >= 0)
                    return entry.effects.DebuffEffects[index].trunDuration;
                break;
            case HitEffects.CCEffects:
                // CCEffects 리스트에서 index번째 trunDuration 반환
                if (entry.effects.CCEffects != null && entry.effects.CCEffects.Count > index && index >= 0)
                    return entry.effects.CCEffects[index].trunDuration;
                break;
                // 새로운 효과 타입이 추가되면 여기에 case 추가
        }
        // 조건에 맞는 값이 없으면 0 반환
        return 0f;
    }
    public int Time_ValueCalculation(HitEffects effectType, Target target, Stats caster, SkillData skillData, int index)
    {

        int value = Mathf.RoundToInt(Time_GetEffectBaseValueAt(effectType, skillData, target, index));

        int valueAdd = 0;
        int valueDefault = 0;

        // 지정된 대상(target)에 맞는 HitEffectEntry 찾기
        var targetEntry = skillData.hitEffects.FirstOrDefault(h => h.target == target);


        // 효과 타입에 따라 분기
        switch (effectType)
        {
            case HitEffects.BuffEffects:
                // 없거나 효과가 없다면 종료
                if (targetEntry == null || targetEntry.effects.BuffEffects.Count == 0)
                {
                    value = 0;
                    return value;
                };
                break;
            case HitEffects.DebuffEffects:
                // 없거나 효과가 없다면 종료
                if (targetEntry == null || targetEntry.effects.DebuffEffects.Count == 0)
                {
                    value = 0;
                    return value;
                }
                break;
            case HitEffects.CCEffects:
                // 없거나 효과가 없다면 종료
                if (targetEntry == null || targetEntry.effects.CCEffects.Count == 0)
                {
                    value = 0;
                    return value;
                }
                break;
            default:
                value = 0;
                return value;
                // 새로운 효과 타입이 추가되면 여기에 case 추가
        }

        // 효과 타입에 따라 해당 리스트에서 index번째 효과를 가져옴
        object effect = null;
        switch (effectType)
        {
            case HitEffects.BuffEffects:
                effect = targetEntry.effects.BuffEffects[index];
                break;
            case HitEffects.DebuffEffects:
                effect = targetEntry.effects.DebuffEffects[index];
                break;
            case HitEffects.CCEffects:
                effect = targetEntry.effects.CCEffects[index];
                break;
        }

        int baseDmg = 0;
        UDictionary<IncreaseType, float> increaseDict = null;
        
        if (effectType == HitEffects.BuffEffects && effect is BuffEffect buffEffect)
        {
            // BuffEffect의 baseValue와 increase 사용
            baseDmg = Mathf.RoundToInt(buffEffect.baseValue);
            increaseDict = buffEffect.timeIncrease;
        }
        else if (effectType == HitEffects.DebuffEffects && effect is DebuffEffect debuffEffect)
        {
            // DebuffEffect의 baseValue와 increase 사용
            baseDmg = Mathf.RoundToInt(debuffEffect.baseValue);
            increaseDict = debuffEffect.timeIncrease;
        }
        else if (effectType == HitEffects.CCEffects && effect is CCEffect ccEffect)
        {
            // CCEffect의 baseValue와 increase 사용
            baseDmg = Mathf.RoundToInt(ccEffect.baseValue);
            increaseDict = ccEffect.timeIncrease;
        }

        valueDefault += baseDmg;

        // increase 딕셔너리 값에 따라 추가 계산
        if (increaseDict != null)
        {
            // ad 비율 적용
            if (increaseDict.TryGetValue(IncreaseType.ad, out float adRatio))
                valueAdd += Mathf.RoundToInt(caster.atk * adRatio);

            // ap 비율 적용
            if (increaseDict.TryGetValue(IncreaseType.ap, out float apRatio))
                valueAdd += Mathf.RoundToInt(caster.atk * apRatio);

            // hp 비율 적용
            if (increaseDict.TryGetValue(IncreaseType.hp, out float hpRatio))
                valueAdd += Mathf.RoundToInt(caster.maxhp * hpRatio);
        }

        // 최종 값 반환
        value = valueDefault + valueAdd;
        return value;
    }


    /// <summary>
    /// 데미지 계산 공식
    /// </summary>
    /// <param name="value">데미지</param>
    /// <param name="target">피격대상</param>
    /// <param name="caster">공격자</param>
    /// <param name="Skill">스킬</param>
    /// <returns></returns>
    public int DamageFormula(int value, Stats target, Stats caster, SkillData Skill)
    {
        int finalDamage = Mathf.RoundToInt((value*(1 - (target.damageReduction - caster.damageIncreased)) - target.def));
        if (finalDamage <= 0)
        {
            finalDamage = 1; // 0이하의 데미지는 1로 처리
        }
        return finalDamage; // 임시로 0 반환
    }



    /////////////////////////////////////////////////////

/*    public bool IsEffectCondition(Stats targetStats, Stats casterStats, SkillData skillData, int hitEffectEntryIndex)
    {
        Target target = skillData.hitEffects[hitEffectEntryIndex].effects.DamageEffect[0].effectCondition.target;
        AttributeType type = skillData.hitEffects[hitEffectEntryIndex].effects.DamageEffect[0].effectCondition.type;
        Condition_statement comparison = skillData.hitEffects[hitEffectEntryIndex].effects.DamageEffect[0].effectCondition.comparison;
        float value = skillData.hitEffects[hitEffectEntryIndex].effects.DamageEffect[0].effectCondition.value;

        var cond = ConditionBuilder
            .Attribute(target, type, comparison, value)
            .Build();

        return cond(casterStats, targetStats, Team.team);
    }

    public bool IsEffectConditionNull(SkillData skillData, int hitEffectEntryIndex)
    {
        bool isnull = !skillData.hitEffects[hitEffectEntryIndex].effects.DamageEffect[0].effectCondition.isactive ? true : false;

        return isnull;
    }*/
}
