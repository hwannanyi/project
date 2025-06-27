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

        // 모든 중간 객체에 대해 null 체크 추가
        if (SkillSave.Instance == null ||
            SkillSave.Instance.Skillaction == null ||
            SkillSave.Instance.Skillaction.skillData == null ||
            SkillSave.Instance.Skillaction.skillData.selectedTargetUnit == null)
        {
            outline.outlineSize = 0;
            return;
        }

        if (characterForm.parentObject == SkillSave.Instance.Skillaction.skillData.selectedTargetUnit)
        {
            outline.outlineSize = 2;
        }
        else
        {
            outline.outlineSize = 0;
        }
    }
}
