/*us/System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffectProjectile : MonoBehaviour
{
    public float speed = 5f;
    public float range = 10f;
    public int damage = 10;
    public Skill skillData;

    private Vector3 startPosition;
    private Vector3 direction;

    public void Initialize(Skill skill, Vector3 targetPosition)
    {
        skillData = skill;
        speed = skill.projectileSpeed; // 스킬 데이터에서 속도 가져오기
        range = skill.range; // 사거리 설정

        startPosition = transform.position; // 시작 위치

        // 마우스 위치로 목표 설정 (z좌표는 고정)
        targetPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        targetPosition.z = startPosition.z; // 목표의 z좌표를 시작 위치의 z좌표와 동일하게 설정

        // x, y 방향으로만 계산하여 direction 설정
        direction = (targetPosition - startPosition).normalized;
    }

    void Update()
    {
        // 이동
        transform.position += direction * speed * Time.deltaTime;

        // 최대 사거리 도달 시 삭제
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject); // 스킬 객체 제거
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 적과 충돌했을 때
       *//* if (other.CompareTag("Enemy"))
        {
            // 데미지 적용
            other.GetComponent<Enemy>().TakeDamage(damage);

            // 효과 적용
            ApplySkillEffects(other.gameObject);

            // 명중 후 삭제
            Destroy(gameObject);
        }*//*
    }

    void ApplySkillEffects(GameObject target)
    {
        // 디버프 적용
        *//*foreach (DebuffEffect debuff in skillData.DebuffEffects)
        {
            if (debuff.Debuff != Debuffs.none)
            {
                target.GetComponent<Enemy>().ApplyDebuff(debuff);
            }
        }*//*

        // 버프 적용 (필요 시 추가)
    }

}



*/

using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.DebugUI;

// 스킬 효과(투사체)를 관리하는 클래스
public class SkillEffectProjectile : MonoBehaviour
{
    public float speed;  // 투사체 이동 속도
    public float range;  // 투사체의 최대 사거리
    public float Xaoe;   // 투사체 길이
    public int damage;   // 투사체가 가하는 피해량
    public Skill skillData;  // 스킬 데이터 참조

    private Vector3 startPosition;  // 투사체 시작 위치
    private Vector3 direction;      // 투사체 이동 방향


    public Transform rotatingVisual;

    public GameObject hitbox;
    public HitboxTile hitboxProject;

    private void Awake()
    {
        
    }
    
    // 투사체 초기화 메서드
    public void Initialize(Skill skill, Vector3 targetPosition)
    {

        

        skillData = skill;
        speed = skill.projectileSpeed;
        range = skill.range;
        Xaoe = skill.Xaoe;

        startPosition = transform.position;

        // direction 벡터 계산 (이동 방향)
        targetPosition.z = startPosition.z;
        direction = (targetPosition - startPosition).normalized;

        // 필수: direction이 0이 아닐 때만 회전 처리
        if (direction != Vector3.zero && rotatingVisual != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotatingVisual.rotation = Quaternion.Euler(0f, 0f, angle);
        }


        GameObject HitboxTile = Instantiate(hitbox, this.transform);
        HitboxTile.transform.localPosition = Vector3.zero;

        // 2. 그 인스턴스에서 SkillProjectileHitbox 스크립트를 가져와 초기화
        HitboxTile hitboxScript = HitboxTile.GetComponent<HitboxTile>();
        if (hitboxScript != null)
        {
            hitboxScript.Initialize(skill);
        }

    }

