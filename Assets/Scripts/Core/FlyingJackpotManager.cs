using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Incredicer.UI;
using Incredicer.TimeFracture;

namespace Incredicer.Core
{
    /// <summary>
    /// Manages randomly spawning flying jackpots that players can tap to catch for rewards.
    /// Rewards scale with player progress to stay relevant throughout the game.
    /// </summary>
    public class FlyingJackpotManager : MonoBehaviour
    {
        public static FlyingJackpotManager Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float minSpawnInterval = 45f;
        [SerializeField] private float maxSpawnInterval = 120f;
        [SerializeField] private float flyDuration = 10f;

        [Header("Rewards")]
        [SerializeField] private float baseMoneyRewardPercent = 0.1f;  // 10% of current money
        [SerializeField] private float minMoneyReward = 100f;
        [SerializeField] private float maxMoneyRewardPercent = 0.25f;  // 25% max
        [SerializeField] private float darkMatterRewardPercent = 0.05f; // 5% of current DM
        [SerializeField] private float timeShardsChance = 0.1f;  // 10% chance for time shards
        [SerializeField] private int baseTimeShardsReward = 1;

        [Header("Visual")]
        [SerializeField] private float jackpotSize = 120f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 1.15f;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Effects")]
        [SerializeField] private int coinRainCount = 30;
        [SerializeField] private float screenShakeIntensity = 0.4f;
        [SerializeField] private float screenShakeDuration = 0.5f;

        [Header("GUI Assets")]
        [SerializeField] private GUISpriteAssets guiAssets;

        // Runtime
        private Canvas jackpotCanvas;
        private GameObject currentJackpot;
        private bool isJackpotActive = false;
        private float nextSpawnTime;
        private Coroutine spawnCoroutine;

        // Events
        public event Action<double, double, int> OnJackpotCaught; // money, darkMatter, timeShards

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            guiAssets = GUISpriteAssets.Instance;
        }

        private void Start()
        {
            CreateJackpotCanvas();
            ScheduleNextSpawn();
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }

        private void OnDestroy()
        {
            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
        }

        private void CreateJackpotCanvas()
        {
            GameObject canvasObj = new GameObject("FlyingJackpotCanvas");
            canvasObj.transform.SetParent(transform);

            jackpotCanvas = canvasObj.AddComponent<Canvas>();
            jackpotCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            jackpotCanvas.sortingOrder = 150; // Above most UI but below popups

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void ScheduleNextSpawn()
        {
            nextSpawnTime = Time.time + UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitUntil(() => Time.time >= nextSpawnTime && !isJackpotActive);

                // Don't spawn if any major UI is open
                if (!IsAnyMajorUIOpen())
                {
                    SpawnFlyingJackpot();
                }

                ScheduleNextSpawn();
            }
        }

        private bool IsAnyMajorUIOpen()
        {
            // Check common UI panels that might interfere
            if (DiceShopUI.Instance != null && DiceShopUI.Instance.IsOpen) return true;
            if (MainMenuUI.Instance != null && MainMenuUI.Instance.IsMenuOpen) return true;
            return false;
        }

        /// <summary>
        /// Spawns a flying jackpot from a random edge of the screen.
        /// </summary>
        public void SpawnFlyingJackpot()
        {
            if (isJackpotActive) return;
            isJackpotActive = true;

            // Create jackpot object
            currentJackpot = new GameObject("FlyingJackpot");
            currentJackpot.transform.SetParent(jackpotCanvas.transform, false);

            RectTransform rt = currentJackpot.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(jackpotSize, jackpotSize);

            // Background with GUI sprite (use star or coin icon)
            Image bg = currentJackpot.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconStar != null)
            {
                bg.sprite = guiAssets.iconStar;
                bg.color = new Color(1f, 0.85f, 0.2f); // Golden color
            }
            else if (guiAssets != null && guiAssets.iconCoin != null)
            {
                bg.sprite = guiAssets.iconCoin;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(1f, 0.85f, 0.2f);
            }

