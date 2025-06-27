using UnityEngine;

public class SkilltargetUI : MonoBehaviour
{
    public SkillSave skillSave;

    public void Awake()
    {
        skillSave = SkillSave.Instance;
    }
    void Update()
    {
        if(skillSave.Skillaction == null || skillSave.Skillaction.skillData.selectedTargetUnit == null)
        {
            return; // 선택된 대상이 없으면 업데이트 중지
        }
        var target = skillSave.Skillaction.skillData.selectedTargetUnit;
        if (target != null)
        {
            transform.position = target.transform.position;
        }
    }
}