    void Update()
    {
        // 투사체를 지속적으로 이동시키는 기능
        transform.position += direction * speed * Time.deltaTime;

        // 최대 사거리에 도달하면 투사체 삭제
        if (Vector3.Distance(startPosition, transform.position) >= range - Xaoe)
            Destroy(gameObject);


    }
    // 충돌 처리 메서드
/*    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"[투사체] 충돌 감지: {other.gameObject.name}");
        // 충돌한 대상이 적일 경우
        if (skillData.projectileType.ToString() == "shot")
        {
            //dfdfdfdfdf
            Destroy(gameObject);
        }

        if (other.CompareTag("Tile"))
        {
            // 실제 피해 처리 로직은 필요 시 활성화
            // target.GetComponent<Enemy>().TakeDamage(damage);
            //Debug.Log("벽에 적중!");

            Destroy(gameObject);
        }
    }*/
}
/*    // 충돌 처리 메서드
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[투사체] 충돌 감지: {other.gameObject.name}");
        // 충돌한 대상이 적일 경우
        if (other.CompareTag("Tile"))
        {
           
           
                // 실제 피해 처리 로직은 필요 시 활성화
                // target.GetComponent<Enemy>().TakeDamage(damage);
                Debug.Log("벽에 적중!");
            
            Destroy(gameObject);
        }
    }

    private void ResizeColliderWithOffset()
    {
        if (boxCollider == null || skillData == null)
        {
            Debug.LogWarning("BoxCollider2D 또는 SkillData 없음");
            return;
        }

        // 크기 조정: 스킬의 Xaoe, Yaoe 반영
        boxCollider.size = new Vector2(skillData.Yaoe, skillData.Xaoe);

        // 오프셋 조정: 방향에 따라 한쪽 방향으로만 확장되게 설정
        Vector2 offset = Vector2.zero;
        float xOffset;
        float yOffset;

        switch (skillData.aoecenter)
        {
            
            case aoeCenter.center:

                offset = Vector2.zero;
                break;

            case aoeCenter.edge:
                     xOffset = (skillData.Xaoe - 1) * 0.5f;
                     offset = new Vector2(xOffset, 0f);
                break;

            case aoeCenter.Rcorner:
                    xOffset = (skillData.Xaoe - 1) * 0.5f;
                    yOffset = (skillData.Yaoe - 1) * 0.5f;
                    offset = new Vector2(yOffset, xOffset);
                break;

            case aoeCenter.Lcorner:
                xOffset = (skillData.Xaoe - 1) * 0.5f;
                yOffset = (skillData.Yaoe - 1) * 0.5f;
                offset = new Vector2(-yOffset, xOffset);
                break;

            default:
                offset = Vector2.zero;
                
                break;
        }
        boxCollider.offset = offset;

        Debug.Log(skillData.aoecenter);
        Debug.Log($"[투사체] 콜라이더 크기: {boxCollider.size}, 오프셋: {boxCollider.offset}");
    }*/

    /* // 범위(AOE) 내 타겟을 계산하는 메서드
     private List<GameObject> GetAoeTargets(Vector3 hitPosition)
     {
         List<GameObject> targets = new List<GameObject>();

         // 스킬 데이터에 따라 공격 범위 결정
         switch (skillData.aoetype)
         {
             case aoeType.single:
                 targets.Add(GetTargetAtPosition(hitPosition));
                 break;

             case aoeType.square:
                 targets.AddRange(GetTargetsInSquare(hitPosition, skillData.Xaoe));
                 break;
         }
         Debug.Log("AOE 대상 수: " + targets.Count);
         return targets;
     }

     // 특정 위치의 타겟을 얻는 메서드
     private GameObject GetTargetAtPosition(Vector3 position)
     {
         Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
         foreach (var col in colliders)
         {
             if (col.CompareTag("Tile"))
                 return col.gameObject;
         }
         return null;
     }*/

/*    // 타겟에 스킬 효과를 적용하는 메서드
    private void ApplySkillEffects(GameObject target)
    {
        foreach (DebuffEffect debuff in skillData.DebuffEffects)
        {
            // 실제 효과 적용은 필요 시 활성화
            // if (debuff.Debuff != Debuffs.none)
            // target.GetComponent<Enemy>().ApplyDebuff(debuff);
        }
    }
}*/
