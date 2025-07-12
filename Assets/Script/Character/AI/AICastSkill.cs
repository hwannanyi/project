using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System.Xml.Linq;
using System.Collections;
using System;

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

        if (character.aIPattern.skillQueueList == null || character.aIPattern.skillQueueList.Count == 0)
            yield break;

        int patternCount = (turnManager.Turn) % character.aIPattern.skillQueueList.Count == 0 ? 
            character.aIPattern.skillQueueList.Count - 1 : (turnManager.Turn) % character.aIPattern.skillQueueList.Count - 1;

        Debug.Log(patternCount);
            if (character.aIPattern.skillQueueList == null ||
            character.aIPattern.skillQueueList.Count <= 1 ||
            character.aIPattern.skillQueueList[patternCount] == null)
                yield break;


            for (int i = 0; i < character.aIPattern.skillQueueList[patternCount].Count; i++)
            {


/*                while (skillManager.isSkillReadyFinal)
                {
                    yield return null; // skillManager.isSkillReadyFinal이 false가 될 때까지 대기
                }*/
                Debug.Log("d");

                var pattern = character.aIPattern.skillQueueList[patternCount][i];
                var targetSkill = new SkillData(pattern.skill, character.name, false);
                if (character.usingSkill.Any(x => x.skillName == pattern.skill.skillName))
                {
                    yield return new WaitForSeconds(pattern.delay);
                    int index = character.usingSkill.FindIndex(x => x.skillName == pattern.skill.skillName);
                    //CharacterSelection.Instance.SelectCharacter2P(character.characterNumber);
                    //CharacterSelection.selectedCharacterIndex = character.characterNumber;
                    skillManager.PrepareSkillCast(index, character.characterNumber);

                /*                                        if (!skillManager.isSkillReady)
                                                        {
                                                            yield break;
                                                        }*/
                Vector3 rotatoin = Vector3.zero;// 기본값 초기화
                switch(pattern.RotationType) // 방향 방식에 따라 방향 지정
                {
                    case Rotation.none:
                        rotatoin = gameObject.transform.position + pattern.Rotation;
                        break;
                    case Rotation.Character:
                        rotatoin = GetClosestCharacterPosition(character, pattern.index, pattern.reverse_order);
                        break;
                    case Rotation.Skill:
                        rotatoin = Vector3.zero; // 임의
                        break;

                }

                float targetPositionX = 0; // 기본값 초기화
                switch (pattern.targetTypeX) // X축 타겟팅 방식에 따라 위치 지정
                {
                    case TargetTypeX.none:
                        targetPositionX = (pattern.coordinate).x;
                        break;
                    case TargetTypeX.Character:
                        targetPositionX = GetClosestCharacterPosition(character, pattern.index, pattern.reverse_order).x;
                        break;
                    case TargetTypeX.Skill:
                        targetPositionX = gameObject.transform.position.x; // 임의
                        break;
                }


                float targetPositionY = 0; // 기본값 초기화
                switch (pattern.targetTypeX) // Y축 타겟팅 방식에 따라 위치 지정
                {
                    case TargetTypeX.none:
                        targetPositionY = (pattern.coordinate).y;
                        break;
                    case TargetTypeX.Character:
                        targetPositionY = GetClosestCharacterPosition(character, pattern.index, pattern.reverse_order).y;
                        break;
                    case TargetTypeX.Skill:
                        targetPositionY = gameObject.transform.position.y; // 임의
                        break;
                }

                Vector3 targetPosition = Vector3.zero; // 기본값 초기화
                targetPosition = new Vector3(targetPositionX, 0f, targetPositionY); // Y축은 0으로 설정



                skillManager.CalculateSkillPosition(character.usingSkill[index], character, true,
                        rotatoin, targetPosition);
                    skillManager.selectedTargetUnit = null;
                    skillManager.ConfirmSkillCast(character.team);
                    skillManager.SkillCastEnemyAI();
                    Debug.Log("스킬실행완료");
                }
            }
        
    }


    public Vector3 GetClosestCharacterPosition(Stats self, int n, bool reverse)
    {
        // 자기 자신, 죽은 캐릭터, 같은 팀 제외
        var candidates = new List<Stats>();
        foreach (var character in CharacterStats.Instance.characterList)
        {
            if (character == self || character.isdie || character.team == self.team) continue;
            candidates.Add(character);
        }

        // 거리순 정렬 (reverse가 true면 역순)
        candidates.Sort((a, b) =>
            Vector3.Distance(self.charPosition, a.charPosition)
            .CompareTo(Vector3.Distance(self.charPosition, b.charPosition)));

        if (reverse)
            candidates.Reverse();

        // n번째(1-based) 캐릭터 반환
        if (candidates.Count >= n)
            return candidates[n - 1].charPosition;
        else if (candidates.Count > 0)
            return reverse ? candidates[candidates.Count - 1].charPosition : candidates[0].charPosition;
        else
            return self.charPosition;
    }

}
