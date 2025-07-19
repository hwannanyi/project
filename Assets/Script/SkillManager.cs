using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance; // 싱글턴 인스턴스 (전역 접근 가능)
    public List<SkillData> UseSkillList = new(); // 현재 사용 가능한 스킬 목록
    //
    //public GameObject skillPrefab; // 생성할 스킬 프리팹 미사용
    public ButtonHandler uiManager;
    public CharacterUIManager ProfileuiManager; // 캐릭터 프로필 UI 매니저
    public ReactTimeUI reactTimeUIManager; // 대응시간 UI 매니저
    public SkillRangeVisualizer skillRangeVisualizer; // 스킬 범위 시각화 매니저
    public TurnManager turnManager; // 턴 매니저

    public CharacterSelection characterSelection; // 캐릭터 선택 스크립트


    ///선택한 스킬이 일시적으로 저장되는곳
    public SkillData selectedSkill = null;
    [HideInInspector] public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;

    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;

    [HideInInspector] public bool isSkillReady = false;
    [HideInInspector] public bool isSkillReadyFinal = false;

    //private bool isWaitingForReaction = false;    // 대응단계로 인해 중단되었는지 여부


    /// 대응을 위한 대기상태 스킬을 저장하는 변수
    private SkillData pendingSkill;
    private GameObject pendingCaster;
    private Stats pendingCharacter;
    private Vector3 pendingAoeCenterPosition;
    private Vector3 pendingTargetPosition;
    private GameObject pendingTargetUnit = null;
    // 대응 중 한 번만 대응하도록 제한
    private bool hasReacted = false;

    public bool waitingForResponse = false;




    //대응상대 스킬 저장
    private int pendingSelectedCharacterIndex;
    private SkillData pendingReactSkill;
    private GameObject pendingReactCaster;
    private Stats pendingReactCharacter;
    private Vector3 pendingReactAoeCenterPosition;
    private Vector3 pendingReactTargetPosition;
    private GameObject pendingReactTargetUnit = null;


    public GameObject targetIndicator; // 타겟 선택 UI 오브젝트


    public GameObject selectedTargetUnit = null;
    private int selectedTargetIndex = -1;

    public bool hasMovedInReact = false; // 대응단계에서 이동 여부


    public List<ActionWrapper> _skillAction = new();
    public List<ActionWrapper> _reactSkillAction = new();

    public List<ActionWrapper> TeamSkill
    {
        get => _skillAction;
        set
        {
            _skillAction = value;
            SkillSave.Instance.TeamSkill = value;
        }
    }

    public List<ActionWrapper> EnemySkill
    {
        get => _reactSkillAction;
        set
        {
            _reactSkillAction = value;
            SkillSave.Instance.EnemySkill = value;
        }
    }

    public bool isCastingSkill = false; // 스킬 시전 중인지 여부
    public float ReactTime = 0.0f; // 대응단계 시간 (초 단위)

    public List<GameObject> validReactTargets = new(); // 대응 가능 캐릭터 목록

    // 메인 타겟 (타겟팅 스킬일 경우)
    public GameObject validMainTarget = null;

    void Awake()
    {
        skillRangeVisualizer = GetComponent<SkillRangeVisualizer>();
        characterSelection = GetComponent<CharacterSelection>();
        turnManager = GetComponent<TurnManager>();

        validReactTargets = new List<GameObject>();

        // 상태 변수 초기화
        selectedSkill = null;
        selectedCharacter = null;
        isSkillReady = false;
        isSkillReadyFinal = false;
        hasReacted = false;
        waitingForResponse = false;
        hasMovedInReact = false;
        isCastingSkill = false;

        SelectedSkillClear();
        isSkillReady = false;
        isSkillReadyFinal = false;
        // 싱글턴 패턴 적용 (중복 방지)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        _skillAction = new List<ActionWrapper>();
        _reactSkillAction = new List<ActionWrapper>();

    }

    void Update()
    {
        if (CameraZoom.isControlMode) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PrepareSkillCast(0, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 0)
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            PrepareSkillCast(1, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 1)
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PrepareSkillCast(2, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 2)
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PrepareSkillCast(3, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 3)
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            PrepareSkillCast(4, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 4)
        }



        // 대응단계 강제 종료 테스트용 (게임 흐름에 따라 UI 버튼 등으로 대체 가능)
        if (Input.GetKeyDown(KeyCode.M))
        {
/*            if (TurnManager.Instance.IsInReactPhase())
            {
                Debug.Log("대응단계 M키 입력 - 대응스킬 먼저 실행");
                ExecuteReactionThenSkill();     // ← 변경됨
                ResetResponseState();
                //ExecuteSingleSkillWithReactionCheck(); // ← 변경됨
            }
            else
            {*/
                Debug.Log("스킬 실행 시도");
                ExecuteSingleSkillWithReactionCheck(); // 스킬실행
            //}
        }

        // 마우스 클릭으로 타겟 유닛 선택
        if (Input.GetMouseButtonDown(0))
        {
            selectedTargetUnit=null; // 클릭시 타겟 초기화
            //UI클릭시 클릭 무시
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            // 타겟 유닛 선택 로직
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float enter;
            Vector3 mouseWorld = Vector3.zero;
            if (groundPlane.Raycast(ray, out enter))
            {
                mouseWorld = ray.GetPoint(enter);
                mouseWorld.y = 0f;
            }

            Collider[] hits = Physics.OverlapSphere(mouseWorld, 0.1f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Character"))
                {
                    GameObject target = hit.gameObject;

                    if (selectedCharacter != null && selectedSkill != null)
                    {
                        Vector3 unitPos = selectedCharacter.charPosition;
                        Vector3 targetPos = target.transform.position;

                        int tileDist = Mathf.Abs(Mathf.RoundToInt(unitPos.x - targetPos.x)) +
                                       Mathf.Abs(Mathf.RoundToInt(unitPos.z - targetPos.z));

                        if (tileDist > selectedSkill.range)
                        {
                            Debug.LogWarning("[SkillManager] 사거리 밖의 유닛입니다.");
                            return;
                        }
                    }
                    else if(!isSkillReady)
                    {
                        CharacterMovement movement = target.GetComponent<CharacterMovement>();
                        if (movement != null)
                        {
                            int number = movement.characterNumber;
                            CharacterSelection.Instance.SelectCharacter(number);
                        }
                    }

                    selectedTargetUnit = target;
                    selectedTargetIndex = CharacterStats.Instance.characters.IndexOf(target);
                    Debug.Log($"[SkillManager] 대상 선택됨: {target.name}");
                    break; // 첫 번째 캐릭터만 처리
                }
            }
            if (selectedSkill != null && selectedCharacter != null) //스킬 확정 기준
            {
                CalculateSkillPosition(selectedSkill, selectedCharacter,false,Vector3.zero, Vector3.zero, selectedTargetUnit); // 항상 호출해야 함
                if (isSkillReady)
                {

                    if (selectedSkill.targeting && selectedTargetUnit == null)
                        return;

                    // 3. 시전 확정
                    SkillRangeVisualizer.Instance.HideSkillRange();
                    ConfirmSkillCast(selectedCharacter.team); // 위치 계산 성공했을 때만 확정
                    if (TurnManager.Instance.IsInReactPhase())
                    {
                        ExecuteReactionThenSkill(0);
                        ResetResponseState();
                    }

                }

            }

        }


