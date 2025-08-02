using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Text;
//using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class ExcelReader : MonoBehaviour
{
    // 읽어 올 파일 이름
    public string csvFileName = "storyTalk";
    public string csvpopUpStoryFileName = "popUpStory";
    // key:value 형태로 저장
    // key(메뉴명)로 value를 뽑아오기 위해
    // 원하는 형태로 선언해도 무방
    public List<Talk> storyTalk = new List<Talk>();
    public List<PopUpTalk> popupstoryTalk = new List<PopUpTalk>();

    // 읽어 온 데이터를 담을 구조체
    // 저는 클래스로 생성했습니다! struct로 생성해도 동일해요.
    [System.Serializable]
    public class Talk
    {
        public string talkID;
        public string characters;
        public string left_side;
        public string right_side;
        public string talk_character;
        public string talk;
        public string id;
        public string character_animated;
        public string production;

    }

    [System.Serializable]
    public class PopUpTalk
    {
        public string talkID;
        public string character;
        public string talk;
        public string tran;
        public string production;
        public string id;
        public string next;
    }

    private void Awake()
    {
        StartCoroutine(ReadCSVstory());
        StartCoroutine(ReadCSVPopUpstory());
    }

    // 파일을 읽어 오는 메서드
    /*    private void ReadCSVstory()
        {
            // 파일 이름.확장자
            string path = "storyTalk.csv";

            // 데이터를 저장하는 리스트
            // 편하게 관리하기 위해 List로 선언
            // 원하는 형태로 선언하시면 됩니다!
            List<Talk> menuList = new List<Talk>();

            // stream reader
            // UTF-8로 인코딩 하려면 해당 StreamReader가 필요함!!
            // Application.dataPath는 Unity의 Assets폴더의 절대경로
            // 뒤에 읽으려는 파일이 있는 경로를 작성
            // ex) Assets > Files에 menu.csv를 읽으려면? "/" + "Files/menu.csv"추가
            StreamReader reader = new StreamReader(Application.dataPath + "/exceldata/" + path);

            // 마지막 줄을 판별하기 위한 bool 타입 변수
            bool isFinish = false;


            while (isFinish == false)
            {
                // ReadLine은 한줄씩 읽어서 string으로 반환하는 메서드
                // 한줄씩 읽어서 data변수에 담으면
                string data = reader.ReadLine(); // 한 줄 읽기

                // data 변수가 비었는지 확인
                if (data == null)
                {
                    // 만약 비었다면? 마지막 줄 == 데이터 없음이니
                    // isFinish를 true로 만들고 반복문 탈출
                    isFinish = true;
                    break;
                }

                // .csv는 ,(콤마)를 기준으로 데이터가 구분되어 있으므로
                // ,(콤마)를 기준으로 데이터를 나눠서 list에 담음
                // ex) 샌드위치,200원,맛있어요! => [샌드위치][200원][맛있어요!]
                var splitData = data.Split(','); // 콤마로 데이터 분할

                // 위에 새성했던 메뉴 객체를 선언해주고
                Talk menu = new();

                // 메뉴를 리스트에 있던 데이터로 초기화
                // menu.name에 splitData[0]번째 있는 데이터를 담는다는 의미
                // 즉, menu 객체 name변수에는 splitData[0]에 담긴 "샌드위치"가 들어갑니다.

                menu.talkID = splitData[0];

                        menu.characters = splitData[1];

                menu.left_side = splitData[2];
                menu.right_side = splitData[3];
                menu.talk_character = splitData[4];
                menu.talk = splitData[5];
                menu.id = splitData[6];
                menu.character_animated = splitData[7];
                menu.production = splitData[8];

                // menu 객체에 다 담았다면 dictionary에 key와 value값으로 저장
                // 이렇게 해두면 dicMenu.Add("샌드위치");로 menu.name, menu.price .. 접근 가능
                storyTalk.Add(menu);
            }
        }

        private void ReadCSVPopUpstory()
        {
            // 파일 이름.확장자
            string path = "popUpStory.csv";

            // 데이터를 저장하는 리스트
            // 편하게 관리하기 위해 List로 선언
            // 원하는 형태로 선언하시면 됩니다!
            List<Talk> menuList = new List<Talk>();

            // stream reader
            // UTF-8로 인코딩 하려면 해당 StreamReader가 필요함!!
            // Application.dataPath는 Unity의 Assets폴더의 절대경로
            // 뒤에 읽으려는 파일이 있는 경로를 작성
            // ex) Assets > Files에 menu.csv를 읽으려면? "/" + "Files/menu.csv"추가
            StreamReader reader = new StreamReader(Application.dataPath + "/exceldata/" + path);

            // 마지막 줄을 판별하기 위한 bool 타입 변수
            bool isFinish = false;


            while (isFinish == false)
            {
                // ReadLine은 한줄씩 읽어서 string으로 반환하는 메서드
                // 한줄씩 읽어서 data변수에 담으면
                string data = reader.ReadLine(); // 한 줄 읽기

                // data 변수가 비었는지 확인
                if (data == null)
                {
                    // 만약 비었다면? 마지막 줄 == 데이터 없음이니
                    // isFinish를 true로 만들고 반복문 탈출
                    isFinish = true;
                    break;
                }

                // .csv는 ,(콤마)를 기준으로 데이터가 구분되어 있으므로
                // ,(콤마)를 기준으로 데이터를 나눠서 list에 담음
                // ex) 샌드위치,200원,맛있어요! => [샌드위치][200원][맛있어요!]
                var splitData = data.Split(','); // 콤마로 데이터 분할

                // 위에 새성했던 메뉴 객체를 선언해주고
                PopUpTalk menu = new();

                // 메뉴를 리스트에 있던 데이터로 초기화
                // menu.name에 splitData[0]번째 있는 데이터를 담는다는 의미
                // 즉, menu 객체 name변수에는 splitData[0]에 담긴 "샌드위치"가 들어갑니다.

                menu.talkID = splitData[0];
                menu.character = splitData[1];
                menu.talk = splitData[2];
                menu.tran = splitData[3];
                menu.production = splitData[4];
                menu.id = splitData[5];
                menu.next = splitData[6];

                // menu 객체에 다 담았다면 dictionary에 key와 value값으로 저장
                // 이렇게 해두면 dicMenu.Add("샌드위치");로 menu.name, menu.price .. 접근 가능
                popupstoryTalk.Add(menu);
            }
        }*/

    private IEnumerator ReadCSVstory()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "exceldata/storyTalk.csv");

        List<Talk> menuList = new List<Talk>();

        string csvText = "";

