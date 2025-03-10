using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Press Enter to fade out and switch scenes
        {
            FindObjectOfType<SceneFader>().FadeToScene("Cutscene-Open");
        }

    }
}
