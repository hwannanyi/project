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
    public GameObject EffectMarkPf;
    public CharStats character;

    public UDictionary<string, GameObject> EffectMark = new();
    private Dictionary<string, Func<bool>> Effect = new();

    public UDictionary<string, Sprite> sprite = new();

    public MoveAllow[] moveAllow = new MoveAllow[] { null, null, null, null };

   

    void Awake()
    {
        characterForm = GetComponent<CharacterForm>();
        outline = GetComponent<SpriteOutline>();

    }

    private void Start()
    {
        StartMakeList();
    }
    public void Update()
    {
        SetHighlightByKey("gurd"); //방어상태?
        SetHighlightByKey("hold"); // 방어(홀드) 상태?
        SetHighlightByKey("parrying"); // 패링 상태?

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
    }

    public void SetHighlight(KeyValuePair<string, Func<bool>> effect)
    {
        string markName = effect.Key;
        bool exists = EffectMark.ContainsKey(markName);
        if (effect.Value())
        {
            Action createMark = () =>
            {
                Debug.Log($"Create Mark: {markName}");
                EffectMark.Add(markName, Instantiate(EffectMarkPf, transform));
                SpriteRenderer effectMarkSp = EffectMark[markName].GetComponent<SpriteRenderer>();
                effectMarkSp.sprite = sprite[markName];
            };
            // exists가 false일 때만 createMark 실행
            (exists ? (Action)(() => { }) : createMark)();
        }
        else
        {
            Action destroyeMark = () =>
            {
                Debug.Log($"Delet Mark: {markName}");
                Destroy(EffectMark[markName]);
                EffectMark.Remove(markName);
            };
            (exists ? destroyeMark : (Action)(() => { }))();
        }
                
    }

    // 키값만 입력하면 자동으로 KeyValuePair를 만들어 SetHighlight에 전달하는 함수
    public void SetHighlightByKey(string key)
    {
        if (Effect.TryGetValue(key, out Func<bool> value))
        {
            SetHighlight(new(key, value)); // C# 9.0 이상에서 타입 생략 가능
        }
    }

    public void StartMakeList()// 이름, 참거짓 리스트(게임 실행도중 변경 없음_
    {
        Effect = new Dictionary<string, Func<bool>>
        {
            { "gurd", IsGurd },
            // 추가 명령어 및 함수 매핑
        };
    }

    public bool IsGurd()
    {
        return character.Character.gurd > 0;
    }

    public bool isHold()
    {
        return character.Character.IsHold();
    }
    public bool isparrying()
    {
        return character.Character.isparrying;
    }
}
