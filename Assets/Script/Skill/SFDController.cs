using UnityEngine;

public class SFDController : MonoBehaviour
{
    public static SFDController Instance;

    public bool isSFD = false;
    public SFDType sfdtype = SFDType.none;

    public bool moveUp = false;
    public bool moveDo = false;
    public bool moveL = false;
    public bool moveR = false;

    public bool skillQ = false;
    public bool skillW = false;
    public bool skillE = false;
    public bool skillR = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        Skill_SFD.OnSFDStart += OnSFDStartHandler; // 이벤트 구독
        SkillPreview.OnSFDStart += OnSFDStartHandler; // 이벤트 구독
        Skill_SFD.OnSFDEnd += OnSFDEndHandler; // 이벤트 구독
        SkillPreview.OnSFDEnd += OnSFDEndHandler; // 이벤트 구독
    }

    void OnDestroy()
    {
        Skill_SFD.OnSFDStart -= OnSFDStartHandler; // 이벤트 해제
        SkillPreview.OnSFDStart -= OnSFDStartHandler; // 이벤트 해제
        Skill_SFD.OnSFDEnd -= OnSFDEndHandler; // 이벤트 해제
        SkillPreview.OnSFDEnd -= OnSFDEndHandler; // 이벤트 해제
    }

    private void OnSFDEndHandler()
    {
        isSFD = false;
        sfdtype = SFDType.none;
        moveUp = false;
        moveDo = false;
        moveL = false;
        moveR = false;
        skillQ = false;
        skillW = false;
        skillE = false;
        skillR = false;
    }

    private void OnSFDStartHandler(SFDType type, float delay)
    {
        isSFD = true;
        sfdtype = type;

        moveUp = false;
        moveDo = false;
        moveL = false;
        moveR = false;
        skillQ = false;
        skillW = false;
        skillE = false;
        skillR = false;

        switch (sfdtype) 
        { 
            case SFDType.moveUp:
                moveUp = true;
                break;
            case SFDType.moveDo:
                moveDo = true;
                break;
            case SFDType.moveUpDo:
                moveUp = true;
                moveDo = true;
                break;
            case SFDType.skillE:
                skillE = true;
                break;
        }
    }

}
