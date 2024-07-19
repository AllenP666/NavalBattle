using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridGeneratorUI : MonoBehaviour
{
    public GameObject cellPrefab;           // Prefab for gridPlayer cell
    public Sprite cellSelectedSprite;       // Sprite for selected cell
    public Sprite cellDefaultSprite;        // Sprite for default cell
    public TextMeshProUGUI rotateText;
    public int rows = 10;                   // Number of rows
    public int columns = 10;                // Number of columns
    public float cellSize = 75f;            // Cell size in pixels
    private const int GridSize = 10;
    public int[,] gridPlayer = new int[GridSize, GridSize];
    private GameObject[,] cellObjects;      // Array to store cell references

    private int selectedShipSize = -1;      // Size of the selected ship
    private bool isHorizontal = true;       // Orientation of the selected ship
    private List<GameObject> placedShips = new List<GameObject>(); // List to store placed ships

    // Dictionary to store remaining ships count by size
    private Dictionary<int, int> remainingShips = new Dictionary<int, int> {
        { 4, 1 }, // 1 four-deck ship
        { 3, 2 }, // 2 three-deck ships
        { 2, 3 }, // 3 two-deck ships
        { 1, 4 }  // 4 one-deck ships
    };

    void Start()
    {
        GenerateGrid();

        // Add button listeners
        GameObject.Find("OneDeckShipButton").GetComponent<Button>().onClick.AddListener(() => SelectShip(1));
        GameObject.Find("TwoDeckShipButton").GetComponent<Button>().onClick.AddListener(() => SelectShip(2));
        GameObject.Find("ThreeDeckShipButton").GetComponent<Button>().onClick.AddListener(() => SelectShip(3));
        GameObject.Find("FourDeckShipButton").GetComponent<Button>().onClick.AddListener(() => SelectShip(4));
        GameObject.Find("RotateButton").GetComponent<Button>().onClick.AddListener(ToggleShipOrientation);
    }

    public void RegenerateGrid()
    {
        ClearGridData();
        PlaceShips();
    }

    public void ClearGrid()
    {
        ClearGridData();
        UpdateCellVisuals();
    }

    void GenerateGrid()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        cellObjects = new GameObject[rows, columns];

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                RectTransform cellRectTransform = cell.GetComponent<RectTransform>();

                // Set cell size and position
                cellRectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                cellRectTransform.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
                cell.name = $"Cell_{x}_{y}";

                cellObjects[y, x] = cell;

                // Add click listener to cell
                int capturedX = x;
                int capturedY = y;
                cell.GetComponent<Button>().onClick.AddListener(() => PlaceSelectedShip(capturedX, capturedY));
            }
        }

        // Set RectTransform size for gridPlayer
        rectTransform.sizeDelta = new Vector2(columns * cellSize, rows * cellSize);
    }

    void PlaceShips()
    {
        ClearGridData();

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
        UpdateCellVisuals();
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
                        gridPlayer[row, col + i] = size;
                    else
                        gridPlayer[row + i, col] = size;
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
        return row >= 0 && row < GridSize && col >= 0 && col < GridSize && gridPlayer[row, col] != 0;
    }

    void UpdateCellVisuals()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject cell = cellObjects[y, x];
                Image cellImage = cell.GetComponent<Image>();

                if (gridPlayer[y, x] != 0)
                {
                    cellImage.sprite = cellSelectedSprite;
                }
                else
                {
                    cellImage.sprite = cellDefaultSprite;
                }
            }
        }
    }

    void ClearGridData()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                gridPlayer[y, x] = 0;
            }
        }
        foreach (var ship in placedShips)
        {
            Destroy(ship);
        }
        placedShips.Clear();

        // Reset remaining ships count
        remainingShips = new Dictionary<int, int> {
            { 4, 1 },
            { 3, 2 },
            { 2, 3 },
            { 1, 4 }
        };
    }

    void SelectShip(int size)
    {
        if (remainingShips.ContainsKey(size) && remainingShips[size] > 0)
        {
            selectedShipSize = size;
        }
        else
        {
            Debug.Log($"No remaining ships of size {size}");
        }
    }

    void ToggleShipOrientation()
    {
        isHorizontal = !isHorizontal;
        if (isHorizontal) 
        {
            rotateText.text = "Horizontal";
        }
        else
        {
            rotateText.text = "Vertical";
        }
    }

    void PlaceSelectedShip(int x, int y)
    {
        if (selectedShipSize == -1)
        {
            RemoveShip(x, y);
            return;
        }

        if (CanPlaceShip(y, x, selectedShipSize, isHorizontal))
        {
            for (int i = 0; i < selectedShipSize; i++)
            {
                if (isHorizontal)
                    gridPlayer[y, x + i] = selectedShipSize;
                else
                    gridPlayer[y + i, x] = selectedShipSize;
            }
            UpdateCellVisuals();
            remainingShips[selectedShipSize]--;
            selectedShipSize = -1; // Reset selection after placing
        }
    }

    void RemoveShip(int x, int y)
    {
        if (gridPlayer[y, x] == 0) return;

        int shipSize = gridPlayer[y, x];
        bool horizontal = false;

        // Determine the orientation of the ship
        if ((x < GridSize - 1 && gridPlayer[y, x + 1] == shipSize) || (x > 0 && gridPlayer[y, x - 1] == shipSize))
        {
            horizontal = true;
        }

        // Find the start position of the ship
        int startX = x;
        int startY = y;

        if (horizontal)
        {
            while (startX > 0 && gridPlayer[y, startX - 1] == shipSize)
            {
                startX--;
            }
        }
        else
        {
            while (startY > 0 && gridPlayer[startY - 1, x] == shipSize)
            {
                startY--;
            }
        }

        // Remove the ship
        for (int i = 0; i < shipSize; i++)
        {
            if (horizontal)
            {
                gridPlayer[startY, startX + i] = 0;
            }
            else
            {
                gridPlayer[startY + i, startX] = 0;
            }
        }

        // Decrease the count of remaining ships of this size
        remainingShips[shipSize]++;

        UpdateCellVisuals();
    }

    public bool AreAllShipsPlaced()
    {
        foreach (var shipCount in remainingShips.Values)
        {
            if (shipCount > 0)
            {
                return false;
            }
        }

        GameData.GridPlayer = gridPlayer;
        return true;
    }

}