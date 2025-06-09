using UnityEngine;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using UnityEngine.TextCore.Text;
using System;
using System.Collections.Generic;

public class SkillHitOn : MonoBehaviour
{
    private SkillData skillData;
    public GameObject casterObj;
    private Stats caster;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private bool isInitialized = false;

    public void Initialize(SkillData skill, GameObject casterObject, Stats character)
    {
        skillData = skill;
        casterObj = casterObject;
        caster = character;
        isInitialized = true;

        HitboxTile[] hitboxes = GetComponentsInChildren<HitboxTile>(true);
        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].EnableCollider();
        }

    }



    private IEnumerator EnableColliderNextFrame()
    {
        yield return new WaitForSeconds(0.05f); // 확실히 타이밍 확보
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    private void IgnoreCasterCollision()
    {
        var skillCollider = GetComponent<Collider>();
        var casterCollider = casterObj.GetComponent<Collider>();

        if (skillCollider != null && casterCollider != null)
        {
            Physics.IgnoreCollision(skillCollider, casterCollider, true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Character")
        {
            var target = other.transform.root.gameObject;

            // 이미 데미지를 준 대상이면 무시
            if (hitTargets.Contains(target))
                return;

            hitTargets.Add(target);

            var manager = CharacterStats.Instance;
            if (manager == null)
            {
                Debug.LogWarning("CharacterStats 매니저 인스턴스가 없습니다.");
                return;
            }


            var self = casterObj.transform.root.gameObject;

            if (target == self)
            {
                Debug.Log("[SkillHitOn] 자기 자신과 충돌 - 무시");
                return;
            }

            var targetStats = manager.GetStats(target);
            var casterStats = manager.GetStats(self);

            if (targetStats == null)
            {
                Debug.LogWarning($" '{target.name}' 은 Stats 정보 없음");
                return;
            }

            if (casterStats == null)
            {
                Debug.LogWarning($" caster '{self.name}' 은 Stats 정보 없음");
                return;
            }

            // 여기서 팀 비교 등 처리
            Debug.Log($"'{target.name}' 가 '{casterObj.name}' 의 스킬에 피격됨!");

            if (skillData.skillTarget.Contains(Target.self))
            {
                if (targetStats == casterStats)
                {
                    float baseValue = skillData.hitEffects
                    .FirstOrDefault(h => h.target == Target.self)?
                    .effects.DamageEffect.FirstOrDefault()?.baseValue ?? 0f;
                    int value = Mathf.RoundToInt(baseValue);
                    ValueCalculation(ref value, Target.self);
                    targetStats.hp += value;
                    Debug.Log($"[Hit] {targetStats.name}이(가) {value} 회복을 함. 남은 HP: {targetStats.hp}");
                }
            }
            if (skillData.skillTarget.Contains(Target.team))
            {
                if (targetStats.team == casterStats.team)
                {
                    float baseValue = skillData.hitEffects
                    .FirstOrDefault(h => h.target == Target.team)?
                    .effects.DamageEffect.FirstOrDefault()?.baseValue ?? 0f;
                    int value = Mathf.RoundToInt(baseValue);
                    ValueCalculation(ref value, Target.team);
                    targetStats.hp += value;
                    Debug.Log($"[Hit] {targetStats.name}이(가) {value} 회복을 함. 남은 HP: {targetStats.hp}");
                }
            }
            if (skillData.skillTarget.Contains(Target.enemy))
            {
                if (targetStats.team != casterStats.team)
                {
                    float baseValue = skillData.hitEffects
                    .FirstOrDefault(h => h.target == Target.enemy)?
                    .effects.DamageEffect.FirstOrDefault()?.baseValue ?? 0f;
                    int value = Mathf.RoundToInt(baseValue);
                    ValueCalculation(ref value, Target.enemy);
                    targetStats.hp -= value;
                    Debug.Log($"[Hit] {targetStats.name}이(가) {value} 피해를 입음. 남은 HP: {targetStats.hp}");
                    CheckDeathOnly(target);
                }
            }
            if (skillData.skillTarget.Contains(Target.spTarget))
            {
                if (1 == 2)
                {
                    float baseValue = skillData.hitEffects
                    .FirstOrDefault(h => h.target == Target.enemy)?
                    .effects.DamageEffect.FirstOrDefault()?.baseValue ?? 0f;
                    int value = Mathf.RoundToInt(baseValue);
                    ValueCalculation(ref value, Target.enemy);
                    targetStats.hp -= value;
                    Debug.Log($"[Hit] {targetStats.name}이(가) {value} 피해를 입음. 남은 HP: {targetStats.hp}");
                }
            }

        }

        if (other.gameObject.tag == "Tile")
        {
            Debug.Log("타일에 충돌!");
        }

        if (other.gameObject.tag == "skill")
        {
            Debug.Log("스킬에 충돌!");
        }
    }


    //최종위력 계산기
    public void ValueCalculation(ref int FinalDamage, Target target)
    {
        int damageUp = 0;
        int damage = 0;

        // 지정된 대상(target)에 맞는 HitEffectEntry 찾기
        var targetEntry = skillData.hitEffects.FirstOrDefault(h => h.target == target);

        // 없거나 데미지 효과가 없다면 종료
        if (targetEntry == null || targetEntry.effects.DamageEffect.Count == 0)
        {
            FinalDamage = 0;
            return;
        }

        foreach (var dmgEffect in targetEntry.effects.DamageEffect)
        {
            int baseDmg = Mathf.RoundToInt(dmgEffect.baseValue);
            damage += baseDmg;

            var increaseDict = dmgEffect.increase;

            if (increaseDict != null)
            {
                if (increaseDict.TryGetValue(IncreaseType.ad, out float adRatio))
                    damageUp += Mathf.RoundToInt(caster.atk * adRatio);

                if (increaseDict.TryGetValue(IncreaseType.ap, out float apRatio))
                    damageUp += Mathf.RoundToInt(caster.atk * apRatio);

                if (increaseDict.TryGetValue(IncreaseType.hp, out float hpRatio))
                    damageUp += Mathf.RoundToInt(caster.maxhp * hpRatio);
            }
        }

        FinalDamage = damage + damageUp;
    }

    public void CheckDeathOnly(GameObject targetObj)
    {
        // Stats를 CharacterStats 매니저에서 가져오기
        Stats targetStats = CharacterStats.Instance.GetStats(targetObj);
        if (targetStats != null && !targetStats.isdie && targetStats.hp <= 0)
        {
            Characterdeath death = targetObj.GetComponent<Characterdeath>();
            if (death != null)
            {
                death.CheckDeath(targetStats);
            }
        }
    }

    public void OnHit()
    {
        hitTargets.Clear();
    }
}

