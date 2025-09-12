using UnityEngine;

public class MapSkillpre : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("skill"))
        {
            spriteRenderer.color = Color.red; // 빨간색으로 변경
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("skill"))
        {
            spriteRenderer.color = new Color32(60, 60, 60, 188); // 원래 색상으로 복원
        }
        
    }
}
