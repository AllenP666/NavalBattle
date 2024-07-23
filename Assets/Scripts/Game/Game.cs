using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private bool isPlayerTurn = true;
    private int[,] gridPlayer = GameData.GridPlayer;
    private int[,] gridBot = GameData.GridBot;
    private int playerHitsCount = 0;
    private int botHitsCount = 0;
    private float shootDelay = 2f;
    public GameObject bombDroppingPrefab;
    public GameObject bombSwimmingPrefab;
    public GameObject smokePrefab;
    public GameObject shipOneDeckFloatingPrefab;
    public GameObject shipTwoDeckFloatingPrefab;
    public GameObject shipThreeDeckFloatingPrefab;
    public GameObject shipFourDeckFloatingPrefab;
    public GameObject botShips;

    public Button gameEndButton;
    public TextMeshProUGUI gameEndText;
    public GameObject gameEndPanel;


    private List<Vector2Int> botPossibleTargets = new List<Vector2Int>();
    private HashSet<Vector2Int> playerShots = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> botShots = new HashSet<Vector2Int>();

    void Start()
    {
        gameEndPanel.SetActive(false);
        gameEndButton.onClick.AddListener(GoToMenu);
    }

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
            if (shootDelay > 0) shootDelay -= Time.deltaTime;
            else 
            {
                Vector2Int botShot = BotShoot();
                Shoot(gridPlayer, botShot.y, botShot.x, isPlayerTurn);
                shootDelay = 2f;
                isPlayerTurn = !isPlayerTurn;
            }
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
                        Shoot(gridBot, y, x, isPlayerTurn);
                        isPlayerTurn = !isPlayerTurn;
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
            GameObject botFog = GameObject.Find(string.Format("BotFog_{0}_{1}", y, x));
            StartCoroutine(ClearFog(botFog));  
        }
        else
        {
            botShots.Add(shotPosition);
        }

        bool isHit = grid[y, x] != 0;
        if (!isPlayerShooting) StartCoroutine(DropBomb(new Vector2Int(y, x), !isPlayerShooting, isHit));
        else StartCoroutine(DropBomb(new Vector2Int(x, y), !isPlayerShooting, isHit));

        if (isHit)
        {
            grid[y, x] = -1;
            Debug.Log($"Hit at ({y}, {x}) - {isPlayerShooting}");
            if (!isPlayerShooting)
            {
                botShots.Add(shotPosition);
                AddPossibleTargets(x, y);
            }
            
            // Проверяем, потоплен ли корабль
            if (IsShipSunk(grid, y, x))
            {
                RevealAroundSunkenShip(grid, y, x, isPlayerShooting);
                SunkShip(grid, y, x, isPlayerShooting);
            }

            if (isPlayerShooting) playerHitsCount++;
            else botHitsCount++;

            if (playerHitsCount == 20) ShowGameEndPanel(true);
            else if (botHitsCount == 20) ShowGameEndPanel(false);

            isPlayerTurn = !isPlayerTurn;
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
                target = new Vector2Int(UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 10));
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

    private IEnumerator ClearFog(GameObject fogPrefab)
    {
        // Находим модель тумана внутри префаба
        Renderer fogRenderer = fogPrefab.GetComponentInChildren<Renderer>();
        
        if (fogRenderer != null)
        {
            Material fogMaterial = fogRenderer.material;
            
            // Убедимся, что мы работаем с экземпляром материала, а не с общим ресурсом
            fogRenderer.material = new Material(fogMaterial);
            fogMaterial = fogRenderer.material;

            // Настройка материала для прозрачности
            fogMaterial.SetFloat("_Mode", 2); // 2 - Fade mode
            fogMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fogMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fogMaterial.SetInt("_ZWrite", 0);
            fogMaterial.DisableKeyword("_ALPHATEST_ON");
            fogMaterial.EnableKeyword("_ALPHABLEND_ON");
            fogMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            fogMaterial.renderQueue = 3000;

            float duration = 1f;
            float elapsedTime = 0f;
            Color startColor = fogMaterial.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / duration);
                
                fogMaterial.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Renderer not found in fog prefab children");
        }
        
        Destroy(fogPrefab);
    }

    private IEnumerator DropBomb(Vector2Int cellPosition, bool isPlayerGrid, bool isHit)
    {
        // Определяем, какую сетку использовать
        string gridName = isPlayerGrid ? "PlayerGrid" : "BotGrid";
        GameObject grid = GameObject.Find("Grids").transform.Find(gridName).gameObject;

        // Находим нужную клетку (используем формат y_x)
        string cellName = $"{gridName.Replace("Grid", "Cell")}_{cellPosition.y}_{cellPosition.x}";
        Transform cellTransform = grid.transform.Find(cellName);

        if (cellTransform == null)
        {
            Debug.LogError($"Cell {cellName} not found!");
            yield break;
        }

        // Создаем и позиционируем падающую бомбу
        GameObject droppingBomb = Instantiate(bombDroppingPrefab, cellTransform.position + Vector3.up * 5f, Quaternion.identity);
        droppingBomb.transform.SetParent(cellTransform);

        // Анимация падения бомбы
        float dropDuration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startPosition = droppingBomb.transform.localPosition;
        Vector3 endPosition = Vector3.zero;

        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;
            droppingBomb.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        // Постепенное исчезновение падающей бомбы
        Renderer bombRenderer = droppingBomb.GetComponentInChildren<Renderer>();
        if (bombRenderer != null)
        {
            Material bombMaterial = bombRenderer.material;
            
            // Создаем экземпляр материала
            bombRenderer.material = new Material(bombMaterial);
            bombMaterial = bombRenderer.material;

            // Настройка материала для прозрачности
            SetupMaterialForTransparency(bombMaterial);

            float fadeDuration = 1f;
            elapsedTime = 0f;
            Color startColor = bombMaterial.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                
                bombMaterial.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Renderer not found in dropping bomb prefab children");
        }

        // Удаляем падающую бомбу
        Destroy(droppingBomb);

        // Определяем высоту для эффекта
        float height = isHit ? 0.75f : 0.15f;

        // Создаем соответствующий эффект в зависимости от попадания
        GameObject effectPrefab = isHit ? smokePrefab : bombSwimmingPrefab;
        Vector3 effectPosition = cellTransform.position + Vector3.up * height;
        GameObject effect = Instantiate(effectPrefab, effectPosition, Quaternion.identity);
        effect.transform.SetParent(cellTransform);

        // Если это сетка бота, поворачиваем эффект на 180 градусов
        if (!isPlayerGrid)
        {
            effect.transform.Rotate(Vector3.up, 180f);
        }

        // Постепенное появление эффекта
        Renderer effectRenderer = effect.GetComponentInChildren<Renderer>();
        if (effectRenderer != null)
        {
            Material effectMaterial = effectRenderer.material;
            
            // Создаем экземпляр материала
            effectRenderer.material = new Material(effectMaterial);
            effectMaterial = effectRenderer.material;

            // Настройка материала для прозрачности
            SetupMaterialForTransparency(effectMaterial);

            float appearDuration = 1f;
            elapsedTime = 0f;
            Color startColor = effectMaterial.color;
            startColor.a = 0f;
            effectMaterial.color = startColor;

            while (elapsedTime < appearDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / appearDuration);
                
                effectMaterial.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Renderer not found in effect prefab children");
        }
    }

    // Вспомогательный метод для настройки материала на прозрачность
    private void SetupMaterialForTransparency(Material material)
    {
        material.SetFloat("_Mode", 2); // 2 - Fade mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }

    bool IsShipSunk(int[,] grid, int y, int x)
    {
        if (grid[y, x] == 0)
        {
            return false; // Это пустая клетка, не часть корабля
        }

        if (grid[y, x] > 0)
        {
            return false; // Это целая часть корабля, корабль не потоплен
        }

        // Найдем начало и конец корабля
        int startX = x, endX = x;
        int startY = y, endY = y;

        // Определим горизонтальный или вертикальный корабль
        bool isHorizontal = (x > 0 && grid[y, x - 1] != 0) || (x < 9 && grid[y, x + 1] != 0);

        if (isHorizontal)
        {
            // Найдем границы корабля по горизонтали
            while (startX > 0 && grid[y, startX - 1] != 0)
            {
                startX--;
            }
            while (endX < 9 && grid[y, endX + 1] != 0)
            {
                endX++;
            }

            // Проверим все части корабля по горизонтали
            for (int i = startX; i <= endX; i++)
            {
                if (grid[y, i] > 0)
                {
                    return false; // Найдена целая часть корабля, корабль не потоплен
                }
            }
        }
        else
        {
            // Найдем границы корабля по вертикали
            while (startY > 0 && grid[startY - 1, x] != 0)
            {
                startY--;
            }
            while (endY < 9 && grid[endY + 1, x] != 0)
            {
                endY++;
            }

            // Проверим все части корабля по вертикали
            for (int j = startY; j <= endY; j++)
            {
                if (grid[j, x] > 0)
                {
                    return false; // Найдена целая часть корабля, корабль не потоплен
                }
            }
        }

        return true; // Все части корабля повреждены, корабль потоплен
    }

    void RevealAroundSunkenShip(int[,] grid, int y, int x, bool isPlayerShooting)
    {
        int rows = 10;
        int cols = 10;

        // Определяем границы корабля
        int minX = x;
        int maxX = x;
        int minY = y;
        int maxY = y;

        // Расширяем границы корабля влево и вправо
        for (int i = x - 1; i >= 0; i--)
        {
            if (grid[y, i] != -1) break;
            minX = i;
        }
        for (int i = x + 1; i < cols; i++)
        {
            if (grid[y, i] != -1) break;
            maxX = i;
        }

        // Расширяем границы корабля вверх и вниз
        for (int j = y - 1; j >= 0; j--)
        {
            if (grid[j, x] != -1) break;
            minY = j;
        }
        for (int j = y + 1; j < rows; j++)
        {
            if (grid[j, x] != -1) break;
            maxY = j;
        }

        // Раскрываем туман вокруг корабля
        for (int j = minY - 1; j <= maxY + 1; j++)
        {
            for (int i = minX - 1; i <= maxX + 1; i++)
            {
                if (j >= 0 && j < rows && i >= 0 && i < cols)
                {
                    if ((!playerShots.Contains(new Vector2Int(i, j)) && isPlayerShooting)
                        || (!botShots.Contains(new Vector2Int(i, j)) && !isPlayerShooting))
                    {
                        // Развеиваем туман
                        if (isPlayerShooting)
                        {
                            GameObject fog = GameObject.Find($"BotFog_{j}_{i}");
                            if (fog != null)
                            {
                                StartCoroutine(ClearFog(fog));
                            }
                        }

                        // Проигрываем анимацию сброса бомб
                        if (!isPlayerShooting) StartCoroutine(DropBomb(new Vector2Int(j, i), !isPlayerShooting, false));
                        else StartCoroutine(DropBomb(new Vector2Int(i, j), !isPlayerShooting, false));

                        // Помечаем клетку как недоступную для стрельбы
                        if (isPlayerShooting)
                        {
                            playerShots.Add(new Vector2Int(i, j));
                        }
                        else
                        {
                            botShots.Add(new Vector2Int(i, j));
                            botPossibleTargets.Remove(new Vector2Int(i, j));
                        }
                    }
                }
            }
        }
    }

    void SunkShip(int[,] grid, int y, int x, bool isPlayerShooting)
    {
        int rows = 10;
        int cols = 10;

        // Определяем границы корабля
        int minX = x;
        int maxX = x;
        int minY = y;
        int maxY = y;

        // Расширяем границы корабля влево и вправо
        for (int i = x - 1; i >= 0; i--)
        {
            if (grid[y, i] != -1) break;
            minX = i;
        }
        for (int i = x + 1; i < cols; i++)
        {
            if (grid[y, i] != -1) break;
            maxX = i;
        }

        // Расширяем границы корабля вверх и вниз
        for (int j = y - 1; j >= 0; j--)
        {
            if (grid[j, x] != -1) break;
            minY = j;
        }
        for (int j = y + 1; j < rows; j++)
        {
            if (grid[j, x] != -1) break;
            maxY = j;
        }
        
        int shipType = Math.Abs(maxX - minX);
        if (shipType < Math.Abs(maxY - minY)) shipType = Math.Abs(maxY - minY);
        shipType++;

        // Выбираем префаб на основе типа корабля
        GameObject floatingPrefab = null;
        switch (shipType)
        {
            case 1:
                floatingPrefab = shipOneDeckFloatingPrefab;
                break;
            case 2:
                floatingPrefab = shipTwoDeckFloatingPrefab;
                break;
            case 3:
                floatingPrefab = shipThreeDeckFloatingPrefab;
                break;
            case 4:
                floatingPrefab = shipFourDeckFloatingPrefab;
                break;
        }

        if (!isPlayerShooting)
        {        
            GameObject ship = GameObject.Find(string.Format("Ship_{0}_({1},{2})_({3},{4})", shipType, minY, minX, maxY, maxX));

            if (ship == null) ship = GameObject.Find(string.Format("Ship_{0}_({1},{2})_({3},{4})", shipType, maxY, maxX, minY, minX));

            // Если корабль найден и префаб определён
            if (ship != null && floatingPrefab != null)
            {
                // Создаём новый объект плавающего корабля на основе префаба
                GameObject floatingShip = Instantiate(floatingPrefab, ship.transform.position, ship.transform.rotation);
                floatingShip.transform.parent = ship.transform.parent;

                // Удаляем исходный объект корабля
                Destroy(ship);
            }
        }
        else
        {
            GameObject shipInstance = Instantiate(floatingPrefab, botShips.transform);
            GameObject cellMin = GameObject.Find(string.Format("BotCell_{0}_{1}", minY, minX));
            GameObject cellMax = GameObject.Find(string.Format("BotCell_{0}_{1}", maxY, maxX));
            Vector3 startPosition = cellMin.transform.localPosition;
            Vector3 endPosition = cellMax.transform.localPosition;

            // Calculate position and rotation
            Vector3 position;
            Quaternion rotation = Quaternion.identity;

            float randomNumber = UnityEngine.Random.Range(0f, 1f);
            if (randomNumber > 0.5f) rotation = Quaternion.Euler(0, 180, 0);
            
            if (minX == maxX)
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

            // Cconfigure the ship
            shipInstance.transform.SetLocalPositionAndRotation(position, rotation);
        }
    }

    private void ShowGameEndPanel(bool isPlayerWin)
    {
        if (isPlayerWin) gameEndText.text = "Победа!";
        else gameEndText.text = "Проигрыш.";
        gameEndPanel.SetActive(true);

        GameObject menuButton = GameObject.Find("MenuButton");
        menuButton.SetActive(false);

    }

    void GoToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}