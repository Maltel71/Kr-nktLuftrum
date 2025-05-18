using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class BasicTutorial : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Button skipButton;

    [Header("Tutorial Settings")]
    [SerializeField] private int spaceShotsNeeded = 2;  // Antal SPACE-skott att träna
    [SerializeField] private int gShotsNeeded = 2;      // Antal G (missil) skott att träna
    [SerializeField] private int bBombsNeeded = 2;      // Antal B (bomb) att träna
    [SerializeField] private int fFlaresNeeded = 2;     // Antal F (flares) att träna

    private enum TutorialStep
    {
        Intro,
        ShootingWithSpace,
        ShootingWithG,
        BombingWithB,
        FlaresWithF,
        Complete
    }

    private TutorialStep currentStep = TutorialStep.Intro;
    private int spacePressed = 0;
    private int gPressed = 0;
    private int bPressed = 0;
    private int fPressed = 0;
    private bool tutorialActive = true;
    private GameMessageSystem messageSystem;

    private void Start()
    {
        // Hitta MessageSystem om det behövs
        messageSystem = FindObjectOfType<GameMessageSystem>();

        // Sätt upp skip-knapp om den finns
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }

        // Starta tutorial
        StartTutorial();
    }

    private void StartTutorial()
    {
        tutorialActive = true;
        ShowTutorialPanel(true);
        ChangeStep(TutorialStep.Intro);

        // Automatiskt gå vidare från intro efter kort tid
        StartCoroutine(AutoAdvanceFromIntro(3.0f));
    }

    private IEnumerator AutoAdvanceFromIntro(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentStep == TutorialStep.Intro)
        {
            ChangeStep(TutorialStep.ShootingWithSpace);
        }
    }

    private void Update()
    {
        if (!tutorialActive) return;

        // Spåra knapptryckningar baserat på aktuellt steg
        switch (currentStep)
        {
            case TutorialStep.ShootingWithSpace:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    spacePressed++;
                    ShowProgress($"Skjutit med SPACE: {spacePressed}/{spaceShotsNeeded}");
                    messageSystem?.ShowBoostMessage($"Bra! {spacePressed}/{spaceShotsNeeded} skott");

                    if (spacePressed >= spaceShotsNeeded)
                    {
                        messageSystem?.ShowBoostMessage("Bra jobbat med SPACE-knappen!");
                        StartCoroutine(DelayedStepChange(TutorialStep.ShootingWithG, 1.5f));
                    }
                }
                break;

            case TutorialStep.ShootingWithG:
                if (Input.GetKeyDown(KeyCode.G))
                {
                    gPressed++;
                    ShowProgress($"Skjutit med G: {gPressed}/{gShotsNeeded}");
                    messageSystem?.ShowBoostMessage($"Bra! {gPressed}/{gShotsNeeded} missiler");

                    if (gPressed >= gShotsNeeded)
                    {
                        messageSystem?.ShowBoostMessage("Bra jobbat med missilerna!");
                        StartCoroutine(DelayedStepChange(TutorialStep.BombingWithB, 1.5f));
                    }
                }
                break;

            case TutorialStep.BombingWithB:
                if (Input.GetKeyDown(KeyCode.B))
                {
                    bPressed++;
                    ShowProgress($"Släppt bomber med B: {bPressed}/{bBombsNeeded}");
                    messageSystem?.ShowBoostMessage($"Bra! {bPressed}/{bBombsNeeded} bomber");

                    if (bPressed >= bBombsNeeded)
                    {
                        messageSystem?.ShowBoostMessage("Bra jobbat med bomberna!");
                        StartCoroutine(DelayedStepChange(TutorialStep.FlaresWithF, 1.5f));
                    }
                }
                break;

            case TutorialStep.FlaresWithF:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    fPressed++;
                    ShowProgress($"Använt flares med F: {fPressed}/{fFlaresNeeded}");
                    messageSystem?.ShowBoostMessage($"Bra! {fPressed}/{fFlaresNeeded} flares");

                    if (fPressed >= fFlaresNeeded)
                    {
                        messageSystem?.ShowBoostMessage("Bra jobbat med flares!");
                        StartCoroutine(DelayedStepChange(TutorialStep.Complete, 1.5f));
                    }
                }
                break;

            case TutorialStep.Complete:
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    StartLevel1();
                }
                break;
        }
    }

    private IEnumerator DelayedStepChange(TutorialStep nextStep, float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeStep(nextStep);
    }

    private void ChangeStep(TutorialStep step)
    {
        currentStep = step;

        switch (step)
        {
            case TutorialStep.Intro:
                instructionText.text = "Välkommen till grundläggande flygträning!\nDu kommer lära dig spelets kontroller.";
                progressText.text = "Träningen börjar om 3 sekunder...";
                break;

            case TutorialStep.ShootingWithSpace:
                instructionText.text = "Skjut med vanliga vapen genom att trycka på SPACE";
                progressText.text = $"Skjutit med SPACE: {spacePressed}/{spaceShotsNeeded}";
                messageSystem?.ShowBoostMessage("Skjut med SPACE-knappen!");
                break;

            case TutorialStep.ShootingWithG:
                instructionText.text = "Avfyra missiler genom att trycka på G";
                progressText.text = $"Skjutit med G: {gPressed}/{gShotsNeeded}";
                messageSystem?.ShowBoostMessage("Skjut med G-knappen (missiler)!");
                break;

            case TutorialStep.BombingWithB:
                instructionText.text = "Släpp bomber genom att trycka på B";
                progressText.text = $"Släppt bomber med B: {bPressed}/{bBombsNeeded}";
                messageSystem?.ShowBoostMessage("Släpp bomber med B-knappen!");
                break;

            case TutorialStep.FlaresWithF:
                instructionText.text = "Använd flares för att undvika missiler genom att trycka på F";
                progressText.text = $"Använt flares med F: {fPressed}/{fFlaresNeeded}";
                messageSystem?.ShowBoostMessage("Använd flares med F-knappen!");
                break;

            case TutorialStep.Complete:
                instructionText.text = "Träningen är klar! Du behärskar nu spelets grundläggande kontroller.";
                progressText.text = "Tryck ENTER för att börja spela!";
                messageSystem?.ShowBoostMessage("Tutorial klar! Tryck ENTER för att starta Level 1");
                break;
        }
    }

    private void ShowProgress(string message)
    {
        progressText.text = message;
    }

    private void ShowTutorialPanel(bool show)
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(show);
        }
    }

    private void StartLevel1()
    {
        tutorialActive = false;
        ShowTutorialPanel(false);

        // Visa meddelande
        messageSystem?.ShowBoostMessage("Level 1 börjar nu! Lycka till!");

        // Starta level 1
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartNextLevel();
        }
        else
        {
            Debug.Log("Level 1 startad! (LevelManager saknas)");
            // Här kan du lägga till annan kod för att starta Level 1 om du inte använder LevelManager
        }
    }

    // För att andra skript ska kunna hoppa över tutorialen
    public void SkipTutorial()
    {
        tutorialActive = false;
        ShowTutorialPanel(false);
        StartLevel1();
    }
}