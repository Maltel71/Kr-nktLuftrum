using UnityEngine;
using UnityEngine.SceneManagement;

public class Startmenu : MonoBehaviour
{    
    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player Has Left The Game");
    }
}