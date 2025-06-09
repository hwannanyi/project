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
    public Image characterProfileSkill1UI;
    public Image characterProfileSkill2UI;
    public Image characterProfileSkill3UI;
    public Image characterProfileSkill4UI;
    public Image characterProfileSkill5UI;

    [Header("스킬쿨타임")]
    public TextMeshProUGUI characterProfileSkill1Text;
    public TextMeshProUGUI characterProfileSkill2Text;
    public TextMeshProUGUI characterProfileSkill3Text;
    public TextMeshProUGUI characterProfileSkill4Text;
    public TextMeshProUGUI characterProfileSkill5Text;

    [Header("이동")]
    public Sprite MoveCount1;
    public Sprite MoveCount2;
    public List<Image> MoveCount;

    public void UpdateCharacterProfile(Stats character)
    {
        if (character != null && character.characterProfileillustration != null)
        {
            characterProfileUI.sprite = character.characterProfileillustration;
            characterProfileUI.enabled = true;
        }
        else
        {
            characterProfileSkill1UI.sprite = characterProfileSkillUI; // 이미지가 없으면 안 보이게
        }
    }

    public void UpdateCharacterProfileSkill(Stats character)
    {
        if(character == null) return; // 캐릭터가 null이면 아무것도 하지 않음

        // 쿨타임 중일 때 적용할 색상
        Color cooldownColor = new Color(0.3f, 0.5f, 1f, 0.5f);
        Color normalColor = Color.white;

        // 1번 스킬
        if (character.usingSkill.Count > 0 && character.usingSkill[0].skillName != null)
        {
            characterProfileSkill1UI.sprite = character.usingSkill[0].skillIcon;
            characterProfileSkill1UI.color = character.usingSkill[0].colldownTime > 0 ? cooldownColor : normalColor;
            characterProfileSkill1Text.text = character.usingSkill[0].colldownTime > 0 ? character.usingSkill[0].colldownTime.ToString() : null;
        }
        else
        {
            characterProfileSkill1UI.sprite = characterProfileSkillUI;
            characterProfileSkill1UI.color = normalColor;
        }
        characterProfileSkill1UI.enabled = true;

        // 2번 스킬
        if (character.usingSkill.Count > 1 && character.usingSkill[1].skillName != null)
        {
            characterProfileSkill2UI.sprite = character.usingSkill[1].skillIcon;
            characterProfileSkill2UI.color = character.usingSkill[1].colldownTime > 0 ? cooldownColor : normalColor;
            characterProfileSkill2Text.text = character.usingSkill[1].colldownTime > 0 ? character.usingSkill[1].colldownTime.ToString() : null;
            
        }
        else
        {
            characterProfileSkill2UI.sprite = characterProfileSkillUI;
            characterProfileSkill2UI.color = normalColor;
        }
        characterProfileSkill2UI.enabled = true;

        // 3번 스킬
        if (character.usingSkill.Count > 2 && character.usingSkill[2].skillName != null)
        {
            characterProfileSkill3UI.sprite = character.usingSkill[2].skillIcon;
            characterProfileSkill3UI.color = character.usingSkill[2].colldownTime > 0 ? cooldownColor : normalColor;
            characterProfileSkill3Text.text = character.usingSkill[2].colldownTime > 0 ? character.usingSkill[2].colldownTime.ToString() : null;
            
        }
        else
        {
            characterProfileSkill3UI.sprite = characterProfileSkillUI;
            characterProfileSkill3UI.color = normalColor;
        }
        characterProfileSkill3UI.enabled = true;

        // 4번 스킬
        if (character.usingSkill.Count > 3 && character.usingSkill[3].skillName != null)
        {
            characterProfileSkill4UI.sprite = character.usingSkill[3].skillIcon;
            characterProfileSkill4UI.color = character.usingSkill[3].colldownTime > 0 ? cooldownColor : normalColor;
            characterProfileSkill4Text.text = character.usingSkill[3].colldownTime > 0 ? character.usingSkill[3].colldownTime.ToString() : null;
            
        }
        else
        {
            characterProfileSkill4UI.sprite = characterProfileSkillUI;
            characterProfileSkill4UI.color = normalColor;
        }
        characterProfileSkill4UI.enabled = true;

        // 5번 스킬
        if (character.usingSkill.Count > 4 && character.usingSkill[4].skillName != null)
        {
            characterProfileSkill5UI.sprite = character.usingSkill[4].skillIcon;
            characterProfileSkill5UI.color = character.usingSkill[4].colldownTime > 0 ? cooldownColor : normalColor;
            characterProfileSkill5Text.text = character.usingSkill[4].colldownTime > 0 ? character.usingSkill[4].colldownTime.ToString() : null;
        }
        else
        {
            characterProfileSkill5UI.sprite = characterProfileSkillUI;
            characterProfileSkill5UI.color = normalColor;
        }
        characterProfileSkill5UI.enabled = true;
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
}
