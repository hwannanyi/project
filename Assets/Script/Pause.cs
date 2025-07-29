using UnityEngine;

public class Pause : MonoBehaviour
{
    // 5가지 잠금 상태를 배열로 관리
    // [캐릭터선택, 이동, 턴넘김, 스킬, 시간정지] 순서
    public bool[] locks = new bool[5];

    void Awake()
    {
        // 초기값 설정 예시 (모두 해제)
        SetLocks(new bool[] { false, false, false, false, false });
    }

    // 잠금 상태를 한 번에 설정하는 메서드
    public void SetLocks(bool[] selected)
    {
        if (selected.Length != 5) return; // 배열 길이 체크
        for (int i = 0; i < 5; i++)
        {
            locks[i] = selected[i];
        }
        // 각 잠금 변수에 적용
        chPickLock = locks[0];
        moveLock = locks[1];
        turnLock = locks[2];
        skillLock = locks[3];
        timeLock = locks[4];
    }

    public bool chPickLock = false; // 캐릭터 선택 잠금 상태
    public bool moveLock = false;   // 이동 잠금 상태
    public bool turnLock = false;   // 턴 넘김 잠금 상태
    public bool skillLock = false;  // 스킬 사용 잠금 상태
    public bool timeLock = false;   // 시간정지 상태

    void Update()
    {

    }
}
