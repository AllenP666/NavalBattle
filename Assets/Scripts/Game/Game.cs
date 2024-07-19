using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool isPlayerTurn = true;
    private int[,] gridPlayer = GameData.GridPlayer;
    private int[,] gridBot = GameData.GridBot;

    private List<Vector2Int> botHits = new List<Vector2Int>();
    private List<Vector2Int> botPossibleTargets = new List<Vector2Int>();
    private HashSet<Vector2Int> playerShots = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> botShots = new HashSet<Vector2Int>();

    void Update()
    {
        if (isPlayerTurn)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                HandlePlayerInput(Input.GetTouch(0).position);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                HandlePlayerInput(Input.mousePosition);
            }
        }
        else
        {
            // Ход бота
            Vector2Int botShot = BotShoot();
            Shoot(gridPlayer, botShot.y, botShot.x, false);
            isPlayerTurn = true;
        }
    }

    void HandlePlayerInput(Vector2 inputPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject touchedObject = hit.collider.gameObject;
            if (touchedObject.name.StartsWith("BotCell_"))
            {
                string[] coordinates = touchedObject.name.Split('_');
                if (coordinates.Length == 3)
                {
                    int y = int.Parse(coordinates[1]);
                    int x = int.Parse(coordinates[2]);
                    Vector2Int shotPosition = new Vector2Int(x, y);
                    if (!playerShots.Contains(shotPosition))
                    {
                        Shoot(gridBot, y, x, true);
                        isPlayerTurn = false;
                    }
                    else
                    {
                        Debug.Log($"You've already shot at ({y}, {x})");
                    }
                }
            }
        }
    }

    void Shoot(int[,] grid, int y, int x, bool isPlayerShooting)
    {
        Vector2Int shotPosition = new Vector2Int(x, y);
        if (isPlayerShooting)
        {
            playerShots.Add(shotPosition);
        }
        else
        {
            botShots.Add(shotPosition);
        }

        if (grid[y, x] != 0)
        {
            grid[y, x] = -1;
            Debug.Log($"Hit at ({y}, {x}) - {isPlayerShooting}");
            if (!isPlayerShooting)
            {
                botHits.Add(shotPosition);
                AddPossibleTargets(x, y);
            }
        }
        else
        {
            Debug.Log($"Miss at ({y}, {x}) - {isPlayerShooting}");
        }
    }

    Vector2Int BotShoot()
    {
        Vector2Int target;
        if (botPossibleTargets.Count > 0)
        {
            target = botPossibleTargets[0];
            botPossibleTargets.RemoveAt(0);
        }
        else
        {
            do
            {
                target = new Vector2Int(Random.Range(0, 10), Random.Range(0, 10));
            } while (botShots.Contains(target));
        }

        return target;
    }

    void AddPossibleTargets(int x, int y)
    {
        // Добавляем потенциальные цели вокруг попадания
        AddTargetIfValid(new Vector2Int(x + 1, y));
        AddTargetIfValid(new Vector2Int(x - 1, y));
        AddTargetIfValid(new Vector2Int(x, y + 1));
        AddTargetIfValid(new Vector2Int(x, y - 1));
    }

    void AddTargetIfValid(Vector2Int target)
    {
        if (target.x >= 0 && target.x < 10 && target.y >= 0 && target.y < 10 && 
            !botShots.Contains(target) && !botPossibleTargets.Contains(target))
        {
            botPossibleTargets.Add(target);
        }
    }
}