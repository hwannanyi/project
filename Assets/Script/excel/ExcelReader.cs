using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Text;
//using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using System.Text.RegularExpressions;


public class ExcelReader : MonoBehaviour
{


    // 읽어 올 파일 이름
    public string csvFileName = "storyTalk";
    public string csvpopUpStoryFileName = "popUpStory";
    // key:value 형태로 저장
    // key(메뉴명)로 value를 뽑아오기 위해
    // 원하는 형태로 선언해도 무방
    public List<Talk> storyTalk = new();
    public List<PopUpTalk> popupstoryTalk = new();

    // 읽어 온 데이터를 담을 구조체
    // 저는 클래스로 생성했습니다! struct로 생성해도 동일해요.
    [System.Serializable]
    public class Talk
    {
        public string talkID;
        public string characters;
        public string transform;
        public string expression;
        public string talk_character;
        public string talk;
        public string id;
        public string character_animated;
        public string production;
        public string backgroud;
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

    private IEnumerator ReadCSVstory()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "exceldata/storyTalk.csv");

        List<Talk> menuList = new();

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

            var splitData = SplitCsvLine(line);

            if (splitData.Count < 8)
            {
                Debug.LogWarning("잘못된 줄 무시: " + line);
                continue;
            }

            Talk menu = new()
            {
                talkID = splitData[0],
                characters = splitData[1],
                transform = splitData[2],
                expression = splitData[3],
                talk_character = splitData[4],
                talk = splitData[5],
                id = splitData[6],
                character_animated = splitData[7],
                production = splitData[8],
                backgroud = splitData[9]
            };

            storyTalk.Add(menu);
        }

        // 이거 하나로 모든 분기 커버
        yield return null;
    }

    private IEnumerator ReadCSVPopUpstory()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "exceldata/popUpStory.csv");

        List<PopUpTalk> menuList = new();

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

            var splitData = SplitCsvLine(line);

            if (splitData.Count < 7)
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

    private static List<string> SplitCsvLine(string line)
    {
        var matches = Regex.Matches(line, @"(?<=^|,)(?:""(?<val>([^""]|"""")*)""|(?<val>[^,]*))");
        var result = new List<string>();
        foreach (Match match in matches)
        {
            string value = match.Groups["val"].Value.Replace("\"\"", "\"");
            result.Add(value);
        }
        return result;
    }
}
