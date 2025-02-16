using System.Collections.Generic;
using UnityEngine;

public class PoolingScript : MonoBehaviour
{
    public static PoolingScript poolingInstance;       // F�lt f�r instansen av detta script
    public GameObject pooledObject;                    // Den prefab som ska lagras i denna poll
    public List<GameObject> pooledObjectList;          // lista som inneh�ller alla instanser
    public int pooledObjectAmount;                     // Hur m�nga instanser vi ska skapa av prefaben
                                                       //
    public PoolItem[] poolItems;                       // Array inneh�llande de olika poolerna

    private void Awake()
    {
        poolingInstance = this;                         // Just den h�r intansen utg�r poolingsystemet
                                                        //
        GameObject pooledInstance;                      // Referens till varje enskilt instans av prefaben

        for (int i = 0; i < pooledObjectAmount; i++)
        { 
            pooledInstance = Instantiate(pooledObject, transform);    // skapa instanserna, lagra dom under pool
            pooledInstance.SetActive(false);                          // inaktivera varje instans
            pooledObjectList.Add(pooledInstance);                     // l�gg till varje instans i listan
        }

    }

    public GameObject GetPooledObject()                                // Funktion som retunerar en instans fr�n poolen   // void f�r att slippa krav som gameobject ger
    {                                                                  //
        for (int i = 0; i < pooledObjectAmount;i++)                    // Loopa igenom
        {                                                              //
            if (!pooledObjectList[i].activeInHierarchy)                // Kolla efter en inaktiv instans
            {                                                          //
                return pooledObjectList[i];                            // Retunera dena
            }                                                          //
        }                                                              //
        return null;                                                   // Se till att alltid returnerea n�got
    }

}

[System.Serializable]                                  // Vis klassen i inspectorn
public class PoolItem                                  // Costumklass som inneh�llr poolens variablar, kan ha som  eget script
{                                                      //
    public GameObject pooledObject;                    // Den prefab som ska lagras i denna poll
    public List<GameObject> pooledObjectList;          // lista som inneh�ller alla instanser
    public int pooledObjectAmount;                     // Hur m�nga instanser vi ska skapa av prefaben
}
