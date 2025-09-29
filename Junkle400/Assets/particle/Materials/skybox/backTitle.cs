using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button playButton;


    void Start()
    {
        
        playButton.onClick.AddListener(backToMenu);
    }

    void backToMenu()
    {
        // insert secne name to switch to that scene
        SceneManager.LoadScene("title");
    }
}
