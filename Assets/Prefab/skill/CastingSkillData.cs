using UnityEngine;

public class CastingSkillData : MonoBehaviour
{
    public SkillData skillData; // SkillData 참조 필드 추가
    public Vector3 targetPos;
    public GameObject characterUnit; // 캐릭터 유닛 참조 필드 추가
    public Stats characterStats; // 캐릭터 스탯 참조 필드 추가
    public GameObject targetUnit; // 타겟 유닛 참조 필드 추가

    public void SetSkillData(SkillData skill, Vector3 targetPos, GameObject charcter,
        Stats character, GameObject target = null)
    {
        skillData = skill;
        this.targetPos = targetPos;
        characterUnit = charcter;
        characterStats = character;
        targetUnit = target;
        // 스킬 데이터에 따라 필요한 초기화 작업 수행

    }
}
