using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.TextCore.Text;

public class SkillCastSkill : MonoBehaviour
{
    public CastingSkillData CastSkillData; // 스킬 데이터 참조 필드 추가

    public SkillData skillData; // SkillData 참조 필드 추가
    public Vector3 targetPos;
    public GameObject caster; // 캐릭터 유닛 참조 필드 추가
    public Stats casterStats; // 캐릭터 스탯 참조 필드 추가
    public GameObject targetUnit; // 타겟 유닛 참조 필드 추가

    void Awake()
    {
        CastSkillData = GetComponent<CastingSkillData>();
    }

    void OnDestroy()
    {
/*        skillData = CastSkillData.skillData;
        targetPos = CastSkillData.targetPos;
        caster = CastSkillData.characterUnit;
        casterStats = CastSkillData.characterStats;
        targetUnit = CastSkillData.targetUnit;*/
        StartSkill(false, CastSkillData.characterStats, CastSkillData.skillData);
    }

    // 유닛 충돌 이벤트에서 호출
    public void StartSkill(bool StartTiming, Stats self, SkillData skillData)
    {

        Debug.Log("연계스킬 실행 시도");
        /*        for (int skillnumber = 0; skillnumber < self.passiveSkill.Count; skillnumber++) // 패시브 스킬이 있는지 확인
                {
        */
        var AddSkills = StartTiming ? skillData.StartAddSkills : skillData.EndAddSkills;

        if (AddSkills == null)
        {
            Debug.Log("연계스킬이 없음");
            return; }
        

        SkillData Skill = AddSkills.skill != null ? AddSkills.skill : null;
        
        Condition condition = AddSkills.condition != null ? AddSkills.condition : null;
        ConditionHit conditionHit = AddSkills.conditionHit != null ? AddSkills.conditionHit : null;
        SkillAutoCast targetrule = AddSkills.targetrule != null ? AddSkills.targetrule : null;
        /*
                    bool asd = HitCondition(target, self, skillData, conditionHit.target, conditionHit.type,
                        conditionHit.comparison, conditionHit.value);
                    Debug.Log(asd);*/
        /*            if (conditionHit.isactive && HitCondition(target, self, skillData, conditionHit.target, conditionHit.type,
                        conditionHit.comparison, conditionHit.value))// 불러올 패시브 스킬이 맞으면 실행
                    {
        */

        if (Skill == null)
        {
            Debug.Log("연계스킬를 못찾음");
            return;
        }
        try
        {
            PassiveSkillCast.Instance.AutoCast(targetrule, self, Skill, gameObject);
        }
        catch
        {
            Debug.LogError("연계스킬 실행 실패: " + Skill.skillName);
        }
        
            //}
        //}
    }

    public bool HitCondition(Stats self, Stats target2, SkillData skillData,
    Target target, TargetUnit targetUnit, Condition_Hit condition_Hit, string name)
    {
        // 조건
        var cond = ConditionBuilder.HitCondition(
            target,
            targetUnit,
            condition_Hit,
        name
        )
        .Build();

        return cond(self, target2, skillData, Team.team);
    }

    public bool IsEffectCondition(Stats targetStats, Stats casterStats, SkillData skillData,
    Target target, AttributeType type, Condition_statement comparison, float value)
    {
        var cond = ConditionBuilder
            .Attribute(target, type, comparison, value)
            .Build();

        return cond(casterStats, targetStats, skillData, Team.team);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
