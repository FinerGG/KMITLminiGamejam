using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace MGJ.Editor
{
    public class GmailPrefabsBuilder : EditorWindow
    {
        [MenuItem("MGJ/Create Gmail Support Prefabs")]
        public static void CreateAllPrefabs()
        {
            CreateSignalRowPrefab();
            CreateEventQueueItemPrefab();

            Debug.Log("Gmail support prefabs created successfully!");
        }

        [MenuItem("MGJ/Create Signal Row Prefab")]
        public static void CreateSignalRowPrefab()
        {
            GameObject signalRow = new GameObject("SignalRow");
            RectTransform rect = signalRow.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);

            // Add SignalRow component
            SignalRow signalRowComponent = signalRow.AddComponent<SignalRow>();

            // Background
            Image bg = signalRow.AddComponent<Image>();
            bg.color = HexToColor("#080D0E");

            HorizontalLayoutGroup layout = signalRow.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(15, 15, 12, 12);
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Number text
            GameObject numberObj = CreateText(signalRow.transform, "01", 14, FontStyles.Normal);
            numberObj.name = "NumberText";
            SetTextColor(numberObj, HexToColor("#59D6C7"));
            RectTransform numberRect = numberObj.GetComponent<RectTransform>();
            numberRect.sizeDelta = new Vector2(40, 30);

            // Signal text
            GameObject signalObj = CreateText(signalRow.transform, "โดเมนภายใน", 14, FontStyles.Normal);
            signalObj.name = "SignalText";
            SetTextColor(signalObj, HexToColor("#E5E7E7"));
            RectTransform signalRect = signalObj.GetComponent<RectTransform>();
            signalRect.sizeDelta = new Vector2(800, 30);

            // Icon (waveform)
            GameObject icon = CreateWaveformIcon(signalRow.transform);
            icon.name = "Icon";

            // Bottom divider
            GameObject divider = new GameObject("Divider");
            RectTransform dividerRect = divider.AddComponent<RectTransform>();
            dividerRect.SetParent(signalRow.transform, false);
            dividerRect.anchorMin = new Vector2(0, 0);
            dividerRect.anchorMax = new Vector2(1, 0);
            dividerRect.pivot = new Vector2(0.5f, 0);
            dividerRect.sizeDelta = new Vector2(0, 1);
            dividerRect.anchoredPosition = Vector2.zero;

            Image dividerImg = divider.AddComponent<Image>();
            dividerImg.color = HexToColor("#1C2728");

            // Wire up references using SerializedObject
            SerializedObject serializedObject = new SerializedObject(signalRowComponent);
            serializedObject.FindProperty("numberText").objectReferenceValue = numberObj.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("signalText").objectReferenceValue = signalObj.GetComponent<TextMeshProUGUI>();
            serializedObject.ApplyModifiedProperties();

            // Save as prefab
            string path = "Assets/Prefabs/SignalRow.prefab";
            PrefabUtility.SaveAsPrefabAsset(signalRow, path);
            DestroyImmediate(signalRow);

            Debug.Log($"Signal Row prefab created at: {path}");
        }

        [MenuItem("MGJ/Create Event Queue Item Prefab")]
        public static void CreateEventQueueItemPrefab()
        {
            GameObject item = new GameObject("EventQueueItem");
            RectTransform rect = item.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 80);

            // Add EventQueueItem component
            EventQueueItem eventQueueComponent = item.AddComponent<EventQueueItem>();

            // Background
            Image bg = item.AddComponent<Image>();
            bg.color = HexToColor("#080D0E");

            // Selected indicator (left border)
            GameObject indicator = new GameObject("SelectedIndicator");
            RectTransform indicatorRect = indicator.AddComponent<RectTransform>();
            indicatorRect.SetParent(item.transform, false);
            indicatorRect.anchorMin = new Vector2(0, 0);
            indicatorRect.anchorMax = new Vector2(0, 1);
            indicatorRect.pivot = new Vector2(0, 0.5f);
            indicatorRect.sizeDelta = new Vector2(3, 0);
            indicatorRect.anchoredPosition = Vector2.zero;

            Image indicatorImg = indicator.AddComponent<Image>();
            indicatorImg.color = HexToColor("#59D6C7");
            indicator.SetActive(false);

            // Content layout
            GameObject content = new GameObject("Content");
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.SetParent(item.transform, false);
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(20, 10);
            contentRect.offsetMax = new Vector2(-20, -10);

            HorizontalLayoutGroup layout = content.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Icon (envelope)
            GameObject icon = CreateEnvelopeIcon(content.transform);
            icon.name = "Icon";

            // Text group
            GameObject textGroup = new GameObject("TextGroup");
            RectTransform textGroupRect = textGroup.AddComponent<RectTransform>();
            textGroupRect.SetParent(content.transform, false);
            textGroupRect.sizeDelta = new Vector2(180, 60);

            VerticalLayoutGroup textLayout = textGroup.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 4;
            textLayout.childAlignment = TextAnchor.UpperLeft;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = false;

            // Category text
            GameObject categoryObj = CreateText(textGroup.transform, "MAIL GATEWAY", 10, FontStyles.Normal);
            categoryObj.name = "CategoryText";
            SetTextColor(categoryObj, HexToColor("#D6A83C"));
            RectTransform categoryRect = categoryObj.GetComponent<RectTransform>();
            categoryRect.sizeDelta = new Vector2(0, 14);

            // Title text
            GameObject titleObj = CreateText(textGroup.transform, "Night shift handover #1842", 12, FontStyles.Bold);
            titleObj.name = "TitleText";
            SetTextColor(titleObj, HexToColor("#E5E7E7"));
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 32);
            TextMeshProUGUI titleTmp = titleObj.GetComponent<TextMeshProUGUI>();
            titleTmp.enableWordWrapping = true;

            // Time text (right aligned)
            GameObject timeObj = CreateText(content.transform, "31s", 10, FontStyles.Normal);
            timeObj.name = "TimeText";
            SetTextColor(timeObj, HexToColor("#8B9696"));
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.sizeDelta = new Vector2(40, 30);
            TextMeshProUGUI timeTmp = timeObj.GetComponent<TextMeshProUGUI>();
            timeTmp.alignment = TextAlignmentOptions.Right;

            // Wire up references
            SerializedObject serializedObject = new SerializedObject(eventQueueComponent);
            serializedObject.FindProperty("categoryText").objectReferenceValue = categoryObj.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("titleText").objectReferenceValue = titleObj.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("timeText").objectReferenceValue = timeObj.GetComponent<TextMeshProUGUI>();
            serializedObject.FindProperty("selectedIndicator").objectReferenceValue = indicatorImg;
            serializedObject.ApplyModifiedProperties();

            // Add button for click selection
            Button btn = item.AddComponent<Button>();
            btn.targetGraphic = bg;
            ColorBlock colors = btn.colors;
            colors.normalColor = HexToColor("#080D0E");
            colors.highlightedColor = HexToColor("#101718");
            colors.pressedColor = HexToColor("#101718");
            colors.selectedColor = HexToColor("#101718");
            btn.colors = colors;

            // Save as prefab
            string path = "Assets/Prefabs/EventQueueItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(item, path);
            DestroyImmediate(item);

            Debug.Log($"Event Queue Item prefab created at: {path}");
        }

        private static GameObject CreateText(Transform parent, string text, int fontSize, FontStyles style)
        {
            GameObject textObj = new GameObject(text.Length > 20 ? "Text" : text);
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.SetParent(parent, false);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.overflowMode = TextOverflowModes.Overflow;

            return textObj;
        }

        private static void SetTextColor(GameObject textObj, Color color)
        {
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.color = color;
        }

        private static GameObject CreateWaveformIcon(Transform parent)
        {
            GameObject icon = new GameObject("WaveformIcon");
            RectTransform rect = icon.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(24, 24);

            // Simple waveform representation using text
            TextMeshProUGUI tmp = icon.AddComponent<TextMeshProUGUI>();
            tmp.text = "〰";
            tmp.fontSize = 18;
            tmp.color = HexToColor("#59D6C7");
            tmp.alignment = TextAlignmentOptions.Center;

            return icon;
        }

        private static GameObject CreateEnvelopeIcon(Transform parent)
        {
            GameObject icon = new GameObject("EnvelopeIcon");
            RectTransform rect = icon.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(32, 32);

            // Envelope icon using TextMeshPro
            TextMeshProUGUI tmp = icon.AddComponent<TextMeshProUGUI>();
            tmp.text = "✉";
            tmp.fontSize = 24;
            tmp.color = HexToColor("#59D6C7");
            tmp.alignment = TextAlignmentOptions.Center;

            return icon;
        }

        private static Color HexToColor(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color32(r, g, b, 255);
        }
    }
}
