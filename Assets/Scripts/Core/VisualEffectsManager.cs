using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace Incredicer.Core
{
    /// <summary>
    /// Manages visual effects like particles, screen effects, and juicy feedback.
    /// Enhanced for maximum satisfaction!
    /// </summary>
    public class VisualEffectsManager : MonoBehaviour
    {
        public static VisualEffectsManager Instance { get; private set; }

        [Header("Particle Prefabs")]
        [SerializeField] private GameObject rollParticlePrefab;
        [SerializeField] private GameObject jackpotParticlePrefab;
        [SerializeField] private GameObject purchaseParticlePrefab;
        [SerializeField] private GameObject skillUnlockParticlePrefab;
        [SerializeField] private GameObject prestigeParticlePrefab;
        [SerializeField] private GameObject moneyCollectParticlePrefab;
        [SerializeField] private GameObject darkMatterParticlePrefab;
        [SerializeField] private GameObject sparkleParticlePrefab;
        [SerializeField] private GameObject comboParticlePrefab;

        [Header("Screen Effects")]
        [SerializeField] private Image screenFlashImage;
        [SerializeField] private CanvasGroup screenFlashGroup;

        [Header("Settings")]
        [SerializeField] private bool effectsEnabled = true;
        [SerializeField] private float particleLifetime = 3f;
        [SerializeField] private float screenShakeIntensity = 0.25f; // Increased for more impact

        public bool EffectsEnabled
        {
            get => effectsEnabled;
            set => effectsEnabled = value;
        }

        private Camera mainCamera;
        private Canvas screenEffectsCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            mainCamera = Camera.main;

            // Create default particle prefabs if not assigned
            CreateDefaultParticlePrefabs();
            CreateScreenEffectsCanvas();
        }

        private void CreateScreenEffectsCanvas()
        {
            // Create a canvas for screen flash effects
            GameObject canvasObj = new GameObject("ScreenEffectsCanvas");
            canvasObj.transform.SetParent(transform);
            screenEffectsCanvas = canvasObj.AddComponent<Canvas>();
            screenEffectsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            screenEffectsCanvas.sortingOrder = 50; // Below UI popups (100+) but above game elements

            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create flash image
            GameObject flashObj = new GameObject("ScreenFlash");
            flashObj.transform.SetParent(canvasObj.transform);
            screenFlashImage = flashObj.AddComponent<Image>();
            screenFlashImage.color = Color.white;
            screenFlashImage.raycastTarget = false;

            RectTransform flashRect = flashObj.GetComponent<RectTransform>();
            flashRect.anchorMin = Vector2.zero;
            flashRect.anchorMax = Vector2.one;
            flashRect.offsetMin = Vector2.zero;
            flashRect.offsetMax = Vector2.zero;

            screenFlashGroup = flashObj.AddComponent<CanvasGroup>();
            screenFlashGroup.alpha = 0f;
            screenFlashGroup.blocksRaycasts = false;
            screenFlashGroup.interactable = false;
        }

        private void CreateDefaultParticlePrefabs()
        {
            // Roll effect - satisfying white burst with more particles
            if (rollParticlePrefab == null)
            {
                rollParticlePrefab = CreateEnhancedBurstParticle("RollParticle",
                    new Color(1f, 1f, 1f, 1f),
                    new Color(0.7f, 0.85f, 1f, 0.8f),
                    25, 0.35f, 5f, true); // More particles, bigger, faster
            }

            // Jackpot effect - EPIC gold explosion with multiple layers
            if (jackpotParticlePrefab == null)
            {
                jackpotParticlePrefab = CreateJackpotParticle();
            }

            // Purchase effect - satisfying green burst with extra juice
            if (purchaseParticlePrefab == null)
            {
                purchaseParticlePrefab = CreateEnhancedBurstParticle("PurchaseParticle",
                    new Color(0.3f, 1f, 0.5f, 1f),
                    new Color(0.6f, 1f, 0.8f, 0.7f),
                    30, 0.45f, 6f, true); // More particles, bigger
            }

            // Skill unlock - magical blue spiral with sparkles
            if (skillUnlockParticlePrefab == null)
            {
                skillUnlockParticlePrefab = CreateSpiralParticle("SkillUnlockParticle",
                    new Color(0.4f, 0.8f, 1f, 1f),
                    new Color(0.7f, 0.95f, 1f, 0.8f),
                    35, 0.5f); // More particles, bigger
            }

            // Prestige/Ascension - MASSIVE purple explosion
            if (prestigeParticlePrefab == null)
            {
                prestigeParticlePrefab = CreatePrestigeParticle();
            }

            // Money collect - quick green sparkles (juicier)
            if (moneyCollectParticlePrefab == null)
            {
                moneyCollectParticlePrefab = CreateSparkleParticle("MoneyCollectParticle",
                    new Color(0.4f, 1f, 0.6f, 1f), 18, 0.4f); // More sparkles, bigger
            }

            // Dark matter - deep purple with trails
            if (darkMatterParticlePrefab == null)
            {
                darkMatterParticlePrefab = CreateDarkMatterParticle();
            }

            // Sparkle effect for ambient use
            if (sparkleParticlePrefab == null)
            {
                sparkleParticlePrefab = CreateSparkleParticle("SparkleParticle",
                    new Color(1f, 1f, 0.8f, 1f), 5, 0.1f);
            }

            // Combo effect - rainbow burst
            if (comboParticlePrefab == null)
            {
                comboParticlePrefab = CreateRainbowBurstParticle();
            }
        }

        private GameObject CreateEnhancedBurstParticle(string name, Color startColor, Color endColor, int count, float size, float speed, bool hasTrails)
        {
            GameObject prefab = new GameObject(name);
            prefab.transform.SetParent(transform);
            prefab.SetActive(false);

            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.7f, speed * 1.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size * 1.4f);
            main.startColor = startColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = 0.3f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;
            shape.arc = 360f;
            shape.arcMode = ParticleSystemShapeMultiModeValue.Random;

            // Color fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(startColor, 0f),
                    new GradientColorKey(endColor, 0.7f),
                    new GradientColorKey(endColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            // Size curve - quick pop then shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.1f, 1.2f),
                new Keyframe(0.3f, 1f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Velocity slowdown
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.speedModifier = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f));

            // Add rotation
            var rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

            // Trails for extra juice
            if (hasTrails)
            {
                var trails = ps.trails;
                trails.enabled = true;
                trails.ratio = 0.5f;
                trails.lifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
                trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
                trails.colorOverLifetime = colorOverLifetime.color;
            }

            SetupRenderer(prefab, startColor);
            return prefab;
        }

        private GameObject CreateJackpotParticle()
        {
            GameObject prefab = new GameObject("JackpotParticle");
            prefab.transform.SetParent(transform);
            prefab.SetActive(false);

            // Main gold burst - EPIC explosion
            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 2f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 15f); // Faster burst
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f); // Bigger particles
            main.startColor = new Color(1f, 0.9f, 0.2f, 1f); // Brighter gold
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = 0.4f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 60),    // More particles
                new ParticleSystem.Burst(0.08f, 35),
                new ParticleSystem.Burst(0.16f, 20),
                new ParticleSystem.Burst(0.25f, 15)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            shape.arc = 360f;

            // Golden color gradient
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient goldGrad = new Gradient();
            goldGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 1f, 0.5f), 0f),
                    new GradientColorKey(new Color(1f, 0.8f, 0f), 0.3f),
                    new GradientColorKey(new Color(1f, 0.6f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = goldGrad;

            // Pop and shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.3f),
                new Keyframe(0.15f, 1.5f),
                new Keyframe(0.4f, 1f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Trails - longer and more visible
            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 0.85f; // More particles have trails
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f); // Longer trails
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            Gradient trailGrad = new Gradient();
            trailGrad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 1f, 0.5f), 0f), // Brighter start
                    new GradientColorKey(new Color(1f, 0.8f, 0.1f), 0.5f),
                    new GradientColorKey(new Color(1f, 0.6f, 0f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.7f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            trails.colorOverLifetime = trailGrad;

            // Add sparkle sub-emitter child - more sparkles!
            GameObject sparkleChild = new GameObject("Sparkles");
            sparkleChild.transform.SetParent(prefab.transform);
            sparkleChild.transform.localPosition = Vector3.zero;

            ParticleSystem sparklePS = sparkleChild.AddComponent<ParticleSystem>();
            var sparkleMain = sparklePS.main;
            sparkleMain.duration = 2f;
            sparkleMain.loop = false;
            sparkleMain.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            sparkleMain.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            sparkleMain.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f); // Bigger sparkles
            sparkleMain.startColor = new Color(1f, 1f, 0.9f, 1f);
            sparkleMain.simulationSpace = ParticleSystemSimulationSpace.World;
            sparkleMain.playOnAwake = true;

            var sparkleEmission = sparklePS.emission;
            sparkleEmission.rateOverTime = 80; // More continuous sparkles
            sparkleEmission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 50),  // More initial burst
                new ParticleSystem.Burst(0.15f, 25)
            });

            var sparkleShape = sparklePS.shape;
            sparkleShape.shapeType = ParticleSystemShapeType.Circle;
            sparkleShape.radius = 0.3f;

            var sparkleColor = sparklePS.colorOverLifetime;
            sparkleColor.enabled = true;
            Gradient sparkleGrad = new Gradient();
            sparkleGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.9f, 0.5f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            sparkleColor.color = sparkleGrad;

            SetupRenderer(sparkleChild, Color.white);
            SetupRenderer(prefab, new Color(1f, 0.85f, 0f));

            return prefab;
        }

        private GameObject CreatePrestigeParticle()
        {
            GameObject prefab = new GameObject("PrestigeParticle");
            prefab.transform.SetParent(transform);
            prefab.SetActive(false);

            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 2.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(10f, 20f); // Faster burst!
            main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1f);   // Bigger particles
            main.startColor = new Color(0.8f, 0.4f, 1f, 1f);            // Brighter purple
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = -0.15f; // Float upward

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 80),     // MASSIVE initial burst
                new ParticleSystem.Burst(0.1f, 50),
                new ParticleSystem.Burst(0.2f, 40),
                new ParticleSystem.Burst(0.35f, 30),
                new ParticleSystem.Burst(0.5f, 20)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            // Purple to pink gradient
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.5f, 1f), 0f),
                    new GradientColorKey(new Color(0.7f, 0.3f, 1f), 0.4f),
                    new GradientColorKey(new Color(0.4f, 0.2f, 0.8f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            // Epic size curve
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.1f, 1.8f),
                new Keyframe(0.3f, 1.2f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Trails - longer and more intense
            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 0.9f;  // Almost all particles have trails
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f); // Longer trails
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            trails.colorOverLifetime = grad;

            // Add ring sub-effect - BIGGER!
            GameObject ringChild = new GameObject("Ring");
            ringChild.transform.SetParent(prefab.transform);
            ringChild.transform.localPosition = Vector3.zero;

            ParticleSystem ringPS = ringChild.AddComponent<ParticleSystem>();
            var ringMain = ringPS.main;
            ringMain.duration = 1.5f;
            ringMain.loop = false;
            ringMain.startLifetime = 1f;
            ringMain.startSpeed = 18f;  // Faster expansion
            ringMain.startSize = 0.3f;  // Bigger ring particles
            ringMain.startColor = new Color(1f, 0.7f, 1f, 0.9f); // Brighter
            ringMain.simulationSpace = ParticleSystemSimulationSpace.World;
            ringMain.playOnAwake = true;

            var ringEmission = ringPS.emission;
            ringEmission.rateOverTime = 0;
            ringEmission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 100),  // More particles
                new ParticleSystem.Burst(0.1f, 60)
            });

            var ringShape = ringPS.shape;
            ringShape.shapeType = ParticleSystemShapeType.Circle;
            ringShape.radius = 0.01f;
            ringShape.arc = 360f;

            var ringColor = ringPS.colorOverLifetime;
            ringColor.enabled = true;
            Gradient ringGrad = new Gradient();
            ringGrad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.7f, 1f), 0f), new GradientColorKey(new Color(0.5f, 0.2f, 0.8f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            ringColor.color = ringGrad;

            SetupRenderer(ringChild, new Color(0.9f, 0.6f, 1f));
            SetupRenderer(prefab, new Color(0.7f, 0.3f, 1f));

            return prefab;
        }

        private GameObject CreateDarkMatterParticle()
        {
            GameObject prefab = new GameObject("DarkMatterParticle");
            prefab.transform.SetParent(transform);
            prefab.SetActive(false);

            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);  // 2x size
            main.startColor = new Color(0.5f, 0.2f, 0.9f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = -0.3f; // Float up slightly

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 20),  // More particles
                new ParticleSystem.Burst(0.1f, 12)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;  // 2x radius
            shape.arc = 360f;

            // Deep purple gradient
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(0.7f, 0.4f, 1f), 0f),
                    new GradientColorKey(new Color(0.4f, 0.1f, 0.8f), 0.5f),
                    new GradientColorKey(new Color(0.2f, 0f, 0.5f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.7f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            // Trails with mystery vibe
            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 0.9f;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
            trails.colorOverLifetime = grad;

            SetupRenderer(prefab, new Color(0.5f, 0.2f, 0.9f));
            return prefab;
        }

        private GameObject CreateSpiralParticle(string name, Color startColor, Color endColor, int count, float size)
        {
            GameObject prefab = new GameObject(name);
            prefab.transform.SetParent(transform);
            prefab.SetActive(false);

            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.7f, size * 1.3f);
            main.startColor = startColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = -0.1f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, count),
                new ParticleSystem.Burst(0.15f, count / 2)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;
            shape.arc = 360f;

            // Velocity for spiral effect
            var velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(2f, 4f);
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(1f, 2f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            // Trails
            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 0.6f;
            trails.lifetime = 0.2f;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

            SetupRenderer(prefab, startColor);
            return prefab;
        }

        private GameObject CreateSparkleParticle(string name, Color color, int count, float size)
        {
            GameObject prefab = new GameObject(name);
            prefab.transform.SetParent(transform);
            prefab.SetActive(false);

            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size * 1.5f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            // Quick flash fade
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(color, 0.3f), new GradientColorKey(color, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            // Twinkle size
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve twinkleCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.1f, 1.5f),
                new Keyframe(0.3f, 0.8f),
                new Keyframe(0.5f, 1.2f),
                new Keyframe(1f, 0f)
            );
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, twinkleCurve);

            SetupRenderer(prefab, color);
            return prefab;
        }

        private GameObject CreateRainbowBurstParticle()
        {
            GameObject prefab = new GameObject("ComboParticle");
            prefab.transform.SetParent(transform);
            prefab.SetActive(false);

            ParticleSystem ps = prefab.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.cyan);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = true;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = 0.2f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;

            // Rainbow gradient
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient rainbow = new Gradient();
            rainbow.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.yellow, 0.25f),
                    new GradientColorKey(Color.green, 0.5f),
                    new GradientColorKey(Color.cyan, 0.75f),
                    new GradientColorKey(new Color(0.5f, 0f, 1f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = rainbow;

            // Trails
            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 0.7f;
            trails.lifetime = 0.25f;
            trails.colorOverLifetime = rainbow;

            SetupRenderer(prefab, Color.white);
            return prefab;
        }

        private void SetupRenderer(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
                renderer = obj.AddComponent<ParticleSystemRenderer>();

            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.SetColor("_Color", color);
            renderer.trailMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.trailMaterial.SetColor("_Color", color);
            renderer.sortingOrder = 10; // Below UI elements but above dice
        }

        // ========== PUBLIC SPAWN METHODS ==========

        public void SpawnRollEffect(Vector3 position)
        {
            SpawnParticle(rollParticlePrefab, position);
        }

        public void SpawnJackpotEffect(Vector3 position)
        {
            if (!effectsEnabled) return;

            SpawnParticle(jackpotParticlePrefab, position);

            // Epic screen flash sequence for jackpot - double flash!
            FlashScreen(new Color(1f, 1f, 0.6f, 0.5f), 0.15f); // Bright initial flash
            DOVirtual.DelayedCall(0.1f, () => FlashScreen(new Color(1f, 0.85f, 0.2f, 0.35f), 0.3f)); // Gold afterglow

            // Strong camera shake with extra punch
            ShakeCamera(0.35f, 0.35f);
        }

        public void SpawnPurchaseEffect(Vector3 position)
        {
            SpawnParticle(purchaseParticlePrefab, position);
            FlashScreen(new Color(0.3f, 1f, 0.5f, 0.25f), 0.2f); // Brighter flash
            ShakeCamera(0.1f, 0.08f); // Subtle satisfying shake
        }

        public void SpawnSkillUnlockEffect(Vector3 position)
        {
            SpawnParticle(skillUnlockParticlePrefab, position);
            FlashScreen(new Color(0.4f, 0.8f, 1f, 0.3f), 0.25f); // Brighter blue
            ShakeCamera(0.15f, 0.12f); // Satisfying shake
        }

        public void SpawnPrestigeEffect(Vector3 position)
        {
            if (!effectsEnabled) return;

            SpawnParticle(prestigeParticlePrefab, position);

            // EPIC screen flash sequence - multiple waves!
            FlashScreen(new Color(1f, 0.8f, 1f, 0.6f), 0.2f); // Bright white-purple initial
            DOVirtual.DelayedCall(0.15f, () => FlashScreen(new Color(0.8f, 0.4f, 1f, 0.4f), 0.4f)); // Purple wave
            DOVirtual.DelayedCall(0.4f, () => FlashScreen(new Color(0.5f, 0.2f, 0.8f, 0.25f), 0.5f)); // Deep purple fade

            // Strong sustained camera shake
            ShakeCamera(0.5f, 0.45f);
        }

        public void SpawnMoneyCollectEffect(Vector3 position)
        {
            SpawnParticle(moneyCollectParticlePrefab, position);
        }

        public void SpawnDarkMatterEffect(Vector3 position)
        {
            SpawnParticle(darkMatterParticlePrefab, position);
        }

        public void SpawnSparkleEffect(Vector3 position)
        {
            SpawnParticle(sparkleParticlePrefab, position);
        }

        public void SpawnSparkleEffect(Vector3 position, Color color)
        {
            if (!effectsEnabled) return;
            if (sparkleParticlePrefab == null) return;

            // Don't spawn particle effects when a popup is open
            if (PopupManager.Instance != null && PopupManager.Instance.IsAnyPopupOpen) return;

            GameObject effect = Instantiate(sparkleParticlePrefab, position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = color;
            }
            Destroy(effect, 2f);
        }

        public void SpawnComboEffect(Vector3 position)
        {
            SpawnParticle(comboParticlePrefab, position);
            FlashScreen(new Color(1f, 1f, 1f, 0.3f), 0.2f); // Brighter
            ShakeCamera(0.12f, 0.1f); // Subtle punch
        }

        /// <summary>
        /// Spawns a big number pop effect for significant currency gains
        /// </summary>
        public void SpawnBigWinEffect(Vector3 position)
        {
            if (!effectsEnabled) return;

            SpawnParticle(jackpotParticlePrefab, position);
            SpawnParticle(comboParticlePrefab, position);

            FlashScreen(new Color(1f, 0.95f, 0.4f, 0.4f), 0.25f);
            ShakeCamera(0.2f, 0.2f);
        }

        /// <summary>
        /// Spawns a subtle sparkle effect at position
        /// </summary>
        public void SpawnMicroSparkle(Vector3 position)
        {
            if (!effectsEnabled) return;
            SpawnParticle(sparkleParticlePrefab, position);
        }

        // ========== SCREEN EFFECTS ==========

        public void FlashScreen(Color color, float duration)
        {
            if (!effectsEnabled || screenFlashGroup == null) return;

            screenFlashImage.color = color;
            screenFlashGroup.DOKill();
            screenFlashGroup.alpha = color.a;
            screenFlashGroup.DOFade(0f, duration).SetEase(Ease.OutQuad);
        }

        public void ShakeCamera(float duration, float intensity)
        {
            if (!effectsEnabled) return;

            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                mainCamera.transform.DOKill();
                mainCamera.transform.DOShakePosition(duration, intensity * screenShakeIntensity, 20, 90f, false, true);
            }
        }

        public void PunchScale(Transform target, float punch = 0.2f, float duration = 0.3f)
        {
            if (!effectsEnabled || target == null) return;

            target.DOKill();
            target.DOPunchScale(Vector3.one * punch, duration, 8, 0.5f);
        }

        public void BounceScale(Transform target, float targetScale = 1.2f, float duration = 0.15f)
        {
            if (!effectsEnabled || target == null) return;

            target.DOKill();
            target.DOScale(targetScale, duration).SetEase(Ease.OutBack)
                .OnComplete(() => target.DOScale(1f, duration).SetEase(Ease.InBack));
        }

        // ========== UTILITY ==========

        private void SpawnParticle(GameObject prefab, Vector3 position)
        {
            if (!effectsEnabled || prefab == null) return;

            // Don't spawn particle effects when a popup is open
            if (PopupManager.Instance != null && PopupManager.Instance.IsAnyPopupOpen) return;

            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            instance.SetActive(true);
            Destroy(instance, particleLifetime);
        }

        public void SpawnCustomEffect(GameObject prefab, Vector3 position, float lifetime = 2f)
        {
            if (!effectsEnabled || prefab == null) return;

            // Don't spawn particle effects when a popup is open
            if (PopupManager.Instance != null && PopupManager.Instance.IsAnyPopupOpen) return;

            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            Destroy(instance, lifetime);
        }
    }
}
