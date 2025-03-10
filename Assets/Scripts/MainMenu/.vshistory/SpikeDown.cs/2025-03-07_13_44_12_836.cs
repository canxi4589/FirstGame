using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDown : MonoBehaviour
{
    private Animator anim;
    private bool isUp = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void ToggleSpikes()
    {
        isUp = !isUp;
        anim.SetTrigger("ActivateSpikes");
        GetComponent<BoxCollider2D>().enabled = isUp; // Enable/Disable collider
    }
}
