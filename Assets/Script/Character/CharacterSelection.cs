using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterSelection : MonoBehaviour
{
    public static CharacterSelection Instance;
    public static int selectedCharacterIndex = -1; // 선택된 캐릭터 (-1: 선택 없음)
    public static int prevSelectedIndex = -1; // 이전에 선택된 캐릭터 인덱스

    public CharacterUIManager characterUIManager;
    public SkillManager skillManager;
    public TurnManager turnManager;
    public StoryManager storyManager; // 스토리 매니저 인스턴스
    public CharacterStats characterStats;

    public Stats selectedCharacter;
    public GameObject MoveArrow;
    void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        skillManager = GetComponent<SkillManager>();
        turnManager = GetComponent<TurnManager>();
        storyManager = GetComponent<StoryManager>();
        // 싱글턴 패턴 적용 (중복 방지)

            Instance = this;

    }


    //  1, 2, 3 키 입력으로 캐릭터 선택
    void Update()
    {
        if (skillManager.isCastingSkill || skillManager.isSkillReady) MoveArrow.SetActive(false);

        try
        {
            if (storyManager.isStoryActive || StoryManager.instance.chPickLock)
                return; // 모든 입력 무시
        }
        catch
        {
            
            return; // StoryManager를 못불려와도 모든입력무시
        }  

        HandleCharacterSelection();

       
        if (selectedCharacterIndex >= characterStats.playerCharacters.Count)
        {
            //OnCharacterSelectedMoveCount2P(selectedCharacterIndex);
            return;
        }
        //OnCharacterSelectedMoveCount(selectedCharacterIndex); 
/*        if (selectedCharacterIndex == -1 && prevSelectedIndex != -1)
        {
            CharacterStats.Instance.characterList[prevSelectedIndex].SetHighlight(true);
        }*/

        
    }


    public Stats PickcharNumber(int index)
    {
        return characterStats.characterList[index];
    }
    void HandleCharacterSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCharacter(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCharacter(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCharacter(3);


        if (Input.GetKey(KeyCode.Alpha1)) Holding(0);
        if (Input.GetKey(KeyCode.Alpha2)) Holding(1);
        if (Input.GetKey(KeyCode.Alpha3)) Holding(2);
        if (Input.GetKey(KeyCode.Alpha4)) Holding(3);
        /*        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectCharacter2P(4);// 0
                if (Input.GetKeyDown(KeyCode.Alpha6)) SelectCharacter2P(5);// 1
                if (Input.GetKeyDown(KeyCode.Alpha7)) SelectCharacter(6);// 2*/
        //if (Input.GetKeyDown(KeyCode.Alpha8)) SelectCharacter2P(7);// 3
    }
    public void SelectCharacter(int index)
    {
        Stats character = characterStats.characterList[index];
        skillManager.Skillcancel();
            if (character.team != Team.team)
            {
                return;
            }
            if (character.isdie)
            {
                Debug.Log("죽은 캐릭터는 선택할 수 없습니다.");
                return;
            }

            if ( selectedCharacterIndex != index)
            {
                // 이전 캐릭터 하이라이트 끄기
                if (prevSelectedIndex >= 0 && prevSelectedIndex < characterStats.characterList.Count)
                {
                characterStats.characterList[prevSelectedIndex].SetHighlight(false);
                }

                selectedCharacterIndex = index;
                prevSelectedIndex = index;

            //선택된 캐릭은 수비
            skillManager.defendingCharacter = characterStats.characterList[selectedCharacterIndex];

            // 새 캐릭터 하이라이트 켜기
            characterStats.characterList[selectedCharacterIndex].SetHighlight(true);

                selectedCharacter = characterStats.characterList[selectedCharacterIndex];

             OnCharacterSelected(index);
                //characterUIManager.ProfileUIOn();
                characterUIManager.UpdateProfileUIBySelection();
                characterUIManager.SelectionMiniprofileUI(selectedCharacterIndex);
                //Debug.Log($"{selectedCharacterIndex}선택된 캐릭터: {CharacterStats.Instance.playerCharacters[selectedCharacterIndex]}");

            }
            else if (selectedCharacterIndex == index)
            {
            CancelSelection();
        }
            else
            {
                Debug.Log("캐릭 선택실패");
            }
            SetArrow();
    }

    public void CancelSelection()
    {
        // 선택 해제 시 하이라이트 끄기
        characterStats.characterList[selectedCharacterIndex].SetHighlight(false);

        selectedCharacter = null;
        skillManager.defendingCharacter = null;
        selectedCharacterIndex = -1;
        //characterUIManager.ProfileUIOn();
        prevSelectedIndex = -1;
        characterUIManager.SelectionMiniprofileUI(selectedCharacterIndex);
        characterUIManager.UpdateProfileUIBySelection();
        Debug.Log("캐릭선택취소");
    }

    public void Holding(int idx)
    {
        Stats character = characterStats.characterList[idx];
        if (character.team != Team.team)
        {
            return;
        }
        if (character.isdie)
        {
            Debug.Log("죽은 캐릭터는 선택할 수 없습니다.");
            return;
        }
        character.Hold();
    }

    /*    public void SelectCharacter2P(int index)
        {
            if (turnManager.IsPlayerActive())
            {
                selectedCharacterIndex = -1;
                return;
            }

            int index1 = index - CharacterStats.Instance.playerCharacters.Count;
    *//*        // 대응단계에서만 유효한 대상 제한
            if (TurnManager.Instance.IsInReactPhase())
            {
                GameObject candidate = CharacterStats.Instance.characters[index];
                if (!SkillManager.Instance.validReactTargets.Contains(candidate))
                {
                    Debug.LogWarning("선택된 캐릭터는 대응 대상이 아닙니다.");
                    //return;
                }
                // 메인 타겟 여부 확인 가능:
                if (SkillManager.Instance.validMainTarget == candidate)
                {
                    Debug.Log("이 캐릭터는 메인 타겟입니다.");
                }
            }*//*
            if (index1 < CharacterStats.Instance.EnemieCharacters.Count)
            {
                var character = CharacterStats.Instance.characterList[index];
                if (character.isdie)
                {
                    Debug.Log("죽은 캐릭터는 선택할 수 없습니다.");
                    return;
                }
                if (CharacterStats.Instance.EnemieCharacters[index1] != null && selectedCharacterIndex != index)
                {
                    // 이전 캐릭터 하이라이트 끄기
                    if (prevSelectedIndex >= 0 && prevSelectedIndex < CharacterStats.Instance.characterList.Count)
                    {
                        CharacterStats.Instance.characterList[prevSelectedIndex].SetHighlight(false);
                    }

                    selectedCharacterIndex = index;
                    prevSelectedIndex = index;

                    // 새 캐릭터 하이라이트 켜기
                    CharacterStats.Instance.characterList[selectedCharacterIndex].SetHighlight(true);

                    OnCharacterSelected2P(index);
                    //characterUIManager.ProfileUIOn();
                    characterUIManager.UpdateProfileUIBySelection();
                    Debug.Log($"{selectedCharacterIndex}선택된 캐릭터: {CharacterStats.Instance.EnemieCharacters[index1]}");

                }
                else if (CharacterStats.Instance.EnemieCharacters[index1] != null && selectedCharacterIndex == index)
                {
                    // 선택 해제 시 하이라이트 끄기
                    CharacterStats.Instance.characterList[selectedCharacterIndex].SetHighlight(false);
                    selectedCharacterIndex = -1;
                    prevSelectedIndex = -1;
                    characterUIManager.UpdateProfileUIBySelection();
                    Debug.Log("캐릭선택취소");
                }
                else
                {
                    Debug.Log("캐릭 선택실패");
                }
            }
        }*/
    public void SetArrow()
    {
        try
        {
            MoveArrow.SetActive(true);
            // 이동가이드를 캐릭터 오브젝트의 자식으로 설정
            MoveArrow.transform.SetParent(selectedCharacter.characterPrefab.transform);
            // 위치/회전/스케일도 초기화
            MoveArrow.transform.localPosition = Vector3.zero;
            MoveArrow.transform.localRotation = Quaternion.identity;
            MoveArrow.transform.localScale = Vector3.one;
        }
        catch
        {

        }
    }
    public void SelectCharacterCen()
    {
        selectedCharacterIndex = -1;
    }
    public void OnCharacterSelected(int index)
    {
        if (index < 0 || index >= characterStats.characterList.Count)
        {
            Debug.LogError("잘못된 캐릭터 인덱스입니다.");
            return;
        }
        Stats character = characterStats.characterList[index];
        
        characterUIManager.UpdateCharacterProfile(character);
        characterUIManager.UpdateCharacterProfileSkill(character, turnManager.isPlayerTurn);
    }

/*    public void OnCharacterSelectedMoveCount(int index)
    {
        if (index < 0 || index >= CharacterStats.Instance.characterList.Count)
        {
            characterUIManager.UpdateMoveCount(0);
            return;
        }
        var character = CharacterStats.Instance.characterList[index];
        characterUIManager.UpdateMoveCount(character.NowMoveCount);
    }
*/


/*    public void OnCharacterSelected2P(int index)
    {
        int index1 = index;// - CharacterStats.Instance.playerCharacters.Count;
        if (index1 < 0 || index1 >= CharacterStats.Instance.characterList.Count)
        {
            Debug.LogError("잘못된 캐릭터 인덱스입니다.");
            return;
        }
        var character = CharacterStats.Instance.characterList[index1];

        characterUIManager.UpdateCharacterProfile(character);
        characterUIManager.UpdateCharacterProfileSkill(character);
    }
    public void OnCharacterSelectedMoveCount2P(int index)
    {
        int index1 = index - CharacterStats.Instance.playerCharacters.Count;
        if (index1 < 0 || index1 >= CharacterStats.Instance.characterList.Count)
        {
            characterUIManager.UpdateMoveCount(0);
            return;
        }
        var character = CharacterStats.Instance.characterList[index1];
        characterUIManager.UpdateMoveCount(character.NowMoveCount);
    }*/
}
