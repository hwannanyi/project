using UnityEngine;
using System.Collections;

public class SkillHitOn : MonoBehaviour
{
    private Skill skillData;
    private GameObject caster;
    private bool isInitialized = false;

    public void Initialize(Skill skill, GameObject casterObject)
    {
        skillData = skill;
        caster = casterObject;
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
        var casterCollider = caster.GetComponent<Collider2D>();

        if (skillCollider != null && casterCollider != null)
        {
            Physics2D.IgnoreCollision(skillCollider, casterCollider, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var statsManager = CharacterStats.Instance;

        var targetStats = other.GetComponentInParent<CharacterStats>();
        var casterStats = caster.GetComponentInParent<CharacterStats>();

        if (targetStats == null)
        {
            Debug.LogWarning($"[SkillHitOn] 충돌한 오브젝트 '{other.name}' 또는 부모에 CharacterStats가 없습니다.");
        }

        if (casterStats == null)
        {
            Debug.LogWarning($"[SkillHitOn] caster '{caster.name}' 또는 부모에 CharacterStats가 없습니다.");
        }

        if (targetStats == null || casterStats == null)
        {
            return;
        }

        if (targetStats == casterStats)
        {
            Debug.Log("[HitboxTile] 자기 자신과 충돌 - 무시");
            return;
        }

        if (!statsManager.characters.Contains(targetStats.gameObject))
        {
            Debug.LogWarning($"{targetStats.name} 이 CharacterStats.characters 리스트에 등록되지 않음");
            return;
        }
    
    /*if (targetStats.team != casterStats.team)
    {
        int damage = Mathf.RoundToInt(skillData.basicValue);
        targetStats.hp -= damage;
        Debug.Log($"[Hit] {targetStats.name}이(가) {damage} 피해를 입음. 남은 HP: {targetStats.hp}");
    }
    else
    {
        Debug.Log("[HitboxTile] 같은 팀 - 무시");
    }*/
    }
}
