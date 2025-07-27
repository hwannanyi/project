using UnityEngine;
using UnityEngine.Events;

public class Tutorial : MonoBehaviour
{
    public StageManager stageManager;

    public UnityEvent<string> PopUpOnStoryStart = new UnityEvent<string>();
    public UnityEvent<string> PopUpOnStoryStop = new UnityEvent<string>();
    public UnityEvent PopUpOnStoryEnd = new UnityEvent();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        try
        {
            stageManager = StageManager.Instance;
        } catch {
            Debug.LogError("StageManager instance not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
