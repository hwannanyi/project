using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.TextCore.Text;
using System.Linq;
using System.Xml.Linq;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;
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

        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectCharacter2P(4);// 0
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectCharacter2P(5);// 1
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectCharacter2P(6);// 2
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectCharacter2P(7);// 3
    }
    void SelectCharacter(int index)
    {
        if (TurnManager.Instance.currentPhase == TurnPhase.EnemyTurn
            || TurnManager.Instance.currentPhase == TurnPhase.ReactPhase_EnemyResponding)
        {
            selectedCharacterIndex = -1;
            return;
        }
        if (index < CharacterStats.Instance.playerCharacters.Count)
        {
            if (CharacterStats.Instance.playerCharacters[index] != null && selectedCharacterIndex != index)
            {

                selectedCharacterIndex = index;

                Debug.Log($"{selectedCharacterIndex}선택된 캐릭터: {CharacterStats.Instance.playerCharacters[selectedCharacterIndex]}");

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

    void SelectCharacter2P(int index)
    {
        if (TurnManager.Instance.currentPhase == TurnPhase.PlayerTurn
            || TurnManager.Instance.currentPhase == TurnPhase.ReactPhase_PlayerResponding)
        {
            selectedCharacterIndex = -1;
            return;
        }
        int index1 = index - CharacterStats.Instance.playerCharacters.Count;
        if (index1 < CharacterStats.Instance.EnemieCharacters.Count)
        {
            if (CharacterStats.Instance.EnemieCharacters[index1] != null && selectedCharacterIndex != index)
            {

                selectedCharacterIndex = index;

                Debug.Log($"{selectedCharacterIndex}선택된 캐릭터: {CharacterStats.Instance.EnemieCharacters[index1]}");

            }
            else if (CharacterStats.Instance.EnemieCharacters[index1] != null && selectedCharacterIndex == index)
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

    public void SelectCharacterCen()
    {
        selectedCharacterIndex = -1;
    }


}
