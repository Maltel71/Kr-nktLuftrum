using System.Collections.Generic;
using UnityEngine;

public class PoolingScript : MonoBehaviour
{
    public static PoolingScript poolingInstance;       // Fält för instansen av detta script
    public GameObject pooledObject;                    // Den prefab som ska lagras i denna poll
    public List<GameObject> pooledObjectList;          // lista som innehåller alla instanser
    public int pooledObjectAmount;                     // Hur många instanser vi ska skapa av prefaben
                                                       //
    public PoolItem[] poolItems;                       // Array innehållande de olika poolerna

    private void Awake()
    {
        poolingInstance = this;                         // Just den här intansen utgör poolingsystemet
                                                        //
        GameObject pooledInstance;                      // Referens till varje enskilt instans av prefaben

        for (int i = 0; i < pooledObjectAmount; i++)
        { 
            pooledInstance = Instantiate(pooledObject, transform);    // skapa instanserna, lagra dom under pool
            pooledInstance.SetActive(false);                          // inaktivera varje instans
            pooledObjectList.Add(pooledInstance);                     // lägg till varje instans i listan
        }

    }

    public GameObject GetPooledObject()                                // Funktion som retunerar en instans från poolen   // void för att slippa krav som gameobject ger
    {                                                                  //
        for (int i = 0; i < pooledObjectAmount;i++)                    // Loopa igenom
        {                                                              //
            if (!pooledObjectList[i].activeInHierarchy)                // Kolla efter en inaktiv instans
            {                                                          //
                return pooledObjectList[i];                            // Retunera dena
            }                                                          //
        }                                                              //
        return null;                                                   // Se till att alltid returnerea något
    }

}

[System.Serializable]                                  // Vis klassen i inspectorn
public class PoolItem                                  // Costumklass som innehållr poolens variablar, kan ha som  eget script
{                                                      //
    public GameObject pooledObject;                    // Den prefab som ska lagras i denna poll
    public List<GameObject> pooledObjectList;          // lista som innehåller alla instanser
    public int pooledObjectAmount;                     // Hur många instanser vi ska skapa av prefaben
}
