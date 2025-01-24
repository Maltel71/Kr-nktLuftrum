using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour
{
    private float currentFps;
    private float smoothedFps;
    private float smoothingFactor = 0.1f; 
    public float X = 60f;
    public float Y = 60f;
    public float width = 600;
    public float height = 350;
    public int fontSize = 22;
    public int framerateCapFreq = 60; //Max möjlig framerate

    private IEnumerator Start()
    {
        Application.targetFrameRate = framerateCapFreq;

        GUI.depth = 2;
        while (true)
        {
            currentFps = 1f / Time.unscaledDeltaTime;
            smoothedFps = (smoothingFactor * currentFps) + (1f - smoothingFactor) * smoothedFps;

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(X, Y, width, height), "FPS: " + Mathf.Round(smoothedFps));
        GUI.skin.label.fontSize = fontSize;
    }
}