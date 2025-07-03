using System.Linq;
using UnityEngine;

public class CharacterHighlight : MonoBehaviour
{
    public CharacterForm characterForm;
    public SpriteOutline outline;
    public int outlineSize;

    void Start()
    {
        // 부모오브젝트 가져오기
        characterForm = GetComponent<CharacterForm>();
        outline = GetComponent<SpriteOutline>();

    }

    public void Update()
    {
        if (outline == null || characterForm == null)
            return;

        // skillData 리스트에 selectedTargetUnit이 characterForm와 일치하는 게 없으면 true
        bool isNotSelected = SkillSave.Instance != null &&
                     SkillSave.Instance.Skillaction != null &&
                     SkillSave.Instance.Skillaction.skillData != null &&
                     !SkillSave.Instance.Skillaction.skillData
                        .Any(s => s.selectedTargetUnit == characterForm.parentObject);

        bool isSelected = SkillSave.Instance != null &&
             SkillSave.Instance.Skillaction != null &&
             SkillSave.Instance.Skillaction.skillData != null &&
             SkillSave.Instance.Skillaction.skillData
                .Any(s => s.selectedTargetUnit == characterForm.parentObject);

        // 모든 중간 객체에 대해 null 체크 추가
        if (isNotSelected)
        {
            outline.outlineSize = 0;
            return;
        }

        if (isSelected)
        {
            outline.outlineSize = 2;
        }
        else
        {
            outline.outlineSize = 0;
        }
    }
}
