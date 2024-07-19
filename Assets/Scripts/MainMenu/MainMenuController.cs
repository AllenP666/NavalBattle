using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject infoText;
    bool isInfoEnabled = false;
    private void Start()
    {
        infoText.SetActive(isInfoEnabled);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("ShipLayoutScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowInfo()
    {
        isInfoEnabled = !isInfoEnabled;
        infoText.SetActive(isInfoEnabled);
    }
}
