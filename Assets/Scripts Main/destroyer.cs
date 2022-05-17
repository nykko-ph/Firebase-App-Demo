using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyer : MonoBehaviour
{
    [SerializeField]
    int chicken, cow, duck, pig, sheep, allAnimals;

    private void Start()
    {
        AnimalCountsInit();
    }

    private void AnimalCountsInit()
    {
        chicken = 0;
        cow = 0;
        duck = 0;
        pig = 0;
        sheep = 0;
        allAnimals = 0;
    }

    void OnTriggerEnter(Collider animal)
    {
        ///Step 1: Count animal
        ///Step 2: Destroy animal(gameobject)

        //all prefabs are assumed to have a child that has the collider and rigidbody component
        //this is done because animator doesn't work with rigidbody
        CountAnimal();
        Destroy(animal.transform.parent.gameObject);
    }

    private void CountAnimal()
    {
        allAnimals++;
    }

    public int GetAnimalCount()
    {
        return allAnimals;
    }
}