/*        if (Skillaction != null && Skillaction.selectedSkill.targeting && Skillaction.selectedTargetUnit != null)
        {
            targetIndicator.SetActive(true);
        }
        else
        {
            targetIndicator.SetActive(false);
        }*/

    }

   

    /// <summary>
    /// 선택된 캐릭터가 사용할 스킬을 지정하고 시전 준비 상태로 만든다.
    /// </summary>
    /// <param name="skillIndex">선택할 스킬의 인덱스 (예: 0 = Q, 1 = W)</param>
    public void PrepareSkillCast(int skillIndex, int CharacterNumber)
    {
        Skillcancel();
        if (CharacterNumber == -1)
        {
            Debug.LogWarning("캐릭터가 선택되지 않았습니다.");
            return;
        }
        
        var character = CharacterStats.Instance.characterList[CharacterNumber];
        if (character.usingSkill[skillIndex].skillName == null)
        {
            Debug.LogWarning($"선택된 캐릭터의 스킬이 비어있습니다. 인덱스: {skillIndex}");
            return;
        }



        var skill = character.usingSkill[skillIndex];
        GameObject caster = CharacterStats.Instance.characters[CharacterNumber];
        var stats = CharacterStats.Instance;
        var characterStats = stats.GetStats(caster);
        if (characterStats.available == false)
        {
            Debug.Log($"[SkillManager] 캐릭터가 스킬사용불가 상태 입니다: {skill.skillName}");
            return;
        }

        // 임시 저장
        selectedSkill = skill;
        selectedCaster = caster;
        selectedCharacter = character;
        if (skill.cost.ContainsKey(CostType.mp) && skill.cost[CostType.mp] > character.mp)
        {// mp 코스트가 캐릭터 mp보다 많을 때 실행할 코드(mp 부족)
            isSkillReady = false;
            selectedSkill = null;
            selectedCaster = null;
            selectedCharacter = null;
            Debug.Log($"[SkillManager] 코스트가 부족합니다: {skill.skillName}");
            return;
        }
        if (!skill.IsAvailable())
        {
            isSkillReady = false;
            selectedSkill = null;
            selectedCaster = null;
            selectedCharacter = null;
            Debug.Log($"[SkillManager] 스킬이 현제 쿨타임입니다: {skill.skillName}");
            return;
        }

        isSkillReady = true;

        Debug.Log($"[SkillManager] 스킬 선택 완료: {skill.skillName}");

        // 스킬 범위 하이라이트 표시
        if (skill.projectile && !skill.targeting) // 논타겟 투사체라면
        {
            Vector3 start = caster.transform.position;
            Vector3 direction = caster.transform.forward; // 기본값, 실제로는 방향 입력 받아야 함
            float range = skill.range;
            float Xaoe = skill.Xaoe;
            float Yaoe = skill.Yaoe;
            SkillRangeVisualizer.Instance.StartNonTargetProjectileRange(start, Xaoe, Yaoe, range);
        }
        else // 일반 스킬이라면
        {
            Vector3 casterPosition = caster.transform.position;
            float range = skill.range;
            float Xaoe = skill.Xaoe;
            float Yaoe = skill.Yaoe;
            Vector3 mouseWorldPos = casterPosition; // 실제로는 마우스 위치 받아야 함
            if (skill.startSkillPosition != StartSkillPosition.player) 
            { 
                SkillRangeVisualizer.Instance.ShowNormalSkillRange(casterPosition, range);
            }
            SkillRangeVisualizer.Instance.StartSkillRangePreview(casterPosition, Xaoe, Yaoe);
        }

        // 이 시점에서 방향/타겟 UI 활성화
        // 예: ShowTargetingUI(skill.range) 등
    }



    /// <summary>
    /// 선택된 스킬과 캐릭터 정보를 기반으로 방향, 시작위치, 중심점, 타겟 위치를 계산합니다.
    /// 이 함수는 ConfirmSkillCast() 전에 호출되어야 합니다.
    /// </summary>
    public void CalculateSkillPosition(SkillData skill, Stats character,bool AI, Vector3 AIDirection, Vector3 Position, GameObject selectedTargetUnit)
    {
        //isSkillReady = true;
        Vector3 startPosition = Vector3.zero;

        switch (skill.startSkillPosition)
        {
            case StartSkillPosition.player:
                startPosition = character.charPosition;
                break;

            case StartSkillPosition.target:
                {
                    if (selectedTargetUnit == null || selectedTargetIndex == -1)
                    {
                        Debug.LogWarning("[SkillManager] 대상 유닛이 선택되지 않았습니다.");
                        return;
                    }
                    startPosition = selectedTargetUnit.transform.position;
                    break;
                }

            case StartSkillPosition.mouse:
                {

                    if (AI)
                    {
                        startPosition = Position;
                        break;
                    }

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                    float enter;
                    Vector3 rawMouse = Vector3.zero;
                    if (groundPlane.Raycast(ray, out enter))
                    {
                        rawMouse = ray.GetPoint(enter);
                        rawMouse.y = 0f;
                    }

                    int tileDist = Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.x - rawMouse.x)) +
                                   Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.z - rawMouse.z));

                    if (tileDist > selectedSkill.range)
                    {
                        Debug.LogWarning("[SkillManager] 사거리 밖의 위치입니다.");
                        isSkillReady = false;
                        return;
                    }

                    // 기존 위치 보정 로직
                    bool evenX = selectedSkill.Xaoe % 2 == 0;
                    bool evenY = selectedSkill.Yaoe % 2 == 0;
                    float x = evenX ? Mathf.Floor(rawMouse.x) + 0.5f : Mathf.Round(rawMouse.x);
                    float y = evenY ? Mathf.Floor(rawMouse.z) + 0.5f : Mathf.Round(rawMouse.z);

                    startPosition = new Vector3(x, 0f, y);


                    break;
                }

            case StartSkillPosition.special:
                startPosition = Vector3.zero;
                break;

            default:
                startPosition = character.charPosition;
                break;
        }

        // 방향 계산
        Vector3 direction;

        // XZ 평면 기준 4방향
