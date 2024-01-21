using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;


[ExecuteInEditMode]
public class DynamicFloor : MonoBehaviour
{
    [SerializeField]
    private Material FloorMaterial;
    [SerializeField]
    private int width = 20;
    [SerializeField]
    private int height = 20;
    [SerializeField]
    private int seed = 1;

    private GameObject[,] cubes;
    private GameObject[,] planes;
    private Cell[,] cells;

    private int currentID = 0;
    private (int, int)[,,,] rules;
    private bool[,] visitedCells;
    private const int numberOfDirections = 4;
    private bool floorGenerated = false;
    private void InitializeCellRules()
    {
        rules = new (int, int)[2, 2, 2, 2];
        rules[0, 0, 0, 0] = (1, 0);

        rules[1, 0, 1, 0] = (2, 90);
        rules[0, 1, 0, 1] = (2, 0);

        rules[0, 0, 1, 1] = (3, 0);
        rules[1, 0, 0, 1] = (3, 90);
        rules[1, 1, 0, 0] = (3, 180);
        rules[0, 1, 1, 0] = (3, 270);

        rules[0, 1, 1, 1] = (4, 0);
        rules[1, 0, 1, 1] = (4, 90);
        rules[1, 1, 0, 1] = (4, 180);
        rules[1, 1, 1, 0] = (4, 270);

        rules[1, 1, 1, 1] = (5, 0);
    }

    private (int, int) GetCorrectCellAsset(bool left, bool up, bool right, bool down)
    {
        return rules[System.Convert.ToInt32(left), System.Convert.ToInt32(up), System.Convert.ToInt32(right), System.Convert.ToInt32(down)];
    }

    private class Cell
    {
        public bool[] directions = new bool[4];
        public bool set;
        public int id;
        public Cell()
        {
            for (int i = 0; i < numberOfDirections; i++)
            {
                directions[i] = false;
            }
            set = false;
            id = 0;
        }

        public Cell(bool left, bool up, bool right, bool down)
        {
            directions[(int)Direction.Left] = left;
            directions[(int)Direction.Up] = up;
            directions[(int)Direction.Right] = right;
            directions[(int)Direction.Down] = down;
            set = true;
            id = 0;
        }

        public bool GetDirection(Direction dir)
        {
            return directions[(int)dir];
        }

        public void SetDirection(Direction dir, bool value)
        {
            directions[(int)dir] = value;
        }

        public int GetNumberOfSetDirections()
        {
            int setDirections = 0;
            for (int i = 0; i < numberOfDirections; i++)
                if (directions[i])
                    setDirections++;
            return setDirections;
        }
    }

    private enum EntropyState
    {
        Impossible,
        Necessary,
        Allowed
    }

    private enum Direction
    {
        Left,
        Up,
        Right,
        Down
    }

    private bool IsAdjacentCellSet(int y, int x, Direction dir)
    {
        if (dir == Direction.Left)
            return InBounds((y, x - 1)) && cells[y, x - 1].set;
        if (dir == Direction.Up)
            return InBounds((y - 1, x)) && cells[y - 1, x].set;
        if (dir == Direction.Right)
            return InBounds((y, x + 1)) && cells[y, x + 1].set;
        if (dir == Direction.Down)
            return InBounds((y + 1, x)) && cells[y + 1, x].set;
        throw new System.Exception("Wrong coordinates");
    }

    private bool IsAdjacentCellReachable(int y, int x, Direction dir)
    {
        if (dir == Direction.Left)
            return InBounds((y, x - 1)) && cells[y, x].GetDirection(Direction.Left);
        if (dir == Direction.Up)
            return InBounds((y - 1, x)) && cells[y, x].GetDirection(Direction.Up);
        if (dir == Direction.Right)
            return InBounds((y, x + 1)) && cells[y, x].GetDirection(Direction.Right);
        if (dir == Direction.Down)
            return InBounds((y + 1, x)) && cells[y, x].GetDirection(Direction.Down);
        throw new System.Exception("Wrong coordinates");
    }

    private ref Cell GetAdjacetCell(int y, int x, Direction dir)
    {
        if (dir == Direction.Left)
            return ref cells[y, x - 1];
        if (dir == Direction.Up)
            return ref cells[y - 1, x];
        if (dir == Direction.Right)
            return ref cells[y, x + 1];
        if (dir == Direction.Down)
            return ref cells[y + 1, x];
        throw new System.Exception("Wrong coordinates");
    }

