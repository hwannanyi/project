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
        if (!isInitialized)
        {
            Debug.LogWarning("Initialize가 먼저 호출되지 않았습니다. 충돌 무시.");
            return;
        }

        Debug.Log($"[충돌 감지] {other.name}");

        if (skillData == null || caster == null)
        {
            Debug.LogWarning("Initialize 정보가 없습니다.");
            return;
        }

        if (other.gameObject.GetInstanceID() == caster.GetInstanceID())
        {
            Debug.Log("사용자 본인과 충돌 - 무시");
            return;
        }

        if (other.GetComponent<SkillHitOn>() != null)
        {
            Debug.Log("다른 스킬과 충돌 - 무시");
            return;
        }

        if (other.CompareTag("Tile"))
        {
            Destroy(gameObject);
            return;
        }

        var statsManager = CharacterStats.Instance;
        if (statsManager == null || statsManager.characters == null)
        {
            Debug.LogError("CharacterStats.Instance 또는 characters가 null입니다.");
            return;
        }

        int targetIndex = statsManager.characters.IndexOf(other.gameObject);
        int casterIndex = statsManager.characters.IndexOf(caster);

        if (targetIndex == -1 || casterIndex == -1)
        {
            Debug.LogWarning("충돌한 오브젝트 또는 caster가 CharacterStats에 없습니다.");
            return;
        }

        Stats targetStats = statsManager.characterList[targetIndex];
        Stats casterStats = statsManager.characterList[casterIndex];

        if (targetStats.team != casterStats.team)
        {
            int damage = Mathf.RoundToInt(skillData.basicValue);
            targetStats.hp -= damage;
            Debug.Log($"[{targetStats.name}]이(가) {damage} 피해를 입음. 남은 HP: {targetStats.hp}");
        }

        if (skillData.projectileType.ToString() == "shot")
        {
            Destroy(gameObject);
        }
    }
}
