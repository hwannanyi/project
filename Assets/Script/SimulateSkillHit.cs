/*using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 적중 시뮬레이션을 담당하는 클래스
/// 실제 스킬 이펙트 없이 데이터 상에서 적중 대상을 판정한다
/// </summary>
public class SimulateSkillHit : MonoBehaviour
{
    public static SimulateSkillHit Instance;

    private List<Vector3> debugTiles = new();
    public bool showDebugGizmos = true;

    /// <summary>
    /// 마지막 시뮬레이션 결과 저장 (디버그용)
    /// </summary>
    public void SetDebugTiles(List<Vector3> tiles)
    {
        debugTiles = tiles;
    }

    void OnDrawGizmos()
    {
        if (skillBeingSimulated != null)
        {
            var tiles = SimulateSkillHit.Instance.GetAffectedTiles(
                skillBeingSimulated,
                skillAOECenter,
                skillTargetPosition
            );

            Gizmos.color = Color.red;
            foreach (var tile in tiles)
            {
                Gizmos.DrawWireCube(tile, Vector3.one);
            }
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 특정 위치에서 시전한 스킬의 적중 대상 리스트를 계산한다
    /// </summary>
    /// <param name="skill">시뮬레이션할 스킬</param>
    /// <param name="center">AOE 중심 위치</param>
    /// <param name="targetPos">스킬이 향하는 방향(또는 메인 타겟)</param>
    /// <param name="caster">시전자</param>
    /// <returns>적중 대상 유닛 리스트</returns>
    public List<GameObject> GetHitTargets(SkillData skill, Vector3 center, Vector3 targetPos, GameObject caster)
    {
        List<GameObject> result = new();

        // 시전자 스탯 확인
        if (!CharacterStats.Instance.characterMap.TryGetValue(caster, out var casterStat))
            return result;

        // --- 투사체 방식 ---
        if (skill.projectile)
        {
            Vector3 direction = (targetPos - center).normalized;

            for (int i = 0; i < Mathf.FloorToInt(skill.range); i++)
            {
                Vector3 stepCenter = center + direction * i;

                foreach (GameObject character in CharacterStats.Instance.characters)
                {
                    if (character == null || character == caster) continue;
                    if (!CharacterStats.Instance.characterMap.TryGetValue(character, out var targetStat)) continue;
                    if (targetStat.team == casterStat.team) continue;

                    Vector3 pos = character.transform.position;

                    if (Mathf.Abs(pos.x - stepCenter.x) <= skill.Xaoe / 2f &&
                        Mathf.Abs(pos.y - stepCenter.y) <= skill.Yaoe / 2f)
                    {
                        result.Add(character);

                        if (!skill.penetration) return result; // 비관통이면 첫 적에서 종료
                    }
                }
            }
        }
        // --- 일반 (비투사체) AOE 범위 감지 ---
        else
        {
            foreach (GameObject character in CharacterStats.Instance.characters)
            {
                if (character == null || character == caster) continue;
                if (!CharacterStats.Instance.characterMap.TryGetValue(character, out var targetStat)) continue;
                if (targetStat.team == casterStat.team) continue;

                Vector3 pos = character.transform.position;

                if (Mathf.Abs(pos.x - center.x) <= skill.Xaoe / 2f &&
                    Mathf.Abs(pos.y - center.y) <= skill.Yaoe / 2f)
                {
                    result.Add(character);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 특정 위치 기준으로 스킬의 AOE 범위 내 타일 좌표 리스트를 반환
    /// (시각적 디버깅용 또는 범위 체크용)
    /// </summary>
    /// <param name="skill">스킬 데이터</param>
    /// <param name="center">중심 위치</param>
    /// <returns>타일 좌표 리스트</returns>
    *//*public List<Vector3> GetAffectedTiles(SkillData skill, Vector3 center)
    {
        List<Vector3> result = new();

        int halfX = Mathf.FloorToInt(skill.Xaoe / 2f);
        int halfY = Mathf.FloorToInt(skill.Yaoe / 2f);

        for (int x = -halfX; x <= halfX; x++)
        {
            for (int y = -halfY; y <= halfY; y++)
            {
                Vector3 tile = new Vector3(center.x + x, center.y + y, 0);
                result.Add(tile);
            }
        }

        return result;
    }*//*
    public List<Vector3> GetAffectedTiles(SkillData skill, Vector3 center, Vector3 targetPos)
    {
        List<Vector3> result = new();

        List<Vector3> centers = skill.projectile
            ? GetProjectileAOECenters(skill, center, targetPos)
            : new List<Vector3> { center };

        foreach (var stepCenter in centers)
        {
            int halfX = Mathf.FloorToInt(skill.Xaoe / 2f);
            int halfY = Mathf.FloorToInt(skill.Yaoe / 2f);

            for (int x = -halfX; x <= halfX; x++)
            {
                for (int y = -halfY; y <= halfY; y++)
                {
                    Vector3 tile = new Vector3(stepCenter.x + x, stepCenter.y + y, 0);
                    result.Add(tile);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 투사체 스킬이 통과하는 모든 AOE 중심 위치를 반환한다
    /// </summary>
    public List<Vector3> GetProjectileAOECenters(SkillData skill, Vector3 center, Vector3 targetPos)
    {
        List<Vector3> centers = new();
        Vector3 direction = (targetPos - center).normalized;

        for (int i = 0; i < Mathf.FloorToInt(skill.range); i++)
        {
            Vector3 stepCenter = center + direction * i;
            centers.Add(stepCenter);
        }

        return centers;
    }
}
*/
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 적중 시뮬레이션을 담당하는 클래스
/// 실제 스킬 이펙트 없이 데이터 상에서 적중 대상을 판정하며,
/// 디버깅 시 시각적으로 범위를 Gizmo로 표시할 수 있다.
/// </summary>
public class SimulateSkillHit : MonoBehaviour
{
    public static SimulateSkillHit Instance;

