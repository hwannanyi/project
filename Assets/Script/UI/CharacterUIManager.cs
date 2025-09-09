using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    public CharacterUIManager Instance;

    [Header("UI 이미지 연결")]
    public Image characterProfileUI;  // UI에 표시될 이미지 컴포넌트

    [Header("UI프로필")]
    public GameObject ProfileUI;

    [Header("스킬")]
    public Sprite characterProfileSkillUI;
    public List<GameObject> characterProfileSkillListUI;
    private List<Image> skillSlotImages;


    [Header("스킬쿨타임")]
    public List<TextMeshProUGUI> characterProfileSkillTextListUI;
    public Color cooldownColor = new();
    public Color nompdownColor = new();

    public GameObject skillhight;
    private RectTransform skillHightRect;
    private List<RectTransform> skillSlotRects;
    /*
        [Header("이동")]
        public Sprite MoveCount1;
        public Sprite MoveCount2;
        public List<Image> MoveCount;*/

    [Header("과열")]
    public List<Image> RageCount;
    public List<Image> RiskCount;

    [Header("이동")]
    public Image hpBar; // Fill Amount 방식
    public Image mpBar;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;

    [Header("미니프로필")]
    public List<MiniprofileUIManager> miniprofileUIManagers;

    public TurnManager turnManager; // 턴 매니저

    void Awake()
    {
        skillHightRect = skillhight.GetComponent<RectTransform>();
        skillSlotRects = new List<RectTransform>();
        foreach (var go in characterProfileSkillListUI)
            skillSlotRects.Add(go.GetComponent<RectTransform>());

        skillSlotImages = new List<Image>();
        foreach (var go in characterProfileSkillListUI)
            skillSlotImages.Add(go.GetComponent<Image>());
    }

    public void OnEnable()
    {
        turnManager.OnTurnChanged -= UpdateRageCount;
        turnManager.OnTurnChanged -= UpdateRiskCount;
        turnManager.OnTurnChanged += UpdateRageCount;
        turnManager.OnTurnChanged += UpdateRiskCount;
    }

    public void Destroy()
    {
        turnManager.OnTurnChanged -= UpdateRageCount;
        turnManager.OnTurnChanged -= UpdateRiskCount;
    }

    public void UpdateCharacterProfile(Stats character)
    {
/*        if (character != null && character.characterProfileillustration != null)
        {
            characterProfileUI.sprite = character.characterProfileillustration;
            characterProfileUI.enabled = true;
        }
        else
        {
            characterProfileUI.sprite = characterProfileSkillUI; // 이미지가 없으면 안 보이게
        }*/
    }

    public void UpdateCharacterProfileSkill(Stats character, bool playerturn)
    {
        if (character == null) return; // 캐릭터가 null이면 아무것도 하지 않음

        // 슬롯 개수만큼 반복 (UI 리스트와 텍스트 리스트 중 더 작은 값 기준)
        int slotCount = Mathf.Min(skillSlotImages.Count, characterProfileSkillTextListUI.Count);
        Color normalColor = Color.white;

        for (int index = 0; index < slotCount; index++)
        {
            Image skillImage = skillSlotImages[index];
            TextMeshProUGUI skillText = characterProfileSkillTextListUI[index];

            // 해당 슬롯에 스킬이 존재하는 경우
            if (character.usingSkill.Count > index && character.usingSkill[index].skillName != null)
            {
                SkillData skill = character.usingSkill[index];
                skillImage.sprite = skill.skillIcon; // 스킬 아이콘 표시
                skillImage.color = skill.colldownTime > 0 ? cooldownColor : normalColor; // 쿨타임 색상 처리

                // MP가 부족할 경우 색상 변경
                if (skill.rageCost > character.rage && skill.hpCost >= character.hp)
                {
                    skillImage.color = nompdownColor;
                }

                // 쿨타임이 있을 경우 쿨타임 숫자 표시, 아니면 빈 문자열
                skillText.text = skill.colldownTime > 0 ? skill.colldownTime.ToString() : string.Empty;

                if (!playerturn)
                {
                    skillImage.color = index == 2 ? nompdownColor : Color.gray;

                }

                if (index == 2 && playerturn)
                {
                    skillImage.color = Color.gray;
                }


                if (character.rest)
                {
                    skillImage.color = new Color(50,50,50);
                }
            }
            else // 스킬이 없는 슬롯
            {
                skillImage.sprite = characterProfileSkillUI; // 기본 아이콘 표시
                skillImage.color = normalColor;
                skillText.text = string.Empty;
            }
            skillImage.enabled = true; // 아이콘 활성화
        }
        // HP/MP 바 갱신
        UpdateHpMpBar(character);
    }

    //선택된 스킬 하이라이트 표시
    public void SkillSelectionhigh(int skillIndex)
    {
        bool isActive = skillIndex >= 0 && skillIndex < skillSlotRects.Count;
        skillhight.SetActive(isActive);

        if (isActive)
            skillHightRect.anchoredPosition = skillSlotRects[skillIndex].anchoredPosition;
    }

    /*    public void UpdateMoveCount(int moveCount)
        {
            // 모든 MoveCount 이미지 비활성화
            foreach (var countImage in MoveCount)
            {
                countImage.enabled = false;
            }

            // 이동 횟수에 해당하는 이미지까지 모두 활성화
            if (moveCount >= 1 && moveCount <= MoveCount.Count)
            {
                for (int i = 0; i < moveCount; i++)
                {
                    MoveCount[i].enabled = true;
                }
            }
        }
    */

    public void UpdateRageCount(Stats ch)
    {
        // 모든 MoveCount 이미지 비활성화
        foreach (var countImage in RageCount)
        {
            countImage.enabled = false;
        }
        int idx = ch.rage;
        // 이동 횟수에 해당하는 이미지까지 모두 활성화
        if (idx >= 1 && idx <= RageCount.Count)
        {
            for (int i = 0; i < idx; i++)
            {
                RageCount[i].enabled = true;
            }
        }
    }

    public void UpdateRiskCount(Stats ch)
    {
        // 모든 MoveCount 이미지 비활성화
        foreach (var countImage in RiskCount)
        {
            countImage.enabled = false;
        }
        int idx = ch.risk;
        // 이동 횟수에 해당하는 이미지까지 모두 활성화
        if (idx >= 1 && idx <= RiskCount.Count)
        {
            for (int i = 0; i < idx; i++)
            {
                RiskCount[i].enabled = true;
            }
        }
    }


    public void ProfileUpdate(Stats character, bool playerturn)
    {
        UpdateCharacterProfile(character);
        UpdateCharacterProfileSkill(character, playerturn);
        //UpdateMoveCount(character.NowMoveCount);
    }

    public void ProfileUIOff()
    {
        ProfileUI.SetActive(false);
    }
    public void ProfileUIOn()
    {
        ProfileUI.SetActive(true);
    }

    public void UpdateHpMpBar(Stats character)
    {
        if (hpBar != null)
            hpBar.fillAmount = (float)character.hp / character.maxhp;
        if (mpBar != null)
            mpBar.fillAmount = (float)character.rage / 5;

        if (hpText != null)
            hpText.text = $"{character.hp} / {character.maxhp}";
        if (mpText != null)
            mpText.text = $"{character.rage} / {5}";
    }

    public void AssignMiniprofileTargets()
    {
        for (int i = 0; i < miniprofileUIManagers.Count; i++)
        {
            List<Stats>  characterStatsList = CharacterStats.Instance.characterList;

            if (i < CharacterStats.Instance.playerCharacters.Count)
            {
                miniprofileUIManagers[i].targetCharacter = characterStatsList[i];
            }
            else
            {
                miniprofileUIManagers[i].targetCharacter = null; // 남는 미니프로필은 비활성화
                miniprofileUIManagers[i].ProfileUIOff();
            }
        }
    }

    public void UpdateProfileUIBySelection()
    {
        bool isActive = CharacterSelection.selectedCharacterIndex != -1;
        ProfileUI.SetActive(isActive);
        /*
                // ProfileUI의 모든 자식 오브젝트 활성/비활성화
                foreach (Transform child in ProfileUI.transform)
                {
                    if (child.gameObject == skillhight) continue;
                    child.gameObject.SetActive(isActive);
                }*/
    }

    public void SelectionMiniprofileUI(int index)
    {

        for (int i = 0; i < miniprofileUIManagers.Count; i++)
        {
            MiniprofileUIManager profile = miniprofileUIManagers[i];
            if (profile == null || profile.ProfileUI == null) continue;

            RectTransform rectTransform = profile.ProfileUI.GetComponent<RectTransform>();
            Image color = profile.ProfileUI.GetComponent<Image>();
            Vector2 pos = rectTransform.anchoredPosition;

                // 선택된 프로필만 오른쪽으로 이동
                (rectTransform.anchoredPosition,color.color) = 
                i == index ? 
                (new Vector2(74f + 26f, pos.y), new Color(1f, 1f, 0.7f, 1f)) 
                : (new Vector2(74f, pos.y), new Color(1f, 1f, 1f, 1f)) ;

        }
    }
}
