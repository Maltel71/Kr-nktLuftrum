using UnityEngine;

public class EnvironmentScroller : MonoBehaviour
{

    public Transform envEndPoint;
    public float scrollSpeed = 2.0f;   
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, envEndPoint.position, scrollSpeed * Time.deltaTime);
    }
}
