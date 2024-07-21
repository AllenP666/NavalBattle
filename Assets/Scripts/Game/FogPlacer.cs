using UnityEngine;

public class FogPlacer : MonoBehaviour
{
    public GameObject fogBrightPrefab;
    public GameObject fogDarkPrefab;
    public GameObject botFog;
    public int gridWidth = 10;  // Ширина сетки
    public int gridHeight = 10; // Высота сетки
    public float fogHeight = 0.6f; // Высота тумана

    private Vector3 GetWorldPosition(int x, int y)
    {
        GameObject cell = GameObject.Find(string.Format("BotCell_{0}_{1}", y, x));
        if (cell != null)
        {
            return cell.transform.position;
        }
        else
        {
            Debug.LogError(string.Format("BotCell_{0}_{1} not found!", y, x));
            return Vector3.zero;
        }
    }

    public void PlaceFogInChessPattern()
    {
        if (botFog == null)
        {
            Debug.LogError("BotFog object is not assigned.");
            return;
        }

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 position = GetWorldPosition(x, y);
                position.y = fogHeight; // Устанавливаем высоту тумана
                GameObject fogPrefab = (x + y) % 2 == 0 ? fogBrightPrefab : fogDarkPrefab;
                
                if (fogPrefab != null)
                {
                    GameObject fogInstance = Instantiate(fogPrefab, position, Quaternion.identity, botFog.transform);
                    fogInstance.name = string.Format("BotFog_{0}_{1}", y, x); // Задаем название
                }
                else
                {
                    Debug.LogError("Fog prefab is not assigned.");
                }
            }
        }
    }
    
    // Можно вызывать этот метод, чтобы расставить туман при старте
    void Start()
    {
        PlaceFogInChessPattern();
    }
}
