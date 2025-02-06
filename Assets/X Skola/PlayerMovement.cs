using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; //Skeppets hastighet
    public float bankingValue = 90f; //Hur mycket skeppet bankar n�r det sv�nger
    private Camera cam; //Referens till kameran
    private Rigidbody rigidbody; //Reeferns till rigidbodyn
    private float camDistance; //Variabel som inneh�ller avst�ndet till kameran
    public float touchOffsetY = 750f; //Offsetv�rde f�r att �ndra skeppets h�jdposition p� sk�rmen
    private Vector3 velocity; //Skeppets hastighet
    private Vector3 lastPosition; //Skeppets position under f�reg�ende frame
    private Vector3 rotation; //Rotationsv�rde som anv�nds till bankning
    private Vector3 touchPos; //Var spelaren placerar fingret p� telefonens sk�rm
    private Vector3 screenToWorld; //Touchposition konverterat till 3D-koordinater

    void Start()
    {
        cam = Camera.main; //Hitta main camera
        rigidbody = GetComponent<Rigidbody>();
        camDistance = (cam.transform.position.y - transform.position.y); //R�kna ut avst�ndet till kameran
    }

   
    void FixedUpdate()
    {
        velocity = transform.position - lastPosition; //R�kna ut skeppets hastighet

        Move();

        lastPosition = transform.position; //S�tt last pos till skeppets nuvarande pos f�r att r�kna ut velocity

    }

    private void Move()
    {
        if (Input.touchCount > 0) //Unders�k om spelaren har n�got finger p� displayen
        {
            touchPos = Input.GetTouch(0).position; //F�rflytta i s� fall skeppet till fingrets position
        }
        else //Ingen spelar-input.
        {
            // Placera skeppet i mitten av sk�rmen och med ett offsetv�rde i h�jd
            touchPos = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 - touchOffsetY);
        }

        touchPos.z = camDistance; //Ers�tt touchpunktens "djup" med avst�ndet till kameran
        screenToWorld = cam.ScreenToWorldPoint(touchPos); //Konvertera touchpositionen (2D) till en punkt i scenen (3D)  

        Vector3 movement = Vector3.Lerp(rigidbody.position, screenToWorld, speed * Time.fixedDeltaTime); //"smootha" skeppets r�relse

        rigidbody.MovePosition(movement); //F�rflytta skeppet till den smoothade positionen
        rotation.z = -velocity.x * bankingValue; //R�kna ut bankingv�rdet
        rigidbody.MoveRotation(Quaternion.Euler(rotation)); //L�t skeppet banka/rotera n�r det sv�nger
    }
}
