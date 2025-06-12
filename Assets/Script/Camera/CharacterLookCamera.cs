using UnityEngine;

public class CharacterLookCamera : MonoBehaviour
{   private enum Mode
    {
        LookAt,
        LookAtInverted, //* 반전 시켜 보기
        CameraForward,
        CameraForwardInverted, //* 반전 시켜 보기
    }

    public SpriteRenderer spriteRenderer;
    public bool norotate = false; // 카메라 회전 무시 여부

    void Update()
    {
        if(norotate)
            return;
        float camYaw = CameraZoom.NormalizedYaw;
        // 카메라가 ±90도 넘으면 x축 반전, 아니면 원래대로
        bool needFlip = Mathf.Abs(camYaw) > 90f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (needFlip ? -1 : 1);
        transform.localScale = scale;
    }

    [SerializeField] private Mode mode;
    private void LateUpdate()
    {
        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAtInverted:
                //* 카메라 방향을 알아내서 그 방향 만큼 돌려줘서 반전시키기
                Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dirFromCamera);
                break;
            case Mode.CameraForward:
                //* 카메라 방향으로 Z축 (앞뒤)을 바꿔주기
                transform.forward = Camera.main.transform.forward;
                break;
            case Mode.CameraForwardInverted:
                //* 카메라 방향으로 Z축 (앞뒤)을 바꿔주고 반전시키기
                transform.forward = -Camera.main.transform.forward;
                break;
            default:

                break;
        }


    }
}
