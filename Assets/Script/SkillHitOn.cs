using UnityEngine;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using UnityEngine.TextCore.Text;
using System;

public class SkillHitOn : MonoBehaviour
{
    private SkillData skillData;
    public GameObject casterObj;
    private Stats caster;
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
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    private void IgnoreCasterCollision()
    {
        var skillCollider = GetComponent<Collider2D>();
        var casterCollider = casterObj.GetComponent<Collider2D>();

        if (skillCollider != null && casterCollider != null)
        {
            Physics2D.IgnoreCollision(skillCollider, casterCollider, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag=="Character")
        {
            var manager = CharacterStats.Instance;
            if (manager == null)
            {
                Debug.LogWarning("CharacterStats 매니저 인스턴스가 없습니다.");
                return;
            }

            var target = other.transform.root.gameObject;
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
            if (skillData.skillTarget.Contains(Target.enemy))
            {
                if (targetStats.team != casterStats.team)
                {
                    int value = Mathf.RoundToInt(skillData.basicValue);
                    ValueCalculation(ref value);
                    targetStats.hp -= value;
                    Debug.Log($"[Hit] {targetStats.name}이(가) {value} 피해를 입음. 남은 HP: {targetStats.hp}");
                }
            }
        }

        if(other.gameObject.tag == "Tile")
        {
            Debug.Log("타일에 충돌!");
        }

        if (other.gameObject.tag == "skill")
        {
            Debug.Log("스킬에 충돌!");
        }
    }


    //최종위력 계산기
    public void ValueCalculation(ref int FinalDamage)
    {
        int damageUp = 0; //증가치
        int damage = Mathf.RoundToInt(skillData.basicValue);//기본위력

        if (skillData.increase.ContainsKey(IncreaseType.none))
        {
            FinalDamage = damage + damageUp;
            return;
        }
        if (skillData.increase.ContainsKey(IncreaseType.ad))//AD
        {
            float AD = skillData.increase[IncreaseType.ad];
            damageUp = damageUp + Mathf.RoundToInt(caster.atk * AD);
        }
        if (skillData.increase.ContainsKey(IncreaseType.ap))//AP
        {
            float AP = skillData.increase[IncreaseType.ap];
            damageUp = damageUp + Mathf.RoundToInt(caster.atk * AP);
        }
        if (skillData.increase.ContainsKey(IncreaseType.hp))//AP
        {
            float HP = skillData.increase[IncreaseType.hp];
            damageUp = damageUp + Mathf.RoundToInt(caster.maxhp * HP);
        }
        FinalDamage = damage + damageUp;

    }
}

