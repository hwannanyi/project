/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEditor;
using static UnityEditor.Progress;
using System.Linq;


public class AddCharacter : MonoBehaviour
{

    public static List<string> characters = new List<string>();
    //public static string[] characters = new string[20];
    public static AddCharacter instance;


    // 등록된 캐릭터들의 정보 (이름 -> 캐릭터 스탯)
    //private Dictionary<string, CharacterStats> characterStatsDict = new Dictionary<string, CharacterStats>();

    void Awake()
    {
        characterAdd("melun");
        characterAdd("melunDago");
        characterAdd("MelunMelun");
        Debug.Log(characters.Count);
    }

    void characterAdd(string chname)
    {
        bool jungbok = CharacterStats.characterList.Any(s => s.name == chname);
        Debug.Log(jungbok);
        if (!jungbok)
        {
            //characters[2]=name; 
            characters.Add(chname);

        }
    }

}*/
