using UnityEditor; 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class snakegame : MonoBehaviour
{
    [Header("Settings")]
    [Range(.1f,.5f)]
    public float frameDuration = 1;
    public int gridSize = 100;

    [Header("Sprite")]
    public Sprite boxSprite;

    GameObject[] boxes;
    float boxSize;
    Vector2Int[] snake;
    Vector2Int food;
    Direction dir;
    Direction Lastdir;

    public enum Direction
    {
        Left,Right,Up,Down
    }

    void Start()
    {
        Lastdir = dir;
        snake = new Vector2Int[] { new Vector2Int(gridSize / 2, gridSize / 2) };
        boxSize = 2 /(float)gridSize;
        GenerateGrid();
        Draw();
        RandomizeFood();
        GameLoop();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W)) {
            if (Lastdir != Direction.Down) {dir = Direction.Up;
            }
            else { dir = Direction.Left; }
                
        }else if (Input.GetKey(KeyCode.A)) {
            if (Lastdir != Direction.Right) {dir = Direction.Left;
            }
            else { dir = Direction.Up; }
                
        }else if (Input.GetKey(KeyCode.S)) {
            if (Lastdir != Direction.Up) {dir = Direction.Down;
            }
            else { dir = Direction.Right; }
                
        }else if (Input.GetKey(KeyCode.D)) {
            if (Lastdir != Direction.Left) {dir = Direction.Right;
            }
            else { dir = Direction.Down; }
                
        }
    }

    public void GameLoop()
    {
        if (snake[0] == food) {
            Vector2Int[] newSnake = new Vector2Int[snake.Length + 1];
            for (int i = 0; i < snake.Length; i++) {
                newSnake[i + 1] = snake[i];
            }
            snake = newSnake;
            RandomizeFood();
        }
        else {
            for (int i = snake.Length - 2; i >= 0; i--) {
                snake[i + 1] = snake[i];
            }
        }

        if(snake.Length == 1) {
            switch (dir) {
                case Direction.Left:
                    snake[0].x--;
                    break;
                case Direction.Right:
                    snake[0].x++;
                    break;
                case Direction.Up:
                    snake[0].y++;
                    break;
                case Direction.Down:
                    snake[0].y--;
                    break;
            }
        }
        else {
            switch (dir) {
                case Direction.Left:
                    snake[0] = new Vector2Int(snake[1].x-1, snake[1].y);
                    break;
                case Direction.Right:
                    snake[0] = new Vector2Int(snake[1].x + 1, snake[1].y);
                    break;
                case Direction.Up:
                    snake[0] = new Vector2Int(snake[1].x, snake[1].y + 1);
                    break;
                case Direction.Down:
                    snake[0] = new Vector2Int(snake[1].x, snake[1].y - 1);
                    break;
            }
        }
        // GameOver Check
        bool borderCollision = (snake[0].x < 0 || snake[0].x > gridSize - 1 || snake[0].y < 0 || snake[0].y > gridSize - 1);
        bool selfCollision = false;

        for (int i = 1; i < snake.Length; i++) {
            if (snake[0] == snake[i]) selfCollision = true;
        }

        if (borderCollision || selfCollision) {
            GameOver();
        }
        Draw();
        Lastdir = dir;
        StartCoroutine(RunFrame(frameDuration));
    }

    private void RandomizeFood()
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>(snake);
        List<Vector2Int> availablePositions = new List<Vector2Int>();

        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                Vector2Int pos = new Vector2Int(x, y);
                if (!occupiedPositions.Contains(pos)) {
                    availablePositions.Add(pos);
                }
            }
        }

        if(availablePositions.Count == 0) {
            Debug.LogError("No available positions to spawn food!");
        }

        food = availablePositions[Random.Range(0, availablePositions.Count)];
    }

    void Draw()
    {
        // clear grid
        for (int i = 0; i < boxes.Length; i++) {

            SpriteRenderer sr = boxes[i].GetComponent<SpriteRenderer>();
            sr.color = Color.black;
        }

        // render food
        boxes[Vector2toIndex(food)].GetComponent<SpriteRenderer>().color = Color.red;

        // render snake
        for (int i = 0; i < snake.Length; i++) {
            boxes[Vector2toIndex(snake[i])].GetComponent<SpriteRenderer>().color = new Color(0, .9f, 0);
        }
        boxes[Vector2toIndex(snake[0])].GetComponent<SpriteRenderer>().color = Color.green;

    }

    void GenerateGrid()
    {
        boxes = new GameObject[gridSize * gridSize];
        GameObject grid = new GameObject("Grid");
        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                //GameObject box = Instantiate(BoxPrefab,new Vector3((x * boxSize) - (((float)gridSize/2) * boxSize) + boxSize/2,(y*boxSize) - (((float)gridSize/2)*boxSize)  + boxSize/2, 0),Quaternion.identity, grid.transform);
                int index = (x * gridSize) + y;
                GameObject box = new GameObject("Box: " + index);
                box.AddComponent<SpriteRenderer>().sprite = boxSprite;
                box.transform.position = new Vector3((x * boxSize) - (((float)gridSize / 2) * boxSize), (y * boxSize) - (((float)gridSize / 2) * boxSize), 0);
                box.transform.parent = grid.transform;
                box.transform.localScale = new Vector3(boxSize,boxSize,1);

                boxes[index] = box;
                box.name = "Box: " + index;
            }
        }
    }

    int Vector2toIndex(Vector2Int vector2) 
    {
        return (vector2.x * gridSize) + vector2.y;
    }
    public IEnumerator RunFrame(float fps)
    {
        yield return new WaitForSecondsRealtime(fps);
        GameLoop();
    }

    void GameOver()
    {
        Debug.Log("Score: " + snake.Length);
        RandomizeFood();
        snake = new Vector2Int[] { new Vector2Int(gridSize/2,gridSize/2)};
    }

    public void setSprite(Sprite sprite)
    {
        boxSprite = sprite;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(snakegame))]
public class ScriptEditor : Editor {
    public static void LoadSpriteFromPackage(snakegame script)
    {
        string assetPath = "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png";

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

        if (texture != null) {
            // Convert non-readable texture to a readable one using RenderTexture
            RenderTexture rt = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, rt); // Copy the texture to the RenderTexture
            RenderTexture.active = rt;

            // Create a new readable Texture2D
            Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = null;
            rt.Release(); // Cleanup RenderTexture

            // Create a sprite from the new readable texture
            float pixelsPerUnit = 255f;
            Sprite sprite = Sprite.Create(readableTexture, new Rect(0, 0, readableTexture.width, readableTexture.height), Vector2.zero, pixelsPerUnit);

            script.setSprite(sprite);
        }
        else {
            Debug.LogError("Failed to load sprite from package! Check the asset path.");
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        snakegame script = (snakegame)target;

        if (script.boxSprite == null) {
            if (GUILayout.Button("LoadSpriteFromPackage")) {
                LoadSpriteFromPackage(script);
            }
        }
    }
}
#endif
