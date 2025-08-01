using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    [Header("UI 이미지 연결")]
    public Image characterProfileUI;  // UI에 표시될 이미지 컴포넌트

    [Header("UI프로필")]
    public GameObject ProfileUI;

    [Header("스킬")]
    public Sprite characterProfileSkillUI;
    public List<Image> characterProfileSkillListUI;

    [Header("스킬쿨타임")]
    public List<TextMeshProUGUI> characterProfileSkillTextListUI;
    public Color cooldownColor = new();
    public Color nompdownColor = new();


    [Header("이동")]
    public Sprite MoveCount1;
    public Sprite MoveCount2;
    public List<Image> MoveCount;


    [Header("이동")]
    public Image hpBar; // Fill Amount 방식
    public Image mpBar;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI mpText;

    [Header("미니프로필")]
    public List<MiniprofileUIManager> miniprofileUIManagers;

    public void UpdateCharacterProfile(Stats character)
    {
        if (character != null && character.characterProfileillustration != null)
        {
            characterProfileUI.sprite = character.characterProfileillustration;
            characterProfileUI.enabled = true;
        }
        else
        {
            characterProfileUI.sprite = characterProfileSkillUI; // 이미지가 없으면 안 보이게
        }
    }

    public void UpdateCharacterProfileSkill(Stats character, bool playerturn)
    {
        if(character == null) return; // 캐릭터가 null이면 아무것도 하지 않음

        // 쿨타임 중일 때 적용할 색상

        Color normalColor = Color.white;

        // 스킬 슬롯 개수만큼 반복
        for (int i = 0; i < characterProfileSkillListUI.Count; i++)
        {
            int index = playerturn ? i : i + 5; // 플레이어 턴이면 i, 그렇지 않으면 i + 5
            var skillImage = characterProfileSkillListUI[i];
            var skillText = characterProfileSkillTextListUI[i];

            // 해당 슬롯에 스킬이 존재하는 경우
            if (character.usingSkill.Count > index && character.usingSkill[index].skillName != null)
            {
                var skill = character.usingSkill[index];
                skillImage.sprite = skill.skillIcon; // 스킬 아이콘 표시
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
                skillImage.sprite = characterProfileSkillUI; // 기본 아이콘
                skillImage.color = normalColor;
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

    public void ProfileUpdate(Stats character, bool playerturn)
    {
        UpdateCharacterProfile(character);
        UpdateCharacterProfileSkill(character, playerturn);
        UpdateMoveCount(character.NowMoveCount);
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

        if (hpText != null)
            hpText.text = $"{character.hp} / {character.maxhp}";
        if (mpText != null)
            mpText.text = $"{character.mp} / {character.maxmp}";
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

        // ProfileUI의 모든 자식 오브젝트 활성/비활성화
        foreach (Transform child in ProfileUI.transform)
        {
            child.gameObject.SetActive(isActive);
        }
    }
}
