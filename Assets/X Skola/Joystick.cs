using UnityEngine;

public class Joystick : MonoBehaviour
{
    public RectTransform joystickBG;                             // Referens till joystickens bakround.
    public Transform joystickButton;                            // Referens till joystick knappen
    public float backgroundSizeOffset = 40f;                   // Offsetvärde som tillåter försändring av begränsning av knappens rörlighet
    private Vector3 touchPos;                                 // Spelarinput som uppdateras löpande.
    private Vector3 joystickBGpos;                           // Spelar input som läses in en gång när spealren nuddar skärmen.




    void Start()
    {
        HideJoystick();                                               // Göm joystick i spelets början
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            touchPos = Input.GetTouch(0).position;                  // lagra den första kontaktpunktens position
            ShowJoystick();                                        // Visa joysticken
            MoveJoystick();
            //joystickBG.position = touchPos;                     // placera joysticken på samma plats som spelarens finger

            if (Input.touches[0].phase == TouchPhase.Began)      // Undersök om spelaren nuddar skärmen
            {
                joystickBG.position = touchPos;                 // Placera joystick bakgrund på samma plats som spelarens finger
            }
        }
        else
        {
            HideJoystick(); // Göm joysticken
        }
    }

    void ShowJoystick()
    {
        joystickBG.gameObject.SetActive(true);              // Visa joystick
    }

    void HideJoystick()
    {
        joystickBG.gameObject.SetActive(false);          // Dölj joystick
    }

    void MoveJoystick()
    {
        Vector3 buttonRirection = touchPos - joystickBG.position;   // Räkna ut riktningen till joystick-knappen
        joystickButton.position = joystickBGpos + buttonRirection; // placera ut knappen
    }
}
