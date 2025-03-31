using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
public class CharacterForm : MonoBehaviour
{
    public Sprite character;
    public SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Characterform();
        }
    }

    public void Characterform()
    {
        if (CharacterStats.Instance.characters.Contains(gameObject))
        {
            int index = CharacterStats.Instance.characters.IndexOf(gameObject);
            spriteRenderer.sprite = CharacterStats.Instance.characterList[index].characterillustration;
        }
    }
}
