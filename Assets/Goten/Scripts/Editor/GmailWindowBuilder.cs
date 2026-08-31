using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace MGJ.Editor
{
    public class GmailWindowBuilder : EditorWindow
    {
        [MenuItem("MGJ/Create Gmail Window Prefab")]
        public static void CreateGmailWindow()
        {
            GameObject window = CreateWindowStructure();

            // Save as prefab
            string path = "Assets/Prefabs/GmailWindow.prefab";
            PrefabUtility.SaveAsPrefabAsset(window, path);

            Debug.Log($"Gmail Window prefab created at: {path}");

            Selection.activeGameObject = window;
        }

        private static GameObject CreateWindowStructure()
        {
            // Main window
            GameObject window = new GameObject("GmailWindow");
            RectTransform windowRect = window.AddComponent<RectTransform>();
            windowRect.sizeDelta = new Vector2(1920, 1080);
            windowRect.anchorMin = new Vector2(0, 0);
            windowRect.anchorMax = new Vector2(1, 1);
            windowRect.anchoredPosition = Vector2.zero;
            windowRect.pivot = new Vector2(0.5f, 0.5f);

            // Add GmailWindow component
            GmailWindow gmailWindow = window.AddComponent<GmailWindow>();

            // Create main background
            GameObject background = CreateBackground(window.transform);

            // Create top bar
            GameObject topBar = CreateTopBar(window.transform);

            // Create left sidebar
            GameObject leftSidebar = CreateLeftSidebar(window.transform);

            // Create main workspace
            GameObject workspace = CreateMainWorkspace(window.transform);

            // Create bottom status bar
            GameObject bottomBar = CreateBottomBar(window.transform);

            // Wire up references to GmailWindow component
            WireUpReferences(gmailWindow, workspace, leftSidebar, bottomBar);

            return window;
        }

        private static GameObject CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            RectTransform rect = bg.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            SetStretchAnchors(rect);

            Image img = bg.AddComponent<Image>();
            img.color = HexToColor("#05090A");

            return bg;
        }

        private static GameObject CreateTopBar(Transform parent)
        {
            GameObject topBar = new GameObject("TopBar");
            RectTransform rect = topBar.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 70);
            rect.anchoredPosition = Vector2.zero;

            Image img = topBar.AddComponent<Image>();
            img.color = HexToColor("#080D0E");

            // Left side - Logo and title
            GameObject leftGroup = new GameObject("LeftGroup");
            RectTransform leftRect = leftGroup.AddComponent<RectTransform>();
            leftRect.SetParent(rect, false);
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0, 1);
            leftRect.pivot = new Vector2(0, 0.5f);
            leftRect.sizeDelta = new Vector2(500, 0);
            leftRect.anchoredPosition = new Vector2(20, 0);

            HorizontalLayoutGroup leftLayout = leftGroup.AddComponent<HorizontalLayoutGroup>();
            leftLayout.childAlignment = TextAnchor.MiddleLeft;
            leftLayout.spacing = 15;
            leftLayout.childControlWidth = false;
            leftLayout.childControlHeight = false;

            // Status dot
            GameObject dot = CreateStatusDot(leftGroup.transform);

            // AEGIS SECURE OS
            GameObject aegisText = CreateText(leftGroup.transform, "AEGIS SECURE OS", 18, FontStyles.Bold);
            SetTextColor(aegisText, HexToColor("#E5E7E7"));

            // Divider
            GameObject divider = CreateVerticalDivider(leftGroup.transform, 30);

            // THREAT OPERATIONS
            GameObject threatText = CreateText(leftGroup.transform, "THREAT OPERATIONS", 14, FontStyles.Normal);
            SetTextColor(threatText, HexToColor("#8B9696"));

            // Right side - Network status and close
            GameObject rightGroup = new GameObject("RightGroup");
            RectTransform rightRect = rightGroup.AddComponent<RectTransform>();
            rightRect.SetParent(rect, false);
            rightRect.anchorMin = new Vector2(1, 0);
            rightRect.anchorMax = new Vector2(1, 1);
            rightRect.pivot = new Vector2(1, 0.5f);
            rightRect.sizeDelta = new Vector2(400, 0);
            rightRect.anchoredPosition = new Vector2(-20, 0);

            HorizontalLayoutGroup rightLayout = rightGroup.AddComponent<HorizontalLayoutGroup>();
            rightLayout.childAlignment = TextAnchor.MiddleRight;
            rightLayout.spacing = 15;
            rightLayout.childControlWidth = false;
            rightLayout.childControlHeight = false;

            // Network text
            GameObject networkText = CreateText(rightGroup.transform, "ISOLATED NETWORK", 14, FontStyles.Normal);
            SetTextColor(networkText, HexToColor("#8B9696"));

            // Timer
            GameObject timerText = CreateText(rightGroup.transform, "00:00", 16, FontStyles.Bold);
            SetTextColor(timerText, HexToColor("#59D6C7"));

            // Close button
            GameObject closeBtn = CreateCloseButton(rightGroup.transform);

            return topBar;
        }

        private static GameObject CreateLeftSidebar(Transform parent)
        {
            GameObject sidebar = new GameObject("LeftSidebar");
            RectTransform rect = sidebar.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 0.5f);
            rect.sizeDelta = new Vector2(310, -140); // Account for top/bottom bars
            rect.anchoredPosition = new Vector2(0, -35);

            Image img = sidebar.AddComponent<Image>();
            img.color = HexToColor("#080D0E");

            VerticalLayoutGroup layout = sidebar.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 0;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Event Queue Header
            GameObject queueHeader = new GameObject("QueueHeader");
            RectTransform queueHeaderRect = queueHeader.AddComponent<RectTransform>();
            queueHeaderRect.SetParent(sidebar.transform, false);
            queueHeaderRect.sizeDelta = new Vector2(0, 40);

            HorizontalLayoutGroup headerLayout = queueHeader.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = false;
            headerLayout.childControlHeight = false;

            GameObject queueTitle = CreateText(queueHeader.transform, "EVENT QUEUE", 12, FontStyles.Normal);
            SetTextColor(queueTitle, HexToColor("#E5E7E7"));
            RectTransform queueTitleRect = queueTitle.GetComponent<RectTransform>();
            queueTitleRect.sizeDelta = new Vector2(180, 40);

            GameObject activeCount = CreateText(queueHeader.transform, "1 ACTIVE", 12, FontStyles.Normal);
            activeCount.name = "ActiveCountText";
            SetTextColor(activeCount, HexToColor("#D6A83C"));
            RectTransform activeCountRect = activeCount.GetComponent<RectTransform>();
            activeCountRect.sizeDelta = new Vector2(90, 40);

            // Event Queue Container
            GameObject queueContainer = new GameObject("EventQueueContainer");
            RectTransform queueContainerRect = queueContainer.AddComponent<RectTransform>();
            queueContainerRect.SetParent(sidebar.transform, false);
            queueContainerRect.sizeDelta = new Vector2(0, 500);

            VerticalLayoutGroup queueLayout = queueContainer.AddComponent<VerticalLayoutGroup>();
            queueLayout.spacing = 5;
            queueLayout.childControlWidth = true;
            queueLayout.childControlHeight = false;
            queueLayout.childForceExpandWidth = true;

            // Network visualization (bottom of sidebar)
            GameObject networkViz = CreateNetworkVisualization(sidebar.transform);

            return sidebar;
        }

        private static GameObject CreateMainWorkspace(Transform parent)
        {
            GameObject workspace = new GameObject("MainWorkspace");
            RectTransform rect = workspace.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(310, 70); // Left and bottom margins
            rect.offsetMax = new Vector2(0, -70); // Right and top margins

            Image img = workspace.AddComponent<Image>();
            img.color = HexToColor("#05090A");

            // Add scroll view
            ScrollRect scroll = workspace.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            // Content container
            GameObject content = new GameObject("Content");
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.SetParent(workspace.transform, false);
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(-40, 1000);
            contentRect.anchoredPosition = new Vector2(0, -20);

            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.spacing = 30;
            contentLayout.padding = new RectOffset(40, 40, 20, 20);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = true;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;

            // Case header
            GameObject caseHeader = CreateText(content.transform, "MAIL GATEWAY / CASE 0004", 12, FontStyles.Normal);
            caseHeader.name = "CaseNumberText";
            SetTextColor(caseHeader, HexToColor("#D6A83C"));
            RectTransform caseHeaderRect = caseHeader.GetComponent<RectTransform>();
            caseHeaderRect.sizeDelta = new Vector2(0, 20);

            // Email title
            GameObject emailTitle = CreateText(content.transform, "Night shift handover #1842", 36, FontStyles.Bold);
            emailTitle.name = "EmailTitleText";
            SetTextColor(emailTitle, HexToColor("#E5E7E7"));
            RectTransform emailTitleRect = emailTitle.GetComponent<RectTransform>();
            emailTitleRect.sizeDelta = new Vector2(0, 50);

            // Timer badge (positioned absolute in upper right)
            GameObject timerBadge = CreateTimerBadge(workspace.transform);

            // Divider
            CreateHorizontalDivider(content.transform);

            // SOURCE section
            CreateInfoSection(content.transform, "SOURCE", "SOC Lead", "SourceValueText");

            CreateHorizontalDivider(content.transform);

            // CAPTURED DATA section
            CreateInfoSection(content.transform, "CAPTURED DATA",
                "สรุปเหตุการณ์ละเอียดแบบอยู่ในระบบ Ticket ภายใน หมายเลข INC-1842",
                "CapturedDataValueText");

            CreateHorizontalDivider(content.transform);

            // OBSERVED SIGNALS section
            GameObject signalsSection = CreateSignalsSection(content.transform);

            CreateHorizontalDivider(content.transform);

            // POLICY DECISION section
            GameObject decisionSection = CreatePolicyDecisionSection(content.transform);

            return workspace;
        }

        private static GameObject CreateInfoSection(Transform parent, string label, string value, string valueObjectName)
        {
            GameObject section = new GameObject($"{label}Section");
            RectTransform rect = section.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(0, 80);

            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            GameObject labelObj = CreateText(section.transform, label, 12, FontStyles.Normal);
            SetTextColor(labelObj, HexToColor("#D6A83C"));
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(0, 20);

            GameObject valueObj = CreateText(section.transform, value, 16, FontStyles.Normal);
            valueObj.name = valueObjectName;
            SetTextColor(valueObj, HexToColor("#E5E7E7"));
            RectTransform valueRect = valueObj.GetComponent<RectTransform>();
            valueRect.sizeDelta = new Vector2(0, 40);

            return section;
        }

        private static GameObject CreateSignalsSection(Transform parent)
        {
            GameObject section = new GameObject("ObservedSignalsSection");
            RectTransform rect = section.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(0, 300);

            VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 15;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            GameObject label = CreateText(section.transform, "OBSERVED SIGNALS", 12, FontStyles.Normal);
            SetTextColor(label, HexToColor("#D6A83C"));
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(0, 20);

            // Signals container with border
            GameObject signalsContainer = new GameObject("SignalsContainer");
            RectTransform signalsRect = signalsContainer.AddComponent<RectTransform>();
            signalsRect.SetParent(section.transform, false);
            signalsRect.sizeDelta = new Vector2(0, 200);

            Image signalsBg = signalsContainer.AddComponent<Image>();
            signalsBg.color = HexToColor("#080D0E");

            Outline outline = signalsContainer.AddComponent<Outline>();
            outline.effectColor = HexToColor("#1C2728");
            outline.effectDistance = new Vector2(1, -1);

            VerticalLayoutGroup signalsLayout = signalsContainer.AddComponent<VerticalLayoutGroup>();
            signalsLayout.padding = new RectOffset(15, 15, 15, 15);
            signalsLayout.spacing = 0;
            signalsLayout.childControlWidth = true;
            signalsLayout.childControlHeight = false;

            ContentSizeFitter fitter = signalsContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            signalsContainer.name = "SignalsContainer";

            return section;
        }

        private static GameObject CreatePolicyDecisionSection(Transform parent)
        {
            GameObject section = new GameObject("PolicyDecisionSection");
            RectTransform rect = section.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(0, 150);

            // Divider at top
            GameObject topDivider = new GameObject("TopDivider");
            RectTransform divRect = topDivider.AddComponent<RectTransform>();
            divRect.SetParent(rect, false);
            divRect.anchorMin = new Vector2(0, 1);
            divRect.anchorMax = new Vector2(1, 1);
            divRect.sizeDelta = new Vector2(0, 2);
            divRect.anchoredPosition = new Vector2(0, 0);
            Image divImg = topDivider.AddComponent<Image>();
            divImg.color = HexToColor("#1C2728");

            // Content with horizontal layout
            GameObject content = new GameObject("Content");
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.SetParent(rect, false);
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, -2);

            HorizontalLayoutGroup layout = content.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 30, 30);
            layout.spacing = 40;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Left side - label
            GameObject leftSide = new GameObject("LeftSide");
            RectTransform leftRect = leftSide.AddComponent<RectTransform>();
            leftSide.transform.SetParent(content.transform, false);

            VerticalLayoutGroup leftLayout = leftSide.AddComponent<VerticalLayoutGroup>();
            leftLayout.childAlignment = TextAnchor.UpperLeft;
            leftLayout.spacing = 5;
            leftLayout.childControlWidth = true;
            leftLayout.childControlHeight = false;

            GameObject policyLabel = CreateText(leftSide.transform, "POLICY DECISION", 12, FontStyles.Normal);
            SetTextColor(policyLabel, HexToColor("#D6A83C"));
            RectTransform policyLabelRect = policyLabel.GetComponent<RectTransform>();
            policyLabelRect.sizeDelta = new Vector2(0, 20);

            GameObject policyDesc = CreateText(leftSide.transform, "การตัดสินใจสามารถส่งผลต่อระบบ", 10, FontStyles.Normal);
            SetTextColor(policyDesc, HexToColor("#8B9696"));
            RectTransform policyDescRect = policyDesc.GetComponent<RectTransform>();
            policyDescRect.sizeDelta = new Vector2(0, 16);

            // Right side - buttons
            GameObject rightSide = new GameObject("ButtonsGroup");
            RectTransform rightRect = rightSide.AddComponent<RectTransform>();
            rightSide.transform.SetParent(content.transform, false);

            HorizontalLayoutGroup buttonLayout = rightSide.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.childAlignment = TextAnchor.MiddleRight;
            buttonLayout.spacing = 20;
            buttonLayout.childControlWidth = false;
            buttonLayout.childControlHeight = false;

            // Quarantine button
            GameObject quarantineBtn = CreateActionButton(rightSide.transform, "กักกัน", HexToColor("#F0445A"), "⊘");
            quarantineBtn.name = "QuarantineButton";

            // Allow button
            GameObject allowBtn = CreateActionButton(rightSide.transform, "อนุญาต", HexToColor("#59D6C7"), "✓");
            allowBtn.name = "AllowButton";

            return section;
        }

        private static GameObject CreateActionButton(Transform parent, string text, Color color, string icon)
        {
            GameObject button = new GameObject($"{text}Button");
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(150, 50);

            Image img = button.AddComponent<Image>();
            img.color = HexToColor("#080D0E");

            Outline outline = button.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1, -1);

            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock colors = btn.colors;
            colors.normalColor = HexToColor("#080D0E");
            colors.highlightedColor = HexToColor("#101718");
            colors.pressedColor = HexToColor("#05090A");
            btn.colors = colors;

            // Button content
            GameObject content = new GameObject("Content");
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.SetParent(button.transform, false);
            SetStretchAnchors(contentRect);

            HorizontalLayoutGroup layout = content.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 10;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            GameObject iconObj = CreateText(content.transform, icon, 20, FontStyles.Normal);
            SetTextColor(iconObj, color);

            GameObject textObj = CreateText(content.transform, text, 16, FontStyles.Bold);
            SetTextColor(textObj, color);

            return button;
        }

        private static GameObject CreateBottomBar(Transform parent)
        {
            GameObject bottomBar = new GameObject("BottomBar");
            RectTransform rect = bottomBar.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(0, 40);
            rect.anchoredPosition = Vector2.zero;

            Image img = bottomBar.AddComponent<Image>();
            img.color = HexToColor("#080D0E");

            // Left side
            GameObject leftText = CreateText(bottomBar.transform, "🛡 POLICY ENGINE ACTIVE", 12, FontStyles.Normal);
            SetTextColor(leftText, HexToColor("#59D6C7"));
            RectTransform leftRect = leftText.GetComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0, 1);
            leftRect.pivot = new Vector2(0, 0.5f);
            leftRect.sizeDelta = new Vector2(300, 0);
            leftRect.anchoredPosition = new Vector2(20, 0);

            // Right side
            GameObject rightText = CreateText(bottomBar.transform, "0 EVENTS RESOLVED", 12, FontStyles.Normal);
            rightText.name = "EventsResolvedText";
            SetTextColor(rightText, HexToColor("#8B9696"));
            RectTransform rightRect = rightText.GetComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(1, 0);
            rightRect.anchorMax = new Vector2(1, 1);
            rightRect.pivot = new Vector2(1, 0.5f);
            rightRect.sizeDelta = new Vector2(300, 0);
            rightRect.anchoredPosition = new Vector2(-20, 0);
            TextAlignmentOptions rightAlign = TextAlignmentOptions.Right;
            rightText.GetComponent<TextMeshProUGUI>().alignment = rightAlign;

            return bottomBar;
        }

        private static GameObject CreateStatusDot(Transform parent)
        {
            GameObject dot = new GameObject("StatusDot");
            RectTransform rect = dot.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(12, 12);

            Image img = dot.AddComponent<Image>();
            img.color = HexToColor("#59D6C7");
            img.sprite = CreateCircleSprite();

            return dot;
        }

        private static GameObject CreateVerticalDivider(Transform parent, float height)
        {
            GameObject divider = new GameObject("Divider");
            RectTransform rect = divider.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(1, height);

            Image img = divider.AddComponent<Image>();
            img.color = HexToColor("#1C2728");

            return divider;
        }

        private static GameObject CreateHorizontalDivider(Transform parent)
        {
            GameObject divider = new GameObject("Divider");
            RectTransform rect = divider.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(0, 1);

            Image img = divider.AddComponent<Image>();
            img.color = HexToColor("#1C2728");

            LayoutElement layout = divider.AddComponent<LayoutElement>();
            layout.preferredHeight = 1;
            layout.minHeight = 1;

            return divider;
        }

        private static GameObject CreateCloseButton(Transform parent)
        {
            GameObject button = new GameObject("CloseButton");
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(40, 40);

            Image img = button.AddComponent<Image>();
            img.color = HexToColor("#080D0E");

            Outline outline = button.AddComponent<Outline>();
            outline.effectColor = HexToColor("#1C2728");
            outline.effectDistance = new Vector2(1, -1);

            Button btn = button.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject xText = CreateText(button.transform, "✕", 20, FontStyles.Normal);
            SetTextColor(xText, HexToColor("#8B9696"));
            RectTransform xRect = xText.GetComponent<RectTransform>();
            SetStretchAnchors(xRect);

            return button;
        }

        private static GameObject CreateTimerBadge(Transform parent)
        {
            GameObject badge = new GameObject("TimerBadge");
            RectTransform rect = badge.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(120, 50);
            rect.anchoredPosition = new Vector2(-40, -40);

            Image img = badge.AddComponent<Image>();
            img.color = HexToColor("#080D0E");

            Outline outline = badge.AddComponent<Outline>();
            outline.effectColor = HexToColor("#1C2728");
            outline.effectDistance = new Vector2(1, -1);

            GameObject timerText = CreateText(badge.transform, "0 SEC", 16, FontStyles.Bold);
            timerText.name = "TimerText";
            SetTextColor(timerText, HexToColor("#D6A83C"));
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            SetStretchAnchors(timerRect);

            return badge;
        }

        private static GameObject CreateNetworkVisualization(Transform parent)
        {
            GameObject viz = new GameObject("NetworkVisualization");
            RectTransform rect = viz.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.sizeDelta = new Vector2(0, 200);

            LayoutElement layout = viz.AddComponent<LayoutElement>();
            layout.minHeight = 200;
            layout.preferredHeight = 200;

            Image bg = viz.AddComponent<Image>();
            bg.color = HexToColor("#05090A");

            // Network label at bottom
            GameObject label = CreateText(viz.transform, "NETWORK SEGMENT 07", 8, FontStyles.Normal);
            SetTextColor(label, HexToColor("#8B9696"));
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0, 0);
            labelRect.pivot = new Vector2(0, 0);
            labelRect.sizeDelta = new Vector2(200, 20);
            labelRect.anchoredPosition = new Vector2(10, 10);

            // TODO: Add network nodes visualization (can be done with UI elements or custom mesh)

            return viz;
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

            return textObj;
        }

        private static void SetTextColor(GameObject textObj, Color color)
        {
            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.color = color;
        }

        private static void SetStretchAnchors(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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

        private static Sprite CreateCircleSprite()
        {
            // Create a simple circle texture
            int size = 32;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * size + x] = dist < radius ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static void WireUpReferences(GmailWindow gmailWindow, GameObject workspace, GameObject leftSidebar, GameObject bottomBar)
        {
            // Find and assign references
            gmailWindow.GetType().GetField("caseNumberText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("Content/CaseNumberText")?.GetComponent<TextMeshProUGUI>());

            gmailWindow.GetType().GetField("emailTitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("Content/EmailTitleText")?.GetComponent<TextMeshProUGUI>());

            gmailWindow.GetType().GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("TimerBadge/TimerText")?.GetComponent<TextMeshProUGUI>());

            gmailWindow.GetType().GetField("sourceValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("Content/SOURCESection/SourceValueText")?.GetComponent<TextMeshProUGUI>());

            gmailWindow.GetType().GetField("capturedDataValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("Content/CAPTURED DATASection/CapturedDataValueText")?.GetComponent<TextMeshProUGUI>());

            gmailWindow.GetType().GetField("signalsContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("Content/ObservedSignalsSection/SignalsContainer"));

            gmailWindow.GetType().GetField("quarantineButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("Content/PolicyDecisionSection/Content/ButtonsGroup/QuarantineButton")?.GetComponent<Button>());

            gmailWindow.GetType().GetField("allowButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, workspace.transform.Find("Content/PolicyDecisionSection/Content/ButtonsGroup/AllowButton")?.GetComponent<Button>());

            gmailWindow.GetType().GetField("eventQueueContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, leftSidebar.transform.Find("EventQueueContainer"));

            gmailWindow.GetType().GetField("activeCountText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, leftSidebar.transform.Find("QueueHeader/ActiveCountText")?.GetComponent<TextMeshProUGUI>());

            gmailWindow.GetType().GetField("eventsResolvedText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(gmailWindow, bottomBar.transform.Find("EventsResolvedText")?.GetComponent<TextMeshProUGUI>());
        }
    }
}
