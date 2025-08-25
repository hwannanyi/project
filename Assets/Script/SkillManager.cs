using System;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public enum SkillTiming
{
    start, casting, end
}

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
    public StoryManager storyManager; // 스토리 매니저 인스턴스
    public CharacterSelection characterSelection; // 캐릭터 선택 스크립트
    public CharacterStats characterStats; // 캐릭터 스탯 매니저
    public SFDController SFD; // SFD 컨트롤러


    ///선택한 스킬이 일시적으로 저장되는곳
    public SkillData selectedSkill = null;
    [HideInInspector] public GameObject selectedCaster = null;
    public Stats selectedCharacter = null;

    public Vector3 selectedAoeCenterPosition = Vector3.zero;
    public Vector3 selectedTargetPosition = Vector3.zero;

    public bool isSkillReady = false;
    public bool isSkillReadyFinal = false;

    //private bool isWaitingForReaction = false;    // 대응단계로 인해 중단되었는지 여부

    // 대응 중 한 번만 대응하도록 제한
    private bool hasReacted = false;

    public bool waitingForResponse = false;

    public GameObject targetIndicator; // 타겟 선택 UI 오브젝트


    public GameObject selectedTargetUnit = null;
    private int selectedTargetIndex = -1;

    public Dictionary<int, ActionWrapper> _skillAction = new();
    public Dictionary<int, ActionWrapper> _reactSkillAction = new();

    // 동기화 객체 선언 (클래스 필드에 추가)
    private readonly object _enemySkillLock = new object();
    private readonly object _teamSkillLock = new object();

    public Dictionary<int, ActionWrapper> TeamSkill
    {
        get => _skillAction;
        set
        {
            _skillAction = value;
            SkillSave.Instance.TeamSkill = value;
        }
    }

    public Dictionary<int, ActionWrapper> EnemySkill
    {
        get => _reactSkillAction;
        set
        {
            _reactSkillAction = value;
            SkillSave.Instance.EnemySkill = value;
        }
    }

    public bool isCastingSkill = false; // 스킬 시전 중인지 여부
    public bool isMoving = false; // 아무 아군 캐릭이 이동 중인지 여부
    public float ReactTime = 0.0f; // 대응단계 시간 (초 단위)

    public List<GameObject> validReactTargets = new(); // 대응 가능 캐릭터 목록

    // 메인 타겟 (타겟팅 스킬일 경우)
    public GameObject validMainTarget = null;

    public int skillCode = 0;

    [Header("포인터")]
    public GameObject cursor; // 포인터 오브젝트
    public bool cursorOn = false; // 포인터 활성화 여부

    private bool skillSelectLocked = false; // 스킬 선택 잠금 여부
    private float skillSelectLockTime = 0.5f;// 스킬 선택 잠금 시간 (초 단위)

    [Header("스킬범위 표시")]
    public GameObject skillPreview; // 스킬 프리팹

    [Header("현제 수비중인 캐릭터")]
    public Stats defendingCharacter; // 현재 수비 중인 캐릭터

    void Awake()
    {
        skillRangeVisualizer = GetComponent<SkillRangeVisualizer>();
        characterSelection = GetComponent<CharacterSelection>();
        turnManager = GetComponent<TurnManager>();
        storyManager = GetComponent<StoryManager>();
        characterStats = GetComponent<CharacterStats>();
        SFD = GetComponent<SFDController>();

        validReactTargets = new List<GameObject>();


        // 상태 변수 초기화
        selectedSkill = null;
        selectedCharacter = null;
        isSkillReady = false;
        isSkillReadyFinal = false;
        hasReacted = false;
        waitingForResponse = false;
        isCastingSkill = false;

        SelectedSkillClear();
        isSkillReady = false;
        isSkillReadyFinal = false;
        // 싱글턴 패턴 적용 (중복 방지)

        Instance = this;



        _skillAction = new Dictionary<int, ActionWrapper>();
        _reactSkillAction = new Dictionary<int, ActionWrapper>();

    }

    void Update()
    {
        
        
        //if (CameraZoom.isControlMode) return;
        if (skillSelectLocked || isCastingSkill)
        {
            // 스킬선택잠금 또는 스킬시전중, 이동중 입력 무시
            return;
        }

               
        try 
        { 
            if (storyManager.isStoryActive || storyManager.skillLock && !SFD.isSFD)
            return; // 모든 입력 무시
        }
        catch
        {
            return; // StoryManager를 못불려와도 모든입력무시
        }

        // 정지상태에서 사용해야할 스킬이 아니면 스킬 선택 취소(선택불가)
        if (Input.GetKeyDown(KeyCode.Q) && SFD.isSFD && !SFD.skillQ)
            return;
        if (Input.GetKeyDown(KeyCode.W) && SFD.isSFD && !SFD.skillW)
            return;
        if (Input.GetKeyDown(KeyCode.E) && SFD.isSFD && !SFD.skillE)
            return;
        if (Input.GetKeyDown(KeyCode.R) && SFD.isSFD && !SFD.skillR)
            return;


        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (turnManager.isPlayerTurn)
            {
                PrepareSkillCast(0, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 0)
                StartCoroutine(SkillSelectLockCoroutine()); // 공격단계에서 스킬 선택 잠금
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (turnManager.isPlayerTurn)
            {
                PrepareSkillCast(1, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 1)
                StartCoroutine(SkillSelectLockCoroutine()); // 공격단계에서 스킬 선택 잠금
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!turnManager.isPlayerTurn)
            {
                PrepareSkillCast(2, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 2)
                React_Instant_Cast(); // 대응단계에서 즉시 시전
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (turnManager.isPlayerTurn)
            {
                PrepareSkillCast(3, CharacterSelection.selectedCharacterIndex); // 1. 스킬 선택 (index 3)
                StartCoroutine(SkillSelectLockCoroutine()); // 공격단계에서 스킬 선택 잠금
            }
        }

        // 마우스 클릭으로 타겟 유닛 선택
        if (Input.GetKeyDown(KeyCode.Return) && !storyManager.skillLock)
        {
            selectedTargetUnit=null; // 클릭시 타겟 초기화

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

                    SkillCastPlayer(skillCode); // 스킬실행
                    ResetResponseState(); // 대응단계 초기화
                }

            }

        }
    }

    // 스킬 선택 잠금
    private System.Collections.IEnumerator SkillSelectLockCoroutine()
    {
        skillSelectLocked = true;
        yield return new WaitForSeconds(skillSelectLockTime);
        skillSelectLocked = false;
    }

    public void React_Instant_Cast()
    {
        if (selectedSkill != null && selectedCharacter != null) //스킬 확정 기준
        {
            selectedTargetPosition = selectedCharacter.charPosition;
            selectedTargetUnit = selectedCharacter.characterPrefab; //시전자 타겟 저장
            selectedAoeCenterPosition = SkillPositionAuto(selectedSkill, selectedCharacter, true,
                        selectedCharacter.charPosition, selectedCharacter.charPosition, selectedTargetUnit).aoeCenterPosition;


            CalculateSkillPosition(selectedSkill, selectedCharacter, false, Vector3.zero, Vector3.zero, selectedTargetUnit); // 항상 호출해야 함
            if (isSkillReady)
            {

                if (selectedSkill.targeting && selectedTargetUnit == null)
                    return;
                // 시전 확정
                ConfirmSkillCast(selectedCharacter.team); // 위치 계산 성공했을 때만 확정
                SkillCastPlayer(skillCode);
                ResetResponseState();
            }

        }
    }
    /// <summary>
    /// 선택된 캐릭터가 사용할 스킬을 지정하고 시전 준비 상태로 만든다.
    /// </summary>
    /// <param name="skillIndex">선택할 스킬의 인덱스 (예: 0 = Q, 1 = W)</param>
    public void PrepareSkillCast(int Index, int CharacterNumber)
    {
        if (CharacterNumber == -1)
        {
            Debug.LogWarning("캐릭터가 선택되지 않았습니다.");
            return;
        }


        Stats character = CharacterStats.Instance.characterList[CharacterNumber];
        if(character.characterPrefab.GetComponent<CharacterMovement>().isMoving == true)
            return; // 캐릭터가 이동 중이면 스킬 선택 불가

        if (character.usingSkill[Index].skillName == null)
        {
            Debug.LogWarning($"선택된 캐릭터의 스킬이 비어있습니다. 인덱스: {Index}");
            return;
        }

        // 선택한 스킬
        SkillData skill = character.usingSkill[Index];

        // 이전에 선택된 스킬을 다시 고를시 스킬취소
        if (selectedSkill == skill)
        {
            // 이동 커서 활성화
            characterSelection.MoveArrow.SetActive(true);
            // 선택한 스킬 초기화
            Skillcancel();
            // 리턴
            return;
        }
        else
        {
            // 선택한 스킬 초기화
            Skillcancel();
        }

        GameObject caster = CharacterStats.Instance.characters[CharacterNumber];
        CharacterStats stats = CharacterStats.Instance;
        Stats characterStats = stats.GetStats(caster);
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
        ProfileuiManager.SkillSelectionhigh(Index); // 스킬 선택 UI 하이라이트
        // 만약 플레이어 턴이면 커서를 활성화하고, 아니면 아무것도 하지 않는다.
        (turnManager.isPlayerTurn
            ? (Action)(() => 
            { cursor.SetActive(true);
                cursor.transform.position = selectedCharacter.charPosition;
            })
            : (Action)(() => { })                   
        )();


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
    /// 선택된 캐릭터가 사용할 스킬을 지정하고 시전 준비 상태로 만든다.
    /// </summary>
    /// <param name="skillIndex">선택할 스킬의 인덱스 (예: 0 = Q, 1 = W)</param>
    public (SkillData skill, GameObject caster, Stats stats) SkillAutoSelected(int skillIndex, int CharacterNumber)
    {
        Skillcancel();
        if (CharacterNumber == -1)
        {
            Debug.LogWarning("캐릭터가 선택되지 않았습니다.");
            return (null, null, null);
        }

        Stats character = characterStats.characterList[CharacterNumber];
        if (character.usingSkill[skillIndex].skillName == null)
        {
            Debug.LogWarning($"선택된 캐릭터의 스킬이 비어있습니다. 인덱스: {skillIndex}");
            return (null, null, null);
        }

        SkillData skill = character.usingSkill[skillIndex];
        GameObject caster = characterStats.characters[CharacterNumber];

        Stats Stats = characterStats.GetStats(caster);
        if (Stats.available == false)
        {
            Debug.Log($"[SkillManager] 캐릭터가 스킬사용불가 상태 입니다: {skill.skillName}");
            return (null, null, null);
        }


        if (skill.cost.ContainsKey(CostType.mp) && skill.cost[CostType.mp] > character.mp)
        {// mp 코스트가 캐릭터 mp보다 많을 때 실행할 코드(mp 부족)
            isSkillReady = false;
            Debug.Log($"[SkillManager] 코스트가 부족합니다: {skill.skillName}");
            return (null, null, null);
        }
        if (!skill.IsAvailable())
        {
            isSkillReady = false;
            Debug.Log($"[SkillManager] 스킬이 현제 쿨타임입니다: {skill.skillName}");
            return (null, null, null);
        }

        isSkillReady = true;

        //스킬저장
        return (skill, caster, character);
    }

    /// <summary>
    /// 선택된 스킬과 캐릭터 정보를 기반으로 방향, 시작위치, 중심점, 타겟 위치를 계산합니다.
    /// 이 함수는 ConfirmSkillCast() 전에 호출되어야 합니다.
    /// </summary>
    public void CalculateSkillPosition(SkillData skill, Stats character, bool AI, Vector3 AIDirection, Vector3 Position, GameObject selectedTargetUnit)
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

                    Vector3 rawCursor = cursor.transform.position;


                    int tileDist = Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.x - rawCursor.x)) +
                                   Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.z - rawCursor.z));

                    if (tileDist > selectedSkill.range)
                    {
                        Debug.LogWarning("[SkillManager] 사거리 밖의 위치입니다.");
                        isSkillReady = false;
                    }

                    // 기존 위치 보정 로직
                    bool evenX = selectedSkill.Xaoe % 2 == 0;
                    bool evenY = selectedSkill.Yaoe % 2 == 0;
                    float x = evenX ? Mathf.Floor(rawCursor.x) + 0.5f : Mathf.Round(rawCursor.x);
                    float y = evenY ? Mathf.Floor(rawCursor.z) + 0.5f : Mathf.Round(rawCursor.z);

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
            /*            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                        float enter;
                        if (groundPlane.Raycast(ray, out enter))
                        {
                            mouseWorldPos = ray.GetPoint(enter);
                            mouseWorldPos.y = 0f;
                        }*/
            mouseWorldPos = cursor.transform.position;
            mouseWorldPos.y = 0f;
        }
        mouseWorldPos = AI ? Position : mouseWorldPos;

        // ② 방향 계산
        if (skill.targeting && selectedTargetUnit != null)
        {
            direction = (selectedTargetUnit.transform.position - startPosition).normalized;
        }
        else
        {

            direction = (mouseWorldPos - startPosition).normalized;
            //AI 캐릭터의 경우, 방향을 AI가 지정한 방향으로 설정
            if (AI && AIDirection != Vector3.zero)
            {
                direction = AIDirection.normalized;
            }


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
            targetPosition = skill.projectile ? startPosition + closestDirection * range : startPosition;
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
    /// 선택된 스킬과 캐릭터 정보를 기반으로 방향, 시작위치, 중심점, 타겟 위치를 계산합니다.
    /// 이 함수는 ConfirmSkillCast() 전에 호출되어야 합니다.
    /// </summary>
    public (Vector3 targetPosition, Vector3 aoeCenterPosition, bool effectiveness) 
        SkillPositionAuto(SkillData skill, Stats character, bool AI, 
        Vector3 AIDirection, Vector3 Position, GameObject selectedTargetUnit)
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
                        return (Vector3.zero, Vector3.zero, false);
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


                    Vector3 rawCursor = cursor.transform.position;


                    int tileDist = Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.x - rawCursor.x)) +
                                   Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.z - rawCursor.z));

                    if (tileDist > selectedSkill.range)
                    {
                        Debug.LogWarning("[SkillManager] 사거리 밖의 위치입니다.");
                        isSkillReady = false;
                        return (Vector3.zero, Vector3.zero, false);
                    }

                    // 기존 위치 보정 로직
                    bool evenX = selectedSkill.Xaoe % 2 == 0;
                    bool evenY = selectedSkill.Yaoe % 2 == 0;
                    float x = evenX ? Mathf.Floor(rawCursor.x) + 0.5f : Mathf.Round(rawCursor.x);
                    float y = evenY ? Mathf.Floor(rawCursor.z) + 0.5f : Mathf.Round(rawCursor.z);

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
            /*            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                        float enter;
                        if (groundPlane.Raycast(ray, out enter))
                        {
                            mouseWorldPos = ray.GetPoint(enter);
                            mouseWorldPos.y = 0f;
                        }*/
            mouseWorldPos = cursor.transform.position;
            mouseWorldPos.y = 0f;
        }

        
        mouseWorldPos = AI ? Position : mouseWorldPos;

        // ② 방향 계산
        if (skill.targeting && selectedTargetUnit != null)
        {
            direction = (selectedTargetUnit.transform.position - startPosition).normalized;
        }
        else
        {

            direction = (mouseWorldPos - startPosition).normalized;
            //AI 캐릭터의 경우, 방향을 AI가 지정한 방향으로 설정
            if (AI && AIDirection != Vector3.zero)
            {
                direction = AIDirection.normalized;
            }


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

            targetPosition = skill.projectile ?
                skill.unlimitedRota ? startPosition + direction * range : startPosition + closestDirection * range 
                : startPosition;
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
        // 디버그용 시각화
        //SimulateSkillHit.Instance.SimulateForDebug(skill, startPosition, targetPosition, aoeCenterPosition);
        return (targetPosition, aoeCenterPosition, true);
    }

    /// <summary>
    /// 선택된 스킬의 실행을 확정
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
        Debug.Log($"[SaveSkill] Skill: {selectedSkill.skillName}, Prefab: {selectedSkill.SkillEffectPrefab}");
        SaveSkill(team == Team.team); // 일반 스킬 저장
        Debug.Log($"[SkillManager] 스킬 저장 완료: {selectedSkill.skillName}" + " " + team);
        SelectedSkillClear();

        // 상태 초기화
        isSkillReady = false;
    }

    /// <summary>
    /// 선택된 스킬의 실행을 확정하고 대응 조건을 판단하여 대응단계로 진입하거나 즉시 시전한다.
    /// 대응단계 중이면 대응자 스킬을 pendingReactSkill로 저장한다.
    /// </summary>
    public void ConfirmSkill(
        Team team,
        SkillData skill,
        GameObject casterObj,
        Stats character,
        Vector3 targetPosition,
        Vector3 aoeCenterPosition,
        GameObject targetObj,
        bool PosEffectiveness,
        ref int skillCode)  
    {
        isSkillReadyFinal = true;
        //respondingCharacter = selectedCharacter;
        if (!isSkillReady || skill == null || casterObj == null || character == null)
        {
            Debug.LogWarning("[SkillManager] 스킬 준비 상태가 아니거나 정보가 부족합니다.");
            return;
        }

        // 타겟팅 스킬 또는 타겟 기반 스킬일 경우, 대상이 반드시 필요
        if ((skill.targeting || skill.startSkillPosition == StartSkillPosition.target)
            && targetObj == null)
        {
            Debug.LogWarning("[SkillManager] 타겟팅 스킬인데 타겟이 없습니다.");
            return;
        }

        if (!isSkillReady || skill == null || casterObj == null || character == null)
        {
            Debug.LogWarning("[SkillManager] 스킬 준비 상태가 아니거나 정보가 부족합니다.");
            return;
        }


        if (!PosEffectiveness)
        {
            Debug.LogWarning("[SkillManager] 유효하지 않은 시전위치 입니다.");
            return;
        }

        Debug.Log($"[SaveSkill] Skill: {skill.skillName}");
        SaveSkillList(team == Team.team,
            skill,
            casterObj,
            character,
            targetPosition,
            aoeCenterPosition,
            targetObj,
            ref skillCode); // 일반 스킬 저장
        Debug.Log($"[SkillManager] 스킬 저장 완료: {skill.skillName}" + " " + team);
        SelectedSkillClear();

        // 상태 초기화
        isSkillReady = false;
    }

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
                EnemySkill = new Dictionary<int, ActionWrapper>();

            lock (_enemySkillLock)
            {
                int newKey = 0;
                while (EnemySkill.ContainsKey(newKey))
                    newKey++;
                action.skillData.selectedSkill.skillCastCode = newKey; // 스킬 코드 저장
                EnemySkill.Add(newKey, action);
                skillCode = newKey; // 스킬 코드 반환
            }
        }
        else
        {
            // 팀이 사용하는 스킬인 경우 TeamSkill 에 저장
            selectedSkill.isreactSkill = false;
            if (TeamSkill == null)
                TeamSkill = new Dictionary<int, ActionWrapper>();

            lock (_teamSkillLock)
            {
                int newKey = 0;
                while (TeamSkill.ContainsKey(newKey))
                    newKey++;
                action.skillData.selectedSkill.skillCastCode = newKey; // 스킬 코드 저장
                TeamSkill.Add(newKey, action);
                skillCode = newKey; // 스킬 코드 반환
            }
        }
    }


    // SaveSkill: 선택된 스킬을 저장하는 함수
    // isReaction이 true이면 대응 스킬로 처리되어 ReactSkillaction 리스트에 저장됨
    // false이면 일반 스킬로 처리되어 Skillaction 리스트에 저장됨
    /// <summary>
    /// 선택한 스킬을 저장한다
    /// </summary> 
    public void SaveSkillList(bool team,
        SkillData skill,
        GameObject casterObj,
        Stats character,
        Vector3 targetPosition, 
        Vector3 aoeCenterPosition,
        GameObject targetObj,
        ref int skillCode)
    {
        // 현재 선택된 스킬 정보를 SelectedSkillList 형태로 구성
        var skillInfo = new SelectedSkill
        {
            selectedSkill = skill,
            selectedCaster = casterObj,
            selectedCharacter = character,
            selectedAoeCenterPosition = aoeCenterPosition,
            selectedTargetPosition = targetPosition,
            selectedTargetUnit = skill.targeting ? targetObj : null,

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
            skill.isreactSkill = true;
            if (EnemySkill == null)
                EnemySkill = new Dictionary<int, ActionWrapper>();

            lock (_enemySkillLock)
            {
                int newKey = 0;
                while (EnemySkill.ContainsKey(newKey))
                    newKey++;
                action.skillData.selectedSkill.skillCastCode = newKey; // 스킬 코드 저장
                EnemySkill.Add(newKey, action);
                skillCode = newKey; // 스킬 코드 반환
            }
        }
        else
        {
            // 팀이 사용하는 스킬인 경우 TeamSkill 에 저장
            skill.isreactSkill = false;
            if (TeamSkill == null)
                TeamSkill = new Dictionary<int, ActionWrapper>();

            lock (_teamSkillLock)
            {
                int newKey = 0;
                while (TeamSkill.ContainsKey(newKey))
                    newKey++;
                action.skillData.selectedSkill.skillCastCode = newKey; // 스킬 코드 저장
                TeamSkill.Add(newKey, action);
                skillCode = newKey; // 스킬 코드 반환
            }
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
        if (skill.skillPreview > 0)
        {
            GameObject skillObject = Instantiate(skillPreview, aoeCenterPosition, Quaternion.identity);
            if (skillObject.TryGetComponent<SkillPreview>(out var effect))
                effect.Initialize(skill, targetPosition, casterObject, character, aoeCenterPosition, target);
        }
        else 
        {
            GameObject skillObject = Instantiate(skill.SkillEffectPrefab, aoeCenterPosition, Quaternion.identity);

            if (skill.projectile)
            {
                if (skillObject.TryGetComponent<SkillEffectProjectile>(out var effect))
                    effect.Initialize(skill, targetPosition, casterObject, character, target);
            }
            else
            {
                if (skillObject.TryGetComponent<SkillEffectHitscan>(out var effect))
                    effect.Initialize(skill, targetPosition, casterObject, character, target);
            } 
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
        character.gurd = skill.gurd.time;
        //프로필 업데이트
        try
        {
            ProfileuiManager.ProfileUpdate(characterSelection.PickcharNumber(CharacterSelection.selectedCharacterIndex),
                turnManager.isPlayerTurn);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkillManager] 프로필 업데이트 실패: {e.Message}");
        }
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

    public void ResetResponseState()
    {
        validReactTargets.Clear();
        hasReacted = false;
        waitingForResponse = false;
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

    /// <summary>
    /// Skillaction 리스트를 순서대로 실행
    /// </summary>
    public void SkillCastPlayer(int skillCode)
    {
        // Skillaction이 null이거나 비어있으면 실행하지 않음
        if (TeamSkill == null || TeamSkill.Count == 0 || TeamSkill[skillCode].skillData == null)
        {
            Debug.Log("[SkillManager] 실행할 스킬이 없습니다.");
            return;
        }

        // 첫 번째 ActionWrapper에서 SelectedSkill 꺼내기
        var selectedAction = TeamSkill[skillCode];
        var skillData = selectedAction.skillData;

        cursor.SetActive(false);// 커서 비활성화

        // 스킬 시전 중 상태로 변경
        isCastingSkill = true; 

        //스킬 실행
        isSkillReadyFinal = false;
        ExecuteSkill(skillData);
        TeamSkill = null;
        //isWaitingForReaction = false; //미사용

        Debug.Log("[SkillManager] 스킬 실행 완료");
    }

    public void SkillCastAI(Team team, int skillcode)
    {
        // Skillaction이 null이거나 비어있으면 실행하지 않음
        if (EnemySkill == null || EnemySkill.Count == 0 || EnemySkill[skillcode].skillData == null)
        {
            Debug.Log("[SkillManager] 실행할 스킬이 없습니다.");
            return;
        }

        // 첫 번째 ActionWrapper에서 SelectedSkill 꺼내기
        var selectedAction = team == Team.team ? TeamSkill[skillcode] : EnemySkill[skillcode];
        var skillData = selectedAction.skillData;

        //스킬 실행
        isSkillReadyFinal = false;
        ExecuteSkill(skillData);
        EnemySkill = null;
        //isWaitingForReaction = false; //미사용
    }


    public void SkillAutoCast(Team team, int skillcode)
    {
        // 첫 번째 ActionWrapper에서 SelectedSkill 꺼내기
        var selectedAction = team == Team.team ? TeamSkill[skillcode] : EnemySkill[skillcode];
        var skillData = selectedAction.skillData;
        //스킬 실행
        isSkillReadyFinal = false;
        ExecuteSkill(skillData);
        if (team == Team.team)
        {
            TeamSkill.Remove(skillcode);
        }
        else
        {
            EnemySkill.Remove(skillcode);
        }


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

    public bool HasAlreadyReacted()
    {
        return hasReacted;
    }

    public void Skillcancel()
    {
        SelectedSkillClear();
        ProfileuiManager.SkillSelectionhigh(-1);
        selectedTargetUnit = null; // 클릭시 타겟 초기화
        isSkillReady = false;
        isSkillReadyFinal = false;
        cursor.SetActive(false);// 커서 비활성화
        skillRangeVisualizer.StopNonTargetProjectileRange();
        skillRangeVisualizer.StopSkillRangePreview();
        skillRangeVisualizer.HideSkillRange();
    }
}
