using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    bool isSpawning = false;

    [SerializeField]
    int delay;
    [SerializeField]
    int startDelay;

    [SerializeField]
    [Range(0, 10)]
    int spawnRange;

    [SerializeField]
    GameObject[] animalsArray = new GameObject[5];


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawning)
        {
            //spawn farm animals (outside of camera view)
            //with a delay based on 'delay'
            //note every unit of 'delay'  is equal to 1 frame (60 frames = 1 second)
            if(delay <= 0)
            {
                SpawnFarmAnimal();
                SetDelay();
            }

            delay--;
        }
    }

    private void SetDelay()
    {
        int variance = UnityEngine.Random.Range(0, 120);
        delay = startDelay - variance;
    }

    private void SpawnFarmAnimal()
    {
        //select animal via RNG (uses UnityEngine.Random with 'int' overload)
        int index = UnityEngine.Random.Range(0,4);

        //use this to instantiate animal around the spawner randomly.
        Vector3 spawnPosition = GetAnimalSpawnPosition();

        Instantiate(animalsArray[index], spawnPosition, transform.rotation);
    }

    private Vector3 GetAnimalSpawnPosition()
    {
        var rand = new System.Random();

        float offset_x = rand.Next(-spawnRange, spawnRange);
        float offset_z = rand.Next(-spawnRange, spawnRange);

        

        Vector3 pos = new Vector3(transform.position.x + offset_x, transform.position.y, transform.position.z + offset_z);

        return pos;
    }
}
