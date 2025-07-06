using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System.Xml.Linq;

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

        if(character.aIPattern.skillQueueList == null)
            return;
        if (turnManager.Turn % 2 == 0)
        {
            Debug.Log("d");
            var pattern = character.aIPattern.skillQueueList[0];
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
                /*                skillManager.selectedCharacter = character;
                                skillManager.selectedCaster = gameObject;*/
                skillManager.CalculateSkillPosition(skillManager.selectedSkill, skillManager.selectedCharacter,true, pattern.Rotation);
                skillManager.selectedTargetUnit = null;
                skillManager.ConfirmSkillCast();
                skillManager.ExecuteSingleSkillWithReactionCheck();
                Debug.Log("스킬실행완료");
            }

                
        }
    }

}
