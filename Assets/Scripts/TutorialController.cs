using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public GameObject[] Pages;
    public int CurrentPage = 0;

    void Start()
    {
        this._disableAllPages();
        this.CurrentPage = 0;
        this.Pages[this.CurrentPage].SetActive(true);
    }

    void Update()
    {
        
    }

    public void CloseTutorial() {
        this._disableAllPages();
        this.CurrentPage = 0;
        this.Pages[this.CurrentPage].SetActive(true);
        gameObject.SetActive(false);
    }

    public void NextPage() {
        if (CurrentPage < Pages.Length - 1) {
            this.CurrentPage++;
            this._disableAllPages();
            this.Pages[this.CurrentPage].SetActive(true);
        }
    }

    public void PreviousPage() {
        if (CurrentPage > 0)
        {
            this.CurrentPage--;
            this._disableAllPages();
            this.Pages[this.CurrentPage].SetActive(true);
        }
    }

    private void _disableAllPages() {
        foreach (GameObject go in Pages) {
            go.SetActive(false);
        }
    }
}
