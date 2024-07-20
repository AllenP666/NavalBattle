using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenuController : MonoBehaviour
{
    public GameObject menuPanel; // Ссылка на панель меню
    public GameObject infoPanel; // Ссылка на панель меню
    public Button menuButton; // Ссылка на кнопку меню
    public Button resumeButton; // Ссылка на кнопку продолжения игры
    public Button exitButton; // Ссылка на кнопку выхода в меню
    public Button infoButton; // Ссылка на кнопку инофрмации об игре
    public Button closeInfoButton; // Ссылка на кнопку закрытия инофрмации об игре


    void Start()
    {
        // Скрываем панель меню при старте
        menuPanel.SetActive(false);
        infoPanel.SetActive(false);

        // Добавляем слушатель события нажатия на кнопку
        menuButton.onClick.AddListener(ToggleMenu);
        resumeButton.onClick.AddListener(ToggleMenu);
        exitButton.onClick.AddListener(GoToMenu);
        infoButton.onClick.AddListener(ToggleInfo);
        closeInfoButton.onClick.AddListener(ToggleInfo);
    }

    void ToggleMenu()
    {
        // Переключаем видимость панели меню
        menuPanel.SetActive(!menuPanel.activeSelf);
        menuButton.gameObject.SetActive(!menuPanel.activeSelf);
    }

    void ToggleInfo()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
        infoPanel.SetActive(!infoPanel.activeSelf);
    }

    void GoToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
