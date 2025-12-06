using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace Incredicer.Editor
{
    public static class FixMissionsPanelPrefab
    {
        [MenuItem("Incredicer/Fix Missions Panel Prefab")]
        public static void Execute()
        {
            string prefabPath = "Assets/Prefabs/UI/MissionsPanel.prefab";

            // Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[FixMissionsPanelPrefab] Could not find prefab at {prefabPath}");
                return;
            }

            // Instantiate to edit
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[FixMissionsPanelPrefab] Could not instantiate prefab");
                return;
            }

            int fixedCount = 0;

            // Find the Tabs container
            Transform tabs = instance.transform.Find("Tabs");
            if (tabs == null)
            {
                Debug.LogWarning("[FixMissionsPanelPrefab] Could not find Tabs");
            }
            else
            {
                // Fix DailyTab text
                Transform dailyTab = tabs.Find("DailyTab");
                if (dailyTab != null)
                {
                    Transform dailyText = dailyTab.Find("Text");
                    if (dailyText != null)
                    {
                        var tmp = dailyText.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.raycastTarget = false;
                            fixedCount++;
                            Debug.Log("[FixMissionsPanelPrefab] Fixed DailyTab/Text raycastTarget");
                        }
                    }

                    // Make sure button has targetGraphic set
                    var dailyButton = dailyTab.GetComponent<Button>();
                    var dailyImage = dailyTab.GetComponent<Image>();
                    if (dailyButton != null && dailyImage != null)
                    {
                        dailyButton.targetGraphic = dailyImage;
                        dailyImage.raycastTarget = true;
                        Debug.Log("[FixMissionsPanelPrefab] Fixed DailyTab button targetGraphic");
                    }
                }

                // Fix WeeklyTab text
                Transform weeklyTab = tabs.Find("WeeklyTab");
                if (weeklyTab != null)
                {
                    Transform weeklyText = weeklyTab.Find("Text");
                    if (weeklyText != null)
                    {
                        var tmp = weeklyText.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.raycastTarget = false;
                            fixedCount++;
                            Debug.Log("[FixMissionsPanelPrefab] Fixed WeeklyTab/Text raycastTarget");
                        }
                    }

                    // Make sure button has targetGraphic set
                    var weeklyButton = weeklyTab.GetComponent<Button>();
                    var weeklyImage = weeklyTab.GetComponent<Image>();
                    if (weeklyButton != null && weeklyImage != null)
                    {
                        weeklyButton.targetGraphic = weeklyImage;
                        weeklyImage.raycastTarget = true;
                        Debug.Log("[FixMissionsPanelPrefab] Fixed WeeklyTab button targetGraphic");
                    }
                }
            }

            // Also fix close button text if needed
            Transform header = instance.transform.Find("Header");
            if (header != null)
            {
                Transform closeButton = header.Find("CloseButton");
                if (closeButton != null)
                {
                    Transform closeX = closeButton.Find("X");
                    if (closeX != null)
                    {
                        var tmp = closeX.GetComponent<TextMeshProUGUI>();
                        if (tmp != null)
                        {
                            tmp.raycastTarget = false;
                            fixedCount++;
                            Debug.Log("[FixMissionsPanelPrefab] Fixed CloseButton/X raycastTarget");
                        }
                    }
                }
            }

            // Save the changes back to prefab
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[FixMissionsPanelPrefab] Fixed {fixedCount} text raycastTargets in prefab");
        }
    }
}
