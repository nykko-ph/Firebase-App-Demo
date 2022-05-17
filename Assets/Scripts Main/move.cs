using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    [SerializeField]
    float speed;

    Animator anim;


    // Start is called before the first frame update
    void Start()
    {
        speed       = 2.0f;

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveForward();
    }

    private void MoveForward()
    {
        //move gameobj
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
