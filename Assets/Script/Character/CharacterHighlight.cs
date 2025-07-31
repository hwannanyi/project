using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Unity.VisualScripting;


public class CharacterHighlight : MonoBehaviour
{
    // 이 스크립트는 캐릭터의 하이라이트를 관리합니다.
    public CharacterForm characterForm;
    public SpriteOutline outline;
    public int outlineSize;

    public GameObject grandParentObj;
    private Action Mark;
    public GameObject EffectMarkPf;

    public Dictionary<string, SpriteRenderer> spriteRenderer = new();
    public Sprite sprite;


    private Dictionary<string, GameObject> EffectMark;
    void Awake()
    {
        characterForm = GetComponent<CharacterForm>();
        outline = GetComponent<SpriteOutline>();
    }
    public void Update()
    {
        if (outline == null || characterForm == null)
            return;

        // SkillSave.Instance와 Skillaction 리스트가 null이거나 비어있는지 체크
        if (SkillSave.Instance == null ||
            SkillSave.Instance.TeamSkill == null ||
            SkillSave.Instance.TeamSkill.Count == 0)
        {
            outline.outlineSize = 0;
            return;
        }

        // Skillaction 리스트 중 selectedTargetUnit이 characterForm.parentObject와 일치하는지 검사
        bool isTarget = SkillSave.Instance.TeamSkill.Values
            .Any(action => action.skillData != null &&
                           action.skillData.selectedTargetUnit == characterForm.parentObject);

        outline.outlineSize = isTarget ? 2 : 0;

        var manager = CharacterStats.Instance;
        var character = manager.GetStats(grandParentObj);


    }

    public void SetHighlight(Stats character, string markName)
    {
            Mark = IsGurd(character) ?
            () =>
            {
                bool exists = EffectMark.ContainsKey(markName);
                Action createMark = () =>
                {
                    EffectMark.Add(markName, Instantiate(EffectMarkPf, transform));
                    SpriteRenderer effectMarkSp = EffectMark[markName].GetComponent<SpriteRenderer>();
                    effectMarkSp.sprite = sprite;
                };
                // exists가 false일 때만 createMark 실행
                (exists ? (Action)(() => { }) : createMark)();
            }
                : () =>
                {
                    bool exists = EffectMark.ContainsKey(markName);
                    Action destroyeMark = () =>
                    {
                        Destroy(EffectMark[markName]);
                        EffectMark.Remove(markName);
                    };
                    (exists ? destroyeMark : (Action)(() => { }))();
                };
    }

    public bool IsGurd(Stats ch)
    {
        return ch.gurd > 0;
    }
}