    bool collapseState(EntropyState state, ref int cellsToCollapse, ref int possibleCellsRemaining)
    {
        if (state == EntropyState.Impossible)
            return false;
        if (state == EntropyState.Necessary)
            return true;
        if (cellsToCollapse > 0 && (float)Random.Range(0, 101) * cellsToCollapse <= (float)possibleCellsRemaining * 100)
        {
            cellsToCollapse--;
            possibleCellsRemaining--;
            return true;
        }
        possibleCellsRemaining--;
        return false;
    }

    private Direction GetOppositeDirection(Direction dir)
    {
        if (dir == Direction.Left)
            return Direction.Right;
        if (dir == Direction.Up)
            return Direction.Down;
        if (dir == Direction.Right)
            return Direction.Left;
        if (dir == Direction.Down)
            return Direction.Up;
        throw new System.Exception("Unexpected direction");
    }

    void collapse(ref Cell cell, int y, int x)
    {
        EntropyState[] directions = new EntropyState[4];
        for (int i = 0; i < numberOfDirections; i++)
            directions[i] = EntropyState.Allowed;

        for (int i = 0; i < numberOfDirections; i++)
        {
            Direction dir = (Direction)i;
            if (IsAdjacentCellSet(y, x, dir))
            {
                if (GetAdjacetCell(y, x, dir).GetDirection(GetOppositeDirection(dir)))
                    directions[i] = EntropyState.Necessary;
                else
                    directions[i] = EntropyState.Impossible;
            }
        }

        int possibleCellsRemaining = 0;
        int setDirections = 0;
        for (int i = 0; i < numberOfDirections; i++)
        {
            if (directions[i] == EntropyState.Allowed)
                possibleCellsRemaining++;
            if (directions[i] == EntropyState.Necessary)
                setDirections++;
        }

        int cellsToCollapse = System.Math.Max(Random.Range(0, possibleCellsRemaining + 16) - 15, 0);
        if ((cellsToCollapse + setDirections) == 1 && possibleCellsRemaining >= 1)
            cellsToCollapse = 2 - setDirections;
        else if ((cellsToCollapse + setDirections) == 1)
            cellsToCollapse = 0;

        Debug.Log(cellsToCollapse + " id: " + currentID + " possible cells remaining: " + possibleCellsRemaining);
        for (int i = 0; i < numberOfDirections; i++)
            cell.SetDirection((Direction)i, collapseState(directions[i], ref cellsToCollapse, ref possibleCellsRemaining));
            
        cell.set = true;
        cell.id = currentID++;

    }
    int Entropy(int y, int x)
    {
        EntropyState[] state = new EntropyState[numberOfDirections];
        for (int i = 0; i < numberOfDirections; i++)
            state[i] = EntropyState.Allowed;

        for (int i = 0; i < numberOfDirections; i++)
        {
            Direction dir = (Direction)i;
            if (IsAdjacentCellSet(y, x, dir))
            {
                if (GetAdjacetCell(y, x, dir).GetDirection(GetOppositeDirection(dir)))
                    state[i] = EntropyState.Necessary;
                else
                    state[i] = EntropyState.Impossible;
            }
        }

        int possibleResults = 1;
        for (int i = 0; i < numberOfDirections; i++)
            if (state[i] == EntropyState.Allowed)
                possibleResults *= 2;

        return possibleResults;
    }

    bool InBounds((int, int) position)
    {
        int y = position.Item1;
        int x = position.Item2;
        if (y < 0 || y >= height)
            return false;
        if (x < 0 || x >= width)
            return false;
        return true;
    }

    private void DFS(int y, int x)
    {
        if (visitedCells[y, x] == true)
            return;
        visitedCells[y, x] = true;
        if (IsAdjacentCellReachable(y, x, Direction.Left))
        {
            DFS(y, x - 1);
        }
        if (IsAdjacentCellReachable(y, x, Direction.Right))
        {
            DFS(y, x + 1);
        }
        if (IsAdjacentCellReachable(y, x, Direction.Up))
        {
            DFS(y - 1, x);
        }
        if (IsAdjacentCellReachable(y, x, Direction.Down))
        {
            DFS(y + 1, x);
        }
    }

    void SetUnreachableCellsToBlank()
    {
        visitedCells = new bool[height, width];

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                visitedCells[i, j] = false;

        DFS(height / 2, width / 2);

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                if (visitedCells[i, j] == false)
                {
                    for(int k = 0; k < numberOfDirections; k++)
                    {
                        cells[i, j].SetDirection((Direction)k, false);
                    }
                }
    }

