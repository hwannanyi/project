using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System.Xml.Linq;
using System.Collections;
using System;
using static Stats;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;
public class PassiveSkillCast : MonoBehaviour
{
    public static PassiveSkillCast Instance;
    public TurnManager turnManager;
    public SkillManager skillManager;

    public bool SkillCasting = false; // 스킬 시전 중인지 여부'
    public bool skillCastLock = false; //다음 스킬 시전 잠금
    //public bool AI = false; //AI

    private void Awake()
    {
        skillManager = GetComponent<SkillManager>();
        turnManager = GetComponent<TurnManager>();

        EventManager.Instance.TurnEnd += OnTurnEnd;
            // 싱글턴 패턴 적용 (중복 방지)
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject); // 이미 인스턴스가 존재하면 새로운 객체 삭제
                return;
            }
    }

    public Stats selfStats; // 자신의 Stats
    public Team team;       // 자신의 팀

    // 예시: "Goblin" 캐릭터가 적중했을 때만 효과 발동   self.passiveSkill.Count
    private ConditionPredicate hitCondition;


    // 유닛 충돌 이벤트에서 호출
    public void OnHitPassive(Stats self, Stats target, SkillData skillData)
    {
        if (self.passiveSkill == null)
        {
            return; // 패시브 스킬이 없으면 종료
        }
        for(int skillnumber = 0; skillnumber < self.passiveSkill.Count; skillnumber++ ) // 패시브 스킬이 있는지 확인
        {
            
            SkillData Skill = self.passiveSkill[skillnumber].passive;//패시브 스킬 불러오기
            if(Skill == null)
            {
                return; // 패시브 스킬이 없으면 종료
            }
            ConditionHit condition = self.passiveSkill[skillnumber].conditionHit;//패시브 스킬 불러오기
            SkillAutoCast passiveTarget = self.passiveSkill[skillnumber].passiveTarget;//패시브 스킬 불러오기

            bool asd = HitCondition(target, self, skillData, condition.target, condition.type,
                condition.comparison, condition.value);
            Debug.Log(asd);
            if (condition.isactive && HitCondition(target, self, skillData, condition.target, condition.type, 
                condition.comparison, condition.value))// 불러올 패시브 스킬이 맞으면 실행
            {

                AutoCast(passiveTarget, self, Skill, null);
            }
        }
    }

    // 가장 가까운 타일을 찾는 메서드
    private Vector3 GetNearestTile(Vector3 currentPosition)
    {
        // 현재 위치에서 가장 가까운 타일의 좌표를 계산
        float x = Mathf.Round(currentPosition.x);
        float y = Mathf.Round(currentPosition.z);

        return new Vector3(x, 0f, y);  // 가장 가까운 타일로 반환
    }


    public void AutoCast(
        SkillAutoCast targetRule, 
        Stats caster, 
        //Stats target, 
        SkillData skill,
        GameObject skillObj
        )
    {
        Stats target = GetClosestCharacter(caster, targetRule.index, 
            targetRule.reverse_order, targetRule.Designation, targetRule.target);
        if (target == null)
        {
            return; // 타겟이 없으면 다음 반복으로 넘어감
        }

        Vector3 rotatoin = Vector3.zero;// 기본값 초기화
        switch (targetRule.RotationType) // 방향 방식에 따라 방향 지정
        {
            case Rotation.none:
                rotatoin = Vector3.zero;
                break;
            case Rotation.self:
                rotatoin = targetRule.Rotation;
                break;
            case Rotation.Character:
                rotatoin = target.charPosition;
                break;
            case Rotation.Skill:
                rotatoin = GetNearestTile(skillObj.transform.position); // 임의
                break;
        }

        float targetPositionX = 0; // 기본값 초기화
        switch (targetRule.targetTypeX) // X축 타겟팅 방식에 따라 위치 지정
        {
            case TargetTypeX.none:
                targetPositionX = (targetRule.coordinate).x;
                break;
            case TargetTypeX.self:
                targetPositionX = caster.charPosition.x + (targetRule.coordinate).x; // 자기 위치
                break;
            case TargetTypeX.Character:
                targetPositionX = target.charPosition.x;
                break;
            case TargetTypeX.Skill:
                targetPositionX = GetNearestTile(skillObj.transform.position).x; // 임의
                break;
        }


        float targetPositionY = 0; // 기본값 초기화
        switch (targetRule.targetTypeY) // Y축 타겟팅 방식에 따라 위치 지정
        {
            case TargetTypeY.none:
                targetPositionY = (targetRule.coordinate).z;
                break;
            case TargetTypeY.self:
                targetPositionY = caster.charPosition.z + (targetRule.coordinate).z; // 자기 위치
                break;
            case TargetTypeY.Character:
                targetPositionY = target.charPosition.z;
                break;
            case TargetTypeY.Skill:
                targetPositionY = GetNearestTile(skillObj.transform.position).z; // 임의
                break;
        }

        Vector3 targetPosition = Vector3.zero; // 기본값 초기화
        targetPosition = new Vector3(targetPositionX + (targetRule.coordinate).x,
            0f, targetPositionY + (targetRule.coordinate).z); // Y축은 0으로 설정

        /*
                    Stats targetObject = GetClosestCharacter(enemy, pattern.index,
                                pattern.reverse_order, pattern.Designation);*/


        int index = caster.usingSkill.FindIndex(x => x.skillName == skill.skillName);
        // 스킬 데이터 가져오기
        SkillData GetSkill = skillManager.SkillAutoSelected(index, caster.characterNumber).skill;

        // 스킬 캐스터 가져오기
        GameObject GetCaster = skillManager.SkillAutoSelected(index, caster.characterNumber).caster;

        // 스킬 캐스터의 Stats 가져오기
        Stats GetStats = skillManager.SkillAutoSelected(index, caster.characterNumber).stats;

        // 타겟 오브젝트 가져오기
        GameObject GetTarget = target.characterPrefab;

        // 스킬 위치 자동 계산
        Vector3 GetTargetPos = skillManager.SkillPositionAuto(GetSkill, GetStats, true,
                rotatoin, targetPosition, GetTarget).targetPosition;

        // 스킬 중앙 자동 계산
        Vector3 GetAoeCenterPos = skillManager.SkillPositionAuto(GetSkill, GetStats, true,
                rotatoin, targetPosition, GetTarget).aoeCenterPosition;



        // 위치 유효성 계산
        bool effectiveness = skillManager.SkillPositionAuto(GetSkill, GetStats, true,
                rotatoin, targetPosition, GetTarget).effectiveness;

        int skillCode = 0;

        skillManager.ConfirmSkill(GetStats.team,
            GetSkill,
            GetCaster,
            GetStats,
            GetTargetPos,
            GetAoeCenterPos,
            GetTarget,
            effectiveness,
            ref skillCode
            );
        //skillCastLock = pattern.isCastingNotCast; // 스킬 시전 잠금 설정
        skillManager.SkillAutoCast(GetStats.team, skillCode);
        SkillCasting = true; // 스킬 시전 중으로 설정
        Debug.Log("스킬실행완료");
    }

    public bool HitCondition(Stats self, Stats target2, SkillData skillData,
        Target target, TargetUnit targetUnit, Condition_Hit condition_Hit, string name)
    {
        // 조건
        var cond = ConditionBuilder.HitCondition(
            target,
            targetUnit,
            condition_Hit,
        name
        )
        .Build();

        return cond(self, target2, skillData, Team.team);
    }

    public bool IsEffectCondition(Stats targetStats, Stats casterStats, SkillData skillData,
    Target target, AttributeType type, Condition_statement comparison, float value)
    {
        var cond = ConditionBuilder
            .Attribute(target, type, comparison, value)
            .Build();

        return cond(casterStats, targetStats, skillData, Team.team);
    }

    void OnDestroy()
    {

        // 이벤트 구독 해제
        if (EventManager.Instance != null)
            EventManager.Instance.TurnEnd -= OnTurnEnd;
    }
    private void OnTurnEnd()
    {
/*        var manager = CharacterStats.Instance;
        var character = manager.GetStats(gameObject);
        if (character.aIPattern.skillQueueList == null)
            return;
        StartCoroutine(OnTurnEndCoroutine());*/

    }

