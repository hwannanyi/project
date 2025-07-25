using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System.Xml.Linq;
using System.Collections;
using System;
using static UnityEngine.GraphicsBuffer;

public class AICastSkill : MonoBehaviour
{
    public TurnManager turnManager;
    public SkillManager skillManager;

    public bool SkillCasting = false; // 스킬 시전 중인지 여부'
    public bool skillCastLock = false; //다음 스킬 시전 잠금
    //public bool AI = false; //AI

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
        var manager = CharacterStats.Instance;
        var character = manager.GetStats(gameObject);
        if (character.aIPattern.skillQueueList == null)
            return;
        StartCoroutine(OnTurnEndCoroutine());
        //character.isPatternEnd = true; // 패턴 종료 상태로 설정
    }

    private IEnumerator OnTurnEndCoroutine()
    {
        TurnManager turnManager = TurnManager.Instance;
        SkillManager skillManager = SkillManager.Instance;

        var manager = CharacterStats.Instance;
        var character = manager.GetStats(gameObject);
        character.isPatternEnd = false;

        if (character.aIPattern.skillQueueList == null || character.aIPattern.skillQueueList.Count == 0)
        {
            character.isPatternEnd = true; // 패턴 종료 상태로 설정
            yield break;
        }

        if (character.usingSkill == null)
        {
            character.isPatternEnd = true;
            yield break;
        }

        int patternCount = (turnManager.Turn) % character.aIPattern.skillQueueList.Count == 0 ? 
            character.aIPattern.skillQueueList.Count - 1 : (turnManager.Turn) % character.aIPattern.skillQueueList.Count - 1;

        Debug.Log(patternCount);
        if (character.aIPattern.skillQueueList == null ||
        character.aIPattern.skillQueueList.Count <= 1 ||
        character.aIPattern.skillQueueList[patternCount] == null)
        {
            character.isPatternEnd = true; // 패턴 종료 상태로 설정
            yield break;
        }

        for (int i = 0; i < character.aIPattern.skillQueueList[patternCount].Count; i++)
        {

            var pattern = character.aIPattern.skillQueueList[patternCount][i];
            if (pattern.skill == null) continue;

            var targetSkill = new SkillData(pattern.skill, character.name, false);
            if (character.usingSkill.Any(x => x.skillName == pattern.skill.skillName))
            {
                    while (skillCastLock && pattern.isCastingNotCast)
                    {
                        yield return null; // SkillCasting이 false가 될 때까지 대기
                    Debug.Log($"while 내부: skillCastLock={skillCastLock}, isCastingNotCast={pattern.isCastingNotCast}");
                }

                if (pattern.condition.isactive && !IsEffectCondition(character, character, pattern.condition.target
                        , pattern.condition.type
                        , pattern.condition.comparison
                        , pattern.condition.value))
                {
                continue; // 조건 불만족 시 다음 반복으로 넘어감
                }


                Stats target = GetClosestCharacter(character, pattern.index, pattern.reverse_order, pattern.Designation, pattern.target);
                if (target == character)
                    continue; // 대상이 자기인 경우 종료

                yield return new WaitForSeconds(pattern.delay);
                    int index = character.usingSkill.FindIndex(x => x.skillName == pattern.skill.skillName);

                //CharacterSelection.Instance.SelectCharacter2P(character.characterNumber);
                //CharacterSelection.selectedCharacterIndex = character.characterNumber;


                Vector3 rotatoin = Vector3.zero;// 기본값 초기화
                switch(pattern.RotationType) // 방향 방식에 따라 방향 지정
                {
                    case Rotation.none:
                        rotatoin = Vector3.zero;
                        break;
                    case Rotation.self:
                        rotatoin = pattern.Rotation;
                        break;
                    case Rotation.Character:
                        rotatoin = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation, pattern.target).charPosition;
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
                    case TargetTypeX.self:
                        targetPositionX = character.charPosition.x + (pattern.coordinate).x; // 자기 위치
                        break;
                    case TargetTypeX.Character:
                        targetPositionX = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation, pattern.target).charPosition.x;
                        break;
                    case TargetTypeX.Skill:
                        targetPositionX = gameObject.transform.position.x; // 임의
                        break;
                }


                float targetPositionY = 0; // 기본값 초기화
                switch (pattern.targetTypeY) // Y축 타겟팅 방식에 따라 위치 지정
                {
                    case TargetTypeY.none:
                        targetPositionY = (pattern.coordinate).z;
                        break;
                    case TargetTypeY.self:
                        targetPositionY = character.charPosition.z + (pattern.coordinate).z; // 자기 위치
                        break;
                    case TargetTypeY.Character:
                        targetPositionY = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation, pattern.target).charPosition.z;
                        break;
                    case TargetTypeY.Skill:
                        targetPositionY = gameObject.transform.position.z; // 임의
                        break;
                }

                Vector3 targetPosition = Vector3.zero; // 기본값 초기화
                targetPosition = new Vector3(targetPositionX+ (pattern.coordinate).x, 0f, targetPositionY+ (pattern.coordinate).y); // Y축은 0으로 설정

                Stats targetObject = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation, pattern.target);


                // 스킬 데이터 가져오기
                SkillData GetSkill = skillManager.SkillAutoSelected(index, character.characterNumber).skill;

                // 스킬 캐스터 가져오기
                GameObject GetCaster = skillManager.SkillAutoSelected(index, character.characterNumber).caster;

                // 스킬 캐스터의 Stats 가져오기
                Stats GetStats = skillManager.SkillAutoSelected(index, character.characterNumber).stats;

                // 스킬 위치 자동 계산
                Vector3 GetTargetPos = skillManager.SkillPositionAuto(GetSkill, GetStats, true,
                        rotatoin, targetPosition, null).targetPosition;

                // 스킬 중앙 자동 계산
                Vector3 GetAoeCenterPos = skillManager.SkillPositionAuto(GetSkill, GetStats, true,
                        rotatoin, targetPosition, null).aoeCenterPosition;

                // 위치 유효성 계산
                bool effectiveness = skillManager.SkillPositionAuto(GetSkill, GetStats, true,
                        rotatoin, targetPosition, null).effectiveness;

                int skillCode = 0;


                skillManager.selectedTargetUnit = null;
                skillManager.ConfirmSkill(character.team,
                    GetSkill,
                    GetCaster,
                    GetStats,
                    GetTargetPos,
                    GetAoeCenterPos,
                    null,
                    effectiveness,
                    ref skillCode
                    );
                skillCastLock = pattern.isCastingNotCast; // 스킬 시전 잠금 설정
                skillManager.SkillAutoCast(character.team, skillCode);
                SkillCasting = true; // 스킬 시전 중으로 설정
            }
        }
        character.isPatternEnd = true; // 패턴 종료 상태로 설정
    }

    public Vector3 GetClosestCharacterPosition(Stats GetClosestCharacter)
    {
        return GetClosestCharacter.charPosition;
    }

    public Stats GetClosestCharacter(Stats self, int n, bool reverse, DesignationType type, TargetTeam targetTeam)
    {
        // 자기 자신, 죽은 캐릭터, 같은 팀 제외
        var candidates = new List<Stats>();
        foreach (var character in CharacterStats.Instance.characterList)
        {
            if (character == self || character.isdie)
                continue;

            switch (targetTeam)
            {
                case TargetTeam.enemy:
                    if (character.team == self.team) continue;
                    break;
                case TargetTeam.team:
                    if (character.team != self.team) continue;
                    break;
                case TargetTeam.all:
                    // 모두 포함 (자기 자신은 이미 제외)
                    break;
            }
            candidates.Add(character);
        }

        // 후보 캐릭터가 없으면 자기 자신 반환
        if (candidates.Count == 0)
            return self;

        switch (type)
        {
            case DesignationType.none:
                return self;
            case DesignationType.hp:
                // HP 기준 정렬 (reverse가 true면 HP 낮은 순)
                candidates.Sort((a, b) => a.hp.CompareTo(b.hp));
                break;
            case DesignationType.hpRatio:
                candidates.Sort((a, b) =>
                    ((float)a.hp / a.maxhp).CompareTo((float)b.hp / b.maxhp));
                break;
            case DesignationType.distance:
                // 거리순 정렬 (reverse가 true면 역순)
                candidates.Sort((a, b) =>
                    Vector3.Distance(self.charPosition, a.charPosition)
                    .CompareTo(Vector3.Distance(self.charPosition, b.charPosition)));
                break;
            case DesignationType.characterNumber:
                candidates.Sort((a, b) => a.characterNumber.CompareTo(b.characterNumber));
                break;
        }

        if (reverse)
            candidates.Reverse();

        // n번째(1-based) 캐릭터 반환
        if (candidates.Count >= n)
            return candidates[n - 1];
        else if (candidates.Count > 0)
            return reverse ? candidates[candidates.Count - 1] : candidates[0];
        else
            return self;
    }


    public bool IsEffectCondition(Stats targetStats, Stats casterStats, 
        Target target, AttributeType type, Condition_statement comparison, float value)
    {
        var cond = ConditionBuilder
            .Attribute(target, type, comparison, value)
            .Build();

        return cond(casterStats, targetStats, null, Team.team);
    }
}
