using UnityEngine;

public class Joystick : MonoBehaviour
{
    public RectTransform joystickBG;                             // Referens till joystickens bakround.
    public Transform joystickButton;                            // Referens till joystick knappen
    public float backgroundSizeOffset = 40f;                   // Offsetv�rde som till�ter f�rs�ndring av begr�nsning av knappens r�rlighet
    private Vector3 touchPos;                                 // Spelarinput som uppdateras l�pande.
    private Vector3 joystickBGpos;                           // Spelar input som l�ses in en g�ng n�r spealren nuddar sk�rmen.




    void Start()
    {
        HideJoystick();                                               // G�m joystick i spelets b�rjan
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            touchPos = Input.GetTouch(0).position;                  // lagra den f�rsta kontaktpunktens position
            ShowJoystick();                                        // Visa joysticken
            MoveJoystick();
            //joystickBG.position = touchPos;                     // placera joysticken p� samma plats som spelarens finger

            if (Input.touches[0].phase == TouchPhase.Began)      // Unders�k om spelaren nuddar sk�rmen
            {
                joystickBG.position = touchPos;                 // Placera joystick bakgrund p� samma plats som spelarens finger
            }
        }
        else
        {
            HideJoystick(); // G�m joysticken
        }
    }

    void ShowJoystick()
    {
        joystickBG.gameObject.SetActive(true);              // Visa joystick
    }

    void HideJoystick()
    {
        joystickBG.gameObject.SetActive(false);          // D�lj joystick
    }

    void MoveJoystick()
    {
        Vector3 buttonRirection = touchPos - joystickBG.position;   // R�kna ut riktningen till joystick-knappen
        joystickButton.position = joystickBGpos + buttonRirection; // placera ut knappen
    }
}
