using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialSystem : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject progressIndicator;
    [SerializeField] private Button skipButton;

    [Header("Tutorial Inställningar")]
    [SerializeField] private float delayBetweenSteps = 1.0f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private AudioClip tutorialSound;

    [Header("Tutorialsteg")]
    [SerializeField] private List<TutorialStep> tutorialSteps;

    private int currentStepIndex = -1;
    private bool tutorialActive = false;
    private bool currentStepCompleted = false;
    private AudioSource audioSource;
    private GameMessageSystem messageSystem;

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
    }

    private void Start()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipTutorial);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Invoke("StartTutorial", 1.0f);
    }

    private void Update()
    {
        if (!tutorialActive || currentStepCompleted) return;

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
        ShowTutorialPanel(true);
        NextStep();
    }

    public void NextStep()
    {
        currentStepIndex++;
        currentStepCompleted = false;

        if (currentStepIndex >= tutorialSteps.Count)
        {
            CompleteTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentStepIndex];

        // Visa information för nuvarande steg
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

            // Visa slutförandemeddelande
            if (messageSystem != null)
            {
                string completionMessage = GetCompletionMessage(tutorialSteps[currentStepIndex].type);
                messageSystem.ShowBoostMessage(completionMessage);
            }

            // Vänta en kort stund innan nästa steg
            StartCoroutine(AdvanceToNextStepAfterDelay());
        }
    }

    private string GetCompletionMessage(TutorialStep.StepType stepType)
    {
        return stepType switch
        {
            TutorialStep.StepType.MoveLeft => "Bra! Du har rört planet åt vänster.",
            TutorialStep.StepType.MoveRight => "Utmärkt! Du har rört planet åt höger.",
            TutorialStep.StepType.MoveForward => "Perfekt! Du har flugit framåt.",
            TutorialStep.StepType.MoveBackward => "Bra! Du har flugit bakåt.",
            TutorialStep.StepType.Shoot => "Perfekt! Du har skjutit.",
            TutorialStep.StepType.Bomb => "Bra jobbat! Du har släppt en bomb.",
            TutorialStep.StepType.Missile => "Missil avfyrad!",
            TutorialStep.StepType.Flare => "Fackla avfyrad!",
            _ => "Steg klarat!"
        };
    }

    private IEnumerator AdvanceToNextStepAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);
        NextStep();
    }

    private void DisplayStep(TutorialStep step)
    {
        if (tutorialText != null)
            tutorialText.text = step.description;

        if (instructionText != null)
            instructionText.text = step.instruction;

        // Visa meddelande om tillgängligt
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("Nytt tutorial-steg: " + step.type.ToString());
        }
    }

    private void ShowTutorialPanel(bool show)
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(show);
    }

    private void CompleteTutorial()
    {
        tutorialActive = false;
        ShowTutorialPanel(false);

        // Visa slutmeddelande
        if (messageSystem != null)
        {
            messageSystem.ShowBoostMessage("Tutorial klar! Level 1 börjar nu!");
        }

        // Ladda Level 1 eller fortsätt spelet
        StartCoroutine(StartLevel1AfterDelay(3.0f));
    }

    private IEnumerator StartLevel1AfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Implementera Level 1-start här
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartNextLevel();
        }
        else
        {
            Debug.LogWarning("LevelManager saknas!");
        }
    }

    public void SkipTutorial()
    {
        if (!tutorialActive)
            return;

        tutorialActive = false;
        StopAllCoroutines();
        ShowTutorialPanel(false);

        // Starta Level 1 direkt
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartNextLevel();
        }
        else
        {
            Debug.LogWarning("LevelManager saknas!");
        }
    }
}