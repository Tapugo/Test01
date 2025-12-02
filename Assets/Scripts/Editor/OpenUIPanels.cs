using UnityEngine;
using UnityEditor;

namespace Incredicer.Editor
{
    public static class OpenUIPanels
    {
        [MenuItem("Incredicer/Open Skill Tree")]
        public static void OpenSkillTree()
        {
            var skillTreeUI = Object.FindObjectOfType<UI.SkillTreeUI>(true);
            if (skillTreeUI != null)
            {
                skillTreeUI.Show();
                Debug.Log("[OpenUIPanels] Skill Tree opened");
            }
            else
            {
                Debug.LogError("[OpenUIPanels] SkillTreeUI not found!");
            }
        }

        [MenuItem("Incredicer/Open Dice Shop")]
        public static void OpenDiceShop()
        {
            var diceShopUI = Object.FindObjectOfType<UI.DiceShopUI>(true);
            if (diceShopUI != null)
            {
                diceShopUI.Show();
                Debug.Log("[OpenUIPanels] Dice Shop opened");
            }
            else
            {
                Debug.LogError("[OpenUIPanels] DiceShopUI not found!");
            }
        }

        [MenuItem("Incredicer/Close All UI Panels")]
        public static void CloseAllPanels()
        {
            var skillTreeUI = Object.FindObjectOfType<UI.SkillTreeUI>(true);
            if (skillTreeUI != null)
            {
                skillTreeUI.Hide();
            }

            var diceShopUI = Object.FindObjectOfType<UI.DiceShopUI>(true);
            if (diceShopUI != null)
            {
                diceShopUI.Hide();
            }

            Debug.Log("[OpenUIPanels] All panels closed");
        }
    }
}
