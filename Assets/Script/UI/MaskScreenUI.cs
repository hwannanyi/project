using UnityEngine;

public class MaskScreenUI : MonoBehaviour
{
    public GameObject maskScreenObj;
    public MaskScreen maskScreen;

    public GameObject charpick;
    public GameObject profile;
    public GameObject turn;
    public GameObject nextTurn;
    public GameObject skillCancel;
    public GameObject skillCast;


    public void Start()
    {
        maskScreenObj.SetActive(false);
    }

    public void SetMaskScreen(GameObject UI)
    {
        maskScreenObj.SetActive(true);

        maskScreen.SetMaskScreen(UI);
    }

    public void DownMaskScreen()
    {
        maskScreenObj.SetActive(false);
    }
}
