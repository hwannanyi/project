using UnityEngine;

public class SkillHitOn : MonoBehaviour
{
    private Skill skillData;
    private GameObject caster; // 스킬을 쏜 주체

    public void Initialize(Skill skill, GameObject casterObject)
    {
        skillData = skill;
        caster = casterObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 사용자 본인과 충돌 무시
        if (other.gameObject == caster && caster != null)
        {
            //Debug.Log("사용자 본인과 충돌 - 무시");
            return;
        }

        // 다른 스킬과 충돌 무시
        if (other.GetComponent<SkillHitOn>() != null)
        {
            //Debug.Log("다른 스킬과 충돌 - 무시");
            return;
        }

        // 벽 타일에 충돌 시 삭제
        if (other.CompareTag("Tile"))
        {
            Destroy(gameObject);
            return;
        }

        // shot 타입 투사체인 경우 기본 충돌 처리
        if (skillData != null && skillData.projectileType.ToString() == "shot")
        {
            Destroy(gameObject);
        }
    }
}