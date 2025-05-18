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
    [SerializeField] private int spaceShotsNeeded = 2;  // Antal SPACE-skott att tr�na
    [SerializeField] private int gShotsNeeded = 2;      // Antal G (missil) skott att tr�na
    [SerializeField] private int bBombsNeeded = 2;      // Antal B (bomb) att tr�na
    [SerializeField] private int fFlaresNeeded = 2;     // Antal F (flares) att tr�na

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
        // Hitta MessageSystem om det beh�vs
        messageSystem = FindObjectOfType<GameMessageSystem>();

        // S�tt upp skip-knapp om den finns
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

        // Automatiskt g� vidare fr�n intro efter kort tid
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

        // Sp�ra knapptryckningar baserat p� aktuellt steg
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
                    ShowProgress($"Sl�ppt bomber med B: {bPressed}/{bBombsNeeded}");
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
                    ShowProgress($"Anv�nt flares med F: {fPressed}/{fFlaresNeeded}");
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
                instructionText.text = "V�lkommen till grundl�ggande flygtr�ning!\nDu kommer l�ra dig spelets kontroller.";
                progressText.text = "Tr�ningen b�rjar om 3 sekunder...";
                break;

            case TutorialStep.ShootingWithSpace:
                instructionText.text = "Skjut med vanliga vapen genom att trycka p� SPACE";
                progressText.text = $"Skjutit med SPACE: {spacePressed}/{spaceShotsNeeded}";
                messageSystem?.ShowBoostMessage("Skjut med SPACE-knappen!");
                break;

            case TutorialStep.ShootingWithG:
                instructionText.text = "Avfyra missiler genom att trycka p� G";
                progressText.text = $"Skjutit med G: {gPressed}/{gShotsNeeded}";
                messageSystem?.ShowBoostMessage("Skjut med G-knappen (missiler)!");
                break;

            case TutorialStep.BombingWithB:
                instructionText.text = "Sl�pp bomber genom att trycka p� B";
                progressText.text = $"Sl�ppt bomber med B: {bPressed}/{bBombsNeeded}";
                messageSystem?.ShowBoostMessage("Sl�pp bomber med B-knappen!");
                break;

            case TutorialStep.FlaresWithF:
                instructionText.text = "Anv�nd flares f�r att undvika missiler genom att trycka p� F";
                progressText.text = $"Anv�nt flares med F: {fPressed}/{fFlaresNeeded}";
                messageSystem?.ShowBoostMessage("Anv�nd flares med F-knappen!");
                break;

            case TutorialStep.Complete:
                instructionText.text = "Tr�ningen �r klar! Du beh�rskar nu spelets grundl�ggande kontroller.";
                progressText.text = "Tryck ENTER f�r att b�rja spela!";
                messageSystem?.ShowBoostMessage("Tutorial klar! Tryck ENTER f�r att starta Level 1");
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
        messageSystem?.ShowBoostMessage("Level 1 b�rjar nu! Lycka till!");

        // Starta level 1
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartNextLevel();
        }
        else
        {
            Debug.Log("Level 1 startad! (LevelManager saknas)");
            // H�r kan du l�gga till annan kod f�r att starta Level 1 om du inte anv�nder LevelManager
        }
    }

    // F�r att andra skript ska kunna hoppa �ver tutorialen
    public void SkipTutorial()
    {
        tutorialActive = false;
        ShowTutorialPanel(false);
        StartLevel1();
    }
}