    // 디버그 시각화용 변수들
    public SkillData debugSkill;
    public Vector3 debugStart;
    public Vector3 debugTarget;
    public Vector3 debugAOECenter;
    public List<Vector3> debugTiles = new();

    private void Awake()
    {

            Instance = this;
 
    }

    /// <summary>
    /// 특정 위치에서 시전한 스킬의 적중 대상 리스트를 계산한다
    /// </summary>
    public List<GameObject> GetHitTargets(SkillData skill, Vector3 startPos, Vector3 targetPos, GameObject caster)
    {
        List<GameObject> result = new();
        Vector3 direction = (targetPos - startPos).normalized;
        Debug.Log("좌표"+targetPos);
        Debug.Log("좌표" + startPos);
        Debug.Log("좌표" + direction);

        for (int i = 0; i <= skill.range; i++)
        {
            Vector3 stepCenter = startPos + direction * i;
            Debug.Log(i);
            Debug.Log(stepCenter);
            foreach (GameObject character in CharacterStats.Instance.characters)
            {
                if (!CharacterStats.Instance.characterMap.TryGetValue(character, out var targetStat)) continue;
                if (!CharacterStats.Instance.characterMap.TryGetValue(caster, out var casterStat)) continue;

                if (targetStat.team == casterStat.team) continue;

                Vector3 pos = character.transform.position;
                Debug.Log(pos);
                Debug.Log(stepCenter);
                if (Mathf.Abs(pos.x - stepCenter.x) <= skill.Xaoe / 2f &&
                    Mathf.Abs(pos.z - stepCenter.z) <= skill.Yaoe / 2f)
                {
                    result.Add(character);
                    if (!skill.penetration) return result; // 첫 대상만 처리
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 특정 위치 기준으로 스킬의 AOE 범위 내 타일 좌표 리스트를 반환
    /// (시각적 디버깅용 또는 범위 체크용)
    /// </summary>
    public List<Vector3> GetAffectedTiles(SkillData skill, Vector3 startPos, Vector3 targetPos)
    {
        List<Vector3> result = new();
        Vector3 direction = (targetPos - startPos).normalized;

        for (int i = 0; i < skill.range; i++)
        {
            Vector3 stepCenter = startPos + direction * i;
            int halfX = Mathf.FloorToInt(skill.Xaoe / 2f);
            int halfY = Mathf.FloorToInt(skill.Yaoe / 2f);

            for (int x = -halfX; x <= halfX; x++)
            {
                for (int y = -halfY; y <= halfY; y++)
                {
                    Vector3 tile = new Vector3(stepCenter.x + x, 0, stepCenter.z + y);
                    result.Add(tile);
                    //Debug.Log(tile);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// 디버그 시 시각적 범위 시뮬레이션
    /// </summary>
    public void SimulateForDebug(SkillData skill, Vector3 start, Vector3 target, Vector3 aoe)
    {
        debugSkill = skill;
        debugStart = start;
        debugTarget = target;
        debugAOECenter = aoe;
        debugTiles = GetAffectedTiles(skill, start, target);
    }

    private void OnDrawGizmos()
    {
        if (debugSkill == null || debugTiles == null) return;

        Gizmos.color = Color.red;
        foreach (var tile in debugTiles)
        {
            Gizmos.DrawWireCube(tile, Vector3.one);
        }
    }
}
