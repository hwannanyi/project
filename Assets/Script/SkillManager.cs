using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;

[System.Serializable]
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance; // 싱글턴 인스턴스 (전역 접근 가능)
    public List<SkillData> UseSkillList = new(); // 현재 사용 가능한 스킬 목록
    //
    //public GameObject skillPrefab; // 생성할 스킬 프리팹 미사용


    ///선택한 스킬이 일시적으로 저장되는곳
    private SkillData selectedSkill = null;
    private GameObject selectedCaster = null;
    public Stats selectedCharacter = null;

    private Vector3 selectedAoeCenterPosition = Vector3.zero;
    private Vector3 selectedTargetPosition = Vector3.zero;

    private bool isSkillReady = false;
    private int currentSkillIndex = 0;            // Skillaction 실행 인덱스 기억
    private bool isWaitingForReaction = false;    // 대응단계로 인해 중단되었는지 여부


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



    private GameObject selectedTargetUnit = null;
    private int selectedTargetIndex = -1;

    public Stats respondingCharacter; // 현재 대응해야 하는 캐릭터

    public bool hasMovedInReact = false; // 대응단계에서 이동 여부


    public SelectedSkill _skillAction;
    public SelectedSkill _reactSkillAction;

    public SelectedSkill Skillaction
    {
        get => _skillAction;
        set
        {
            _skillAction = value;
            SkillSave.Instance.Skillaction = value;
        }
    }

    public SelectedSkill ReactSkillaction
    {
        get => _reactSkillAction;
        set
        {
            _reactSkillAction = value;
            SkillSave.Instance.ReactSkillaction = value;
        }
    }


    public List<GameObject> validReactTargets = new(); // 대응 가능 캐릭터 목록

    // 메인 타겟 (타겟팅 스킬일 경우)
    public GameObject validMainTarget = null;

    void Awake()
    {
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PrepareSkillCast(0); // 1. 스킬 선택 (index 0)
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            PrepareSkillCast(1); // 1. 스킬 선택 (index 1)
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            PrepareSkillCast(2); // 1. 스킬 선택 (index 1)
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PrepareSkillCast(3); // 1. 스킬 선택 (index 1)
        }

        // 마우스 클릭 또는 타겟 방향 확정 후
        if (Input.GetKeyDown(KeyCode.Return)) // Enter로 스킬 확정
        {
            // 2. 위치 계산 (예: 마우스 클릭 기반)
            if(selectedSkill == null || selectedCharacter == null) { Debug.Log("선택한 스킬 또는 대상 없음!!!"); return;}

            CalculateSkillPosition(selectedSkill, selectedCharacter); // 항상 호출해야 함
            if (isSkillReady)
            {
                // 3. 시전 확정 + 대응 체크
                ConfirmSkillCast(); // 위치 계산 성공했을 때만 확정
            }

        }


        // 대응단계 강제 종료 테스트용 (게임 흐름에 따라 UI 버튼 등으로 대체 가능)
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (TurnManager.Instance.IsInReactPhase())
            {
                Debug.Log("대응단계 M키 입력 - 대응스킬 먼저 실행");
                ExecuteReactionThenSkill();     // ← 변경됨
                ResetResponseState();
                ExecuteSingleSkillWithReactionCheck(); // ← 변경됨
            }
            else
            {
                Debug.Log("일반 스킬 실행 시도");
                ExecuteSingleSkillWithReactionCheck(); // ← 변경됨
            }
        }

        // 마우스 클릭으로 타겟 유닛 선택
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
            float enter;
            Vector3 mouseWorld = Vector3.zero;
            if (groundPlane.Raycast(ray, out enter))
            {
                mouseWorld = ray.GetPoint(enter);
                mouseWorld.z = 0f;
            }

            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
            if (hit != null && hit.CompareTag("Character"))
            {
                GameObject target = hit.gameObject;

                if (selectedCharacter != null && selectedSkill != null)
                {
                    Vector3 unitPos = selectedCharacter.charPosition;
                    Vector3 targetPos = target.transform.position;

                    int tileDist = Mathf.Abs(Mathf.RoundToInt(unitPos.x - targetPos.x)) +
                                   Mathf.Abs(Mathf.RoundToInt(unitPos.y - targetPos.y));

                    if (tileDist > selectedSkill.range)
                    {
                        Debug.LogWarning("[SkillManager] 사거리 밖의 유닛입니다.");
                        return;
                    }
                }

                selectedTargetUnit = target;
                selectedTargetIndex = CharacterStats.Instance.characters.IndexOf(target);
                Debug.Log($"[SkillManager] 대상 선택됨: {target.name}");
            }
        }
    }

   

    /// <summary>
    /// 현재 선택된 캐릭터 인덱스가 올바르고, 행동 가능하면 이름을 반환함
    /// </summary>
    /// <param name="name">선택된 캐릭터의 이름 반환</param>
    /// <returns>행동 가능하면 true</returns>
    public bool TryGetSelectedCharacterName(out string name)
    {
        name = "";
        int selectedIndex = CharacterSelection.selectedCharacterIndex;

        if (selectedIndex == -1)
        {
            Debug.LogWarning("[SkillManager] 캐릭터가 선택되지 않았습니다.");
            return false;
        }

        bool isPlayerTurn = TurnManager.Instance.currentPhase == TurnPhase.PlayerTurn
                         || TurnManager.Instance.currentPhase == TurnPhase.ReactPhase_PlayerResponding;

        if (isPlayerTurn)
        {
            if (TurnManager.Instance.playerUseSkillTurn >= TurnManager.Instance.playerSkillTurn)
                return false;

            if (selectedIndex < 0 || selectedIndex >= CharacterStats.Instance.playerCharacters.Count)
            {
                Debug.LogWarning("선택된 플레이어 캐릭터 인덱스가 범위를 벗어났습니다.");
                return false;
            }

            TurnManager.Instance.playerUseSkillTurn++;
            name = CharacterStats.Instance.playerCharacters[selectedIndex];
            return true;
        }
        else
        {
            if (TurnManager.Instance.enemyUseSkillTurn >= TurnManager.Instance.enemySkillTurn)
                return false;

            int enemyIndex = selectedIndex - CharacterStats.Instance.playerCharacters.Count;
            if (enemyIndex < 0 || enemyIndex >= CharacterStats.Instance.EnemieCharacters.Count)
            {
                Debug.LogWarning("선택된 적 캐릭터 인덱스가 범위를 벗어났습니다.");
                return false;
            }

            TurnManager.Instance.enemyUseSkillTurn++;
            name = CharacterStats.Instance.EnemieCharacters[enemyIndex];
            return true;
        }
    }


    /// <summary>
    /// 선택된 캐릭터가 사용할 스킬을 지정하고 시전 준비 상태로 만든다.
    /// </summary>
    /// <param name="skillIndex">선택할 스킬의 인덱스 (예: 0 = Q, 1 = W)</param>
    public void PrepareSkillCast(int skillIndex)
    {
        int index = CharacterSelection.selectedCharacterIndex;
        if (index == -1)
        {
            Debug.LogWarning("캐릭터가 선택되지 않았습니다.");
            return;
        }

        var character = CharacterStats.Instance.characterList[index];
        var skill = character.usingSkill[skillIndex];
        GameObject caster = CharacterStats.Instance.characters[index];

        // 임시 저장
        selectedSkill = skill;
        selectedCaster = caster;
        selectedCharacter = character;
        isSkillReady = true;

        Debug.Log($"[SkillManager] 스킬 선택 완료: {skill.skillName}");

        // 이 시점에서 방향/타겟 UI 활성화
        // 예: ShowTargetingUI(skill.range) 등
    }



    /// <summary>
    /// 선택된 스킬과 캐릭터 정보를 기반으로 방향, 시작위치, 중심점, 타겟 위치를 계산합니다.
    /// 이 함수는 ConfirmSkillCast() 전에 호출되어야 합니다.
    /// </summary>
    public void CalculateSkillPosition(SkillData skill, Stats character)
    {
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
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
                    float enter;
                    Vector3 rawMouse = Vector3.zero;
                    if (groundPlane.Raycast(ray, out enter))
                    {
                        rawMouse = ray.GetPoint(enter);
                        rawMouse.z = 0f;
                    }

                    int tileDist = Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.x - rawMouse.x)) +
                                   Mathf.Abs(Mathf.RoundToInt(selectedCharacter.charPosition.y - rawMouse.y));

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
                    float y = evenY ? Mathf.Floor(rawMouse.y) + 0.5f : Mathf.Round(rawMouse.y);

                    startPosition = new Vector3(x, y, 0f);
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

        // ① 마우스 기반 방향 계산이 필요한 경우
        Vector3 mouseWorldPos = Vector3.zero;
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        Vector3 closestDirection = directions[0];
        if (!skill.targeting || selectedTargetUnit == null)
        {
            mouseWorldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
            mouseWorldPos.z = 0f;
        }

        // ② 방향 계산
        if (skill.targeting && selectedTargetUnit != null)
        {
            direction = (selectedTargetUnit.transform.position - startPosition).normalized;
        }
        else
        {
            direction = (mouseWorldPos - startPosition).normalized;

            // 마우스 기반 스킬만 가장 가까운 4방향으로 제한
            
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

            direction = closestDirection;
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
            targetPosition = startPosition + closestDirection * skill.range;
            targetPosition.z = 0f;
        }

        // AOE 중심 계산
        Vector3 aoeCenterPosition = Vector3.zero;
        Vector3 offset = Vector3.zero;

        switch (skill.aoecenter)
        {
            case aoeCenter.center:
                aoeCenterPosition = startPosition;
                break;

            case aoeCenter.edge:
            case aoeCenter.Rcorner:
            case aoeCenter.Lcorner:
                AoeCenterPosition(skill, closestDirection, startPosition, ref aoeCenterPosition, ref offset);
                break;

            default:
                aoeCenterPosition = startPosition;
                break;
        }

        if (skill.aoecenter == aoeCenter.Rcorner || skill.aoecenter == aoeCenter.Lcorner)
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
    public void ConfirmSkillCast()
    {
        respondingCharacter = selectedCharacter;
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

        // 대응단계 중인 경우 → 대응자 쪽 처리
        if (TurnManager.Instance.IsInReactPhase())
        {
            /*if (hasReacted)
            {
                Debug.LogWarning("[SkillManager] 이미 대응했습니다.");
                return;
            }*/

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
            SaveSkill(true);//대응자용 타겟 저장
            hasReacted = true;

            Debug.Log($"[SkillManager] 대응 스킬 저장 완료: {selectedSkill.skillName}");
            return;
        }

        // 대응 조건 없으면 바로 실행
        Debug.Log($"[SaveSkill] Skill: {selectedSkill.skillName}, Prefab: {selectedSkill.SkillEffectPrefab}");
        SaveSkill(false); // 일반 스킬 저장
        Debug.Log($"[SkillManager] 스킬 저장 완료: {selectedSkill.skillName}");
        selectedSkillClear();

        // 상태 초기화
        isSkillReady = false;
    }//waitingForResponse = true;

    // SaveSkill: 선택된 스킬을 저장하는 함수
    // isReaction이 true이면 대응 스킬로 처리되어 ReactSkillaction 리스트에 저장됨
    // false이면 일반 스킬로 처리되어 Skillaction 리스트에 저장됨
    /// <summary>
    /// 선택한 스킬을 저장한다
    /// </summary>
    /// <param name="isReaction">대응단계 구분</param>
    public void SaveSkill(bool isReaction = false)
    {
        // 현재 선택된 스킬 정보를 SelectedSkillList 형태로 구성
        var skillInfo = new SelectedSkill
        {
            selectedSkill = selectedSkill,
            selectedCaster = selectedCaster,
            selectedCharacter = selectedCharacter,
            selectedAoeCenterPosition = selectedAoeCenterPosition,
            selectedTargetPosition = selectedTargetPosition,
            selectedTargetUnit = selectedTargetUnit,
        };

/*        // 구성된 스킬 정보를 ActionWrapper에 포장
        var action = new ActionWrapper
        {
            type = ActionType.Skill,
            skillData = skillInfo
        };
*/
        // 대응 여부에 따라 다른 리스트에 추가
        if (isReaction)
        {
            // 대응 스킬인 경우 ReactSkillaction에 저장
            ReactSkillaction = skillInfo;
        }
        else
        {
            // 일반 스킬인 경우 Skillaction에 저장
            Skillaction = skillInfo;
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

        Debug.Log($"[SkillManager] 스킬 실행 완료: {skill.skillName}");
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
            case aoeCenter.center:
                offset = Vector3.zero;
                break;

            case aoeCenter.edge:
                offset = closestDirection * xOffset;
                break;

            case aoeCenter.Rcorner:
                if(closestDirection.x < 0)
                {
                    yOffset = -yOffset;
                }
                if(closestDirection.x == 0)
                    offset = (closestDirection * xOffset) + new Vector3(-yOffset * closestDirection.y, 0f, 0f);
                else
                    offset = (closestDirection * xOffset) + new Vector3(0f, yOffset, 0f);
                break;

            case aoeCenter.Lcorner:
                if (closestDirection.x < 0)
                {
                    yOffset = -yOffset;
                }
                if (closestDirection.x == 0)
                    offset = (closestDirection * xOffset) + new Vector3(yOffset * closestDirection.y, 0f, 0f);
                else
                    offset = (closestDirection * xOffset) + new Vector3(0f, -yOffset, 0f);
                break;
        }

        aoeCenterPosition = startPosition + offset;
        Poffset = offset;
    }

