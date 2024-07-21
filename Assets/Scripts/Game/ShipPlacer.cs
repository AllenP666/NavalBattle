using System.Collections.Generic;
using UnityEngine;

public class ShipPlacer : MonoBehaviour
{
    private int[,] gridPlayer = GameData.GridPlayer;
    private int[,] gridBot = GameData.GridBot;
    public GameObject shipOneDeckPrefab;
    public GameObject shipTwoDeckPrefab;
    public GameObject shipThreeDeckPrefab;
    public GameObject shipFourDeckPrefab;
    public GameObject playerShips;

    void Start()
    {
        PlaceShips(gridPlayer, playerShips);

        List<Ship> shipsPlayer = DetectShips(gridPlayer);
        List<Ship> shipsBot = DetectShips(gridBot);

        foreach (Ship ship in shipsPlayer)
        {
            Debug.Log(ship + ". For player.");
        }

        foreach (Ship ship in shipsBot)
        {
            Debug.Log(ship + ". For bot.");
        }
    }

    private List<Ship> DetectShips(int[,] grid)
    {
        List<Ship> ships = new List<Ship>();
        bool[,] visited = new bool[10, 10];

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (grid[i, j] > 0 && !visited[i, j])
                {
                    ships.Add(FindShip(grid, visited, i, j));
                }
            }
        }

        return ships;
    }

    private Ship FindShip(int[,] grid, bool[,] visited, int startX, int startY)
    {
        int endX = startX;
        int endY = startY;
        int shipType = grid[startX, startY];
        visited[startX, startY] = true;

        // Check horizontal direction
        if (startY + 1 < 10 && grid[startX, startY + 1] == shipType)
        {
            endY = startY;
            while (endY + 1 < 10 && grid[startX, endY + 1] == shipType)
            {
                endY++;
                visited[startX, endY] = true;
            }
        }
        // Check vertical direction
        else if (startX + 1 < 10 && grid[startX + 1, startY] == shipType)
        {
            endX = startX;
            while (endX + 1 < 10 && grid[endX + 1, startY] == shipType)
            {
                endX++;
                visited[endX, startY] = true;
            }
        }

        return new Ship(startX, startY, endX, endY, shipType);
    }

    public class Ship
    {
        public int StartX { get; }
        public int StartY { get; }
        public int EndX { get; }
        public int EndY { get; }
        public int ShipType { get; }

        public Ship(int startX, int startY, int endX, int endY, int shipType)
        {
            StartX = startX; // Сделано так, потому что изначалаьно координаты задаются как y,x
            StartY = startY; // В этой части кода проще всего поменять значения
            EndX = endX;
            EndY = endY;
            ShipType = shipType;
        }

        public override string ToString()
        {
            return string.Format("{0}-deck ship: Start ({1}, {2}) - End ({3}, {4})", ShipType, StartX, StartY, EndX, EndY);
        }
    }

    private void PlaceShips(int[,] grid, GameObject parent)
    {
        List<Ship> ships = DetectShips(grid);

        foreach (Ship ship in ships)
        {
            GameObject shipPrefab = GetShipPrefab(ship.ShipType);
            Vector3 startPosition = GetWorldPosition(ship.StartX, ship.StartY);
            Vector3 endPosition = GetWorldPosition(ship.EndX, ship.EndY);

            // Calculate position and rotation
            Vector3 position;
            Quaternion rotation = Quaternion.identity;

            float randomNumber = Random.Range(0f, 1f);
            if (randomNumber > 0.5f) rotation = Quaternion.Euler(0, 180, 0);
            
            if (ship.StartX == ship.EndX)
            {
                // Horizontal ship
                if (randomNumber > 0.5f) rotation = Quaternion.Euler(0, -90, 0);
                else rotation = Quaternion.Euler(0, 90, 0);
                position = new Vector3((startPosition.x + endPosition.x) / 2, 0.5f, startPosition.z);
            }
            else
            {
                // Vertical ship
                position = new Vector3(startPosition.x, 0.5f, (startPosition.z + endPosition.z) / 2);
            }

            // Instantiate and configure the ship
            GameObject shipInstance = Instantiate(shipPrefab, position, rotation);
            shipInstance.name = string.Format("Ship_{0}_({1},{2})_({3},{4})", ship.ShipType, ship.StartX, ship.StartY, ship.EndX, ship.EndY);
            shipInstance.transform.SetParent(parent.transform);
        }
    }

    private GameObject GetShipPrefab(int shipType)
    {
        switch (shipType)
        {
            case 1: return shipOneDeckPrefab;
            case 2: return shipTwoDeckPrefab;
            case 3: return shipThreeDeckPrefab;
            case 4: return shipFourDeckPrefab;
            default: return null;
        }
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        GameObject cell = GameObject.Find(string.Format("PlayerCell_{0}_{1}", y, x));
        if (cell != null)
        {
            return cell.transform.position;
        }
        else
        {
            Debug.LogError(string.Format("PlayerCell_{0}_{1} not found!", y, x));
            return Vector3.zero;
        }
    }
}