Vector3[] directions = {
    new(0, 0, 1),   // 위(북)
    new(0, 0, -1),  // 아래(남)
    new(-1, 0, 0),  // 왼쪽(서)
    new(1, 0, 0)    // 오른쪽(동)
};
        Vector3 closestDirection = directions[0];
        // 마우스 기반 방향 계산이 필요한 경우
        Vector3 mouseWorldPos = Vector3.zero;

        if (!skill.targeting || selectedTargetUnit == null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float enter;
            if (groundPlane.Raycast(ray, out enter))
            {
                mouseWorldPos = ray.GetPoint(enter);
                mouseWorldPos.y = 0f;
            }
        }
        mouseWorldPos = AI ? Position : mouseWorldPos;

        // ② 방향 계산
        if (skill.targeting && selectedTargetUnit != null)
        {
            direction = (selectedTargetUnit.transform.position - startPosition).normalized;
        }
        else
        {
            

            //AI 캐릭터의 경우, 방향을 AI가 지정한 방향으로 설정
/*            if (AI)
            {
                direction = (AIDirection - startPosition).normalized;
            }
            else
            {*/
                direction = (mouseWorldPos - startPosition).normalized;
            //}

            // 4방향 중 가장 가까운 방향 찾기
            float maxDot = Vector3.Dot(direction, directions[0]);
            foreach (var dir in directions)
            {
                float dot = Vector3.Dot(direction, dir);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    closestDirection = dir;
                }
            }
        }
        /*Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        mouseWorldPos.z = 0f;

        Vector3 direction = (mouseWorldPos - startPosition).normalized;
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        Vector3 closestDirection = directions[0];
        float maxDot = Vector3.Dot(direction, directions[0]);

        foreach (var dir in directions)
        {
            float dot = Vector3.Dot(direction, dir);
            if (dot > maxDot)
            {
                maxDot = dot;
                closestDirection = dir;
            }
        }*/

        // 여기서 타겟팅 스킬인 경우, 타겟 유닛의 위치를 targetPosition으로 강제 지정
        Vector3 targetPosition;
        if (skill.targeting && selectedTargetUnit != null)
        {
            targetPosition = selectedTargetUnit.transform.position;
        }
        else
        {
            // 비타겟팅 스킬 방향 계산

            float dx = Mathf.Abs(mouseWorldPos.x - startPosition.x);
            float dy = Mathf.Abs(mouseWorldPos.z - startPosition.z);

            float mousePlayerRange = dx >= dy ? dx : dy;
            float range = skill.RangeAdjustment ? mousePlayerRange : skill.range;
            targetPosition = startPosition + closestDirection * range;
            targetPosition.y = 0f;
        }

        // AOE 중심 계산
        Vector3 aoeCenterPosition = Vector3.zero;
        Vector3 offset = Vector3.zero;

        switch (skill.aoecenter)
        {
            case AoeCenter.center:
                aoeCenterPosition = startPosition;
                break;

            case AoeCenter.edge:
            case AoeCenter.Rcorner:
            case AoeCenter.Lcorner:
                AoeCenterPosition(skill, closestDirection, startPosition, ref aoeCenterPosition, ref offset);
                break;

            default:
                aoeCenterPosition = startPosition;
                break;
        }

        if (skill.aoecenter == AoeCenter.Rcorner || skill.aoecenter == AoeCenter.Lcorner)
        {
            targetPosition += offset;
        }

        // 계산된 위치 저장
        selectedAoeCenterPosition = aoeCenterPosition;
        selectedTargetPosition = targetPosition;

        // 디버그용 시각화
        SimulateSkillHit.Instance.SimulateForDebug(skill, startPosition, targetPosition, aoeCenterPosition);


        Debug.Log($"[SkillManager] 스킬 위치 계산 완료: 시작={startPosition}, AOE중심={aoeCenterPosition}, 타겟={targetPosition}");
        return;
    }


    /// <summary>
    /// 선택된 스킬의 실행을 확정하고 대응 조건을 판단하여 대응단계로 진입하거나 즉시 시전한다.
    /// 대응단계 중이면 대응자 스킬을 pendingReactSkill로 저장한다.
    /// </summary>
    public void ConfirmSkillCast(Team team)
    {
        isSkillReadyFinal = true;
        //respondingCharacter = selectedCharacter;
        if (!isSkillReady || selectedSkill == null || selectedCaster == null || selectedCharacter == null)
        {
            Debug.LogWarning("[SkillManager] 스킬 준비 상태가 아니거나 정보가 부족합니다.");
            return;
        }

        // 타겟팅 스킬 또는 타겟 기반 스킬일 경우, 대상이 반드시 필요
        if ((selectedSkill.targeting || selectedSkill.startSkillPosition == StartSkillPosition.target)
            && selectedTargetUnit == null)
        {
            Debug.LogWarning("[SkillManager] 타겟팅 스킬인데 타겟이 없습니다.");
            return;
        }

        if (!isSkillReady || selectedSkill == null || selectedCaster == null || selectedCharacter == null)
        {
            Debug.LogWarning("[SkillManager] 스킬 준비 상태가 아니거나 정보가 부족합니다.");
            return;
        }

        /*// 대응단계 중인 경우 → 대응자 쪽 처리
        if (TurnManager.Instance.IsInReactPhase())
        {
            *//*if (hasReacted)
            {
                Debug.LogWarning("[SkillManager] 이미 대응했습니다.");
                return;
            }*//*

            // 이동 허용 → SkillManager 쪽에 이동했음을 알림
            SkillManager.Instance.MarkReactMove();

            // 대응 행동 제한 확인
            if (TurnManager.Instance.IsPlayerReactPhase())
            {
                if (TurnManager.Instance.playerReactTrun <= TurnManager.Instance.playerUseSkillReactTrun)
                    return;

                ++TurnManager.Instance.playerUseSkillReactTrun;
            }
            else
            {
                if (TurnManager.Instance.enemyReactTrun <= TurnManager.Instance.enemyUseSkillReactTrun)
                    return;

                ++TurnManager.Instance.enemyUseSkillReactTrun;
            }

            // 대응자 스킬 저장
            Debug.Log($"[SaveSkill] Skill: {selectedSkill.skillName}, Prefab: {selectedSkill.SkillEffectPrefab}");
            SaveSkill(false);//대응자용 타겟 저장
            hasReacted = true;

            Debug.Log($"[SkillManager] 대응 스킬 저장 완료: {selectedSkill.skillName}");
            return;
        }*/

       
        Debug.Log($"[SaveSkill] Skill: {selectedSkill.skillName}, Prefab: {selectedSkill.SkillEffectPrefab}");
        SaveSkill(team == Team.team ? true : false); // 일반 스킬 저장
        Debug.Log($"[SkillManager] 스킬 저장 완료: {selectedSkill.skillName}" + " " + team);
        SelectedSkillClear();

        // 상태 초기화
        isSkillReady = false;
    }//waitingForResponse = true;

    // SaveSkill: 선택된 스킬을 저장하는 함수
    // isReaction이 true이면 대응 스킬로 처리되어 ReactSkillaction 리스트에 저장됨
    // false이면 일반 스킬로 처리되어 Skillaction 리스트에 저장됨
    /// <summary>
    /// 선택한 스킬을 저장한다
    /// </summary>
    /// <param name="team">아군적군 구분</param>
    public void SaveSkill(bool team = true)
    {
        // 현재 선택된 스킬 정보를 SelectedSkillList 형태로 구성
        var skillInfo = new SelectedSkill
        {
            selectedSkill = selectedSkill,
            selectedCaster = selectedCaster,
            selectedCharacter = selectedCharacter,
            selectedAoeCenterPosition = selectedAoeCenterPosition,
            selectedTargetPosition = selectedTargetPosition,
            selectedTargetUnit = selectedSkill.targeting ? selectedTargetUnit : null,

        };

        // ActionWrapper로 감싸기
        var action = new ActionWrapper
        {
            type = ActionType.Skill,
            skillData = skillInfo
        };

        // 대응 여부에 따라 다른 리스트에 추가
        if (!team)
        {
            // 적이 사용하는 스킬인 경우 EnemySkill 에 저장
            selectedSkill.isreactSkill = true;
            if (EnemySkill == null)
                EnemySkill = new List<ActionWrapper>();
            EnemySkill.Add(action);
        }
        else
        {
            // 팀이 사용하는 스킬인 경우 TeamSkill 에 저장
            selectedSkill.isreactSkill = false;
            if (TeamSkill == null)
                TeamSkill = new List<ActionWrapper>();
            TeamSkill.Add(action);
        }
    }

    /// <summary>
    /// 스킬을 실제로 시전한다
    /// </summary>
    /// <param name="skill">스킬정보</param>
    /// <param name="aoeCenterPosition">스킬의 중심점</param>
    /// <param name="targetPosition">대상</param>
    /// <param name="casterObject">스킬의 실제 오브젝트</param>
    /// <param name="character">스킬을 쓰는 캐릭터</param>
    public void ExecuteSkill(SkillData skill, Vector3 aoeCenterPosition, Vector3 targetPosition, 
        GameObject casterObject, Stats character, GameObject target = null)
    {

        GameObject skillObject = Instantiate(skill.SkillEffectPrefab, aoeCenterPosition, Quaternion.identity);

        if (skill.projectile)
        {
            SkillEffectProjectile effect = skillObject.GetComponent<SkillEffectProjectile>();
            if (effect != null)
                effect.Initialize(skill, targetPosition, casterObject, character, target);
        }
        else
        {
            SkillEffectHitscan effect = skillObject.GetComponent<SkillEffectHitscan>();
            if (effect != null)
                effect.Initialize(skill, targetPosition, casterObject, character, target);
        }

        // 코스트 차감
        if (skill.cost != null && character != null)
        {
            foreach (var pair in skill.cost)
            {
                switch (pair.Key)
                {
                    case CostType.mp:
                        character.mp -= pair.Value;
                        break;
                    case CostType.hp:
                        character.hp -= pair.Value;
                        break;
                        // 필요에 따라 다른 코스트 타입도 추가
                }
            }
        }


        // 쿨타임 시작
        skill.StartCooldown();
        //프로필 업데이트
        ProfileuiManager.ProfileUpdate(character);
        //선택된 스킬범위 삭제
        SkillRangeVisualizer.Instance.StopNonTargetProjectileRange();
        SkillRangeVisualizer.Instance.StopSkillRangePreview();
        Debug.Log($"[SkillManager] 스킬 실행 완료: {skill.skillName}");
        skillRangeVisualizer.StartSkillTargetRangePreview(null);


    }

    private void ExecuteSkill(SelectedSkill skill)
    {

        if (skill == null || skill.selectedSkill == null)
        {
            Debug.LogError("[SkillManager] ExecuteSkill: skill 또는 skill.selectedSkill이 null입니다.");
            return;
        }

        if (skill.selectedSkill.SkillEffectPrefab == null)
        {
            Debug.LogError($"[SkillManager] SkillEffectPrefab이 null입니다. 스킬 이름: {skill.selectedSkill.skillName}");
            return;
        }

        ExecuteSkill(
            skill.selectedSkill,
            skill.selectedAoeCenterPosition,
            skill.selectedTargetPosition,
            skill.selectedCaster,
            skill.selectedCharacter,
            skill.selectedTargetUnit
        );
    }


    /// <summary>
    /// 스킬의 중심점과 시작위치를 정한다
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="closestDirection"></param>
    /// <param name="startPosition"></param>
    /// <param name="aoeCenterPosition"></param>
    /// <param name="Poffset"></param>
    public void AoeCenterPosition(SkillData skill, Vector3 closestDirection, Vector3 startPosition, ref Vector3 aoeCenterPosition, ref Vector3 Poffset)
    {
        float xOffset = (skill.Xaoe - 1) * 0.5f;
        float yOffset = (skill.Yaoe - 1) * 0.5f;
        Vector3 offset = Vector3.zero;
        switch (skill.aoecenter)
        {
            case AoeCenter.center:
                offset = Vector3.zero;
                break;

            case AoeCenter.edge:
                offset = closestDirection * xOffset;
                break;

            case AoeCenter.Rcorner:
                if(closestDirection.x < 0)
                {
                    yOffset = -yOffset;
                }
                if(closestDirection.x == 0)
                    offset = (closestDirection * xOffset) + new Vector3(0f, 0f, -yOffset * closestDirection.y);
                else
                    offset = (closestDirection * xOffset) + new Vector3(0f, 0f, yOffset);
                break;

            case AoeCenter.Lcorner:
                if (closestDirection.x < 0)
                {
                    yOffset = -yOffset;
                }
                if (closestDirection.x == 0)
                    offset = (closestDirection * xOffset) + new Vector3(yOffset * closestDirection.y, 0f, 0f);
                else
                    offset = (closestDirection * xOffset) + new Vector3(0f, 0f, -yOffset);
                break;
        }

        aoeCenterPosition = startPosition + offset;
        Poffset = offset;
    }


    //미사용 함수