            // Add glow effect container
            GameObject glowObj = new GameObject("Glow");
            glowObj.transform.SetParent(currentJackpot.transform, false);
            RectTransform glowRt = glowObj.AddComponent<RectTransform>();
            glowRt.anchorMin = Vector2.zero;
            glowRt.anchorMax = Vector2.one;
            glowRt.offsetMin = new Vector2(-20, -20);
            glowRt.offsetMax = new Vector2(20, 20);
            glowObj.transform.SetAsFirstSibling();

            Image glowImg = glowObj.AddComponent<Image>();
            glowImg.color = new Color(1f, 0.9f, 0.3f, 0.5f);
            glowImg.raycastTarget = false;

            // Pulsing glow animation
            glowObj.transform.DOScale(1.3f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            glowImg.DOFade(0.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            // "JACKPOT!" label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(currentJackpot.transform, false);
            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0.5f, 0);
            labelRt.anchorMax = new Vector2(0.5f, 0);
            labelRt.pivot = new Vector2(0.5f, 1);
            labelRt.anchoredPosition = new Vector2(0, -10);
            labelRt.sizeDelta = new Vector2(200, 40);

            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "TAP ME!";
            labelText.fontSize = UIDesignSystem.FontSizeLabel;
            labelText.fontStyle = FontStyles.Bold;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            if (GameUI.Instance != null && GameUI.Instance.SharedFont != null)
            {
                labelText.font = GameUI.Instance.SharedFont;
                GameUI.ApplyTextOutline(labelText);
            }

            // Make it clickable
            Button btn = currentJackpot.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(OnJackpotTapped);

            // Calculate start and end positions (random edge to random edge)
            Vector2 startPos, endPos;
            CalculateFlightPath(out startPos, out endPos);

            rt.anchoredPosition = startPos;

            // Animate flight
            Sequence flightSeq = DOTween.Sequence();

            // Slight curve in the path for more interesting movement
            Vector2 midPoint = (startPos + endPos) / 2f;
            midPoint.y += UnityEngine.Random.Range(-200f, 200f);

            // Use a path for curved movement
            Vector3[] path = new Vector3[] {
                startPos,
                midPoint,
                endPos
            };

            flightSeq.Append(rt.DOPath(path, flyDuration, PathType.CatmullRom).SetEase(Ease.Linear));
            flightSeq.OnComplete(OnJackpotMissed);

            // Pulsing scale animation
            currentJackpot.transform.DOScale(pulseAmount, 1f / pulseSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            // Rotation animation
            currentJackpot.transform.DORotate(new Vector3(0, 0, 360f), 360f / rotationSpeed, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            // Play spawn sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClickSound(); // Use existing sound
            }

            Debug.Log("[FlyingJackpot] Spawned flying jackpot!");
        }

        private void CalculateFlightPath(out Vector2 startPos, out Vector2 endPos)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Convert to canvas coordinates
            RectTransform canvasRt = jackpotCanvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRt.rect.width > 0 ? canvasRt.rect.width : 1080;
            float canvasHeight = canvasRt.rect.height > 0 ? canvasRt.rect.height : 1920;

            // Random start edge (0=left, 1=right, 2=top, 3=bottom)
            int startEdge = UnityEngine.Random.Range(0, 4);
            int endEdge;

            // Ensure end edge is different and preferably opposite
            do
            {
                endEdge = UnityEngine.Random.Range(0, 4);
            } while (endEdge == startEdge);

            startPos = GetEdgePosition(startEdge, canvasWidth, canvasHeight);
            endPos = GetEdgePosition(endEdge, canvasWidth, canvasHeight);
        }

        private Vector2 GetEdgePosition(int edge, float width, float height)
        {
            float padding = jackpotSize;
            float randomY = UnityEngine.Random.Range(height * 0.2f, height * 0.8f);
            float randomX = UnityEngine.Random.Range(width * 0.2f, width * 0.8f);

            switch (edge)
            {
                case 0: // Left
                    return new Vector2(-padding, randomY - height / 2f);
                case 1: // Right
                    return new Vector2(width + padding, randomY - height / 2f);
                case 2: // Top
                    return new Vector2(randomX - width / 2f, height / 2f + padding);
                case 3: // Bottom
                    return new Vector2(randomX - width / 2f, -height / 2f - padding);
                default:
                    return Vector2.zero;
            }
        }

