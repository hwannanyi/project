using System.Collections.Generic;
using UnityEngine;

public class SkillTargetManager : MonoBehaviour
{
    /*    public SkillManager skillManager;*/

    /*    void Awake()
        {
            skillManager = SkillManager.Instance;
        }*/

    // 예시: 특정 키를 눌렀을 때 출력
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            PrintRespondingCharacters();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Character 태그를 가진 오브젝트가 들어오면 중복 없이 리스트에 추가
        if (other.CompareTag("Character"))
        {
            if (!SkillManager.Instance.validReactTargets.Contains(other.gameObject))
            {
                SkillManager.Instance.validReactTargets.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Character 태그를 가진 오브젝트가 나가면 리스트에서 제거
        if (other.CompareTag("Character"))
        {
            SkillManager.Instance.validReactTargets.Remove(other.gameObject);
        }
    }

    // validReactTargets의 모든 오브젝트 이름을 출력
    public void PrintRespondingCharacters()
    {
        Debug.Log("=== respondingCharacter 목록 ===");
        foreach (var obj in SkillManager.Instance.validReactTargets)
        {
            if (obj != null)
                Debug.Log(obj.name);
            else
                Debug.Log("null");
        }
    }

    // 중복 없이 리스트에 추가하는 메서드 (List 사용 시)
    public void AddTargetToList(GameObject obj)
    {
        if (obj == null) return;
        var list = SkillManager.Instance.validReactTargets;
        if (!list.Contains(obj))
        {
            list.Add(obj);
        }
    }
}
