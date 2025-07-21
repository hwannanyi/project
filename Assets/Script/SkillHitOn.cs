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
    //private bool isInitialized = false;

    public SkillHitEffects skillHitEffects;

    public void Initialize(SkillData skill, GameObject casterObject, Stats character)
    {
        skillData = skill;
        casterObj = casterObject;
        caster = character;
        //isInitialized = true;

        HitboxTile[] hitboxes = GetComponentsInChildren<HitboxTile>(true);
        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].EnableCollider();
        }

    }

    public void Awake()
    {
        skillHitEffects = GetComponent<SkillHitEffects>();
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

/*            if (target == self)
            {
                Debug.Log("[SkillHitOn] 자기 자신과 충돌 - 무시");
                return;
            }*/

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


            skillHitEffects.TargetOnHit(target, self, skillData, Target.self);
            skillHitEffects.TargetOnHit(target, self, skillData, Target.enemy);
            skillHitEffects.TargetOnHit(target, self, skillData, Target.team);
            //OnHit(target, skillData);
            // SkillHitOn.cs에서 적중 시
            targetStats.lastHitSkillData = skillData; // 마지막 적중 스킬 데이터 저장

            PassiveSkillCast.Instance.OnHitPassive(targetStats, casterStats , skillData);
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