/*    /// <summary>
    /// 생성될 스킬에서 투사체, 히트스캔을 구분하고 알맞은 프리팹 스킬 오브젝트를 생성한다
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="skillPrefab"></param>
    /// <param name="aoeCenterPosition"></param>
    /// <param name="targetPosition"></param>
    /// <param name="casterObject"></param>
    /// <param name="character"></param>
    public void castskillon(SkillData skill, GameObject skillPrefab, Vector3 aoeCenterPosition, Vector3 targetPosition, GameObject casterObject, Stats character)
    {
        GameObject prefab = skill.SkillEffectPrefab;
        GameObject skillObject = Instantiate(skillPrefab, aoeCenterPosition, Quaternion.identity);


        //초기화 - 사용자 정보까지 넘김
        if (skill.projectile)
        {
            SkillEffectProjectile skillEffect = skillObject.GetComponent<SkillEffectProjectile>();
            if (skillEffect != null)
            {
                skillEffect.Initialize(skill, targetPosition, casterObject, character);
            }
        }
        else
        {
            SkillEffectHitscan skillEffect = skillObject.GetComponent<SkillEffectHitscan>();
            if (skillEffect != null)
            {
                skillEffect.Initialize(skill, targetPosition, casterObject, character);
            }
        }
    }*/


    // 일반 스킬 실행 함수
    public void ExecuteCurrentSkill()
    {
        if (Skillaction != null)
        {
            ExecuteSkill(Skillaction);
            Skillaction = null;
        }
    }

    // 대응 스킬 실행 함수
    public void ExecuteReactSkillList()
    {
        if (ReactSkillaction != null)
        {
            ExecuteSkill(ReactSkillaction);
            ReactSkillaction = null;
        }
    }

    public void ExecuteReactionThenSkill()
    {
        if (ReactSkillaction != null)
        {
            ExecuteSkill(ReactSkillaction);
            ReactSkillaction = null;
        }

        if (Skillaction != null)
        {
            ExecuteSkill(Skillaction);
            Skillaction = null;
        }

        isWaitingForReaction = false;
        Debug.Log("[SkillManager] 대응단계 종료");
        if(TurnManager.Instance.IsInReactPhase())
        {
            TurnManager.Instance.ExitReactPhase();
            
        }
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
            selectedSkillClear();



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

        if (Skillaction == null || Skillaction.selectedSkill == null)
        {
            Debug.Log("[SkillManager] 실행할 스킬이 없습니다.");
            return;
        }

        Vector3 aoeCenter = Skillaction.selectedAoeCenterPosition;
        Vector3 targetPos = Skillaction.selectedTargetUnit != null
            ? Skillaction.selectedTargetUnit.transform.position
            : Skillaction.selectedTargetPosition;

        
        var skill = Skillaction.selectedSkill;

        if (skill.react != React.no && ReactManager.Instance.CanRespond(skill))
        {
            Debug.Log($"[SkillManager] 대응 가능한 스킬 발견: {skill.skillName} - 대응단계 진입");

            validReactTargets = SimulateSkillHit.Instance.GetHitTargets(
            skill,
    Skillaction.selectedAoeCenterPosition,
    Skillaction.selectedTargetUnit != null ? Skillaction.selectedTargetUnit.transform.position : Skillaction.selectedTargetPosition,
    Skillaction.selectedCaster
);


            // 추가 조건: 타겟팅 스킬일 때만 메인 타겟 저장
            if (skill.targeting) // ← bool 타입의 타겟팅 여부
            {
                validMainTarget = Skillaction.selectedTargetUnit;
            }
            else
            {
                validMainTarget = null;
            }

            TurnManager.Instance.EnterReactPhase();
            ReactManager.Instance.EnterResponsePhase(skill, Skillaction.selectedCaster);
            isWaitingForReaction = true;

            return;
        }

        // 대응 대상 없으면 스킬 즉시 실행
        ExecuteSkill(Skillaction);
        Skillaction = null;
        isWaitingForReaction = false;

        Debug.Log("[SkillManager] 스킬 실행 완료");
    }


    /// <summary>
    /// 플레이어가 선택한 스킬과 대상 등을 초기화한다
    /// </summary>
    public void selectedSkillClear()
    {
        selectedSkill = null;
        selectedCaster = null;
        selectedCharacter = null;
        selectedAoeCenterPosition = Vector3.zero;
        selectedTargetPosition = Vector3.zero;
        selectedTargetUnit = null; //시전자 타겟 저장

        selectedTargetUnit = null;
        selectedTargetIndex = -1;
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

    public Stats GetRespondingCharacter()
    {
        return respondingCharacter;
    }
}