#if UNITY_ANDROID && !UNITY_EDITOR
    UnityWebRequest www = UnityWebRequest.Get(path);
    yield return www.SendWebRequest();

    if (www.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("CSV 불러오기 실패: " + www.error);
        yield break;
    }
    csvText = www.downloadHandler.text;
#else
        csvText = File.ReadAllText(path);
#endif

        string[] lines = csvText.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var splitData = line.Split(',');

            if (splitData.Length < 9)
            {
                Debug.LogWarning("잘못된 줄 무시: " + line);
                continue;
            }

            Talk menu = new()
            {
                talkID = splitData[0],
                characters = splitData[1],
                left_side = splitData[2],
                right_side = splitData[3],
                talk_character = splitData[4],
                talk = splitData[5],
                id = splitData[6],
                character_animated = splitData[7],
                production = splitData[8]
            };

            storyTalk.Add(menu);
        }

        // 이거 하나로 모든 분기 커버
        yield return null;
    }

    private IEnumerator ReadCSVPopUpstory()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "exceldata/popUpStory.csv");

        List<PopUpTalk> menuList = new List<PopUpTalk>();

        string csvText = "";

#if UNITY_ANDROID && !UNITY_EDITOR
    UnityWebRequest www = UnityWebRequest.Get(path);
    yield return www.SendWebRequest();

    if (www.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("CSV 불러오기 실패: " + www.error);
        yield break;
    }
    csvText = www.downloadHandler.text;
#else
        csvText = File.ReadAllText(path);
#endif

        string[] lines = csvText.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var splitData = line.Split(',');

            if (splitData.Length < 7)
            {
                Debug.LogWarning("잘못된 줄 무시: " + line);
                continue;
            }

            PopUpTalk menu = new()
            {
                talkID = splitData[0],
                character = splitData[1],
                talk = splitData[2],
                tran = splitData[3],
                production = splitData[4],
                id = splitData[5],
                next = splitData[6]
            };

            popupstoryTalk.Add(menu);
        }

        // 이거 하나로 모든 분기 커버
        yield return null;
    }


}
