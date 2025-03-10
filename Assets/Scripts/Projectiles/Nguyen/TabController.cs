using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabimages;
    public GameObject[] pages;
    // Start is called before the first frame update
    void Start()
    {
        Activatetab(0);
    }

    // Update is called once per frame
    public void Activatetab(int tab)
    {
        for(int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabimages[i].color = Color.gray;
        }
        pages[tab].SetActive(true);
        tabimages[tab].color = Color.white;
    }
}
