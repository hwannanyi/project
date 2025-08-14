using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
//using static UnityEditor.ShaderData;

public class StoryManager : MonoBehaviour
{
    public static StoryManager instance; // 싱글톤 인스턴스

    public SkillManager skillManager; // 스킬 매니저 참조 필드
    public StageDataManager stageDataManager; // 스테이지 데이터 매니저 참조 필드
    public AITurn aITurn; // AI 턴 매니저 참조 필드
    public TurnManager turnManager; // 턴 매니저 참조 필드

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
    public List<TalkData> talkRead = new();
    public List<Sprite> characterSprites = new(); // 캐릭터 이미지 리스트
    private Dictionary<string, GameObject> characterUIPool = new(); // 오브젝트 풀 리스트

    [Header("팝업대화창")]
    public GameObject popUpStoryUI; // 스토리 UI 오브젝트
    public List<PopUpTalkData> PopUptalklist = new();
    public List<PopUpTalkData> PopUptalkRead = new();
    public TextMeshProUGUI popUptalktext; // 대화 텍스트 UI
    public RectTransform popUptalkRect; // 팝업 대화창 RectTransform
    public int currentPopUpTalkIndex = 0; // 현재 대사 인덱스
    public bool popUpisStoryActive = false; // 팝업 스토리 UI 활성화 여부
    public bool ispopUpStoryEnd = false; // 스토리 종료 여부     

    public TextMeshProUGUI popUptalkNexttext; // 대화 텍스트 UI

    [Header("이미 본거야")]
    public List<string> readStoryID = new(); // 읽은 스토리 ID 리스트
    public List<string> readpopupStoryID = new(); // 읽은 스토리 ID 리스트


    private Dictionary<string, Action> LockActions; // 잠금 액션 딕셔너리

    [Header("잠금")]
    public bool chPickLock = false; // 캐릭터 선택 잠금 상태
    public bool moveLock = false;   // 이동 잠금 상태
    public bool turnLock = false;   // 턴 넘김 잠금 상태
    public bool skillLock = false;  // 스킬 사용 잠금 상태
    public bool timeLock = false;   // 시간정지 상태

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        excelReader = GetComponent<ExcelReader>();
        skillManager = GetComponent<SkillManager>();
        stageDataManager = GetComponent<StageDataManager>();
        InitLockActions();
        instance = this;
    }

    void Start()
    {
        (isStoryEnd,ispopUpStoryEnd) = stageDataManager.CurrentStage.storyTiming.Count == 0 ? (true, true) : (false, false); 
    }

