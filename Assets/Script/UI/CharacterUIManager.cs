using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    [Header("UI 이미지 연결")]
    public Image characterProfileUI;  // UI에 표시될 이미지 컴포넌트

    [Header("스킬")]
    public Sprite characterProfileSkillUI;
    public Image characterProfileSkill1UI;
    public Image characterProfileSkill2UI;
    public Image characterProfileSkill3UI;
    public Image characterProfileSkill4UI;
    public Image characterProfileSkill5UI;

    [Header("스킬")]
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

        if (character.usingSkill[0].skillName != null)
        {
            characterProfileSkill1UI.sprite = character.usingSkill[0].skillIcon;
        }
        else
        {
            characterProfileSkill1UI.sprite = characterProfileSkillUI; // 이미지가 없으면 안 보이게
        }
        characterProfileSkill1UI.enabled = true;


        if (character.usingSkill[1].skillName != null)
        {
            characterProfileSkill2UI.sprite = character.usingSkill[1].skillIcon;
        }
        else
        {
            characterProfileSkill2UI.sprite = characterProfileSkillUI; // 이미지가 없으면 빈칸아이콘
        }
        characterProfileSkill2UI.enabled = true;

        if (character.usingSkill[2].skillName != null)
        {
            characterProfileSkill3UI.sprite = character.usingSkill[2].skillIcon;
        }
        else
        {
            characterProfileSkill3UI.sprite = characterProfileSkillUI; // 이미지가 없으면 빈칸아이콘
        }
        characterProfileSkill3UI.enabled = true;

        if (character.usingSkill[3].skillName != null)
        {
            characterProfileSkill4UI.sprite = character.usingSkill[3].skillIcon;
        }
        else
        {
            characterProfileSkill4UI.sprite = characterProfileSkillUI; // 이미지가 없으면 안 보이게
        }
        characterProfileSkill4UI.enabled = true;


        if (character.usingSkill[4].skillName != null)
        {
            characterProfileSkill5UI.sprite = character.usingSkill[4].skillIcon;
        }
        else
        {
            characterProfileSkill5UI.sprite = characterProfileSkillUI; // 이미지가 없으면 안 보이게
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
}
