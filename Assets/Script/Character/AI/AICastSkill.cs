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
    public CharacterSelection characterSelection;

    public bool SkillCasting = false; // 스킬 시전 중인지 여부'
    public bool skillCastLock = false; //다음 스킬 시전 잠금
    public List<SkillQueue> patternsQueue; //패턴 스킬큐
    public List<SkillQueue> patternsQueue_turn_end; //패턴 스킬큐

    public int patternCount = 0; // AI 패턴 횟수
    public int nowPattern = 0;
    public List<int> nowPatternCount =new();

    private Coroutine skillCoroutine = null;

    public void Awake()
    {
        EventManager.Instance.TurnEnd -= OnTurnEnd; // 혹시 남아있을 구독 제거
        EventManager.Instance.TurnEnd += OnTurnEnd;
        TurnManager.TurnEnd += TurnEndPattern;
    }

    public void Start()
    {
        nowPattern = 0;
        turnManager = TurnManager.Instance;
        skillManager = SkillManager.Instance;
        patternsQueue = new List<SkillQueue>();
        OnTurnEnd();
    }

    void OnEnable()
    {


        EventManager.Instance.isMove += ServeTurnPatternStart;
        SkillManager.SkillCast += ServeTurnPatternStart;
    }

    void OnDestroy()
    {
        EventManager.Instance.TurnEnd -= OnTurnEnd;
        TurnManager.TurnEnd -= TurnEndPattern;
                EventManager.Instance.isMove -= ServeTurnPatternStart;
                SkillManager.SkillCast -= ServeTurnPatternStart;
    }
    public void OnTurnEnd()
    {

        if (!gameObject.activeInHierarchy) return;
        nowPattern = 0;
        nowPatternCount = new List<int>();

        CharacterStats manager = CharacterStats.Instance;
        Stats character = manager.GetStats(gameObject);

        if (character.noPattern == true)
        {
            character.isPatternEnd = true; // 패턴 종료 상태로 설정
            return;
        }

        List<List<Pattern>> patternList = TurnManager.Instance.isTurn_cooperation ?
            character.aIPattern.patterns_turn_alone :
            character.aIPattern.patterns_turn_cooperation;

        int turn = TurnManager.Instance.isTurn_cooperation ? TurnManager.Instance.turn_alone : TurnManager.Instance.turn_cooperation;

        int patternCount = turn % patternList.Count == 0 ?
        patternList.Count - 1 : turn % patternList.Count - 1;



        if (patternList[patternCount] == null)
            return;

        List<SkillQueue> patterns = new List<SkillQueue>();
        for (int i = 0; i < patternList[patternCount].Count; i++)
        {
            if (AnyEnemyHasStatus(character, patternList[patternCount][i].statusType)
                || (patternList[patternCount][i].statusType == StatusType.none))
            {
                Pattern pattern = patternList[patternCount][i];
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

                    if (!pattern.at_once)
                        continue;
                    for (int k = 0; k < pattern.at_onces.Count; k++)
                        nowPatternCount.Add(pattern.at_onces[k]);
                }
            }
        }
        patternsQueue = patterns == null ? null : new List<SkillQueue>(patterns);
        Pattern_Turn_alone_end();

        if (patternsQueue == null)
            return;
        if(TurnManager.Instance.isTurn_cooperation)
            return;


        if (skillCoroutine != null)
        {
            StopCoroutine(OnTurnEndCoroutine());
        }
        skillCoroutine = StartCoroutine(OnTurnEndCoroutine());

        //character.isPatternEnd = true; // 패턴 종료 상태로 설정
    }

    public void Pattern_Turn_alone_end() 
    {
        if(!TurnManager.Instance.isTurn_cooperation)
            return;

        CharacterStats manager = CharacterStats.Instance;
        Stats character = manager.GetStats(gameObject);

                List<List<Pattern>> patternList = TurnManager.Instance.isTurn_cooperation ?
            character.aIPattern.patterns_turn_cooperation_end :
            character.aIPattern.patterns_turn_cooperation_end;

        int turn = TurnManager.Instance.isTurn_cooperation ? TurnManager.Instance.turn_alone : TurnManager.Instance.turn_cooperation;

        int patternCount = turn % patternList.Count == 0 ?
        patternList.Count - 1 : turn % patternList.Count - 1;



        if (patternList[patternCount] == null)
            return;

        List<SkillQueue> patterns = new List<SkillQueue>();
        for (int i = 0; i < patternList[patternCount].Count; i++)
        {
            if (AnyEnemyHasStatus(character, patternList[patternCount][i].statusType)
                || (patternList[patternCount][i].statusType == StatusType.none))
            {
                Pattern pattern = patternList[patternCount][i];
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
        patternsQueue_turn_end = patterns == null ? null : new List<SkillQueue>(patterns);
    }

    public void ServeTurnPatternStart()
    {
        CharacterStats manager = CharacterStats.Instance;
        Stats character = manager.GetStats(gameObject);
        if (!turnManager.isTurn_cooperation) return;

        if (character.noPattern == true)
        {
            //character.isPatternEnd = true; // 패턴 종료 상태로 설정
            return;
        }
        StartCoroutine(ServeTurnPatten(nowPattern, nowPatternCount[nowPattern % nowPatternCount.Count]));
        nowPattern += nowPatternCount[nowPattern % nowPatternCount.Count];
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

    public IEnumerator OnTurnEndCoroutine()
    {


        CharacterStats manager = CharacterStats.Instance;
        CharacterSelection characterSelection = CharacterSelection.Instance;
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
            if (pattern.currentIndex >= turnManager.Turn) continue;

            var targetSkill = new SkillData(pattern.skill, character.name);

            // 사용 중인 스킬에 해당 스킬이 없으면 다음 패턴으로 넘어감
            if (!character.usingSkill.Any(x => x.skillName == pattern.skill.skillName)) continue;


            while (skillCastLock && pattern.isCastingNotCast)
            {
                yield return null; // SkillCasting이 false가 될 때까지 대기
            }

            if (pattern.condition.isactive && !IsEffectCondition(character, character, pattern.condition.target
                    , pattern.condition.type
                    , pattern.condition.comparison
                    , pattern.condition.value))
            {
                continue; // 조건 불만족 시 다음 반복으로 넘어감
            }


            //Stats target = GetClosestCharacter(character, pattern.index, pattern.reverse_order, pattern.Designation, pattern.target);
            Stats target = characterSelection.selectedCharacter;
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
            switch (pattern.RotationType) // 방향 방식에 따라 방향 지정
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

            if (pattern.isWithin_Range)
                targetPosition = Within_Range(targetPosition, character.charPosition, GetSkill.range);


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


            skillManager.ConfirmSkill(character.team,
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
            skillManager.SkillAutoCast(character.team, skillCode);
            Debug.Log("ai스킬 실행완료");
            SkillCasting = true; // 스킬 시전 중으로 설정

        }
        character.isPatternEnd = true; // 패턴 종료 상태로 설정
        patternsQueue = new List<SkillQueue>();
    }


    public void TurnEndPattern()
    {
        CharacterStats manager = CharacterStats.Instance;
        Stats character = manager.GetStats(gameObject);

        if (patternsQueue_turn_end.Count == 0) 
        {
            character.isPatternEnd = true;
            return; 
        }
        StartCoroutine(TurnEndCoroutine());
    }
    public IEnumerator TurnEndCoroutine()
    {
        CharacterStats manager = CharacterStats.Instance;
        CharacterSelection characterSelection = CharacterSelection.Instance;
        Stats character = manager.GetStats(gameObject);

        for (int i = 0; i < patternsQueue_turn_end.Count; i++)
        {

            var pattern = patternsQueue_turn_end[i];
            if (pattern.skill == null) continue;

            // 일정턴이 되어야 스킬 발동
            if (pattern.currentIndex >= turnManager.Turn) continue;

            var targetSkill = new SkillData(pattern.skill, character.name);

            // 사용 중인 스킬에 해당 스킬이 없으면 다음 패턴으로 넘어감
            if (!character.usingSkill.Any(x => x.skillName == pattern.skill.skillName)) continue;


            while (skillCastLock && pattern.isCastingNotCast)
            {
                yield return null; // SkillCasting이 false가 될 때까지 대기
            }

            if (pattern.condition.isactive && !IsEffectCondition(character, character, pattern.condition.target
                    , pattern.condition.type
                    , pattern.condition.comparison
                    , pattern.condition.value))
            {
                continue; // 조건 불만족 시 다음 반복으로 넘어감
            }


            //Stats target = GetClosestCharacter(character, pattern.index, pattern.reverse_order, pattern.Designation, pattern.target);
            Stats target = characterSelection.selectedCharacter;
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
            switch (pattern.RotationType) // 방향 방식에 따라 방향 지정
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

            if (pattern.isWithin_Range)
                targetPosition = Within_Range(targetPosition, character.charPosition, GetSkill.range);


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


            skillManager.ConfirmSkill(character.team,
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
            skillManager.SkillAutoCast(character.team, skillCode);
            SkillCasting = true; // 스킬 시전 중으로 설정

        }
        character.isPatternEnd = true; // 패턴 종료 상태로 설정
        patternsQueue_turn_end = new List<SkillQueue>();
    }

    public IEnumerator ServeTurnPatten(int stidx, int count)
    {
        CharacterStats manager = CharacterStats.Instance;
        CharacterSelection characterSelection = CharacterSelection.Instance;
        Stats character = manager.GetStats(gameObject);
        character.isPatternEnd = false;


        if (character.usingSkill == null)
        {
            character.isPatternEnd = true;
            yield break;
        }


        for (int i = stidx; i < stidx + count; i++)
        {

            var pattern = patternsQueue[i % patternsQueue.Count];
            if (pattern.skill == null) continue;

            // 일정턴이 되어야 스킬 발동
            if (pattern.currentIndex >= turnManager.Turn) continue;

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
            Stats target = characterSelection.selectedCharacter;
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
            switch (pattern.RotationType) // 방향 방식에 따라 방향 지정
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

            if (pattern.isWithin_Range)
                targetPosition = Within_Range(targetPosition, character.charPosition, GetSkill.range);
            

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


            skillManager.ConfirmSkill(character.team,
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
            skillManager.SkillAutoCast(character.team, skillCode);
            Debug.Log("ai스킬 실행완료");
            SkillCasting = true; // 스킬 시전 중으로 설정

        }
    }



    public Vector3 GetClosestCharacterPosition(Stats GetClosestCharacter)
    {
        return GetClosestCharacter.charPosition;
    }

    public Vector3 Within_Range(Vector3 pos, Vector3 chpos, float i)
    {
        Vector3 newPos = pos;

        // 두 점 사이의 거리 계산
        float distance = Vector3.Distance(pos, chpos);

        // 거리가 i보다 크면 제한된 위치로 조정
        if (distance > i)
        {
            // 방향 벡터 계산 (chpos에서 pos로의 방향)
            Vector3 direction = (pos - chpos).normalized;

            // i 거리만큼만 이동한 새 위치 계산
            newPos = chpos + (direction * i);

            // 좌표 반올림 (타일 기반 게임에서 필요)
            newPos.x = Mathf.Round(newPos.x);
            newPos.z = Mathf.Round(newPos.z);
            newPos.y = 0f; // Y축은 0으로 고정
        }

        return newPos;
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
            if (s.team == self.team) continue; // 같은 팀이면 무시 (적만 검사)
            if (s.HasStatus(status)) return true;
        }

        return false;
    }
}
