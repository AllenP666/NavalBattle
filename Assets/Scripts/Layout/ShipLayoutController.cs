using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShipLayoutController : MonoBehaviour
{
    public GameObject popupPanel; // Панель с правилами
    public GameObject attentionText; // Панель с правилами
    public Button toggleButton; // Кнопка для открытия/закрытия всплывающего окна
    public TextMeshProUGUI rulesText; // Текст с правилами
    public GridGeneratorUI gridGeneratorUI;
    private const int GridSize = 10;
    private int[,] gridBot = new int[GridSize, GridSize];
    private Dictionary<int, int> remainingShips = new Dictionary<int, int> {
        { 4, 1 }, // 1 four-deck ship
        { 3, 2 }, // 2 three-deck ships
        { 2, 3 }, // 3 two-deck ships
        { 1, 4 }  // 4 one-deck ships
    };
    private void Start()
    {
        // Скрыть панель с правилами при старте
        popupPanel.SetActive(false);
        attentionText.SetActive(false);

        // Привязать метод к кнопке
        toggleButton.onClick.AddListener(TogglePopup);

        // Установить текст правил
        rulesText.text = "Правила расстановки кораблей:\n" +
                         "1. Расставьте все свои корабли на игровом поле.\n" +
                         "2. Корабли не могут касаться друг друга боками или углами.\n" +
                         "3. Корабли могут быть расставлены только горизонтально или вертикально.\n" +
                         "4. У каждого игрока есть:\n" +
                         "   - Один четырехпалубный корабль\n" +
                         "   - Два трехпалубных корабля\n" +
                         "   - Три двухпалубных корабля\n" +
                         "   - Четыре однопалубных корабля";
    }

    private void TogglePopup()
    {
        // Переключить видимость панели
        popupPanel.SetActive(!popupPanel.activeSelf);
    }
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void GoToGame()
    {
        if (gridGeneratorUI.AreAllShipsPlaced())
        {
            PlaceShips();
            GameData.GridBot = gridBot;
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            StartCoroutine(ShowMessage());
        }
    }

    private IEnumerator ShowMessage()
    {
        attentionText.SetActive(true);
        yield return new WaitForSeconds(3f);
        attentionText.SetActive(false);
    }

    private void PlaceShips()
    {
        // Temporary dictionary to keep track of ships being placed
        Dictionary<int, int> tempRemainingShips = new Dictionary<int, int>(remainingShips);

        foreach (var shipSize in tempRemainingShips)
        {
            for (int i = 0; i < shipSize.Value; i++)
            {
                if (PlaceShip(shipSize.Key))
                {
                    remainingShips[shipSize.Key]--;
                }
                else
                {
                    Debug.Log($"Failed to place ship of size {shipSize.Key}");
                }
            }
        }
    }

    bool PlaceShip(int size)
    {
        bool placed = false;
        int attempts = 0;
        const int maxAttempts = 100; // Limit the number of attempts to place a ship

        while (!placed && attempts < maxAttempts)
        {
            int row = Random.Range(0, GridSize);
            int col = Random.Range(0, GridSize);
            bool horizontal = Random.value > 0.5f;

            if (CanPlaceShip(row, col, size, horizontal))
            {
                for (int i = 0; i < size; i++)
                {
                    if (horizontal)
                        gridBot[row, col + i] = size;
                    else
                        gridBot[row + i, col] = size;
                }
                placed = true;
            }
            attempts++;
        }

        return placed;
    }

    bool CanPlaceShip(int row, int col, int size, bool horizontal)
    {
        if (horizontal)
        {
            if (col + size > GridSize) return false;
            for (int i = -1; i <= size; i++)
                for (int j = -1; j <= 1; j++)
                    if (IsOccupied(row + j, col + i)) return false;
        }
        else
        {
            if (row + size > GridSize) return false;
            for (int i = -1; i <= 1; i++)
                for (int j = -1; j <= size; j++)
                    if (IsOccupied(row + j, col + i)) return false;
        }
        return true;
    }

    bool IsOccupied(int row, int col)
    {
        return row >= 0 && row < GridSize && col >= 0 && col < GridSize && gridBot[row, col] != 0;
    }
}