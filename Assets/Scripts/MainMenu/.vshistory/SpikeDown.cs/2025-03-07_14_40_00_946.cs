using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDown : MonoBehaviour
{
    private Animator anim;
    private BoxCollider2D spikeCollider;

    void Start()
    {
        anim = GetComponent<Animator>();
        spikeCollider = GetComponent<BoxCollider2D>();
    }

    public void HideSpikes()
    {
        anim.SetTrigger("Hide"); // Play animation using a trigger
        Invoke("DisableSpikes", 1.0f); // Adjust time to match animation length
    }

    private void DisableSpikes()
    {
        spikeCollider.enabled = false;  // Disable collision
    }

}
