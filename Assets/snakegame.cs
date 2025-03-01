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

    GameObject[] boxes;
    float boxSize;
    Vector2Int[] snake;
    Vector2Int food;
    Direction dir;
    Direction Lastdir;
    //Sound
    AudioSource audioSource;
    AudioClip gameOverClip;
    float gameOverClipTime = 1;
    AudioClip foodClip;
    float foodClipTime = 1;
    AudioClip gameStartClip;

    public enum Direction
    {
        Left,Right,Up,Down
    }

    private void Awake()
    {
        //initialize
        audioSource = gameObject.AddComponent<AudioSource>();
        gameOverClip = Resources.Load<AudioClip>("gameOver");
        foodClip = Resources.Load<AudioClip>("food");
        gameStartClip = Resources.Load<AudioClip>("gameStart");
        gameOverClipTime = gameOverClip.length;
        foodClipTime = foodClip.length;
        Lastdir = dir;
        snake = new Vector2Int[] { new Vector2Int(gridSize / 2, gridSize / 2) };
        boxSize = 2f /(float)gridSize;
        GenerateGrid();
        RandomizeFood(sound:false);
    }

    void Start()
    {
        PlayAudio(gameStartClip);
        GameLoop();
    }

    void Update()
    {
        if (snake.Length > 1) {
            if (Input.GetKey(KeyCode.W)) {
                if (Lastdir != Direction.Down){dir = Direction.Up;}else { dir = Direction.Left; }}
            else if (Input.GetKey(KeyCode.A)) {
                if (Lastdir != Direction.Right){dir = Direction.Left;}else { dir = Direction.Up; }}
            else if (Input.GetKey(KeyCode.S)) {
                if (Lastdir != Direction.Up){dir = Direction.Down;}else { dir = Direction.Right; }}
            else if (Input.GetKey(KeyCode.D)) {
                if (Lastdir != Direction.Left) {dir = Direction.Right;}else { dir = Direction.Down; }}
        }
        else {
            // No restriction for single size snake.
            if (Input.GetKey(KeyCode.W)) dir = Direction.Up;
            else if (Input.GetKey(KeyCode.A)) dir = Direction.Left;
            else if (Input.GetKey(KeyCode.S)) dir = Direction.Down;
            else if (Input.GetKey(KeyCode.D)) dir = Direction.Right;
        }
    }

    public void GameLoop()
    {
        if (snake[0] == food) {
            StopAllCoroutines();
            RandomizeFood();
            Vector2Int[] newSnake = new Vector2Int[snake.Length + 1];
            for (int i = 0; i < snake.Length; i++) {
                newSnake[i + 1] = snake[i];
            }
            snake = newSnake;
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
            return;
        }

        Draw();
        Lastdir = dir;
        StartCoroutine(RunFrame(frameDuration));
    }

    private void RandomizeFood(bool sound = true,bool anim = true)
    {
        if(sound) PlayAudio(foodClip);
        
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
        MeshRenderer food_mr = boxes[Vector2toIndex(food)].GetComponent<MeshRenderer>();
        if (anim) {
            food_mr.material.color = Color.white;
            StartCoroutine(ChangeColorOverTime(food_mr, Color.red, foodClipTime*4/5));
        }
    }

    void Draw()
    {
        // clear grid
        for (int i = 0; i < boxes.Length; i++) {

            MeshRenderer box_mr = boxes[i].GetComponent<MeshRenderer>();
            box_mr.material.color = Color.black;
            box_mr.material.SetColor("_EmissionColor", Color.black);
        }

        // render food
        MeshRenderer food_mr = boxes[Vector2toIndex(food)].GetComponent<MeshRenderer>();
        food_mr.material.color = Color.red;
        food_mr.material.EnableKeyword("_EMISSION");
        food_mr.material.SetColor("_EmissionColor", Color.red);

        // render snake
        Color HeadColor = new Color(0, .9f, 0);
        for (int i = 0; i < snake.Length; i++) {
            if (i == 0) {
                MeshRenderer head_mr = boxes[Vector2toIndex(snake[0])].GetComponent<MeshRenderer>();
                //StartCoroutine(ChangeColorOverTime(head_mr, Color.green, frameDuration/2));
                head_mr.material.color = HeadColor;
                head_mr.material.EnableKeyword("_EMISSION");
                head_mr.material.SetColor("_EmissionColor", HeadColor);
                continue;
            }
            MeshRenderer snake_mr = boxes[Vector2toIndex(snake[i])].GetComponent<MeshRenderer>();
            //StartCoroutine(ChangeColorOverTime(snake_mr, Color.green, (frameDuration*2/3) / (snake.Length-i)));
            snake_mr.material.color = Color.green;
            snake_mr.material.EnableKeyword("_EMISSION");
            snake_mr.material.SetColor("_EmissionColor", Color.green);
        }
    }

    void GenerateGrid()
    {
        boxes = new GameObject[gridSize * gridSize];
        GameObject grid = new GameObject("Grid");
        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                int index = (x * gridSize) + y;
                Vector3 position = new Vector3((x * boxSize) - (((float)gridSize / 2) * boxSize), (y * boxSize) - (((float)gridSize / 2) * boxSize), 0);
                GameObject box = CreateSquare(position,boxSize);
                box.transform.parent = grid.transform;
                boxes[index] = box;
            }
        }
    }

    int Vector2toIndex(Vector2Int vector2) 
    {
        return (vector2.x * gridSize) + vector2.y;
    }
    public IEnumerator RunFrame(float fps = 0.14f)
    {
        yield return new WaitForSeconds(fps);
        GameLoop();
    }

    void GameOver()
    {
        RandomizeFood(false,false);
        audioSource.Stop();
        ScreenFade(gameOverClipTime*2/3);
        PlayAudio(gameOverClip);
        snake = new Vector2Int[] { new Vector2Int(gridSize/2,gridSize/2)};
        StartCoroutine(RunFrame(gameOverClipTime));
    }

    public static GameObject CreateSquare(Vector3 position, float size = 0.08f, Material material = null)
    {
        GameObject square = new GameObject("Square", typeof(MeshFilter), typeof(MeshRenderer));
        square.transform.position = position;

        Mesh mesh = new Mesh();
        Vector3[] vertices = {
        new Vector3(0, 0, 0),  // Bottom-left
        new Vector3(size, 0, 0),  // Bottom-right
        new Vector3(0, size, 0),  // Top-left
        new Vector3(size, size, 0)  // Top-right
        };
        int[] triangles = { 0, 2, 1, 1, 2, 3 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        square.GetComponent<MeshFilter>().mesh = mesh;

        MeshRenderer renderer = square.GetComponent<MeshRenderer>();

        if (material != null) {
            renderer.material = material; // Use the provided material
        }
        else {
            renderer.material = new Material(Shader.Find("Standard")); // Default material
            renderer.material.color = Color.white; // Default color
        }

        return square;
    }

    public static IEnumerator ChangeColorOverTime(MeshRenderer renderer, Color targetColor, float duration)
    {
        if (renderer == null) yield break;

        Material material = renderer.material;
        Color startColor = material.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            material.color = Color.Lerp(startColor, targetColor, t);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.Lerp(startColor, targetColor, t));

            yield return null;
        }

        material.color = targetColor; // Ensure final color is exact
    }
    public void ScreenFade(float duration)
    {
        for (int i = 0; i < boxes.Length; i++) {
            MeshRenderer box_mr = boxes[i].GetComponent<MeshRenderer>();
            StartCoroutine(ChangeColorOverTime(box_mr, Color.black, duration));
        }
    }

    public void PlayAudio(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(snakegame))]
public class ScriptEditor : Editor {
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        snakegame script = (snakegame)target;
    }
}
#endif
