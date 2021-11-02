using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ButtomManager : MonoBehaviour
{
    public GameObject menu;
    public GameObject closeMenu;
    public GameObject openMenu;

    public List<string> number = new List<string>();
    public TMP_Text knumberXText;
    public TMP_Text knumberYText;
    public TMP_Text mnumberXText;
    public TMP_Text mnumberYText;
    public TMP_Text pnumberXText;
    public TMP_Text pnumberYText;

    public void Play()
    {
        SceneManager.LoadScene(2);
    }

    public void Tutorial()
    {
        SceneManager.LoadScene(1);
    }

    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
    public void Missions()
    {
        SceneManager.LoadScene(3);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OpenMenu()
    {
        closeMenu.gameObject.SetActive(false);
        openMenu.gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        openMenu.gameObject.SetActive(false);
        closeMenu.gameObject.SetActive(true);
    }

    public void GenereteNumber()
    {
        string knumberChooseX = number[Random.Range(0, number.Count)];
        knumberXText.text = knumberChooseX;

        string knumberChooseY = number[Random.Range(0, number.Count)];
        knumberYText.text = knumberChooseY;

        string mnumberChooseX = number[Random.Range(0, number.Count)];
        mnumberXText.text = mnumberChooseX;

        string mnumberChooseY = number[Random.Range(0, number.Count)];
        mnumberYText.text = mnumberChooseY;

        string pnumberChooseX = number[Random.Range(0, number.Count)];
        pnumberXText.text = pnumberChooseX;

        string pnumberChooseY = number[Random.Range(0, number.Count)];
        pnumberYText.text = pnumberChooseY;

        if((knumberChooseX == "1" && knumberChooseY == "4") || (mnumberChooseX == "1" && mnumberChooseY == "4") || (pnumberChooseX == "1" && pnumberChooseY == "4") ||
            (knumberChooseX == "5" && knumberChooseY == "3") || (mnumberChooseX == "5" && mnumberChooseY == "3") || (pnumberChooseX == "5" && pnumberChooseY == "3")
            || (knumberChooseX == "4" && knumberChooseY == "4") || (mnumberChooseX == "4" && mnumberChooseY == "4") || (pnumberChooseX == "4" && pnumberChooseY == "4")
            || (knumberChooseX == "5" && knumberChooseY == "4") || (mnumberChooseX == "5" && mnumberChooseY == "4") || (pnumberChooseX == "5" && pnumberChooseY == "4")
            || (knumberChooseX == "4" && knumberChooseY == "6") || (mnumberChooseX == "4" && mnumberChooseY == "6") || (pnumberChooseX == "4" && pnumberChooseY == "6")
            || (knumberChooseX == "4" && knumberChooseY == "5") || (mnumberChooseX == "4" && mnumberChooseY == "5") || (pnumberChooseX == "4" && pnumberChooseY == "5")
            || (knumberChooseX == "5" && knumberChooseY == "5") || (mnumberChooseX == "5" && mnumberChooseY == "5") || (pnumberChooseX == "5" && pnumberChooseY == "5")
            || (knumberChooseX == "6" && knumberChooseY == "5") || (mnumberChooseX == "6" && mnumberChooseY == "5") || (pnumberChooseX == "6" && pnumberChooseY == "5")
            || (knumberChooseX == "6" && knumberChooseY == "4") || (mnumberChooseX == "6" && mnumberChooseY == "4") || (pnumberChooseX == "6" && pnumberChooseY == "4")
            || (knumberChooseX == "5" && knumberChooseY == "6") || (mnumberChooseX == "5" && mnumberChooseY == "6") || (pnumberChooseX == "5" && pnumberChooseY == "6")
            || (knumberChooseX == "6" && knumberChooseY == "6") || (mnumberChooseX == "6" && mnumberChooseY == "6") || (pnumberChooseX == "6" && pnumberChooseY == "6"))
        {
            GenereteNumber();
        }
    }
}
