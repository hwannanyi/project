using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System.Collections.Generic;
using static ExcelReader;
using UnityEngine.Events;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    public static StoryManager instance; // 싱글톤 인스턴스
    public UnityEvent<string> OnStoryStart = new UnityEvent<string>();
    public UnityEvent<SkillManager> OnStoryStop = new UnityEvent<SkillManager>();
    public UnityEvent OnStoryEnd = new UnityEvent();

    public Sprite background; // 배경 이미지 스프라이트
    public Image backgroundImage; // 배경 이미지 UI 컴포넌트

    public GameObject StoryUI; // 스토리 UI 오브젝트
    public GameObject charUI; // 스토리 UI 오브젝트
    public GameObject GameUI; // 스토리 UI 오브젝트

    public bool isStoryActive = false; // 스토리 UI 활성화 여부
    public bool isStoryEnd = false; // 스토리 종료 여부

    public ExcelReader excelReader; // 스토리 데이터를 읽어오는 ExcelReader 참조 필드

    public GameObject characterUI;
    public TextMeshProUGUI characterName; // 캐릭터 이름 UI
    public TextMeshProUGUI talktext; // 대화 텍스트 UI
    public int currentTalkIndex = 0; // 현재 대사 인덱스
    public List<TalkData> talklist = new();
    public List<Sprite> characterSprites = new(); // 캐릭터 이미지 리스트
    private Dictionary<string, GameObject> characterUIPool = new(); // 오브젝트 풀 리스트

    [Header("팝업대화창")]
    public GameObject popUpStoryUI; // 스토리 UI 오브젝트
    public List<PopUpTalkData> PopUptalklist = new();
    public List<PopUpTalkData> PopUptalkRead = new();
    public TextMeshProUGUI popUptalktext; // 대화 텍스트 UI
    public RectTransform popUptalkRect; //
    public int currentPopUpTalkIndex = 0; // 현재 대사 인덱스
    public bool popUpisStoryActive = false; // 팝업 스토리 UI 활성화 여부
     
    [Header("이미 본거야")]
    public List<string> readStoryID = new(); // 읽은 스토리 ID 리스트
    public List<string> readpopupStoryID = new(); // 읽은 스토리 ID 리스트

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        excelReader = GetComponent<ExcelReader>();
        instance = this;
    }

    void Start()
    {
        try
        {
            // 스토리 시작 이벤트 발생
            StartCoroutine(WaitForExcelDataAndStartStory());
        }
        catch
        {
            StoryStart("에러"); // 스토리 시작 실패 시 에러 스토리 시작
        }
    }

    private IEnumerator WaitForExcelDataAndStartStory()
    {
        var stageManager = StageManager.Instance.CurrentStage;
        // 데이터가 로드될 때까지 대기
        while (excelReader == null || excelReader.storyTalk == null || excelReader.storyTalk.Count == 0)
        {
            yield return null; // 한 프레임 대기
        }
        StoryStart(stageManager.ID);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SkillManager skillManager = SkillManager.Instance;
            OnStoryStop.Invoke(skillManager); // SkillManager 인스턴스를 전달
        }

        if (!isStoryActive && popUpisStoryActive && Input.GetKeyDown(KeyCode.Return)) 
        {
            if (PopUptalkRead.Count >= 0 && currentPopUpTalkIndex < PopUptalkRead.Count - 1)
            {
                currentPopUpTalkIndex++;
                ShowCurrentPopUpTalk();
                LayoutRebuilder.ForceRebuildLayoutImmediate(popUptalkRect);
            }
            else
            {
                PopUpStoryStop();
            }
        }

        if (isStoryActive && Input.GetKeyDown(KeyCode.Return))
        {
            if (talklist.Count >= 0 && currentTalkIndex < talklist.Count - 1)
            {
                currentTalkIndex++;
                ShowCurrentTalk();
            }
            else
            {
                StoryEnd();
                if (popUpisStoryActive)
                    popUpStoryUI.SetActive(true);
            }
        }
    }

    public void StoryStart(string storyID)
    {
        isStoryActive = true;
        StoryUI.SetActive(true); // 스토리 UI 활성화
        GameUI.SetActive(false); // 게임 UI 비활성화


        LoadStory(storyID); // 예시로 스토리 ID 1을 시작
    }

    public void StoryReStart(string storyID)
    {
        isStoryActive = true;
        StoryUI.SetActive(true); // 스토리 UI 활성화
        GameUI.SetActive(false); // 게임 UI 비활성화
    }

    public void Stor1yStart(SkillManager storyID)
    {
        Debug.Log(storyID.waitingForResponse);
        popUpStoryUI.SetActive(true); // 팝업 스토리 UI 활성화
    }

    public void StoryStop()
    {
        isStoryActive = false;
        StoryUI.SetActive(false); // 스토리 UI 활성화
        GameUI.SetActive(true); // 게임 UI 비활성화
    }

    public void StoryEnd()
    {
        isStoryActive = false; // 스토리 UI 비활성화
        isStoryEnd = true; // 스토리 종료 상태 설정
        StoryUI.SetActive(false); // 스토리 UI 비활성화
        GameUI.SetActive(true); // 게임 UI 비활성화
        currentTalkIndex = 0; // 대사 인덱스 초기화
        talklist = new List<TalkData>(); // 대사 리스트 초기화
        characterSprites = new List<Sprite>(); // 캐릭터 이미지 리스트 초기화
        characterUIPool.Clear(); // 오브젝트 풀 초기화
        characterUIPool = new Dictionary<string, GameObject>(); // 오브젝트 풀 딕셔너리 초기화

    }

    [System.Serializable]
    public class TalkData
    {
        public string talkID;
        public List<string> characters;
        public List<string> left_side;
        public List<string> right_side;
        public string talk_character;
        public string talk;
        public string id;
        public string character_animated;
        public string production;

    }

    // 현재 대사를 UI에 표시하는 함수
    private void ShowCurrentTalk()
    {
        if (talklist.Count > 0 && currentTalkIndex < talklist.Count)
        {
            popUpStoryUI.SetActive(false);

            characterName.text = talklist[currentTalkIndex].talk_character;
            talktext.text = talklist[currentTalkIndex].talk;
            HowCharacterUI(talklist[currentTalkIndex].talk_character);
            if (talklist[currentTalkIndex].production == "stop")
            {
                currentTalkIndex++;
                StoryStop();
            }
        }

    }
    public void LoadStory(string storyID)
    {
        talklist.Clear(); // 기존 리스트 초기화

        // 스토리 데이터를 가져와서 storyID가 같은 것만 리스트에 저장
        foreach (var talk in excelReader.storyTalk)
        {
            if (talk.talkID == storyID)
            {
                // 새 TalkData 객체 생성
                TalkData newTalk = new TalkData();
                newTalk.talkID = talk.talkID;

                // 역슬래시(\) 기준으로 분할하여 리스트에 저장
                // null 체크 및 분할
                newTalk.characters = !string.IsNullOrEmpty(talk.characters)
                    ? new List<string>(talk.characters.Split('\\'))
                    : new List<string>();
                newTalk.left_side = !string.IsNullOrEmpty(talk.left_side)
                    ? new List<string>(talk.left_side.Split('\\'))
                    : new List<string>();
                newTalk.right_side = !string.IsNullOrEmpty(talk.right_side)
                    ? new List<string>(talk.right_side.Split('\\'))
                    : new List<string>();

                newTalk.talk_character = talk.talk_character;
                newTalk.talk = talk.talk;
                newTalk.id = talk.id;
                newTalk.character_animated = talk.character_animated;
                newTalk.production = talk.production;

                // 일치하는 Talk 객체를 리스트에 추가
                talklist.Add(newTalk);
            }
        }
        LoadCharacterSprites();

    }


    public void LoadCharacterSprites()
    {
        // talklist가 비어있지 않으면 첫 번째 대사의 characters만 추출
        if (talklist.Count == 0 || talklist[0].characters == null)
            return;

        HashSet<string> characterNames = new HashSet<string>(talklist[0].characters);

        Addressables.LoadAssetsAsync<Sprite>("CharacterSprites", null).Completed += (AsyncOperationHandle<IList<Sprite>> handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                characterSprites.Clear();
                foreach (var sprite in handle.Result)
                {
                    if (characterNames.Contains(sprite.name))
                    {
                        characterSprites.Add(sprite);
                    }
                }
                CreateCharacterUIPool(); // 캐릭터 UI 오브젝트 풀 생성
                ShowCurrentTalk(); // 첫 번째 대사 표시
            }
            else
            {
                Debug.LogWarning("캐릭터 이미지 리스트를 Addressable에서 불러오지 못했습니다.");
            }
        };
    }

    // 입력받은 캐릭터 이름(name)에 해당하는 Sprite를 찾아서 characterUI의 이미지를 변경
    public void HowCharacterUI(string name)
    {
        foreach (var kvp in characterUIPool)
        {
            if (kvp.Key == name)
            {
                kvp.Value.SetActive(true); // 이름이 일치하면 활성화
            }
            else
            {
                kvp.Value.SetActive(false); // 그 외는 비활성화
            }
        }
    }

    // characterSprites의 개수만큼 오브젝트 풀 생성
    private void CreateCharacterUIPool()
    {
        // 기존 풀 비우기
        foreach (var obj in characterUIPool.Values)
            Destroy(obj);
        characterUIPool.Clear();

        // characterSprites 리스트의 각 Sprite에 대해 오브젝트 생성 및 Sprite 적용
        foreach (var sprite in characterSprites)
        {
            GameObject obj = Instantiate(characterUI, charUI.transform); // 프리팹 생성
            obj.SetActive(false); // 초기에는 비활성화

            // Image 컴포넌트에 Sprite 적용
            var image = obj.GetComponent<Image>();
            if (image != null)
                image.sprite = sprite;

            // 딕셔너리 키는 Sprite의 이름(캐릭터 이름)
            characterUIPool[sprite.name] = obj;
        }
    }


    //////////////////////////////////////////////////////////////////////////////////////////////
    //팝업창 대화

    [System.Serializable]
    public class PopUpTalkData
    {
        public string talkID;
        public string character;
        public string talk;
        public string tran;
        public string production;
        public string id;
    }

    public void PopUpStoryStart(string storyID)
    {
        popUpisStoryActive = true; // 팝업 스토리 UI 활성화 상태 설정
        LoadPopUpStory(storyID); // 팝업 스토리 로드
    }
    public void PopUpStoryStop()
    {
        readpopupStoryID.Add(PopUptalkRead[0].id); // 팝업 대사 ID를 읽은 스토리 ID 리스트에 추가
        popUpStoryUI.SetActive(false); // 팝업 스토리 UI 비활성화
        popUpisStoryActive = false; // 팝업 스토리 UI 활성화 상태 설정
        currentPopUpTalkIndex = 0; // 팝업 대사 인덱스 초기화
    }

    public void PopUpStoryReStart(string ID)
    {
        if (!isStoryActive)
            popUpStoryUI.SetActive(true); // 팝업 스토리 UI 활성화
        ReadPopUpStory(ID); // 팝업 대사 읽기
        popUpisStoryActive = true; // 팝업 스토리 UI 활성화 상태 설정
    }

    public void PopUpStoryEnd()
    {
        popUpisStoryActive = false; // 팝업 스토리 UI 활성화 상태 설정

        popUpStoryUI.SetActive(false); // 팝업 스토리 UI 비활성화
        PopUptalklist = new(); // 팝업 대화 리스트 초기화
        readpopupStoryID = new(); // 읽은 팝업 스토리 ID 리스트 초기화
        currentPopUpTalkIndex = 0; // 팝업 대사 인덱스 초기화

    }
    public void LoadPopUpStory(string storyID)
    {
        PopUptalklist.Clear(); // 기존 리스트 초기화

        // 스토리 데이터를 가져와서 storyID가 같은 것만 리스트에 저장
        foreach (var talk in excelReader.popupstoryTalk)
        {
            if (talk.talkID == storyID)
            {
                // 새 TalkData 객체 생성
                PopUpTalkData newTalk = new PopUpTalkData();
                newTalk.talkID = talk.talkID;
                newTalk.character = talk.character;
                newTalk.talk = talk.talk;
                newTalk.tran = talk.tran;
                newTalk.production = talk.production;
                newTalk.id = talk.id;

                // 일치하는 Talk 객체를 리스트에 추가
                PopUptalklist.Add(newTalk);
            }
        }
    }
    public void ReadPopUpStory(string ID)
    {
        PopUptalkRead.Clear(); // 기존 내용 초기화

        foreach (var talk in PopUptalklist)
        {
            if (talk.id == ID)
            {
                PopUptalkRead.Add(talk);
            }
        }
        ShowCurrentPopUpTalk();
    }

    // 현재 대사를 UI에 표시하는 함수
    private void ShowCurrentPopUpTalk()
    {
        if (PopUptalkRead.Count > 0 && currentPopUpTalkIndex < PopUptalkRead.Count)
        {
            popUptalktext.text = PopUptalkRead[currentPopUpTalkIndex].talk;
        }
        else
        {
            PopUpStoryStop();
        }
    }
}
