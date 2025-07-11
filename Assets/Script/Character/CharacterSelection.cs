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
    public static int prevSelectedIndex = -1; // 이전에 선택된 캐릭터 인덱스
    public int asd;

    public CharacterUIManager characterUIManager;
    public SkillManager skillManager;
    public TurnManager turnManager;

    public GameObject selectedCharacter;
    public CharacterStats characterStats;
    void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        skillManager = GetComponent<SkillManager>();
        turnManager = GetComponent<TurnManager>();
        // 싱글턴 패턴 적용 (중복 방지)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 변경되어도 삭제되지 않음
        }
        else
        {
            Destroy(gameObject); // 이미 인스턴스가 존재하면 새로운 객체 삭제
            return;
        }
    }

    //  1, 2, 3 키 입력으로 캐릭터 선택
    void Update()
    {
        asd = selectedCharacterIndex;
        if (CameraZoom.isControlMode) return;
        HandleCharacterSelection();

       
        if (selectedCharacterIndex >= CharacterStats.Instance.playerCharacters.Count)
        {
            OnCharacterSelectedMoveCount2P(selectedCharacterIndex);
            return;
        }
        OnCharacterSelectedMoveCount(selectedCharacterIndex); 
/*        if (selectedCharacterIndex == -1 && prevSelectedIndex != -1)
        {
            CharacterStats.Instance.characterList[prevSelectedIndex].SetHighlight(true);
        }*/

        
    }

    void HandleCharacterSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCharacter(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCharacter(1);
        //if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter(2);
        //if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCharacter(3);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter2P(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCharacter2P(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectCharacter2P(4);// 0
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectCharacter2P(5);// 1
        //if (Input.GetKeyDown(KeyCode.Alpha7)) SelectCharacter2P(6);// 2
        //if (Input.GetKeyDown(KeyCode.Alpha8)) SelectCharacter2P(7);// 3
    }
    public void SelectCharacter(int index)
    {
        
        SkillManager.Instance.Skillcancel();
        if (!turnManager.isPlayerTurn && !turnManager.IsInReactPhase())
        {
            selectedCharacterIndex = -1;
            return;
        }
/*        // 대응단계에서만 유효한 대상 제한
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
        }*/

        if (index < CharacterStats.Instance.playerCharacters.Count)
        {
            var character = CharacterStats.Instance.characterList[index];
            if (character.isdie)
            {
                Debug.Log("죽은 캐릭터는 선택할 수 없습니다.");
                return;
            }
            if (CharacterStats.Instance.playerCharacters[index] != null && selectedCharacterIndex != index)
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

                OnCharacterSelected(index);
                //characterUIManager.ProfileUIOn();
                characterUIManager.UpdateProfileUIBySelection();
                Debug.Log($"{selectedCharacterIndex}선택된 캐릭터: {CharacterStats.Instance.playerCharacters[selectedCharacterIndex]}");

            }
            else if (CharacterStats.Instance.playerCharacters[index] != null && selectedCharacterIndex == index)
            {
                // 선택 해제 시 하이라이트 끄기
                CharacterStats.Instance.characterList[selectedCharacterIndex].SetHighlight(false);
                selectedCharacterIndex = -1;
                //characterUIManager.ProfileUIOn();
                prevSelectedIndex = -1;
                characterUIManager.UpdateProfileUIBySelection();
                Debug.Log("캐릭선택취소");
            }
            else
            {
                Debug.Log("캐릭 선택실패");
            }
        }
    }

    public void SelectCharacter2P(int index)
    {
        if (turnManager.IsPlayerActive())
        {
            selectedCharacterIndex = -1;
            return;
        }

        int index1 = index - CharacterStats.Instance.playerCharacters.Count;
/*        // 대응단계에서만 유효한 대상 제한
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
        }*/
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
    }

    public void SelectCharacterCen()
    {
        selectedCharacterIndex = -1;
    }
    public void OnCharacterSelected(int index)
    {
        if (index < 0 || index >= CharacterStats.Instance.characterList.Count)
        {
            Debug.LogError("잘못된 캐릭터 인덱스입니다.");
            return;
        }
        var character = CharacterStats.Instance.characterList[index];
        
        characterUIManager.UpdateCharacterProfile(character);
        characterUIManager.UpdateCharacterProfileSkill(character);
    }

    public void OnCharacterSelectedMoveCount(int index)
    {
        if (index < 0 || index >= CharacterStats.Instance.characterList.Count)
        {
            characterUIManager.UpdateMoveCount(0);
            return;
        }
        var character = CharacterStats.Instance.characterList[index];
        characterUIManager.UpdateMoveCount(character.NowMoveCount);
    }



    public void OnCharacterSelected2P(int index)
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
    }
}
