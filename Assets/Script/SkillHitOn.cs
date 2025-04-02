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
        if (caster == null || skillData == null)
        {
            Debug.LogWarning("[HitboxTile] caster 또는 skillData가 설정되지 않음");
            return;
        }

        GameObject target = other.transform.root.gameObject;      // 충돌한 오브젝트의 루트 (캐릭터 본체)
        GameObject self = caster.transform.root.gameObject;       // caster도 루트 기준으로 비교

        if (target == self)
        {
            Debug.Log("[HitboxTile] 자기 자신과 충돌 - 무시");
            return;
        }

        var statsManager = CharacterStats.Instance;
        if (statsManager == null || statsManager.characters == null)
        {
            Debug.LogError("[HitboxTile] CharacterStats 또는 characters 리스트가 null입니다.");
            return;
        }

        int targetIndex = statsManager.characters.IndexOf(target);
        int casterIndex = statsManager.characters.IndexOf(self);

        if (targetIndex == -1 || casterIndex == -1)
        {
            Debug.LogWarning("충돌한 오브젝트 또는 caster가 CharacterStats에 없습니다.");
            Debug.Log($"[target: {target.name}], [caster: {self.name}]");
            return;
        }

        Stats targetStats = statsManager.characterList[targetIndex];
        Stats casterStats = statsManager.characterList[casterIndex];

        if (targetStats.team != casterStats.team)
        {
            int damage = Mathf.RoundToInt(skillData.basicValue);
            targetStats.hp -= damage;
            Debug.Log($"[Hit] {targetStats.name}이(가) {damage} 피해를 입음. 남은 HP: {targetStats.hp}");
        }
        else
        {
            Debug.Log("[HitboxTile] 같은 팀 - 무시");
        }
    }
}
