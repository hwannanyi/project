using UnityEngine;
using System.Collections.Generic;

public class MapSkillpre : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private HashSet<Collider> overlaps = new HashSet<Collider>();
    private Color defaultColor = new Color32(60, 60, 60, 188);

    private Color red1Color = new Color32(255, 93, 93, 188);

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = defaultColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("skillpre"))
        {
            overlaps.Add(other);
            spriteRenderer.color = red1Color;
        }

        if (other.CompareTag("skill"))
        {
            overlaps.Add(other);
            spriteRenderer.color = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("skillpre"))
        {
            overlaps.Remove(other);
            if (overlaps.Count == 0)
                spriteRenderer.color = defaultColor;
        }

        if (other.CompareTag("skill"))
        {
            overlaps.Remove(other);
            if (overlaps.Count == 0)
                spriteRenderer.color = defaultColor;
        }
    }

    private void LateUpdate()
    {
        // 파괴된 콜라이더가 HashSet에 남아있으면 제거
        if (overlaps.Count > 0)
        {
            overlaps.RemoveWhere(c => c == null);
            if (overlaps.Count == 0)
                spriteRenderer.color = defaultColor;
        }
    }
}