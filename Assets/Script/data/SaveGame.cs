using UnityEngine;

public class SaveGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            int i = StageManager.Instance.CurrentStage.stagenumber; // 현재 스테이지 번호 가져오기
            if (i == -1)
                return;
            
            StageClearData(i); // 스테이지 클리어 데이터 저장 메소드 호출
            
        }
    }

    public void StageClearData(int i)
    {
        DataManager.instance.data.isUnlock[i] = true; // 해당 스테이지의 잠금 해제 상태를 true로 설정
        // 게임 데이터를 저장하는 메소드 호출
        DataManager.instance.SaveGameData();
        Debug.Log("게임 데이터 저장됨");
    }
}
