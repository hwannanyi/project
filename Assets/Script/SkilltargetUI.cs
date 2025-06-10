using UnityEngine;

public class SkilltargetUI : MonoBehaviour
{
    void Update()
    {
        if(SkillSave.Instance.Skillaction == null || SkillSave.Instance.Skillaction.selectedTargetUnit == null)
        {
            return; // 선택된 대상이 없으면 업데이트 중지
        }
        var target = SkillSave.Instance.Skillaction.selectedTargetUnit;
        if (target != null)
        {
            transform.position = target.transform.position;
        }
    }
}