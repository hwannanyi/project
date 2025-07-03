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

        // SkillSave.Instance와 Skillaction 리스트가 null이거나 비어있는지 체크
        if (SkillSave.Instance == null ||
            SkillSave.Instance.Skillaction == null ||
            SkillSave.Instance.Skillaction.Count == 0)
        {
            outline.outlineSize = 0;
            return;
        }

        // Skillaction 리스트 중 selectedTargetUnit이 characterForm.parentObject와 일치하는지 검사
        bool isTarget = SkillSave.Instance.Skillaction
            .Any(action => action.skillData != null &&
                           action.skillData.selectedTargetUnit == characterForm.parentObject);

        outline.outlineSize = isTarget ? 2 : 0;
    }
}
