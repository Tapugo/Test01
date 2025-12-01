using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using Incredicer.Skills;
using Incredicer.UI;

namespace Incredicer.Editor
{
    public static class SetupActiveSkills
    {
        private const string BUTTON_RED_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Sprites/Components/Button/Button01_Demo_Red.png";
        private const string GUI_FONT_PATH = "Assets/Layer Lab/GUI-CasualFantasy/ResourcesData/Fonts/TMP_TiltWarp.asset";

        [MenuItem("Incredicer/Setup Active Skills")]
        public static void Execute()
        {
            // Create ActiveSkillManager if it doesn't exist
            ActiveSkillManager asm = Object.FindObjectOfType<ActiveSkillManager>();
            if (asm == null)
            {
                GameObject asmObj = new GameObject("ActiveSkillManager");
                asm = asmObj.AddComponent<ActiveSkillManager>();
                Debug.Log("[SetupActiveSkills] Created ActiveSkillManager");
            }

            // Find canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[SetupActiveSkills] No canvas found!");
                return;
            }

            // Check if button already exists
            GameObject existingBtn = GameObject.Find("ActiveSkillButton");
            if (existingBtn != null)
            {
                Debug.Log("[SetupActiveSkills] ActiveSkillButton already exists");
                return;
            }

            // Load assets
            Sprite redSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BUTTON_RED_PATH);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(GUI_FONT_PATH);

            // Create button
            GameObject btnObj = new GameObject("ActiveSkillButton");
            btnObj.transform.SetParent(canvas.transform);

            RectTransform rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(20, 200);
            rt.sizeDelta = new Vector2(260, 130);

            Image img = btnObj.AddComponent<Image>();
            if (redSprite != null)
            {
                img.sprite = redSprite;
                img.type = Image.Type.Sliced;
            }
            img.color = Color.white;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            btn.colors = colors;

            // Add LayoutElement
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.minWidth = 260;
            le.minHeight = 130;
            le.preferredWidth = 260;
            le.preferredHeight = 130;

            // Create text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 8);
            textRT.offsetMax = new Vector2(-10, -8);
            textRT.localScale = Vector3.one;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Roll Burst\n<size=60%>Locked</size>";
            tmp.fontSize = 40;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            if (font != null)
            {
                tmp.font = font;
            }

            // Start hidden (will show when skill is unlocked)
            btnObj.SetActive(false);

            // Wire up to GameUI
            GameUI gameUI = canvas.GetComponent<GameUI>();
            if (gameUI != null)
            {
                SerializedObject so = new SerializedObject(gameUI);
                so.FindProperty("activeSkillButton").objectReferenceValue = btn;
                so.FindProperty("activeSkillButtonText").objectReferenceValue = tmp;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(gameUI);
                Debug.Log("[SetupActiveSkills] Wired button to GameUI");
            }

            EditorUtility.SetDirty(btnObj);
            Debug.Log("[SetupActiveSkills] Created ActiveSkillButton");
        }
    }
}
