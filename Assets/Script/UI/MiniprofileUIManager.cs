using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.TextCore.Text;

public class MiniprofileUIManager : MonoBehaviour
{
    [Header("UI 이미지 연결")]
    public Image characterProfileUI;  // UI에 표시될 이미지 컴포넌트
    public Sprite characterProfiledefortUI;

    [Header("UI프로필")]
    public GameObject ProfileUI;
    [HideInInspector] public Stats targetCharacter; // 이 미니프로필이 표시할 캐릭터

    [Header("스킬쿨타임")]
    public List<TextMeshProUGUI> characterProfileSkillTextListUI;
    public Color cooldownColor = new();
    public Color nompdownColor = new();
    public Color normalColor = new();
    [Header("스킬")]
    public List<Image> characterProfileSkillListUI;


    [Header("이동")]
    public Sprite MoveCount1;
    public Sprite MoveCount2;
    public List<Image> MoveCount;


    [Header("이동")]
    public Image hpBar; // Fill Amount 방식
    public Image mpBar;

    void Update()
    {
        if (ProfileUI == null || !ProfileUI.activeInHierarchy)
            return;
        if (string.IsNullOrEmpty(targetCharacter.name))
            return;
        UpdateCharacterProfile(targetCharacter);
        UpdateCharacterProfileSkill(targetCharacter);
        UpdateMoveCount(targetCharacter.NowMoveCount);
        UpdateHpMpBar(targetCharacter);

    }

    public void UpdateCharacterProfile(Stats character)
    {
        if (!string.IsNullOrEmpty(character.name) && character.characterProfileillustration != null)
        {
            characterProfileUI.sprite = character.characterProfileillustration;
            characterProfileUI.enabled = true;
            if (character.isdie)
            {
                characterProfileUI.color = new Color(0.7f, 0.7f, 0.7f, 1f);  // 죽은 캐릭터는 반투명 처리
            }
        }
        else
        {
            characterProfileUI.sprite = characterProfiledefortUI; // 이미지가 없으면 안 보이게
        }
    }

    public void UpdateCharacterProfileSkill(Stats character)
    {
        if (string.IsNullOrEmpty(character.name)) return; // 캐릭터가 null이면 아무것도 하지 않음

        // 스킬 슬롯 개수만큼 반복
        for (int i = 0; i < characterProfileSkillListUI.Count; i++)
        {
            var skillImage = characterProfileSkillListUI[i];
            var skillText = characterProfileSkillTextListUI[i];

            // 해당 슬롯에 스킬이 존재하는 경우
            if (character.usingSkill.Count > i && character.usingSkill[i].skillName != null)
            {
                var skill = character.usingSkill[i];
                skillImage.color = skill.colldownTime > 0 ? cooldownColor : normalColor; // 쿨타임 색상 처리

                // MP 부족 시 색상 처리
                if (skill.cost.ContainsKey(CostType.mp) && skill.cost[CostType.mp] > character.mp)
                {
                    skillImage.color = nompdownColor;
                }

                // 쿨타임 텍스트 표시
                skillText.text = skill.colldownTime > 0 ? skill.colldownTime.ToString() : null;
            }
            else // 스킬이 없는 슬롯
            {
                skillImage.color = new Color(0f, 0f, 0f, 0f);
                skillText.text = null;
            }
            skillImage.enabled = true; // 아이콘 활성화
        }
        UpdateHpMpBar(character);
    }

    public void UpdateMoveCount(int moveCount)
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
            mpBar.fillAmount = (float)character.mp / character.maxmp;
    }
}
