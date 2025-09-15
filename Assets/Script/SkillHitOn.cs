using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;

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

    private void OnTriggerEnter()
    {

    }


    public void ColliderHitOn(Collider other)
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

            Stats targetStats = manager.GetStats(target);
            Stats casterStats = manager.GetStats(self);

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

            // 패링 성공시 파괴
            if (skillData.parryingT && targetStats.isparrying)
            {
                SkillManager.Instance.isCastingSkill = false;
                Destroy(gameObject);
            }

            if(HasAllStatuses(targetStats, skillData.status, skillData.statusNot))
                return;

            skillHitEffects.TargetOnHit(target, self, skillData, Target.self);
            skillHitEffects.TargetOnHit(target, self, skillData, Target.enemy);
            skillHitEffects.TargetOnHit(target, self, skillData, Target.team);

            //상태적용
            if (skillData.statusApply != null)
            {
                for (int i = 0; i < skillData.statusApply.Count; i++)
                {
                    targetStats.AddStatus(skillData.statusApply[i]);
                }
            }

            //상태적용
            if (skillData.statusEffects != null)
            {
                for (int i = 0; i < skillData.statusEffects.Count; i++)
                {
                    var src = skillData.statusEffects[i];
                    if (src == null) continue;

                    // (선택) 이미 같은 status가 있으면 중복 방지하려면 아래 if 활성화
                    // if (targetStats.statusEffects.Any(se => se.status == src.status)) continue;

                    var clone = src.Clone();              // 깊은 복사
                    targetStats.statusEffects.Add(clone); // 원본과 분리된 객체
                    targetStats.AddStatus(clone.status);  // StatusType 플래그 추가(필요하다면)
                }
            }


            /*            if (casterStats.team == Team.enemy && targetStats.team == Team.team)
                        {
                            // 적중 시 홀드 게이지 증가
                            foreach (var effect in skillData.holdHit)
                            {
                                targetStats.holdGauge.Add(new HoldEffect
                                {
                                    holdGauge = effect.holdGauge,
                                    effect = effect.effect,
                                    value = effect.value,
                                    tic = effect.tic,
                                    curtic = effect.curtic
                                });

                                WorldHoldBar.Create(
                                    CharacterStats.Instance.HoldBar,
                                    target.transform,
                                    CharacterStats.Instance.canvas,
                                    targetStats,
                                    targetStats.holdGauge.Count - 1);
                            }

                            // 적중 시 연타 게이지 증가
                            foreach (var effect in skillData.keyMashingHit)
                            {
                                targetStats.keyMashing.Add(new MashingEffect
                                {
                                    keyMashingCount = effect.keyMashingCount,
                                    effect = effect.effect,
                                    value = effect.value,
                                    time = effect.time
                                });
                            }
                        }*/
            //OnHit(target, skillData);
            // SkillHitOn.cs에서 적중 시
            targetStats.lastHitSkillData = skillData; // 마지막 적중 스킬 데이터 저장
            CheckDeathOnly(target);
            PassiveSkillCast.Instance.OnHitPassive(targetStats, casterStats, skillData);

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

    // 여러 상태를 모두 가지고 있는지 확인
    public bool HasAllStatuses(Stats target, IList<StatusType> list, bool Not)
    {
        bool istrue = Not ? true : false;
        if (list == null) return true;
        foreach (var st in list)
        {
            if (st == StatusType.none) continue;
            if (!target.statuses.Contains(st)) return istrue;
        }
        istrue = !istrue;
        return istrue;
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

