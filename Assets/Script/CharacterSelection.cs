using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;
using System.Linq;
using System.Xml.Linq;

public class CharacterSelection : MonoBehaviour
{
    
    public static int selectedCharacterIndex = -1; // 선택된 캐릭터 (-1: 선택 없음)


    void Awake()
    {
        
    }

    //  1, 2, 3 키 입력으로 캐릭터 선택
    void Update()
    {
        HandleCharacterSelection();
    }

    void HandleCharacterSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCharacter(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCharacter(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCharacter(3);
    }
    void SelectCharacter(int index)
    {

        if (index < CharacterStats.Instance.playerCharacters.Count)
        {
            if (CharacterStats.Instance.playerCharacters[index] != null && selectedCharacterIndex != index)
            {

                selectedCharacterIndex = index;

                Debug.Log($"선택된 캐릭터: {CharacterStats.Instance.playerCharacters[selectedCharacterIndex]}");

            }
            else if (CharacterStats.Instance.playerCharacters[index] != null && selectedCharacterIndex == index)
            {
                selectedCharacterIndex = -1;
                Debug.Log("캐릭선택취소");
            }
            else
            {
                Debug.Log("캐릭 선택실패");
            }
        }
    }


}
