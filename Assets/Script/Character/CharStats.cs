using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharStats : MonoBehaviour
{

    public Stats Character;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var manager = CharacterStats.Instance;
        Character = manager.GetStats(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
