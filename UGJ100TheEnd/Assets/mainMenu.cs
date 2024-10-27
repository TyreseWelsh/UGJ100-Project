using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    [Header("Menu Objects")]
    [SerializeField] private GameObject mainMenuObject;
    [SerializeField] private GameObject creditsObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playGame()
    {
        SceneManager.LoadScene("S_Level1");
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void credits()
    {
        mainMenuObject.SetActive(false);
        creditsObject.SetActive(true);
    }

    public void returnToMenu()
    {
        mainMenuObject.SetActive(true);
        creditsObject.SetActive(false);
    }
}
