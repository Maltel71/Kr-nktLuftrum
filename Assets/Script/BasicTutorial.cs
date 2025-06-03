using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BasicTutorial : MonoBehaviour
{
    [Header("UI Referenser")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private bool createOwnTutorialUI = true; // Skapa egen UI automatiskt

    [Header("Tutorial Inställningar")]
    [SerializeField] private float delayBetweenSteps = 1.0f;
    [SerializeField] private Color instructionColor = Color.white;
    [SerializeField] private Color completionColor = Color.green;
    [SerializeField] private AudioClip tutorialSound;
    [SerializeField] private float completionMessageDuration = 2.0f;

    [Header("Tutorialsteg")]
    [SerializeField] private List<TutorialStep> tutorialSteps;

    private int currentStepIndex = -1;
    private bool tutorialActive = false;
    private bool currentStepCompleted = false;
    private bool showingCompletionMessage = false;
    private AudioSource audioSource;
    private GameMessageSystem messageSystem;
    private Canvas tutorialCanvas;

    [System.Serializable]
    public class TutorialStep
    {
        public enum StepType
        {
            MoveLeft,
            MoveRight,
            MoveForward,
            MoveBackward,
            Shoot,
            Bomb,
            Missile,
            Flare,
            Complete
        }

        public StepType type;
        [TextArea(2, 5)]
        public string description;
        [TextArea(1, 3)]
        public string instruction;
        public string keyBinding;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        messageSystem = FindObjectOfType<GameMessageSystem>();

        // Skapa egen tutorial UI om den inte finns
        if (createOwnTutorialUI && tutorialText == null)
        {
            CreateTutorialUI();
        }

        SetupTutorialPanel();
    }

    private void CreateTutorialUI()
    {
        Debug.Log("Skapar egen tutorial UI...");

        // Skapa Canvas
        GameObject canvasObj = new GameObject("TutorialCanvas");
        tutorialCanvas = canvasObj.AddComponent<Canvas>();
        tutorialCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tutorialCanvas.sortingOrder = 100; // Högst upp

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Skapa Panel
        GameObject panelObj = new GameObject("TutorialPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);

        tutorialPanel = panelObj;
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // Halvtransparent svart bakgrund

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.8f);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = new Vector2(20, -100);
        panelRect.offsetMax = new Vector2(-20, -20);

        // Skapa Text
        GameObject textObj = new GameObject("TutorialText");
        textObj.transform.SetParent(panelObj.transform, false);

        tutorialText = textObj.AddComponent<TextMeshProUGUI>();
        tutorialText.text = "Tutorial Starting...";
        tutorialText.fontSize = 24;
        tutorialText.color = instructionColor;
        tutorialText.alignment = TextAlignmentOptions.Center;
        tutorialText.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 10);
        textRect.offsetMax = new Vector2(-20, -10);

        Debug.Log("Tutorial UI skapad!");
    }

    private void SetupTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    private void Start()
    {
        Invoke("StartTutorial", 1.0f);
    }

    private void Update()
    {
        if (!tutorialActive || currentStepCompleted || showingCompletionMessage) return;

        if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Count)
        {
            TutorialStep currentStep = tutorialSteps[currentStepIndex];
            CheckStepCompletion(currentStep);
        }
    }

    private void CheckStepCompletion(TutorialStep step)
    {
        switch (step.type)
        {
            case TutorialStep.StepType.MoveLeft:
                if (Input.GetKey(KeyCode.A))
                {
                    MarkStepCompleted();
                }
                break;

            case TutorialStep.StepType.MoveRight:
                if (Input.GetKey(KeyCode.D))
                {
                    MarkStepCompleted();
                }
                break;

            case TutorialStep.StepType.MoveForward:
                if (Input.GetKey(KeyCode.W))
                {
                    MarkStepCompleted();
                }
                break;

            case TutorialStep.StepType.MoveBackward:
                if (Input.GetKey(KeyCode.S))
                {
                    MarkStepCompleted();
                }
                break;

            case TutorialStep.StepType.Shoot:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    MarkStepCompleted();
                }
                break;

            case TutorialStep.StepType.Bomb:
                if (Input.GetKeyDown(KeyCode.B))
                {
                    MarkStepCompleted();
                }
                break;

            case TutorialStep.StepType.Missile:
                if (Input.GetKeyDown(KeyCode.M))
                {
                    MarkStepCompleted();
                }
                break;

            case TutorialStep.StepType.Flare:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    MarkStepCompleted();
                }
                break;
        }
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Count == 0)
        {
            Debug.LogWarning("Inga tutorialsteg definierade!");
            return;
        }

        tutorialActive = true;
        NextStep();
    }

    public void NextStep()
    {
        currentStepIndex++;
        currentStepCompleted = false;
        showingCompletionMessage = false;

        if (currentStepIndex >= tutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentStepIndex];

        // Visa information för nuvarande steg - PERMANENT tills steget är klart
        DisplayStep(step);

        // Spela ljud om tillgängligt
        if (tutorialSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(tutorialSound);
        }
    }

    private void MarkStepCompleted()
    {
        if (!currentStepCompleted)
        {
            currentStepCompleted = true;
            showingCompletionMessage = true;

            // Visa slutförandemeddelande
            ShowCompletionMessage();

            // Vänta en kort stund innan nästa steg
            StartCoroutine(AdvanceToNextStepAfterDelay());
        }
    }

    private void ShowCompletionMessage()
    {
        string completionMessage = GetCompletionMessage(tutorialSteps[currentStepIndex].type);

        if (tutorialText != null)
        {
            tutorialText.text = completionMessage;
            tutorialText.color = completionColor; // Grön för slutförd
        }

        // Visa även i GameMessageSystem som bonus
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage(completionMessage);
        }
    }

    private string GetCompletionMessage(TutorialStep.StepType stepType)
    {
        return stepType switch
        {
            TutorialStep.StepType.MoveLeft => "Good! You moved left.",
            TutorialStep.StepType.MoveRight => "Excellent! You moved right.",
            TutorialStep.StepType.MoveForward => "Perfect! You flew forward.",
            TutorialStep.StepType.MoveBackward => "Good! You flew backward.",
            TutorialStep.StepType.Shoot => "Perfect! Weapon fired.",
            TutorialStep.StepType.Bomb => "Well done! Bomb dropped.",
            TutorialStep.StepType.Missile => "Missile fired successfully!",
            TutorialStep.StepType.Flare => "Flare launched!",
            _ => "Step completed!"
        };
    }

    private IEnumerator AdvanceToNextStepAfterDelay()
    {
        yield return new WaitForSeconds(completionMessageDuration);
        showingCompletionMessage = false;
        NextStep();
    }

    private void DisplayStep(TutorialStep step)
    {
        string message = GetInstructionMessage(step);

        // Visa i vår permanenta tutorial text
        if (tutorialText != null)
        {
            tutorialText.text = message;
            tutorialText.color = instructionColor; // Vit för instruktioner
        }

        // Aktivera tutorial panel
        ShowTutorialPanel(true);

        Debug.Log($"Tutorial Step {currentStepIndex + 1}: {message}");
    }

    private string GetInstructionMessage(TutorialStep step)
    {
        string baseMessage = step.type switch
        {
            TutorialStep.StepType.MoveLeft => "MOVE LEFT",
            TutorialStep.StepType.MoveRight => "MOVE RIGHT",
            TutorialStep.StepType.MoveForward => "MOVE FORWARD",
            TutorialStep.StepType.MoveBackward => "MOVE BACKWARD",
            TutorialStep.StepType.Shoot => "FIRE WEAPONS",
            TutorialStep.StepType.Bomb => "DROP BOMB",
            TutorialStep.StepType.Missile => "FIRE MISSILE",
            TutorialStep.StepType.Flare => "LAUNCH FLARE",
            _ => "COMPLETE ACTION"
        };

        string keyInfo = step.type switch
        {
            TutorialStep.StepType.MoveLeft => "Press A",
            TutorialStep.StepType.MoveRight => "Press D",
            TutorialStep.StepType.MoveForward => "Press W",
            TutorialStep.StepType.MoveBackward => "Press S",
            TutorialStep.StepType.Shoot => "Press SPACE",
            TutorialStep.StepType.Bomb => "Press B",
            TutorialStep.StepType.Missile => "Press M",
            TutorialStep.StepType.Flare => "Press F",
            _ => ""
        };

        return $"{baseMessage}\n{keyInfo}";
    }

    private void ShowTutorialPanel(bool show)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(show);
        }
    }

    private void HideTutorialPanel()
    {
        ShowTutorialPanel(false);
    }

    private void CompleteTutorial()
    {
        tutorialActive = false;

        // Visa slutmeddelande
        string finalMessage = "TUTORIAL COMPLETED!\nLet's go to war!";

        if (tutorialText != null)
        {
            tutorialText.text = finalMessage;
            tutorialText.color = Color.yellow;
        }

        // Dölj tutorial efter en stund
        StartCoroutine(HideTutorialAfterDelay(3.0f));

        // Ladda Level 1
        StartCoroutine(StartLevel1AfterDelay(3.0f));
    }

    private IEnumerator HideTutorialAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideTutorialPanel();
    }

    private IEnumerator StartLevel1AfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Debug.Log("BasicTutorial: Tutorial complete - loading Level1");

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.currentLevel = 1;
        }

        LoadLevel1();
    }

    private void LoadLevel1()
    {
        string[] possibleNames = {
            "Level1",
            "Level 1",
            "Scenes/Level1",
            "Level_1"
        };

        foreach (string sceneName in possibleNames)
        {
            try
            {
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    Debug.Log($"BasicTutorial: Loading scene: {sceneName}");
                    SceneManager.LoadScene(sceneName);
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"BasicTutorial: Could not load {sceneName}: {e.Message}");
            }
        }

        Debug.LogWarning("BasicTutorial: Could not find Level1 by name - loading scene index 2");
        if (SceneManager.sceneCountInBuildSettings > 2)
        {
            SceneManager.LoadScene(2);
        }
        else
        {
            Debug.LogError("BasicTutorial: Not enough scenes in build for Level1!");
        }
    }

    public void SkipTutorial()
    {
        if (!tutorialActive)
            return;

        tutorialActive = false;
        StopAllCoroutines();
        HideTutorialPanel();

        Debug.Log("BasicTutorial: Tutorial skipped - loading Level1");

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.currentLevel = 1;
        }

        LoadLevel1();
    }

    // Debug metoder
    [ContextMenu("Force Next Step")]
    public void ForceNextStep()
    {
        if (tutorialActive && !showingCompletionMessage)
        {
            MarkStepCompleted();
        }
    }

    [ContextMenu("Skip to End")]
    public void SkipToEnd()
    {
        CompleteTutorial();
    }

    private void OnDestroy()
    {
        // Städa upp om vi skapade egen UI
        if (tutorialCanvas != null)
        {
            Destroy(tutorialCanvas.gameObject);
        }
    }
}