/*    private IEnumerator OnTurnEndCoroutine()
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

            Debug.Log("d");

            var pattern = character.aIPattern.skillQueueList[patternCount][i];
            var targetSkill = new SkillData(pattern.skill, character.name, false);
            if (character.usingSkill.Any(x => x.skillName == pattern.skill.skillName))
            {

                while (skillCastLock && pattern.isCastingNotCast)
                {
                    yield return null; // SkillCasting이 false가 될 때까지 대기
                }

*//*
                if (pattern.condition.isactive && !IsEffectCondition(character, character, pattern.condition.target
                    , pattern.condition.type
                    , pattern.condition.comparison
                    , pattern.condition.value))
                {
                    continue; // 조건 불만족 시 다음 반복으로 넘어감
                }*//*


                yield return new WaitForSeconds(pattern.delay);
                int index = character.usingSkill.FindIndex(x => x.skillName == pattern.skill.skillName);
                //CharacterSelection.Instance.SelectCharacter2P(character.characterNumber);
                //CharacterSelection.selectedCharacterIndex = character.characterNumber;
                skillManager.PrepareSkillCast(index, character.characterNumber);

                Vector3 rotatoin = Vector3.zero;// 기본값 초기화
                switch (pattern.RotationType) // 방향 방식에 따라 방향 지정
                {
                    case Rotation.none:
                        rotatoin = gameObject.transform.position + pattern.Rotation;
                        break;
                    case Rotation.Character:
                        rotatoin = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation, targetRule.target.charPosition;
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
                        targetPositionX = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation, targetRule.target).charPosition.x;
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
                    case TargetTypeY.Character:
                        targetPositionY = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation).charPosition.z;
                        break;
                    case TargetTypeY.Skill:
                        targetPositionY = gameObject.transform.position.z; // 임의
                        break;
                }

                Vector3 targetPosition = Vector3.zero; // 기본값 초기화
                targetPosition = new Vector3(targetPositionX + (pattern.coordinate).x, 0f, targetPositionY + (pattern.coordinate).y); // Y축은 0으로 설정

                Stats targetObject = GetClosestCharacter(character, pattern.index,
                            pattern.reverse_order, pattern.Designation);

                skillManager.CalculateSkillPosition(character.usingSkill[index], character, true,
                        rotatoin, targetPosition, targetObject.characterPrefab);
                skillManager.selectedTargetUnit = null;
                skillManager.ConfirmSkillCast(character.team);
                skillCastLock = pattern.isCastingNotCast; // 스킬 시전 잠금 설정
                skillManager.SkillCastAI(character.team, 0);
                SkillCasting = true; // 스킬 시전 중으로 설정
                Debug.Log("스킬실행완료");
            }
        }

    }*/


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
            return null;
    }



}
