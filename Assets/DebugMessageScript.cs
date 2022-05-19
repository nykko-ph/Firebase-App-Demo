using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMessageScript : MonoBehaviour
{
    Animator anim;
    int timer;

    [SerializeField]
    int delayInSeconds;

    Text text_child;
    private void Start()
    {
        timer = 0;
        anim = GetComponent<Animator>();
        text_child = transform.GetChild(0).GetComponent<Text>();
    }


    // Update is called once per frame
    void Update()
    {
        if(timer > 0)
        {
            timer--;
            if(timer == 0)
            {
                PopOut();
            }
        }
    }

    public void SetMessage(string message)
    {
        text_child.text = message;
    }

    private void PopOut()
    {
        anim.SetBool("isPopOut", true);
    }

    public void SetTimer()
    {
        timer = delayInSeconds * 60;
    }

    public void DestroyMessage()
    {
        Destroy(gameObject);
    }
}
