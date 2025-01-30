using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootLaser : MonoBehaviour
{
    public float shootRate;
    private float shootRateTimeStamp;
    public float shotLifetime = 2f;

    public GameObject m_shotPrefab;
    public KeyCode fireButton;


    void Update()
    {
        if (Input.GetKey(fireButton))
        {
            if (Time.time > shootRateTimeStamp)
            {
                shootRateTimeStamp = Time.time + shootRate;
                GameObject laser = Instantiate(m_shotPrefab, transform.position, transform.rotation) as GameObject;
                GameObject.Destroy(laser, 2f);
            }
        }
    }
}