/*    // 일반 스킬 실행 함수
    public void ExecuteCurrentSkill(int i)
    {
        if (Skillaction != null && Skillaction.Count > 0)
        {
            var action = Skillaction[i];
            if (action.type == ActionType.Skill && action.skillData != null)
            {
                ExecuteSkill(action.skillData); // SelectedSkill만 전달
            }
            Skillaction.RemoveAt(0); // 실행한 스킬만 제거
        }
    }

    // 대응 스킬 실행 함수
    public void ExecuteReactSkillList()
    {
        if (ReactSkillaction != null && ReactSkillaction.Count > 0)
        {
            var action = ReactSkillaction[0];
            if (action.type == ActionType.Skill && action.skillData != null)
            {
                ExecuteSkill(action.skillData);
            }
            ReactSkillaction.RemoveAt(0);
        }
    }*/



    // 대응 스킬 → 일반 스킬 순차 실행 함수
    public void ExecuteReactionThenSkill(int i)
    {
        if (EnemySkill != null && EnemySkill.Count > 0)
        {
            var action = EnemySkill[i];
            if (action.type == ActionType.Skill && action.skillData != null)
            {
                ExecuteSkill(action.skillData);
            }
            EnemySkill.RemoveAt(0);
        }
        //isWaitingForReaction = false;
    }

    public void ResetResponseState()
    {
        validReactTargets.Clear();
        hasReacted = false;
        waitingForResponse = false;
    }

    /// <summary>
    /// 대응단계 종료시 스킬을 실행한다, ExecuteSkill을 호출한다
    /// </summary>
    public void ContinuePendingSkill()
    {
        if (waitingForResponse && pendingSkill != null && pendingCaster != null)
        {
            if (pendingReactSkill != null)
            {
                Vector3 reactPos = pendingReactSkill.targeting && pendingReactTargetUnit != null ?
                    pendingReactTargetUnit.transform.position : pendingReactTargetPosition;

                ExecuteSkill(pendingReactSkill, pendingReactAoeCenterPosition, reactPos, pendingReactCaster, pendingReactCharacter);
                Debug.Log($"[SkillManager] 대응 스킬 실행: {pendingReactSkill.skillName}");
            }

            Vector3 targetPos = pendingSkill.targeting && pendingTargetUnit != null ?
                pendingTargetUnit.transform.position : pendingTargetPosition;

            ExecuteSkill(pendingSkill, pendingAoeCenterPosition, targetPos, pendingCaster, pendingCharacter);
            Debug.Log($"[SkillManager] 본 스킬 실행: {pendingSkill.skillName}");
            SelectedSkillClear();



            // 초기화
            pendingSkill = null;
            pendingCaster = null;
            pendingCharacter = null;
            pendingAoeCenterPosition = Vector3.zero;
            pendingTargetPosition = Vector3.zero;
            pendingTargetUnit = null;

            pendingReactSkill = null;
            pendingReactCaster = null;
            pendingReactCharacter = null;
            pendingReactAoeCenterPosition = Vector3.zero;
            pendingReactTargetPosition = Vector3.zero;
            pendingReactTargetUnit = null;

            waitingForResponse = false; 
        }
    }

    public void CastSkillImmediately(SkillData skill, GameObject caster)
    {
        Vector3 position = caster.transform.position;

        if (skill.SkillEffectPrefab != null)
        {
            Instantiate(skill.SkillEffectPrefab, position, Quaternion.identity);
            Debug.Log($"[SkillManager] 대응 스킬 즉시 발동: {skill.skillName}");
        }
        else
        {
            Debug.LogWarning("[SkillManager] 대응 스킬 프리팹이 비어 있음");
        }
    }


    /*    /// <summary>
        /// 대응단계 종료 시 대응 스킬 실행 후 본래 중단되었던 스킬 실행 재개
        /// </summary>
        public void EndResponsePhase()
        {
            Debug.Log("[TurnManager] 대응단계");

            TurnManager.Instance.ExitReactPhase();

            // 대응 스킬 먼저 실행
            ExecuteReactSkillList();

            // 대응 상태 초기화
            isWaitingForReaction = false;
            hasMovedInReact = false;
            hasReacted = false;

            // 대응으로 중단되었던 스킬 실행 이어서 처리
            ExecuteSingleSkillWithReactionCheck();
        }*/

    /// <summary>
    /// Skillaction 리스트를 순서대로 실행하며, 대응 가능한 스킬은 대응단계 진입 후 실행을 중단.
    /// 대응단계 종료 시 다시 이 함수를 호출하면 이어서 실행됨.
    /// </summary>
    public void ExecuteSingleSkillWithReactionCheck()
    {
        // Skillaction이 null이거나 비어있으면 실행하지 않음
        if (TeamSkill == null || TeamSkill.Count == 0 || TeamSkill[0].skillData == null)
        {
            Debug.Log("[SkillManager] 실행할 스킬이 없습니다.");
            return;
        }

        // 첫 번째 ActionWrapper에서 SelectedSkill 꺼내기
        var selectedAction = TeamSkill[0];
        var skillData = selectedAction.skillData;

        /*Vector3 aoeCenter = skillData.selectedAoeCenterPosition;
        Vector3 targetPos = skillData.selectedTargetUnit != null
            ? skillData.selectedTargetUnit.transform.position
            : skillData.selectedTargetPosition;

        var skill = skillData.selectedSkill;

        if (skill.react != React.no && ReactManager.Instance.CanRespond(skill))
        {
            
            if (skill.targeting) { skillRangeVisualizer.StartSkillTargetRangePreview(skillData.selectedTargetUnit); }

            //isWaitingForReaction = true; //미사용

            // validReactTargets의 첫 번째 오브젝트만 처리
           *//* if (validReactTargets != null && validReactTargets.Count > 0)
            {
                var enumerator = validReactTargets.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var obj = enumerator.Current;
                    if (obj == null) return;

                    // CharacterMovement 컴포넌트 가져오기
                    CharacterMovement cm = obj.GetComponent<CharacterMovement>();
                    if (cm != null)
                    {
                        // 오브젝트 이름과 characterNumber 디버그 출력
                        Debug.Log($"이름: {obj.name}, characterNumber: {cm.characterNumber}");
                        // characterSelection에 characterNumber 전달
                      CharacterSelection.prevSelectedIndex = CharacterSelection.selectedCharacterIndex;
                        CharacterSelection.selectedCharacterIndex = cm.characterNumber;
                        characterSelection.SelectCharacter(cm.characterNumber);
                    }
                    else
                    {
                        // CharacterMovement가 없을 때 경고 출력
                        Debug.LogWarning($"{obj.name}에 CharacterMovement 컴포넌트가 없습니다.");
                    }
                }
            }*//*
        }
*/
        //스킬 실행
        isSkillReadyFinal = false;
        ExecuteSkill(skillData);
        TeamSkill = null;
        //isWaitingForReaction = false; //미사용

        Debug.Log("[SkillManager] 스킬 실행 완료");
    }

    public void SkillCastEnemyAI()
    {
        // Skillaction이 null이거나 비어있으면 실행하지 않음
        if (EnemySkill == null || EnemySkill.Count == 0 || EnemySkill[0].skillData == null)
        {
            Debug.Log("[SkillManager] 실행할 스킬이 없습니다.");
            return;
        }

        // 첫 번째 ActionWrapper에서 SelectedSkill 꺼내기
        var selectedAction = EnemySkill[0];
        var skillData = selectedAction.skillData;

        /*        Vector3 aoeCenter = skillData.selectedAoeCenterPosition;
                Vector3 targetPos = skillData.selectedTargetUnit != null
                    ? skillData.selectedTargetUnit.transform.position
                    : skillData.selectedTargetPosition;*/

        /*var skill = skillData.selectedSkill;

        if (skill.react != React.no && ReactManager.Instance.CanRespond(skill))
        {
            Debug.Log($"[SkillManager] 대응 가능한 스킬 발견: {skill.skillName} - 대응단계 진입");
            hasMovedInReact = true;

            *//*            validReactTargets = SimulateSkillHit.Instance.GetHitTargets(
                        skill,
                Skillaction.selectedAoeCenterPosition,
                Skillaction.selectedTargetUnit != null ? Skillaction.selectedTargetUnit.transform.position : Skillaction.selectedTargetPosition,
                Skillaction.selectedCaster
            );*/

        /*
                    // 추가 조건: 타겟팅 스킬일 때만 메인 타겟 저장
                    if (skill.targeting) // ← bool 타입의 타겟팅 여부
                    {
                        validMainTarget = Skillaction.selectedTargetUnit;
                    }
                    else
                    {
                        validMainTarget = null;
                    }*//*

        if (skill.targeting) { skillRangeVisualizer.StartSkillTargetRangePreview(skillData.selectedTargetUnit); }
        TurnManager.Instance.EnterReactPhase();
        ReactManager.Instance.EnterResponsePhase(skill, skillData.selectedCaster);
        isWaitingForReaction = true;

        // validReactTargets의 첫 번째 오브젝트만 처리
        if (validReactTargets != null && validReactTargets.Count > 0)
        {
            var enumerator = validReactTargets.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var obj = enumerator.Current;
                if (obj == null) return;

                // CharacterMovement 컴포넌트 가져오기
                CharacterMovement cm = obj.GetComponent<CharacterMovement>();
                if (cm != null)
                {
                    // 오브젝트 이름과 characterNumber 디버그 출력
                    Debug.Log($"이름: {obj.name}, characterNumber: {cm.characterNumber}");
                    // characterSelection에 characterNumber 전달
*//*                        CharacterSelection.prevSelectedIndex = CharacterSelection.selectedCharacterIndex;
                        CharacterSelection.selectedCharacterIndex = cm.characterNumber;*//*
                        characterSelection.SelectCharacter(cm.characterNumber);
                    }
                    else
                    {
                        // CharacterMovement가 없을 때 경고 출력
                        Debug.LogWarning($"{obj.name}에 CharacterMovement 컴포넌트가 없습니다.");
                    }
                }
            }

*//*            // 대응시간 UI 시작
            if (reactTimeUIManager != null)
                reactTimeUIManager.SetReactTime(skill.reactTime); // 메서드명 변경*//*

            // reactTime만큼 기다렸다가 스킬 실행
            *//*            float waitTime = skill.reactTime;
                        Instance.StartCoroutine(ExecuteSkillAfterDelay(waitTime, skillData));*//*

            isSkillReadyFinal = false;
            ExecuteSkill(skillData);
            Skillaction = null;
            ReactSkillaction = null;
            isWaitingForReaction = false;
            return; // 대응단계에서는 return
        }*/

        //스킬 실행
        isSkillReadyFinal = false;
        ExecuteSkill(skillData);
        EnemySkill = null;
        //isWaitingForReaction = false; //미사용
    }

    // 대응시간 코루틴
    private System.Collections.IEnumerator ExecuteSkillAfterDelay(float delay, SelectedSkill skillData)
    {
        yield return new WaitForSeconds(delay);
        // 대응시간이 끝난 뒤 스킬 실행
        isSkillReadyFinal = false;
        ExecuteSkill(skillData);
        TeamSkill = null;
        EnemySkill = null;
        //isWaitingForReaction = false; //미사용
        Debug.Log("[SkillManager] 스킬 실행 완료 (reactTime 대기 후)");
    }


    /// <summary>
    /// 플레이어가 선택한 스킬과 대상 등을 초기화한다
    /// </summary>
    public void SelectedSkillClear()
    {
        selectedSkill = null;
        selectedCaster = null;
        selectedCharacter = null;
        selectedAoeCenterPosition = Vector3.zero;
        selectedTargetPosition = Vector3.zero;
        selectedTargetUnit = null; //시전자 타겟 저장

        selectedTargetUnit = null;
        selectedTargetIndex = -1;
        skillRangeVisualizer.StartSkillTargetRangePreview(null);
    }

    /// <summary>
    /// 현제 스킬타겟을 정하고 있는가?
    /// </summary>
    /// <returns></returns>
    public bool IsSkillTargetingActive()
    {
        return selectedSkill != null && isSkillReady;
    }

    public bool HasMovedInReactPhase()
    {
        return hasMovedInReact;
    }

    public void MarkReactMove()
    {
        hasMovedInReact = true;
        hasReacted = true; // 스킬 사용과 동일하게 "반응 1회 완료"로 간주
        Debug.Log("[SkillManager] 대응단계에서 이동 선택 완료");
    }

    public bool HasAlreadyReacted()
    {
        return hasReacted;
    }

/*    public Stats GetRespondingCharacter()
    {
        return respondingCharacter;
    }*/

    public void Skillcancel()
    {
        SelectedSkillClear();
        selectedTargetUnit = null; // 클릭시 타겟 초기화
        isSkillReady = false;
        isSkillReadyFinal = false;
        SkillRangeVisualizer.Instance.StopNonTargetProjectileRange();
        SkillRangeVisualizer.Instance.StopSkillRangePreview();
        SkillRangeVisualizer.Instance.HideSkillRange();
    }
}
