using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System.Xml.Linq;
using System.Collections;
using System;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class AICastSkill : MonoBehaviour
{
    public TurnManager turnManager;
    public SkillManager skillManager;

    public bool SkillCasting = false; // 스킬 시전 중인지 여부'
    public bool skillCastLock = false; //다음 스킬 시전 잠금
    public List<SkillQueue> patternsQueue; //패턴 스킬큐

    private Coroutine skillCoroutine = null;


    private void Awake()
    {

    }

    public void Start()
    {
        turnManager = TurnManager.Instance;
        skillManager = SkillManager.Instance;
        patternsQueue = new List<SkillQueue>();
    }

    void OnEnable()
    {
        EventManager.Instance.TurnEnd -= OnTurnEnd; // 혹시 남아있을 구독 제거
        EventManager.Instance.TurnEnd += OnTurnEnd;
    }

    void OnDestroy()
    {

        // 이벤트 구독 해제
        if (EventManager.Instance != null)
            EventManager.Instance.TurnEnd -= OnTurnEnd;
    }
    private void OnTurnEnd()
    {
        if (!gameObject.activeInHierarchy) return;

        CharacterStats manager = CharacterStats.Instance;
        Stats character = manager.GetStats(gameObject);

        if (character.aIPattern.patterns == null)
            return;

        int patternCount = (turnManager.Turn) % character.aIPattern.patterns.Count == 0 ?
        character.aIPattern.patterns.Count - 1 : (turnManager.Turn) % character.aIPattern.patterns.Count - 1;

        if (character.aIPattern.patterns[patternCount] == null)
            return;

        List<SkillQueue> patterns = new List<SkillQueue>();
        for (int i = 0; i < character.aIPattern.patterns[patternCount].Count; i++)
        {
            Debug.Log(character.aIPattern.patterns[patternCount][i].statusType);
            Debug.Log(AnyEnemyHasStatus(character, character.aIPattern.patterns[patternCount][i].statusType));

            if (AnyEnemyHasStatus(character, character.aIPattern.patterns[patternCount][i].statusType)
                || (character.aIPattern.patterns[patternCount][i].statusType == StatusType.none))
            {
                Pattern pattern = character.aIPattern.patterns[patternCount][i];
                int Count = Random.Range(
                    pattern.count_repeat,
                    pattern.count_repeat +
                    pattern.count_repeat_Random);
                for (int j = 0; j < Count; j++)
                {
                    List<SkillQueue> skill_repeat = new List<SkillQueue>(pattern.skill_repeat);
                    skill_repeat = pattern.isindex_mix ? ShuffleList(skill_repeat) : skill_repeat;

                    // 무작위 인덱스에 삽입하는 코드:
                    // +1은 리스트 끝에도 삽입 가능하게 함
                    int idx = patterns.Count + 1 - pattern.Random_index >= 0 ? patterns.Count + 1 - pattern.Random_index : 0;
                    int randomIndex = pattern.isRandom_index ? Random.Range(idx, patterns.Count + 1) : patterns.Count;

                    patterns.InsertRange(randomIndex, skill_repeat);
                }
            }
        }

        patternsQueue = patterns == null ? null : new List<SkillQueue>(patterns);

        if (patternsQueue == null)
            return;

        if(skillCoroutine != null)
        {
            StopCoroutine(OnTurnEndCoroutine());
        }
        skillCoroutine = StartCoroutine(OnTurnEndCoroutine());

        //character.isPatternEnd = true; // 패턴 종료 상태로 설정
    }
    //랜덤섞기
    private List<int> ShuffleIndices(int count, int? seed = null)
    {
        var indices = Enumerable.Range(0, count).ToList();
        if (indices.Count <= 1) return indices;

        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        // Fisher-Yates
        for (int k = indices.Count - 1; k > 0; k--)
        {
            int r = rng.Next(k + 1);
            int tmp = indices[k];
            indices[k] = indices[r];
            indices[r] = tmp;
        }
        return indices;
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        int random1, random2;
        T temp;

        for (int i = 0; i < list.Count; ++i)
        {
            random1 = Random.Range(0, list.Count);
            random2 = Random.Range(0, list.Count);

            temp = list[random1];
            list[random1] = list[random2];
            list[random2] = temp;
        }

        return list;
    }

    public void OnDisable()
    {
        StopAllCoroutines(); // 모든 코루틴 중지

        // 이벤트 구독 해제
        EventManager.Instance.TurnEnd -= OnTurnEnd;
    }

    private IEnumerator OnTurnEndCoroutine()
    {
        

        CharacterStats manager = CharacterStats.Instance;
        Stats character = manager.GetStats(gameObject);
        character.isPatternEnd = false;

/*        if (character.aIPattern.skillQueueList == null || character.aIPattern.skillQueueList.Count == 0)
        {
            character.isPatternEnd = true; // 패턴 종료 상태로 설정
            yield break;
        }*/

        if (character.usingSkill == null)
        {
            character.isPatternEnd = true;
            yield break;
        }


/*        if (character.aIPattern.skillQueueList == null ||
        character.aIPattern.skillQueueList.Count <= 1 ||
        character.aIPattern.skillQueueList[patternCount] == null)
        {
            character.isPatternEnd = true; // 패턴 종료 상태로 설정
            yield break;
        }*/

        for (int i = 0; i < patternsQueue.Count; i++)
        {

            var pattern = patternsQueue[i];
            if (pattern.skill == null) continue;

            // 일정턴이 되어야 스킬 발동
            if(pattern.currentIndex >= turnManager.Turn) continue;

            var targetSkill = new SkillData(pattern.skill, character.name);

            // 사용 중인 스킬에 해당 스킬이 없으면 다음 패턴으로 넘어감
            if (!character.usingSkill.Any(x => x.skillName == pattern.skill.skillName)) continue; 


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


                //Stats target = GetClosestCharacter(character, pattern.index, pattern.reverse_order, pattern.Designation, pattern.target);
                Stats target = skillManager.defendingCharacter;
                if (target == character)
                    continue; // 대상이 자기인 경우 종료

            if (pattern.delay > 0)
            {
                // delay부터 delay+delayRandom 사이의 랜덤 값 생성
                float randomDelay = pattern.delay;
                if (pattern.delayRandom > 0)
                {
                    // 랜덤 값 생성 (최소값=delay, 최대값=delay+delayRandom)
                    float rawRandom = Random.Range(pattern.delay, pattern.delay + pattern.delayRandom);
                    // 소수점 둘째 자리까지 반올림
                    randomDelay = Mathf.Round(rawRandom * 100f) / 100f;
                   
                }
                yield return new WaitForSeconds(randomDelay);
            }
                    int index = character.usingSkill.FindIndex(x => x.skillName == pattern.skill.skillName);

                //CharacterSelection.Instance.SelectCharacter2P(character.characterNumber);
                //CharacterSelection.selectedCharacterIndex = character.characterNumber;


                Vector3 rotatoin = Vector3.zero;// 기본값 초기화
                switch(pattern.RotationType) // 방향 방식에 따라 방향 지정
                {
                    case Rotation.none:
                        rotatoin = pattern.Rotation;
                    break;
                    case Rotation.self:
                        rotatoin = pattern.Rotation;
                        break;
                    case Rotation.Character:
                        rotatoin = target.charPosition;
                        break;
                    case Rotation.Skill:
                        rotatoin = Vector3.zero; // 임의
                        break;
                }

                float targetPositionX = 0; // 기본값 초기화
                switch (pattern.targetTypeX) // X축 타겟팅 방식에 따라 위치 지정
                {
                    case TargetTypeX.none:
                        targetPositionX = 0;
                        break;
                    case TargetTypeX.self:
                        targetPositionX = character.charPosition.x; // 자기 위치
                        break;
                    case TargetTypeX.Character:
                        targetPositionX = target.charPosition.x;
                        break;
                    case TargetTypeX.Skill:
                        targetPositionX = gameObject.transform.position.x; // 임의
                        break;
                }


                float targetPositionY = 0; // 기본값 초기화
                switch (pattern.targetTypeY) // Y축 타겟팅 방식에 따라 위치 지정
                {
                    case TargetTypeY.none:
                        targetPositionY = 0;
                        break;
                    case TargetTypeY.self:
                        targetPositionY = character.charPosition.z; // 자기 위치
                        break;
                    case TargetTypeY.Character:
                        targetPositionY = target.charPosition.z;
                        break;
                    case TargetTypeY.Skill:
                        targetPositionY = gameObject.transform.position.z; // 임의
                        break;
                }

                Vector3 targetPosition = Vector3.zero; // 기본값 초기화


            //숫자랜덤
            int XRandom = Mathf.RoundToInt(Random.Range((pattern.coordinate).x, (pattern.coordinate).x + (pattern.coordinateRandom).x));
            int ZRandom = Mathf.RoundToInt(Random.Range((pattern.coordinate).z, (pattern.coordinate).z + (pattern.coordinateRandom).z));

            targetPosition = new Vector3(targetPositionX + XRandom, 0f, targetPositionY + ZRandom);// Y축은 0으로 설정


            // 스킬 데이터 가져오기
            SkillData GetSkill = skillManager.SkillAutoSelected(index, character.characterNumber).skill;

                // 스킬 캐스터 가져오기
                GameObject GetCaster = skillManager.SkillAutoSelected(index, character.characterNumber).caster;

                // 스킬 캐스터의 Stats 가져오기
                Stats GetStats = skillManager.SkillAutoSelected(index, character.characterNumber).stats;


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


                skillManager.ConfirmSkill(character.isPlayerTeam,
                    GetSkill,
                    GetCaster,
                    GetStats,
                    GetTargetPos,
                    GetAoeCenterPos,
                    GetTarget,
                    effectiveness,
                    ref skillCode
                    );
                skillCastLock = pattern.isCastingNotCast; // 스킬 시전 잠금 설정
                skillManager.SkillAutoCast(character.isPlayerTeam, skillCode);
                Debug.Log("ai스킬 실행완료");
                SkillCasting = true; // 스킬 시전 중으로 설정
            
        }
        character.isPatternEnd = true; // 패턴 종료 상태로 설정
        patternsQueue = new List<SkillQueue>();
    }

    public Vector3 GetClosestCharacterPosition(Stats GetClosestCharacter)
    {
        return GetClosestCharacter.charPosition;
    }

    public Stats GetClosestCharacter(Stats self, int n, bool reverse, DesignationType type, TargetTeam targetTeam)
    {
        // 자기 자신, 죽은 캐릭터, 같은 팀 제외
        var candidates = new List<Stats>();
        foreach (Stats character in CharacterStats.Instance.characterList)
        {
            if (character == self || character.isdie)
                continue;

            switch (targetTeam)
            {
                case TargetTeam.enemy:
                    if (character.isPlayerTeam == self.isPlayerTeam) continue;
                    break;
                case TargetTeam.team:
                    if (character.isPlayerTeam != self.isPlayerTeam) continue;
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




    ///////////////////////////////////////////////////////////

    public void ConditionPattern(Stats ch)
    {



    }

    public bool AnyEnemyHasStatus(Stats self, StatusType status)
    {
        var manager = CharacterStats.Instance;
        if (manager == null) return false;

        var list = manager.characterList;
        if (list == null) return false;

        foreach (var s in list)
        {
            if (s == null) continue;
            if (s.isdie) continue;           // 죽은 캐릭터 무시
            if (s.isPlayerTeam == self.isPlayerTeam) continue; // 같은 팀이면 무시 (적만 검사)
            if (s.HasStatus(status)) return true;
        }

        return false;
    }
}
