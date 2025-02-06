using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f; //Skeppets hastighet
    public float bankingValue = 90f; //Hur mycket skeppet bankar när det svänger
    private Camera cam; //Referens till kameran
    private Rigidbody rigidbody; //Reeferns till rigidbodyn
    private float camDistance; //Variabel som innehåller avståndet till kameran
    public float touchOffsetY = 750f; //Offsetvärde för att ändra skeppets höjdposition på skärmen
    private Vector3 velocity; //Skeppets hastighet
    private Vector3 lastPosition; //Skeppets position under föregående frame
    private Vector3 rotation; //Rotationsvärde som används till bankning
    private Vector3 touchPos; //Var spelaren placerar fingret på telefonens skärm
    private Vector3 screenToWorld; //Touchposition konverterat till 3D-koordinater

    void Start()
    {
        cam = Camera.main; //Hitta main camera
        rigidbody = GetComponent<Rigidbody>();
        camDistance = (cam.transform.position.y - transform.position.y); //Räkna ut avståndet till kameran
    }

   
    void FixedUpdate()
    {
        velocity = transform.position - lastPosition; //Räkna ut skeppets hastighet

        Move();

        lastPosition = transform.position; //Sätt last pos till skeppets nuvarande pos för att räkna ut velocity

    }

    private void Move()
    {
        if (Input.touchCount > 0) //Undersök om spelaren har något finger på displayen
        {
            touchPos = Input.GetTouch(0).position; //Förflytta i så fall skeppet till fingrets position
        }
        else //Ingen spelar-input.
        {
            // Placera skeppet i mitten av skärmen och med ett offsetvärde i höjd
            touchPos = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 - touchOffsetY);
        }

        touchPos.z = camDistance; //Ersätt touchpunktens "djup" med avståndet till kameran
        screenToWorld = cam.ScreenToWorldPoint(touchPos); //Konvertera touchpositionen (2D) till en punkt i scenen (3D)  

        Vector3 movement = Vector3.Lerp(rigidbody.position, screenToWorld, speed * Time.fixedDeltaTime); //"smootha" skeppets rörelse

        rigidbody.MovePosition(movement); //Förflytta skeppet till den smoothade positionen
        rotation.z = -velocity.x * bankingValue; //Räkna ut bankingvärdet
        rigidbody.MoveRotation(Quaternion.Euler(rotation)); //Låt skeppet banka/rotera när det svänger
    }
}
