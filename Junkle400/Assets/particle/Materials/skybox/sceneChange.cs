using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button playButton;

    void Start()
    {
        
        playButton.onClick.AddListener(PlayGame);
    }

    void PlayGame()
    {
        // insert secne name to switch to that scene
        SceneManager.LoadScene("howtoPlay");
    }
}
