using UnityEngine;

public class StatusUpdate : MonoBehaviour
{

    public CharacterStats characterStats;
    public EventManager eventManager;

    public void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        eventManager = GetComponent<EventManager>();
    }

    public void Start()
    {
        eventManager.isMove -= FaceFace;
        eventManager.isMove += FaceFace;
    }

    public void OnDestroy()
    {
        eventManager.isMove -= FaceFace;
    }

    public void FaceFace()
    {
        Stats boss = characterStats.Boss;
        Stats ch1 = characterStats.PlayerCharacter1;
        Stats ch2 = characterStats.PlayerCharacter2;

        bool ch1FaceFace = boss.charRotation == ChRotation.right && ch1.charRotation == ChRotation.left ||
                        boss.charRotation == ChRotation.left && ch1.charRotation == ChRotation.right ||
                        boss.charRotation == ChRotation.up && ch1.charRotation == ChRotation.down ||
                        boss.charRotation == ChRotation.down && ch1.charRotation == ChRotation.up;
        bool ch2FaceFace = boss.charRotation == ChRotation.right && ch2.charRotation == ChRotation.left ||
                        boss.charRotation == ChRotation.left && ch2.charRotation == ChRotation.right ||
                        boss.charRotation == ChRotation.up && ch2.charRotation == ChRotation.down ||
                        boss.charRotation == ChRotation.down && ch2.charRotation == ChRotation.up;

        if(!ch1FaceFace)
            ch1.AddStatus(StatusType.FaceFace);
        else
            ch1.RemoveStatus(StatusType.FaceFace);
        if (!ch2FaceFace)
            ch2.AddStatus(StatusType.FaceFace);
        else
            ch2.RemoveStatus(StatusType.FaceFace);
    //Debug.Log("FaceFace" + ch1FaceFace + ch2FaceFace);
    }
}