        private void OnJackpotTapped()
        {
            if (currentJackpot == null) return;

            Debug.Log("[FlyingJackpot] Jackpot caught!");

            // Stop all animations
            currentJackpot.transform.DOKill();

            // Calculate rewards based on progress
            var rewards = CalculateRewards();

            // Apply rewards
            ApplyRewards(rewards.money, rewards.darkMatter, rewards.timeShards);

            // Play EPIC celebration effects
            PlayCatchEffects(rewards.money, rewards.darkMatter, rewards.timeShards);

            // Show floating reward text
            ShowRewardText(rewards.money, rewards.darkMatter, rewards.timeShards);

            // Cleanup
            CleanupJackpot();
        }

        private (double money, double darkMatter, int timeShards) CalculateRewards()
        {
            double currentMoney = CurrencyManager.Instance?.Money ?? 0;
            double currentDM = CurrencyManager.Instance?.DarkMatter ?? 0;
            double lifetimeMoney = CurrencyManager.Instance?.LifetimeMoney ?? 0;
            int fractureLevel = TimeFractureManager.Instance?.FractureLevel ?? 0;

            // Base reward scales with progress
            double progressMultiplier = 1.0 + (fractureLevel * 0.2);  // +20% per fracture level

            // Money reward: percentage of current money, with minimum
            double moneyRewardPercent = Mathf.Lerp(baseMoneyRewardPercent, maxMoneyRewardPercent,
                Mathf.Clamp01((float)(lifetimeMoney / 1000000.0)));  // Scale up to 1M lifetime

            double moneyReward = Math.Max(minMoneyReward, currentMoney * moneyRewardPercent);
            moneyReward *= progressMultiplier;

            // Scale minimum based on lifetime money
            double scaledMinimum = Math.Max(minMoneyReward, lifetimeMoney * 0.001); // 0.1% of lifetime
            moneyReward = Math.Max(moneyReward, scaledMinimum);

            // Dark matter reward (only if unlocked)
            double dmReward = 0;
            if (Dice.DiceManager.Instance != null && Dice.DiceManager.Instance.DarkMatterUnlocked)
            {
                dmReward = Math.Max(1, currentDM * darkMatterRewardPercent * progressMultiplier);
            }

            // Time shards (rare bonus, scales with fracture level)
            int timeShardsReward = 0;
            if (UnityEngine.Random.value < timeShardsChance + (fractureLevel * 0.02f))
            {
                timeShardsReward = baseTimeShardsReward + fractureLevel;
            }

            return (moneyReward, dmReward, timeShardsReward);
        }

        private void ApplyRewards(double money, double darkMatter, int timeShards)
        {
            if (CurrencyManager.Instance != null)
            {
                if (money > 0)
                    CurrencyManager.Instance.AddMoney(money, true);

                if (darkMatter > 0)
                    CurrencyManager.Instance.AddDarkMatter(darkMatter);

                if (timeShards > 0)
                    CurrencyManager.Instance.AddTimeShards(timeShards);
            }

            OnJackpotCaught?.Invoke(money, darkMatter, timeShards);
        }

        private void PlayCatchEffects(double money, double darkMatter, int timeShards)
        {
            Vector3 jackpotWorldPos = currentJackpot.transform.position;

            // EPIC screen shake
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.ShakeCamera(screenShakeDuration, screenShakeIntensity);

                // Golden flash
                VisualEffectsManager.Instance.FlashScreen(new Color(1f, 0.85f, 0.2f, 0.6f), 0.4f);

                // Spawn jackpot particles
                VisualEffectsManager.Instance.SpawnJackpotEffect(jackpotWorldPos);
            }

            // Spawn coin rain effect
            StartCoroutine(SpawnCoinRain());

