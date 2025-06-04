using UnityEngine;
using UnityEngine.UI;

public class PixelArtUICanvasGenerator : MonoBehaviour
{
    public Sprite buttonDefault;
    public Sprite buttonSelected;
    public Sprite buttonSpeech;
    public Sprite buttonDocument;

    [ContextMenu("Create PixelArt UI Canvas")]
    public void CreateCanvas()
    {
        // Canvas 생성
        GameObject canvasGO = new GameObject("PixelArtCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // 버튼 생성 예시
        CreateSpriteButton("Q_Button", buttonDefault, new Vector2(-200, -100), canvasGO.transform, "Q");
        CreateSpriteButton("W_Button", buttonDefault, new Vector2(-100, -100), canvasGO.transform, "W");
        CreateSpriteButton("E_Button", buttonDefault, new Vector2(0, -100), canvasGO.transform, "E");
        CreateSpriteButton("R_Button", buttonDefault, new Vector2(100, -100), canvasGO.transform, "R");
        CreateSpriteButton("T_Button", buttonDefault, new Vector2(200, -100), canvasGO.transform, "T");

        CreateSpriteButton("CastButton", buttonSpeech, new Vector2(150, -200), canvasGO.transform, "시전");
        CreateSpriteButton("CancelButton", buttonDocument, new Vector2(-150, -200), canvasGO.transform, "취소");

        Debug.Log("✅ PixelArt UI Canvas 생성 완료");
    }

    void CreateSpriteButton(string name, Sprite sprite, Vector2 anchoredPos, Transform parent, string labelText)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent);
        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;

        Image img = btnGO.AddComponent<Image>();
        img.sprite = sprite;

        Button btn = btnGO.AddComponent<Button>();

        // 텍스트
        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform);
        Text txt = txtGO.AddComponent<Text>();
        txt.text = labelText;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.color = Color.black;
        txt.raycastTarget = false;

        RectTransform txtRT = txt.GetComponent<RectTransform>();
        txtRT.anchorMin = txtRT.anchorMax = new Vector2(0.5f, 0.5f);
        txtRT.pivot = new Vector2(0.5f, 0.5f);
        txtRT.anchoredPosition = Vector2.zero;
        txtRT.sizeDelta = rt.sizeDelta;
    }
}