    private bool IsErrorInGeneration()
    {
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                if (cells[i, j].GetNumberOfSetDirections() == 1)
                {
                    return true;
                }
        return false;
    }

    IEnumerator StartFloorGeneration()
    {
        floorGenerated = false;
        Random.InitState(seed);
        yield return null;
        while (!floorGenerated)
        {
            GenerateFloor();
        }
    }

    void GenerateFloor()
    {
        currentID = 0;
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            DestroyImmediate(child);
        }

        const int materialArraySize = 6;
        Material[] materials = new Material[materialArraySize];
        Texture2D[] textures = new Texture2D[materialArraySize];
        for (int i = 0; i < materialArraySize; i++)
        {
            materials[i] = new Material(FloorMaterial);
            textures[i] = new Texture2D(512, 512);
        }


        textures[0].LoadImage(File.ReadAllBytes("Assets/Floor/error.png"));
        textures[1].LoadImage(File.ReadAllBytes("Assets/Floor/blank.png"));
        textures[2].LoadImage(File.ReadAllBytes("Assets/Floor/straight_line.png"));
        textures[3].LoadImage(File.ReadAllBytes("Assets/Floor/curved_line.png"));
        textures[4].LoadImage(File.ReadAllBytes("Assets/Floor/t_line.png"));
        textures[5].LoadImage(File.ReadAllBytes("Assets/Floor/all_directions.png"));

        for (int i = 0; i < materialArraySize; i++)
        {
            textures[i].wrapMode = TextureWrapMode.Clamp;
            materials[i].mainTexture = textures[i];
        }


        cubes = new GameObject[height, width];
        planes = new GameObject[height, width];
        cells = new Cell[height, width];
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                cells[i, j] = new Cell();

        for (int i = 0; i < height; i++)
        {
            cells[i, 0].set = true;
            cells[i, width - 1].set = true;
        }
        for (int i = 0; i < width; i++)
        {
            cells[0, i].set = true;
            cells[height - 1, i].set = true;
        }
        cells[height / 2, width / 2].SetDirection(Direction.Down, true);
        cells[height / 2, width / 2].SetDirection(Direction.Up, true);
        cells[height / 2, width / 2].set = true;

        int idx = 0;
        while (true)
        {
            Debug.Log("id:" + idx++);
            List<(int, int)> candidates = new List<(int, int)>();
            int leastEntropy = (1 << 30);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (cells[i, j].set)
                        continue;
                    int currentEntropy = Entropy(i, j);
                    if (currentEntropy < leastEntropy)
                    {
                        leastEntropy = currentEntropy;
                        candidates.Clear();
                    }
                    if (currentEntropy == leastEntropy)
                        candidates.Add((i, j));
                }
            }
            if (candidates.Count == 0)
                break;
            int randomCandidateIndex = Random.Range(0, candidates.Count);
            (int, int) coordinates = candidates[randomCandidateIndex];
            Cell randomCandidate = cells[coordinates.Item1, coordinates.Item2];
            collapse(ref randomCandidate, coordinates.Item1, coordinates.Item2);
        }
        SetUnreachableCellsToBlank();
        if (IsErrorInGeneration())
        {
            Debug.Log("Discarding wrongly generated floor");
            return;
        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                planes[i, j] = GameObject.CreatePrimitive(PrimitiveType.Plane);
                planes[i, j].transform.SetParent(this.transform);
                planes[i, j].transform.localPosition = new Vector3(i * 1, 2, j * 1);
                planes[i, j].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                int materialIdx = GetCorrectCellAsset(cells[i, j].GetDirection(Direction.Left), cells[i, j].GetDirection(Direction.Up),
                                                    cells[i, j].GetDirection(Direction.Right), cells[i, j].GetDirection(Direction.Down)).Item1;
                int rotation = GetCorrectCellAsset(cells[i, j].GetDirection(Direction.Left), cells[i, j].GetDirection(Direction.Up),
                                                    cells[i, j].GetDirection(Direction.Right), cells[i, j].GetDirection(Direction.Down)).Item2;
                planes[i, j].GetComponent<MeshRenderer>().sharedMaterial = materials[materialIdx];
                planes[i, j].transform.eulerAngles = new Vector3(0, rotation, 0);
                planes[i, j].name = "Floor tile";
            }
        }
        floorGenerated = true;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void OnValidate()
    {
        InitializeCellRules();
        StartCoroutine(nameof(StartFloorGeneration));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