/*    private IEnumerator WaitForExcelDataAndStartStory()
    {
        // 데이터가 로드될 때까지 대기
        while (excelReader == null || excelReader.storyTalk == null || excelReader.storyTalk.Count == 0)
        {
            yield return null; // 한 프레임 대기
        }
    }*/

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("씨발");
        }

        NextPopUpStory();
        if (isStoryActive && Input.GetKeyDown(KeyCode.Return))
        {
            if (talkRead.Count >= 0 && currentTalkIndex < talkRead.Count - 1)
            {
                currentTalkIndex++;
                ShowCurrentTalk();
            }
            else
            {
                Action end = (talkRead[^1] == talklist[^1]) ?
                StoryEnd : StoryStop; // 마지막 대사면 팝업 스토리 종료, 아니면 팝업 스토리 중지
                end.Invoke();
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

        // 읽은 스토리 ID 리스트 초기화
        readStoryID = new(); 

        LoadStory(storyID); // 예시로 스토리 ID 1을 시작
    }

    public void StoryReStart(string ID)
    {
        if (talklist.Count == 0)
        {
            StoryStart(stageDataManager.CurrentStage.ID);
            StoryReStart(ID); // 스토리 ID가 없으면 현재 스테이지의 ID로 시작
        }
        else 
        {
            isStoryActive = true;
            ReadStory(ID); // 스토리 ID로 대사 읽기
            StoryUI.SetActive(true); // 스토리 UI 활성화
            GameUI.SetActive(false); // 게임 UI 비활성화
        }
    }

    public void StoryStop()
    {
        readStoryID.Add(talkRead[0].id); // 대사 ID를 읽은 스토리 ID 리스트에 추가
        isStoryActive = false;
        currentTalkIndex = 0; // 대사 인덱스 초기화
        talkRead = new(); // 대사 리스트 초기화
        StoryUI.SetActive(false); // 스토리 UI 활성화
        GameUI.SetActive(true); // 게임 UI 비활성화
    }

    public void StoryEnd()
    {
        readStoryID.Add(talkRead[0].id); // 대사 ID를 읽은 스토리 ID 리스트에 추가
        UnPause(); // 모든 잠금 해제
        isStoryActive = false; // 스토리 UI 비활성화
        isStoryEnd = true; // 스토리 종료 상태 설정
        StoryUI.SetActive(false); // 스토리 UI 비활성화
        GameUI.SetActive(true); // 게임 UI 비활성화
        currentTalkIndex = 0; // 대사 인덱스 초기화
        talklist = new(); // 대사 리스트 초기화
        talkRead = new(); // 대사 리스트 초기화
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
        if (talkRead.Count > 0 && currentTalkIndex < talkRead.Count)
        {
            popUpStoryUI.SetActive(false);

            characterName.text = talkRead[currentTalkIndex].talk_character;
            talktext.text = talkRead[currentTalkIndex].talk;
            HowCharacterUI(talkRead[currentTalkIndex].talk_character);
/*            if (talklist[currentTalkIndex].production == "stop")
            {
                currentTalkIndex++;
                StoryStop();
            }*/
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



    //스토리 일부를 읽는 함수
    public void ReadStory(string ID)
    {
        talkRead.Clear(); // 기존 내용 초기화

        foreach (var talk in talklist)
        {
            if (talk.id == ID)
            {
                talkRead.Add(talk);
            }
        }
        ShowCurrentTalk();
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
        public List<string> production;
        public string id;
        public string next;
    }

    public void PopUpStoryStart(string storyID)
    {
        popUpisStoryActive = true; // 팝업 스토리 UI 활성화 상태 설정
        ispopUpStoryEnd = false; // 팝업 스토리 종료 상태 초기화
        readpopupStoryID = new(); // 읽은 팝업 스토리 ID 리스트 초기화
        LoadPopUpStory(storyID); // 팝업 스토리 로드
    }
    public void PopUpStoryStop()
    {
        readpopupStoryID.Add(PopUptalkRead[0].id); // 팝업 대사 ID를 읽은 스토리 ID 리스트에 추가
        popUpStoryUI.SetActive(false); // 팝업 스토리 UI 비활성화
        popUpisStoryActive = false; // 팝업 스토리 UI 활성화 상태 설정
        currentPopUpTalkIndex = 0; // 팝업 대사 인덱스 초기화
        Time.timeScale = 1f; // 시간 스케일을 1로 설정하여 게임 속도 정상화
    }

    public void PopUpStoryReStart(string ID)
    {

        if (PopUptalklist.Count == 0)
        {
            PopUpStoryStart(stageDataManager.CurrentStage.ID);
            PopUpStoryReStart(ID); // 팝업 스토리 ID가 없으면 현재 스테이지의 ID로 시작
        }
        else
        {
            Debug.Log("실행스토리" + ID); // 디버그 로그 출력
            ReadPopUpStory(ID); // 팝업 대사 읽기
            popUpisStoryActive = true; // 팝업 스토리 UI 활성화 상태 설정
            if (!isStoryActive)
            {
                popUpStoryUI.SetActive(true); // 팝업 스토리 UI 활성화
                LayoutRebuilder.ForceRebuildLayoutImmediate(popUptalkRect);
            }
        }
    
    }

    public void PopUpStoryEnd()
    {
        readpopupStoryID.Add(PopUptalkRead[0].id); // 팝업 대사 ID를 읽은 스토리 ID 리스트에 추가
        popUpisStoryActive = false; // 팝업 스토리 UI 활성화 상태 설정
        ispopUpStoryEnd = true; // 팝업 스토리 종료 상태 설정
        popUpStoryUI.SetActive(false); // 팝업 스토리 UI 비활성화
        PopUptalklist = new(); // 팝업 대화 리스트 초기화
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
                newTalk.production = !string.IsNullOrEmpty(talk.production)
                ? new List<string>(talk.production.Split('\\'))
                : new List<string>();
                newTalk.id = talk.id;
                newTalk.next = talk.next;

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
        popUptalkRect.anchoredPosition = UItrans();
        PopUpStoryProductionLock(PopUptalkRead[currentPopUpTalkIndex].production);
    }

    // 현재 대사를 UI에 표시하는 함수
    private void ShowCurrentPopUpTalk()
    {
        if (PopUptalkRead.Count > 0 && currentPopUpTalkIndex < PopUptalkRead.Count)
        {
            string talk = PopUptalkRead[currentPopUpTalkIndex].talk;
            if (!string.IsNullOrEmpty(talk) && talk.Contains("/"))
            {
                popUptalktext.text = string.Join("\n", talk.Split('/'));
                popUptalkNexttext.text = Nexttext(PopUptalkRead[currentPopUpTalkIndex].next);
            }
            else
            {
                popUptalktext.text = talk;
            }
        }
        else
        {
            PopUpStoryStop();
        }
    }
    public string Nexttext(string next)
    {
        string isNext = "Enter>>>";
        isNext = next == "chPick" ? "Character Select" : isNext; // 캐릭터 선택이 필요한 경우
        isNext = next == "skillPick" ? "Skill Select" : isNext; // 스킬 선택이 필요한 경우
        isNext = next == "skillCast" ? "Skill Cast" : isNext; // 스킬 시전이 필요한 경우
        isNext = next == "turn" ? "Pass the turn" : isNext; // 턴 넘기기가 필요한 경우
        isNext = next == "move" ? "Character Move" : isNext; // 이동이 필요한 경우
        return isNext+">>>";
    }
    public void NextPopUpStory()
    {
        try
        {
            string next = PopUptalkRead[currentPopUpTalkIndex].next;
            bool isNumber = float.TryParse(next, out float numberValue);
            bool isnext = isNumber ? isNumber : IsnextStory(next);
            if (!isStoryActive && popUpisStoryActive &&
                ((Input.GetKeyDown(KeyCode.Return) && String.IsNullOrEmpty(PopUptalkRead[currentPopUpTalkIndex].next)) || isnext)
                )
            {
                if(isNumber) StartCoroutine(AutoTextAndNext(numberValue)); // 숫자 값이면 자동 텍스트 실행
                else AdvancePopUpStory();
            }
        }
        catch
        {
        }
    }

    // 대기 후 대사 넘김
    private IEnumerator AutoTextAndNext(float time)
    {
        yield return new WaitForSeconds(time);
        AdvancePopUpStory();
    }

    // 대사 넘김 로직 분리
    private void AdvancePopUpStory()
    {
        if (PopUptalkRead.Count >= 0 && currentPopUpTalkIndex < PopUptalkRead.Count - 1)
        {
            currentPopUpTalkIndex++;
            ShowCurrentPopUpTalk();
            LayoutRebuilder.ForceRebuildLayoutImmediate(popUptalkRect);
            popUptalkRect.anchoredPosition = UItrans();
            PopUpStoryProductionLock(PopUptalkRead[currentPopUpTalkIndex].production);
        }
        else
        {
            Action end = (PopUptalkRead[PopUptalkRead.Count - 1] == PopUptalklist[PopUptalklist.Count - 1]) ?
                PopUpStoryEnd : PopUpStoryStop;
            UnPause();
            end.Invoke();
        }
    }

    /////////////////////////////////////////////////////////
    //등장위치
    /// <summary>
    /// 팝업 대화창의 좌표를 화면 크기에 비례하여 반환합니다.
    /// tran 문자열이 "x y" 형식(-1~1 범위)일 때,
    /// x: -1(왼쪽), 0(중앙), 1(오른쪽)
    /// y: -1(아래), 0(중앙), 1(위)
    /// 잘못된 tran 값이면 (0,0) 반환
    /// </summary>
    public Vector2 UItrans()
    {
        // tran에서 좌표값 파싱
        string tran = PopUptalkRead[currentPopUpTalkIndex].tran;
        float x = 0f, y = 0f;
        if (!string.IsNullOrEmpty(tran))
        {
            var parts = tran.Split(' ');
            // tran이 "숫자 숫자" 형식일 때만 파싱
            if (parts.Length == 2 && float.TryParse(parts[0], out float tx) && float.TryParse(parts[1], out float ty))
            {
                // -1~1 범위로 제한
                x = Mathf.Clamp(tx, -1f, 1f);
                y = Mathf.Clamp(ty, -1f, 1f);
            }
        }

        // 부모 RectTransform(캔버스) 기준으로 실제 좌표 계산
        RectTransform canvasRect = popUptalkRect.parent as RectTransform;
        if (canvasRect != null)
        {
            float halfWidth = canvasRect.rect.width / 2f;
            float halfHeight = canvasRect.rect.height / 2f;
            // x, y를 화면 크기에 비례한 픽셀 좌표로 변환
            float px = x * halfWidth;
            float py = y * halfHeight;
            return new Vector2(px, py);
        }
        // 잘못된 tran 값 또는 부모가 없으면 (0,0) 반환
        return Vector2.zero;
    }


    /////////////////////////////////////////////////////////
    //연출효과

    // 델리게이트 맵 초기화 (Awake 또는 생성자에서 호출)
    private void InitLockActions()
    {
        LockActions = new Dictionary<string, Action>
    {
        { "pause", PauseOn },
        { "moveLock", MoveLock },
        { "skillLock", SkillLock },
        { "turnLock", TurnLock },
        { "chPickLock", ChPickLock },
        { "timeLock", TimeLock }
        // 추가 명령어 및 함수 매핑
    };
    }

    // 명령어 실행 함수
    public void PopUpStoryProductionLock(List<string> Production)
    {
        if (LockActions == null)
            InitLockActions();

        //잠금 비활성화용
        bool hasMoveLock = false;
        bool hasTimeLock = false;
        bool hasSkillLock = false;
        bool hasTurnLock = false;
        bool hasChPickLock = false;
        bool hasPause = false;

        foreach (var command in Production)
        {

            if (LockActions.TryGetValue(command, out var action))
            {
                action.Invoke();//델리게이트 실행
                if (command == "moveLock")
                    hasMoveLock = true;
                if (command == "timeLock")
                    hasTimeLock = true;
                if (command == "skillLock")
                    hasSkillLock = true;
                if (command == "turnLock")
                    hasTurnLock = true;
                if (command == "chPickLock")
                    hasChPickLock = true;
                if (command == "pause")
                    hasPause = true;
            }
            else
            {
                Debug.LogWarning($"알 수 없는 명령어: {command}");
            }
        }
        if (hasPause)
            return; // 게임 일시 정지 상태로 유지
        Time.timeScale = 1f; // 게임 시간 재개
        // moveLock 명령어가 없으면 이동 잠금 해제
        if (!hasMoveLock)
            moveLock = false; // 이동 잠금 해제
        if (!hasTimeLock)
            timeLock = false; // 시간 정지 해제
        if (!hasSkillLock)
            skillLock = false; // 스킬 잠금 해제
        if (!hasTurnLock)
        {
            turnLock = false; // 턴 잠금 해제
            Time.timeScale = 1f; // 게임 시간 재개
        }
        if (!hasChPickLock)
            chPickLock = false; // 캐릭터 선택 잠금 해제
    }

    public void SkillLock()
    {
        skillLock = true; // 스킬 잠금 활성화
    }

    public void TurnLock()
    {

        turnLock = true; // 턴 잠금 활성화
    }

    public void MoveLock()
    {
        moveLock = true; // 이동 잠금 활성화
    }

    public void ChPickLock()
    {
        chPickLock = true; // 캐릭터 선택 잠금 활성화
    }
    public void TimeLock()
    {
        timeLock = true; // 시간 정지 활성화
        Time.timeScale = 0f; // 게임 시간 정지
    }


    public void UnPause()
    {
        skillLock = false; // 스킬 잠금 비활성화
        turnLock = false; // 턴 잠금 비활성화
        moveLock = false; // 이동 잠금 비활성화
        chPickLock = false; // 캐릭터 선택 잠금 비활성화
        timeLock = false; // 시간 정지 비활성화
        Time.timeScale = 1f; // 게임 시간 재개
    }

    public void PauseOn()
    {
        SkillLock();
        TurnLock();
        MoveLock();
        ChPickLock();
        TimeLock();
    }

    /////////////////////////////////////////////////////////
    //자동 넘기기 조건문
    public bool IsnextStory(string next)
    {
        bool isNext = false;
        isNext = next == "chPick" ? IschPick() : isNext; // 캐릭터 선택이 필요한 경우
        isNext = next == "skillPick" ? ISskillPick() : isNext; // 스킬 선택이 필요한 경우
        isNext = next == "skillCast" ? ISskillCast() : isNext; // 스킬 시전이 필요한 경우
        isNext = next == "turn" ? ISturn() : isNext; // 턴 넘기기가 필요한 경우
        isNext = next == "move" ? ISmove() : isNext; // 이동이 필요한 경우
        return isNext;
    }

    public bool IschPick()
    {
        return CharacterSelection.selectedCharacterIndex != -1; // 캐릭터가 선택되지 않은 상태
    }

    public bool ISskillPick() 
    {
        return skillManager.isSkillReady;
    }

    public bool ISskillCast()
    {
        return Input.GetKeyDown(KeyCode.Return);
    }

    public bool ISturn()
    {
        return Input.GetKeyDown(KeyCode.Space) || (!turnManager.isPlayerTurn && aITurn.AIturnEnd);
    }

    public bool ISmove()
    {
        return IschPick() && (
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow));
    }
}