            // Play jackpot sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJackpotSound();
            }

            // Catch animation on the jackpot
            if (currentJackpot != null)
            {
                currentJackpot.transform.DOScale(2f, 0.2f).SetEase(Ease.OutBack);

                Image img = currentJackpot.GetComponent<Image>();
                if (img != null)
                {
                    img.DOFade(0f, 0.3f);
                }
            }
        }

        private IEnumerator SpawnCoinRain()
        {
            // Create coin rain from top of screen
            for (int i = 0; i < coinRainCount; i++)
            {
                SpawnFallingCoin();
                yield return new WaitForSeconds(0.03f);
            }
        }

        private void SpawnFallingCoin()
        {
            GameObject coin = new GameObject("FallingCoin");
            coin.transform.SetParent(jackpotCanvas.transform, false);

            RectTransform rt = coin.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40, 40);

            // Random X position at top of screen
            RectTransform canvasRt = jackpotCanvas.GetComponent<RectTransform>();
            float canvasWidth = canvasRt.rect.width > 0 ? canvasRt.rect.width : 1080;
            float canvasHeight = canvasRt.rect.height > 0 ? canvasRt.rect.height : 1920;

            float randomX = UnityEngine.Random.Range(-canvasWidth / 2f * 0.8f, canvasWidth / 2f * 0.8f);
            rt.anchoredPosition = new Vector2(randomX, canvasHeight / 2f + 50);

            Image img = coin.AddComponent<Image>();
            if (guiAssets != null && guiAssets.iconCoin != null)
            {
                img.sprite = guiAssets.iconCoin;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(1f, 0.85f, 0.2f);
            }
            img.raycastTarget = false;

            // Animate falling
            float fallDuration = UnityEngine.Random.Range(1f, 2f);
            float endY = -canvasHeight / 2f - 100;

            Sequence coinSeq = DOTween.Sequence();
            coinSeq.Append(rt.DOAnchorPosY(endY, fallDuration).SetEase(Ease.InQuad));
            coinSeq.Join(coin.transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(-720f, 720f)), fallDuration, RotateMode.FastBeyond360));
            coinSeq.Join(img.DOFade(0f, fallDuration * 0.3f).SetDelay(fallDuration * 0.7f));
            coinSeq.OnComplete(() => Destroy(coin));

            // Add some horizontal wobble
            rt.DOAnchorPosX(randomX + UnityEngine.Random.Range(-50f, 50f), fallDuration * 0.5f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void ShowRewardText(double money, double darkMatter, int timeShards)
        {
            if (GameUI.Instance == null) return;

            // Show money reward
            if (money > 0)
            {
                GameUI.Instance.ShowFloatingText(
                    Vector3.zero,
                    $"+${GameUI.FormatNumber(money)}",
                    UIDesignSystem.MoneyGreen
                );
            }

            // Show DM reward with slight delay
            if (darkMatter > 0)
            {
                StartCoroutine(ShowDelayedText(0.2f, $"+{GameUI.FormatNumber(darkMatter)} DM", UIDesignSystem.DarkMatterPurple));
            }

            // Show time shards with more delay
            if (timeShards > 0)
            {
                StartCoroutine(ShowDelayedText(0.4f, $"+{timeShards} Time Shards!", UIDesignSystem.TimeShardsBlue));
            }
        }

        private IEnumerator ShowDelayedText(float delay, string text, Color color)
        {
            yield return new WaitForSeconds(delay);
            if (GameUI.Instance != null)
            {
                GameUI.Instance.ShowFloatingText(Vector3.zero, text, color);
            }
        }

        private void OnJackpotMissed()
        {
            Debug.Log("[FlyingJackpot] Jackpot missed!");
            CleanupJackpot();
        }

        private void CleanupJackpot()
        {
            if (currentJackpot != null)
            {
                currentJackpot.transform.DOKill();
                Destroy(currentJackpot, 0.5f);
                currentJackpot = null;
            }
            isJackpotActive = false;
        }

        /// <summary>
        /// Force spawn a jackpot (for testing or special events).
        /// </summary>
        [ContextMenu("Force Spawn Jackpot")]
        public void ForceSpawnJackpot()
        {
            if (!isJackpotActive)
            {
                SpawnFlyingJackpot();
            }
        }

        /// <summary>
        /// Set spawn timing (can be modified by upgrades or events).
        /// </summary>
        public void SetSpawnInterval(float min, float max)
        {
            minSpawnInterval = min;
            maxSpawnInterval = max;
        }
    }
}
