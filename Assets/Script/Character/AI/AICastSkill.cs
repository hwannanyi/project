using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System.Xml.Linq;
using System.Collections;

public class AICastSkill : MonoBehaviour
{
    public TurnManager turnManager;
    public SkillManager skillManager;


    private void Awake()
    {
        EventManager.Instance.TurnEnd += OnTurnEnd;

    }

    void OnDestroy()
    {

        // 이벤트 구독 해제
        if (EventManager.Instance != null)
            EventManager.Instance.TurnEnd -= OnTurnEnd;
    }
    private void OnTurnEnd(bool value)
    {
        TurnManager turnManager = TurnManager.Instance;
        SkillManager skillManager = SkillManager.Instance;

        var manager = CharacterStats.Instance;
        var character = manager.GetStats(gameObject);
        if (character.aIPattern.skillQueueList == null)
            return;
        StartCoroutine(OnTurnEndCoroutine());

    }

    /*private void OnTurnEnd(bool value)  
    {
        TurnManager turnManager = TurnManager.Instance;
        SkillManager skillManager = SkillManager.Instance;

        var manager = CharacterStats.Instance;

        var character = manager.GetStats(gameObject);

        if(character.aIPattern.skillQueueList == null)
            return;

        if (turnManager.Turn % 2 == 0)
        {
            for(int i = 0; i < character.aIPattern.skillQueueList.Count; i++) { 
            Debug.Log("d");
                
            var pattern = character.aIPattern.skillQueueList[i];
            var targetSkill = new SkillData(pattern.skill, character.name, false);
                if (character.usingSkill.Any(x => x.skillName == pattern.skill.skillName))
                {
                    int index = character.usingSkill.FindIndex(x => x.skillName == pattern.skill.skillName);
                    CharacterSelection.Instance.SelectCharacter2P(character.characterNumber);
                    CharacterSelection.selectedCharacterIndex = character.characterNumber;
                    skillManager.PrepareSkillCast(index);

                    if (!skillManager.isSkillReady)
                    {
                        return;
                    }
                    *//*                skillManager.selectedCharacter = character;
                                    skillManager.selectedCaster = gameObject;*//*
                    skillManager.CalculateSkillPosition(skillManager.selectedSkill, skillManager.selectedCharacter, true,
                        pattern.Rotation, GetClosestCharacterPosition(character));
                    skillManager.selectedTargetUnit = null;
                    skillManager.ConfirmSkillCast();
                    skillManager.ExecuteSingleSkillWithReactionCheck();
                    Debug.Log("스킬실행완료");
                }
            }

                
        }
    }*/

    private IEnumerator OnTurnEndCoroutine()
    {
        TurnManager turnManager = TurnManager.Instance;
        SkillManager skillManager = SkillManager.Instance;

        var manager = CharacterStats.Instance;
        var character = manager.GetStats(gameObject);

        if (character.aIPattern.skillQueueList == null)
            yield break;

        if (turnManager.Turn % 2 == 0)
        {
            for (int i = 0; i < character.aIPattern.skillQueueList.Count; i++)
            {


/*                while (skillManager.isSkillReadyFinal)
                {
                    yield return null; // skillManager.isSkillReadyFinal이 false가 될 때까지 대기
                }*/
                Debug.Log("d");

                var pattern = character.aIPattern.skillQueueList[i];
                var targetSkill = new SkillData(pattern.skill, character.name, false);
                if (character.usingSkill.Any(x => x.skillName == pattern.skill.skillName))
                {
                    yield return new WaitForSeconds(character.aIPattern.skillQueueList[i].delay);
                    int index = character.usingSkill.FindIndex(x => x.skillName == pattern.skill.skillName);
                    //CharacterSelection.Instance.SelectCharacter2P(character.characterNumber);
                    //CharacterSelection.selectedCharacterIndex = character.characterNumber;
                    skillManager.PrepareSkillCast(index, character.characterNumber);
                    /*
                                        if (!skillManager.isSkillReady)
                                        {
                                            yield break;
                                        } */
                    skillManager.CalculateSkillPosition(character.usingSkill[index], character, true,
                        pattern.Rotation, GetClosestCharacterPosition(character));
                    skillManager.selectedTargetUnit = null;
                    skillManager.ConfirmSkillCast(character.team);
                    skillManager.SkillCastEnemyAI();
                    Debug.Log("스킬실행완료");
                }
            }
        }
    }

    public Vector3 GetClosestCharacterPosition(Stats self)
    {
        Stats closest = null;
        float minDist = float.MaxValue;

        foreach (var character in CharacterStats.Instance.characterList)
        {
            // 자기 자신, 죽은 캐릭터, 같은 팀 제외
            if (character == self || character.isdie || character.team == self.team) continue;

            float dist = Vector3.Distance(self.charPosition, character.charPosition);
            if (dist < minDist)
            {
                minDist = dist;
                closest = character;
            }
        }

        return closest != null ? closest.charPosition : self.charPosition;
    }

}
