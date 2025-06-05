using UnityEngine;
using UnityEngine.UI;

public class CharacterUIManager : MonoBehaviour
{
    [Header("UI 이미지 연결")]
    public Image characterProfileUI;  // UI에 표시될 이미지 컴포넌트

    public void UpdateCharacterProfile(Stats character)
    {
        if (character != null && character.characterProfileillustration != null)
        {
            characterProfileUI.sprite = character.characterProfileillustration;
            characterProfileUI.enabled = true;
        }
        else
        {
            characterProfileUI.enabled = false; // 이미지가 없으면 안 보이게
        }
    }
}
