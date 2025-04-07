using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance; // 싱글턴 인스턴스 (전역 접근 가능)
    public List<SkillData> UseSkillList = new(); // 현재 사용 가능한 스킬 목록
    public GameObject skillPrefab; // 생성할 스킬 프리팹

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
        // 키 입력을 감지하여 특정 기능 실행
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Characterform(); // 캐릭터들의 스킬을 리스트에 저장
        }

        if (CharacterSelection.selectedCharacterIndex != -1)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CastSkill(0); // 스킬 사용
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            CastSkill(1); // 스킬 사용
        }
    }

    /// <summary>
    /// 모든 캐릭터의 스킬을 리스트(UseSkillList)에 추가하는 함수
    /// </summary>
    public void Characterform()
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
    }

    /// <summary>
    /// 캐릭터의 위치에서 스킬을 생성하고, SkillEffect를 초기화하는 함수
    /// </summary>
    /// <summary>
    /// 선택된 캐릭터의 스킬을 하나만 실행하는 함수
    /// </summary>
    public void CastSkill(int skillcast)
    {
        
        string name ;
        int index;
        if (TurnManager.Instance.IsPlayerTeamTurn)
        {
            if (TurnManager.Instance.playerUseSkillTurn >= TurnManager.Instance.playerSkillTurn)
            {
                return;
            }
            TurnManager.Instance.playerUseSkillTurn++;
            name = CharacterStats.Instance.playerCharacters[CharacterSelection.selectedCharacterIndex];
            index = CharacterStats.Instance.characterList.FindIndex(Character => Character.name == name);
        }
        else
        {
            if (TurnManager.Instance.enemyUseSkillTurn >= TurnManager.Instance.enemySkillTurn)
            {
                return;
            }
            TurnManager.Instance.enemyUseSkillTurn++;
            name = CharacterStats.Instance.EnemieCharacters[CharacterSelection.selectedCharacterIndex - CharacterStats.Instance.playerCharacters.Count];
            index = CharacterStats.Instance.characterList.FindIndex(Character => Character.name == name);
        }

        
        // 스킬 프리팹을 캐릭터 리스트의 캐리선책번째 캐릭터의 입력번째 스킬 프리팹으로 설정
        if (CharacterStats.Instance.characterList[index].usingSkill.Count < skillcast+1)
        {
            Debug.LogWarning("스킬을 찾을 수 없습니다");
            return;
        }
        skillPrefab = CharacterStats.Instance.characterList[index].usingSkill[skillcast].SkillEffectPrefab;

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
                startPosition = character.charPosition + Vector3.forward * 2;
                break;

            case StartSkillPosition.mouse:
                startPosition = Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
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

        // 7. 스킬 이펙트 프리팹 생성
        GameObject skillObject = Instantiate(skillPrefab, aoeCenterPosition, Quaternion.identity);

        // 8. 초기화 - 사용자 정보까지 넘김

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

    public void characterNumber(int skillcast)
    {
        
    }
}
