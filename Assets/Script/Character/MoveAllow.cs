
using UnityEngine;
public class MoveAllow : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        // 충돌이 끝난 오브젝트가 타일인 경우
        if (other.CompareTag("Tile") || other.CompareTag("MapBorder"))
        {
            // 모든 Renderer 컴포넌트 활성화
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }
    }
    // 타일에 부딪혔을 때 호출되는 메서드
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 타일인 경우
        if (other.CompareTag("Tile") || other.CompareTag("MapBorder"))
        {
            // 모든 Renderer 컴포넌트 비활성화
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
    }

    // 타일에 접촉 중일 때 계속 호출되는 메서드
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Tile") || other.CompareTag("MapBorder"))
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
    }

}
