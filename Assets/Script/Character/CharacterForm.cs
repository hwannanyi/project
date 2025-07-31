using System.Collections;
using UnityEngine;
public class CharacterForm : MonoBehaviour
{
    public Sprite character;
    public SpriteRenderer spriteRenderer;
    public Sprite blueTeam;
    public Sprite redTeam;
    public bool isTeamForm;
    public GameObject parentObject;
    public GameObject highlightEffect; // 인스펙터에서 할당
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    private void Awake()
    {
        StartCoroutine(TrySetCharacterData());
    }

    private IEnumerator TrySetCharacterData()
    {
        while (!CharacterStats.Instance.characters.Contains(parentObject))
        {
            yield return null; // 다음 프레임까지 대기
        }

        Characterform();
        SetHighlight(false);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Characterform();
        }
        if(CharacterSelection.selectedCharacterIndex == -1)
        {
            SetHighlight(false);
        }
    }

    public void Characterform()
    {
        if (isTeamForm)
        {
            if (CharacterStats.Instance.characters.Contains(gameObject))
            {
                int index = CharacterStats.Instance.characters.IndexOf(gameObject);
                if(CharacterStats.Instance.characterList[index].team == Team.team) 
                {
                    spriteRenderer.sprite = blueTeam;
                }
                else
                {
                    spriteRenderer.sprite = redTeam;
                }

            }
            else
            {
                Debug.Log("실패");
            }
        }
        else
        {
            if (CharacterStats.Instance.characters.Contains(parentObject))
            {
                int index = CharacterStats.Instance.characters.IndexOf(parentObject);
                spriteRenderer.sprite = CharacterStats.Instance.characterList[index].characterillustration;

                // 스프라이트의 세로 길이의 절반만큼 오브젝트를 부모 기준(로컬 좌표)으로 위로 이동
                float halfHeight = spriteRenderer.sprite.bounds.size.y / 2f;
                Vector3 localPos = transform.localPosition;
                localPos.y += halfHeight;
                transform.localPosition = localPos;
            }
        }
    }

    public void SetHighlight(bool isOn)
    {
        if (!isTeamForm)
            return;
        if (highlightEffect != null)
        highlightEffect.SetActive(isOn);
    }


}
