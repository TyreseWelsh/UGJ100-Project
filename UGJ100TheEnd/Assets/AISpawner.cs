using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    public GameObject[] spawnedAI;
    public Transform[] spawnPoints;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            spawnAI();
        }
    }

    public void spawnAI()
    {
        int index = 0;
        foreach(GameObject AI in spawnedAI)
        {
            
            Instantiate(AI, spawnPoints[index]);
            index++;
        }
    }
}
