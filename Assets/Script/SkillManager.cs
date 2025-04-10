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
    private Stats selectedCharacter = null;

    private Vector3 selectedAoeCenterPosition = Vector3.zero;
    private Vector3 selectedTargetPosition = Vector3.zero;

    private bool isSkillReady = false;
    /////////////////////////////////////////////////

    /// 대응을 위한 대기상태 스킬을 저장하는 변수
    private SkillData pendingSkill;
    private GameObject pendingCaster;
    private Stats pendingCharacter;
    private Vector3 pendingAoeCenterPosition;
    private Vector3 pendingTargetPosition;

    // 대응 중 한 번만 대응하도록 제한
    private bool hasReacted = false;

    public bool waitingForResponse = false;
    /// /////////////////////////////////////

    //대응상대 스킬 저장
    private int pendingSelectedCharacterIndex;
    private SkillData pendingReactSkill;
    private GameObject pendingReactCaster;
    private Stats pendingReactCharacter;
    private Vector3 pendingReactAoeCenterPosition;
    private Vector3 pendingReactTargetPosition;
    //////////////////////////////////////////

    private GameObject selectedTargetUnit = null;
    private int selectedTargetIndex = -1;

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

        // 마우스 클릭 또는 타겟 방향 확정 후
        if (Input.GetKeyDown(KeyCode.Return)) // Enter로 스킬 확정
        {
            // 2. 위치 계산 (예: 마우스 클릭 기반)
            CalculateSkillPosition(selectedSkill, selectedCharacter);

            // 3. 시전 확정 + 대응 체크
            ConfirmSkillCast();
        }

        // 대응단계 강제 종료 테스트용 (게임 흐름에 따라 UI 버튼 등으로 대체 가능)
        if (Input.GetKeyDown(KeyCode.M))
        {
            EndResponsePhase();
        }

        // 마우스 클릭으로 타겟 유닛 선택
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(mouseWorld);
            if (hit != null)
            {
                GameObject target = hit.gameObject;

                if (target.CompareTag("Character"))
                {
                    selectedTargetUnit = target;
                    selectedTargetIndex = CharacterStats.Instance.characters.IndexOf(target);
                    Debug.Log($"[SkillManager] 대상 선택됨: {target.name}");
                }
            }
        }
    }

    /// <summary>
    /// 모든 캐릭터의 스킬을 리스트(UseSkillList)에 추가하는 함수 미사용
    /// </summary>
    /*public void Characterform()
    {
        // 캐릭터 리스트를 순회하며 각 캐릭터의 스킬을 저장
        for (int i = 0; i < CharacterStats.Instance.characterList.Count; i++)
        {
            for (int j = 0; j < CharacterStats.Instance.characterList[i].useSkill.Count; j++)
            {
                // 캐릭터 이름과 스킬을 SkillData로 만들어 리스트에 추가
                UseSkillList.Add(new SkillData(CharacterStats.Instance.characterList[i].useSkill[j], CharacterStats.Instance.characterList[i].name));
            }
        }
    }*/

    /// <summary>
    /// 캐릭터의 위치에서 스킬을 생성하고, SkillEffect를 초기화하는 함수
    /// </summary>
    /// <summary>
    /// 선택된 캐릭터의 스킬을 하나만 실행하는 함수
    /// </summary>
    /*public void CastSkill(int skillcast)
    {
        string name ;
        int index;

        bool isPlayerTurn = TurnManager.Instance.currentPhase == TurnPhase.PlayerTurn
                            || TurnManager.Instance.currentPhase == TurnPhase.ReactPhase_PlayerResponding;

        int selectedIndex = CharacterSelection.selectedCharacterIndex;
        if (selectedIndex == -1)
        {
            Debug.LogWarning("[SkillManager] 캐릭터가 선택되지 않아 스킬을 사용할 수 없습니다.");
            return;
        }

        // 행동 제한 체크
        if (isPlayerTurn)
        {
            if (TurnManager.Instance.playerUseSkillTurn >= TurnManager.Instance.playerSkillTurn)
                return;

            if (selectedIndex < 0 || selectedIndex >= CharacterStats.Instance.playerCharacters.Count)
            {
                Debug.LogWarning("선택된 플레이어 캐릭터 인덱스가 범위를 벗어났습니다.");
                return;
            }

            TurnManager.Instance.playerUseSkillTurn++;
            name = CharacterStats.Instance.playerCharacters[selectedIndex];
        }
        else
        {
            if (TurnManager.Instance.enemyUseSkillTurn >= TurnManager.Instance.enemySkillTurn)
                return;

            int enemyIndex = selectedIndex - CharacterStats.Instance.playerCharacters.Count;
            if (enemyIndex < 0 || enemyIndex >= CharacterStats.Instance.EnemieCharacters.Count)
            {
                Debug.LogWarning("선택된 적 캐릭터 인덱스가 범위를 벗어났습니다.");
                return;
            }

            TurnManager.Instance.enemyUseSkillTurn++;
            name = CharacterStats.Instance.EnemieCharacters[enemyIndex];
        }

        // name → index 변환
        index = CharacterStats.Instance.characterList.FindIndex(Character => Character.name == name);*//*

        
        // 스킬 프리팹을 캐릭터 리스트의 캐리선책번째 캐릭터의 입력번째 스킬 프리팹으로 설정
        if (CharacterStats.Instance.characterList[index].usingSkill.Count < skillcast+1)
        {
            Debug.LogWarning("스킬을 찾을 수 없습니다");
            return;
        }*//*

        //코드수정으로 미사용됨
        //skillPrefab = CharacterStats.Instance.characterList[index].usingSkill[skillcast].SkillEffectPrefab;

        // 1. 캐릭터와 스킬 설정
        var character = CharacterStats.Instance.characterList[index];
        var skill = character.usingSkill[skillcast];

        // 실제 씬에 존재하는 캐릭터 인스턴스를 가져온다
        GameObject casterObject = CharacterStats.Instance.characters[index];

        if (casterObject == null)
        {
            Debug.LogWarning("[SkillManager] 캐릭터 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 2. 스킬 시작 위치 결정
        Vector3 startPosition = Vector3.zero;

        switch (skill.startSkillPosition)
        {
            case StartSkillPosition.player:
                startPosition = character.charPosition;
                break;

            case StartSkillPosition.target:
                int targetIndex = CharacterSelection.selectedCharacterIndex;
                if (targetIndex == -1)
                {
                    Debug.LogWarning("대상이 선택되지 않았습니다.");
                    return;
                }

                GameObject targetObj = CharacterStats.Instance.characters[targetIndex];
                if (targetObj != null)
                {
                    startPosition = targetObj.transform.position;
                }
                else
                {
                    Debug.LogWarning("대상 GameObject 없음.");
                    return;
                }
                break;

            case StartSkillPosition.mouse:
                Vector3 rawMousePos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
                rawMousePos.z = 0f;

                Vector3Int tile = Vector3Int.RoundToInt(rawMousePos);
                startPosition = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0f);
                break;

            case StartSkillPosition.special:
                startPosition = new Vector3(0, 0, 0);
                break;

            default:
                startPosition = character.charPosition;
                break;
        }

        startPosition.z = 0f;

        // 3. 8방향 중 가장 가까운 방향 계산
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
        mouseWorldPos.z = 0f;

        Vector3 direction = (mouseWorldPos - startPosition).normalized;

        // 8방향 벡터
        Vector3[] directions = new Vector3[]
        {
        Vector3.up, Vector3.down, Vector3.left, Vector3.right,
        //(Vector3.up + Vector3.right).normalized,
        //(Vector3.up + Vector3.left).normalized,
        //(Vector3.down + Vector3.right).normalized,
        //(Vector3.down + Vector3.left).normalized
        };

        // 가장 가까운 방향 선택
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
        }

        // 4. 타겟 위치를 선택된 방향으로 range만큼 떨어진 위치로 설정
        Vector3 targetPosition = startPosition + closestDirection * skill.range;
        targetPosition.z = 0f;


        // 5. AOE 중심점 계산
        Vector3 aoeCenterPosition = Vector3.zero;
        Vector3 Poffset = Vector3.zero;

        switch (skill.aoecenter)
        {
            case aoeCenter.center:
                aoeCenterPosition = startPosition;
                break;

            case aoeCenter.edge:
            case aoeCenter.Rcorner:
            case aoeCenter.Lcorner:
                AoeCenterPosition(skill, closestDirection, startPosition, ref aoeCenterPosition, ref Poffset);
                break;

            default:
                aoeCenterPosition = startPosition;
                break;
        }

        // 6. Poffset 적용 (특정 AOE 모드일 경우만)
        switch (skill.aoecenter)
        {
            case aoeCenter.Rcorner:
            case aoeCenter.Lcorner:
                targetPosition += Poffset;
                break;
        }

        // --- [6. 대응단계 진입] ---
        GameObject target = null; // 현재 타겟팅 로직 없음. 추후 구현 예정

        if (skill.react != React.no && ReactManager.Instance.CanRespond(skill))
        {
            bool phase = TurnManager.Instance.IsInReactPhase();
            if (!phase)
            {
                if (CharacterSelection.selectedCharacterIndex == -1)
                {
                    Debug.LogWarning("[SkillManager] 캐릭터가 선택되지 않았습니다.");
                    return;
                }

                if (!waitingForResponse)// 이미 대응 중이면 덮어쓰기 금지
                {
                    // 대응용으로 임시 저장
                    pendingSkill = skill;
                    pendingCaster = casterObject;
                    pendingCharacter = character;
                    pendingAoeCenterPosition = aoeCenterPosition;
                    pendingTargetPosition = targetPosition;
                    waitingForResponse = true;

                    TurnManager.Instance.EnterReactPhase(character); // 대응단계 활성화
                    ReactManager.Instance.EnterResponsePhase(skill, casterObject);

                } 
                return; // 대응 우선 처리
            }
            else
            {
                if (hasReacted)
                {
                    Debug.LogWarning("이미 대응한 턴입니다");
                    return;
                }
                if (TurnManager.Instance.IsPlayerReactPhase())
                {
                    if(TurnManager.Instance.playerReactTrun <= TurnManager.Instance.playerUseSkillReactTrun)
                    {
                        return;
                    }
                    ++TurnManager.Instance.playerUseSkillReactTrun;
                }
                else
                {
                    if (TurnManager.Instance.enemyReactTrun <= TurnManager.Instance.enemyUseSkillReactTrun)
                    {
                        return;
                    }
                    ++TurnManager.Instance.enemyUseSkillReactTrun;
                }

                

                pendingReactSkill = skill;
                pendingReactCaster = casterObject;
                pendingReactCharacter = character;
                pendingReactAoeCenterPosition = aoeCenterPosition;
                pendingReactTargetPosition = targetPosition;
                ReactManager.Instance.EnterResponsePhase(skill, casterObject);
                hasReacted = true;

                return; // 대응 우선 처리
            }
        }

        // 7. 스킬 이펙트 프리팹 생성
        castskillon(skill, skill.SkillEffectPrefab, aoeCenterPosition, targetPosition, casterObject, character);
    }*/
    //더이상 미사용



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
                    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
                        new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
                    mouseWorld.z = 0f;

                    bool evenX = selectedSkill.Xaoe % 2 == 0;
                    bool evenY = selectedSkill.Yaoe % 2 == 0;

                    float x, y;

                    if (evenX)
                    {
                        // 무조건 가장 가까운 0.5 단위로 강제 정렬
                        x = Mathf.Floor(mouseWorld.x) + 0.5f;
                    }
                    else
                    {
                        x = Mathf.Round(mouseWorld.x); // 홀수는 정수 위치
                    }

                    if (evenY)
                    {
                        y = Mathf.Floor(mouseWorld.y) + 0.5f;
                    }
                    else
                    {
                        y = Mathf.Round(mouseWorld.y);
                    }

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
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(
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
        }

        // 타겟 위치
        Vector3 targetPosition = startPosition + closestDirection * skill.range;
        targetPosition.z = 0f;

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

        Debug.Log($"[SkillManager] 스킬 위치 계산 완료: 시작={startPosition}, AOE중심={aoeCenterPosition}, 타겟={targetPosition}");
    }


    /// <summary>
    /// 선택된 스킬의 실행을 확정하고 대응 조건을 판단하여 대응단계로 진입하거나 즉시 시전한다.
    /// 대응단계 중이면 대응자 스킬을 pendingReactSkill로 저장한다.
    /// </summary>
    public void ConfirmSkillCast()
    {
        if (!isSkillReady || selectedSkill == null || selectedCaster == null || selectedCharacter == null)
        {
            Debug.LogWarning("[SkillManager] 스킬 준비 상태가 아니거나 정보가 부족합니다.");
            return;
        }

        // 대응단계 중인 경우 → 대응자 쪽 처리
        if (TurnManager.Instance.IsInReactPhase())
        {
            if (hasReacted)
            {
                Debug.LogWarning("[SkillManager] 이미 대응했습니다.");
                return;
            }

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
            pendingReactSkill = selectedSkill;
            pendingReactCaster = selectedCaster;
            pendingReactCharacter = selectedCharacter;
            pendingReactAoeCenterPosition = selectedAoeCenterPosition;
            pendingReactTargetPosition = selectedTargetPosition;
            hasReacted = true;

            Debug.Log($"[SkillManager] 대응 스킬 저장 완료: {pendingReactSkill.skillName}");
            return;
        }

        // 일반 시전 시 대응 조건 판단
        if (selectedSkill.react != React.no && ReactManager.Instance.CanRespond(selectedSkill))
        {
            pendingSkill = selectedSkill;
            pendingCaster = selectedCaster;
            pendingCharacter = selectedCharacter;
            pendingAoeCenterPosition = selectedAoeCenterPosition;
            pendingTargetPosition = selectedTargetPosition;
            waitingForResponse = true;

            TurnManager.Instance.EnterReactPhase(selectedCharacter); // ← 핵심 수정: Stats로 전달
            ReactManager.Instance.EnterResponsePhase(selectedSkill, selectedCaster);

            Debug.Log($"[SkillManager] 대응단계 진입: 스킬 = {selectedSkill.skillName}");
            return;
        }

        // 대응 조건 없으면 바로 실행
        ExecuteSkill(selectedSkill, selectedAoeCenterPosition, selectedTargetPosition, selectedCaster, selectedCharacter);
        Debug.Log($"[SkillManager] 스킬 즉시 실행: {selectedSkill.skillName}");

        // 상태 초기화
        isSkillReady = false;
    }

    /// <summary>
    /// 스킬을 
    /// </summary>
    /// <param name="skill">스킬정보</param>
    /// <param name="aoeCenterPosition">스킬의 중심점</param>
    /// <param name="targetPosition">대상</param>
    /// <param name="casterObject">스킬의 실제 오브젝트</param>
    /// <param name="character">스킬을 쓰는 캐릭터</param>
    public void ExecuteSkill(SkillData skill, Vector3 aoeCenterPosition, Vector3 targetPosition, GameObject casterObject, Stats character)
    {
        GameObject skillObject = Instantiate(skill.SkillEffectPrefab, aoeCenterPosition, Quaternion.identity);

        if (skill.projectile)
        {
            SkillEffectProjectile effect = skillObject.GetComponent<SkillEffectProjectile>();
            if (effect != null)
                effect.Initialize(skill, targetPosition, casterObject, character);
        }
        else
        {
            SkillEffectHitscan effect = skillObject.GetComponent<SkillEffectHitscan>();
            if (effect != null)
                effect.Initialize(skill, targetPosition, casterObject, character);
        }

        Debug.Log($"[SkillManager] 스킬 실행 완료: {skill.skillName}");
    }


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

    // castskill에서 나온 스킬을 생성
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
    }



    public void ContinuePendingSkill()
    {
        if (waitingForResponse && pendingSkill != null && pendingCaster != null)
        {
            if (pendingReactSkill != null)
            {
                ExecuteSkill(pendingReactSkill, pendingReactAoeCenterPosition, pendingReactTargetPosition, pendingReactCaster, pendingReactCharacter);
                Debug.Log($"[SkillManager] 대응 스킬 실행: {pendingReactSkill.skillName}");
            }

            ExecuteSkill(pendingSkill, pendingAoeCenterPosition, pendingTargetPosition, pendingCaster, pendingCharacter);
            Debug.Log($"[SkillManager] 본 스킬 실행: {pendingSkill.skillName}");


            // 초기화
            pendingSkill = null;
            pendingCaster = null;
            pendingCharacter = null;
            pendingAoeCenterPosition = Vector3.zero;
            pendingTargetPosition = Vector3.zero;

            pendingReactSkill = null;
            pendingReactCaster = null;
            pendingReactCharacter = null;
            pendingReactAoeCenterPosition = Vector3.zero;
            pendingReactTargetPosition = Vector3.zero;

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

    public void EndResponsePhase()
    {
        Debug.Log("[ResponseManager] 대응단계 종료");

        TurnManager.Instance.ExitReactPhase(); // 대응단계에서 일반턴으로 복귀
        SkillManager.Instance.ContinuePendingSkill();
        hasReacted = false;
    }
}
