using UnityEngine;

[CreateAssetMenu(fileName = "Standing", menuName = "Scriptable Objects/Standing")]
public class Standing : ScriptableObject
{
    public UDictionary<string, Sprite> expression = new();
}
