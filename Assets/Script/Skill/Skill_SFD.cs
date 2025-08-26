using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Skill_SFD : MonoBehaviour
{
    public static event Action<SFDType, float> OnSFDStart; // SFD 시작 이벤트
    public static event Action OnSFDEnd; // SFD 종료 이벤트

    public CastingSkillData castingSkillData;
    public StoryManager storyManager;
    public bool isSFD = false;


    public void Start()
    {
        SkillData skill = castingSkillData.skillData;

            storyManager = StoryManager.instance;
            isSFD = false;
        if(skill.SFDtype == SFDType.none)
            return;
        if (!skill.skillPreviewStop)
        {
            if (storyManager.currentPopUpTalkIndex == 13 && storyManager.PopUptalkRead.Any(t => t.talkID == "1") && !isSFD)
            {

                isSFD = true;
                OnSFDStart?.Invoke(skill.SFDtype, skill.SFDtime); // 이벤트 발생
                StartCoroutine(skill.SFD(skill.SFDtype, skill.SFDtime));
            }

            if (storyManager.currentPopUpTalkIndex == 14 && storyManager.PopUptalkRead.Any(t => t.talkID == "2") && !isSFD)
            {

                isSFD = true;
                OnSFDStart?.Invoke(skill.SFDtype, skill.SFDtime); // 이벤트 발생
                StartCoroutine(skill.SFD(skill.SFDtype, skill.SFDtime));
            }
        }
        
    }

    public void OnDestroy()
    {
        SkillData skill = castingSkillData.skillData;
        if (!skill.skillPreviewStop)
        {
            if (skill.SFDtype == SFDType.none)
                return;
            OnSFDEnd?.Invoke(); // 이벤트 발생
            isSFD = false;
        }
    }
}
