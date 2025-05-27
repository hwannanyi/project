using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 적중 시뮬레이션을 담당하는 클래스
/// 실제 스킬 이펙트 없이 데이터 상에서 적중 대상을 판정한다
/// </summary>
public class SimulateSkillHit : MonoBehaviour
{
    public static SimulateSkillHit Instance;

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

        // 전체 캐릭터 순회
        foreach (GameObject character in CharacterStats.Instance.characters)
        {
            if (character == null || character == caster) continue;

            Stats targetStat = character.GetComponent<Stats>();
            if (targetStat == null) continue;

            // 같은 팀 제외
            if (targetStat.team == caster.GetComponent<Stats>().team) continue;

            // AOE 범위 내에 있는가?
            Vector3 pos = character.transform.position;
            bool inRange = false;

            // 사각형 범위 체크 (Xaoe, Yaoe는 float이므로 절대값 기반)
            if (Mathf.Abs(pos.x - center.x) <= skill.Xaoe / 2f &&
                Mathf.Abs(pos.y - center.y) <= skill.Yaoe / 2f)
            {
                inRange = true;
            }

            // 비관통형 투사체일 경우: 첫 번째 대상만 추가
            if (inRange)
            {
                if (!skill.penetration)
                {
                    result.Add(character);
                    break; // 첫 대상만
                }
                else
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
    public List<Vector3> GetAffectedTiles(SkillData skill, Vector3 center)
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
    }
}
