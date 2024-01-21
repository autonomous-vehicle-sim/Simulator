using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;


[ExecuteInEditMode]
public class DynamicFloor : MonoBehaviour
{
    [SerializeField]
    private Material _floorMaterial;
    [SerializeField]
    private int _width = 20;
    [SerializeField]
    private int _height = 20;
    [SerializeField]
    private int _seed = 1;

    private GameObject[,] _cubes;
    private GameObject[,] _planes;
    private Cell[,] _cells;

    private int _currentID = 0;
    private (int, int)[,,,] _rules;
    private bool[,] _visitedCells;
    private const int _NUMBER_OF_DIRECTIONS = 4;
    private bool _floorGenerated = false;
    private void InitializeCellRules()
    {
        _rules = new (int, int)[2, 2, 2, 2];
        _rules[0, 0, 0, 0] = (1, 0);

        _rules[1, 0, 1, 0] = (2, 90);
        _rules[0, 1, 0, 1] = (2, 0);

        _rules[0, 0, 1, 1] = (3, 0);
        _rules[1, 0, 0, 1] = (3, 90);
        _rules[1, 1, 0, 0] = (3, 180);
        _rules[0, 1, 1, 0] = (3, 270);

        _rules[0, 1, 1, 1] = (4, 0);
        _rules[1, 0, 1, 1] = (4, 90);
        _rules[1, 1, 0, 1] = (4, 180);
        _rules[1, 1, 1, 0] = (4, 270);

        _rules[1, 1, 1, 1] = (5, 0);
    }

    private (int, int) GetCorrectCellAsset(bool left, bool up, bool right, bool down)
    {
        return _rules[System.Convert.ToInt32(left), System.Convert.ToInt32(up), System.Convert.ToInt32(right), System.Convert.ToInt32(down)];
    }

    private class Cell
    {
        private bool[] _directions = new bool[4];
        public bool Set;
        public int Id;
        public Cell()
        {
            for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
            {
                _directions[i] = false;
            }
            Set = false;
            Id = 0;
        }

        public Cell(bool left, bool up, bool right, bool down)
        {
            _directions[(int)Direction.Left] = left;
            _directions[(int)Direction.Up] = up;
            _directions[(int)Direction.Right] = right;
            _directions[(int)Direction.Down] = down;
            Set = true;
            Id = 0;
        }

        public bool GetDirection(Direction dir)
        {
            return _directions[(int)dir];
        }

        public void SetDirection(Direction dir, bool value)
        {
            _directions[(int)dir] = value;
        }

        public int GetNumberOfSetDirections()
        {
            int setDirections = 0;
            for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
                if (_directions[i])
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
            return InBounds((y, x - 1)) && _cells[y, x - 1].Set;
        if (dir == Direction.Up)
            return InBounds((y - 1, x)) && _cells[y - 1, x].Set;
        if (dir == Direction.Right)
            return InBounds((y, x + 1)) && _cells[y, x + 1].Set;
        if (dir == Direction.Down)
            return InBounds((y + 1, x)) && _cells[y + 1, x].Set;
        throw new System.Exception("Wrong coordinates");
    }

    private bool IsAdjacentCellReachable(int y, int x, Direction dir)
    {
        if (dir == Direction.Left)
            return InBounds((y, x - 1)) && _cells[y, x].GetDirection(Direction.Left);
        if (dir == Direction.Up)
            return InBounds((y - 1, x)) && _cells[y, x].GetDirection(Direction.Up);
        if (dir == Direction.Right)
            return InBounds((y, x + 1)) && _cells[y, x].GetDirection(Direction.Right);
        if (dir == Direction.Down)
            return InBounds((y + 1, x)) && _cells[y, x].GetDirection(Direction.Down);
        throw new System.Exception("Wrong coordinates");
    }

    private ref Cell GetAdjacetCell(int y, int x, Direction dir)
    {
        if (dir == Direction.Left)
            return ref _cells[y, x - 1];
        if (dir == Direction.Up)
            return ref _cells[y - 1, x];
        if (dir == Direction.Right)
            return ref _cells[y, x + 1];
        if (dir == Direction.Down)
            return ref _cells[y + 1, x];
        throw new System.Exception("Wrong coordinates");
    }

    private bool CollapseState(EntropyState state, ref int cellsToCollapse, ref int possibleCellsRemaining)
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

    private void Collapse(ref Cell cell, int y, int x)
    {
        EntropyState[] directions = new EntropyState[4];
        for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
            directions[i] = EntropyState.Allowed;

        for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
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
        for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
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

        Debug.Log(cellsToCollapse + " id: " + _currentID + " possible cells remaining: " + possibleCellsRemaining);
        for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
            cell.SetDirection((Direction)i, CollapseState(directions[i], ref cellsToCollapse, ref possibleCellsRemaining));
            
        cell.Set = true;
        cell.Id = _currentID++;

    }
    private int Entropy(int y, int x)
    {
        EntropyState[] state = new EntropyState[_NUMBER_OF_DIRECTIONS];
        for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
            state[i] = EntropyState.Allowed;

        for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
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
        for (int i = 0; i < _NUMBER_OF_DIRECTIONS; i++)
            if (state[i] == EntropyState.Allowed)
                possibleResults *= 2;

        return possibleResults;
    }

    private bool InBounds((int, int) position)
    {
        int y = position.Item1;
        int x = position.Item2;
        if (y < 0 || y >= _height)
            return false;
        if (x < 0 || x >= _width)
            return false;
        return true;
    }

    private void DFS(int y, int x)
    {
        if (_visitedCells[y, x] == true)
            return;
        _visitedCells[y, x] = true;
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

    private void SetUnreachableCellsToBlank()
    {
        _visitedCells = new bool[_height, _width];

        for (int i = 0; i < _height; i++)
            for (int j = 0; j < _width; j++)
                _visitedCells[i, j] = false;

        DFS(_height / 2, _width / 2);

        for (int i = 0; i < _height; i++)
            for (int j = 0; j < _width; j++)
                if (_visitedCells[i, j] == false)
                {
                    for(int k = 0; k < _NUMBER_OF_DIRECTIONS; k++)
                    {
                        _cells[i, j].SetDirection((Direction)k, false);
                    }
                }
    }

    private bool IsErrorInGeneration()
    {
        for (int i = 0; i < _height; i++)
            for (int j = 0; j < _width; j++)
                if (_cells[i, j].GetNumberOfSetDirections() == 1)
                {
                    return true;
                }
        return false;
    }

    private IEnumerator StartFloorGeneration()
    {
        _floorGenerated = false;
        Random.InitState(_seed);
        yield return null;
        while (!_floorGenerated)
        {
            GenerateFloor();
        }
    }

    private void GenerateFloor()
    {
        _currentID = 0;
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
            materials[i] = new Material(_floorMaterial);
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


        _cubes = new GameObject[_height, _width];
        _planes = new GameObject[_height, _width];
        _cells = new Cell[_height, _width];
        for (int i = 0; i < _height; i++)
            for (int j = 0; j < _width; j++)
                _cells[i, j] = new Cell();

        for (int i = 0; i < _height; i++)
        {
            _cells[i, 0].Set = true;
            _cells[i, _width - 1].Set = true;
        }
        for (int i = 0; i < _width; i++)
        {
            _cells[0, i].Set = true;
            _cells[_height - 1, i].Set = true;
        }
        _cells[_height / 2, _width / 2].SetDirection(Direction.Down, true);
        _cells[_height / 2, _width / 2].SetDirection(Direction.Up, true);
        _cells[_height / 2, _width / 2].Set = true;

        int idx = 0;
        while (true)
        {
            Debug.Log("id:" + idx++);
            List<(int, int)> candidates = new List<(int, int)>();
            int leastEntropy = (1 << 30);
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    if (_cells[i, j].Set)
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
            Cell randomCandidate = _cells[coordinates.Item1, coordinates.Item2];
            Collapse(ref randomCandidate, coordinates.Item1, coordinates.Item2);
        }
        SetUnreachableCellsToBlank();
        if (IsErrorInGeneration())
        {
            Debug.Log("Discarding wrongly generated floor");
            return;
        }

        for (int i = 0; i < _height; i++)
        {
            for (int j = 0; j < _width; j++)
            {
                _planes[i, j] = GameObject.CreatePrimitive(PrimitiveType.Plane);
                _planes[i, j].transform.SetParent(this.transform);
                _planes[i, j].transform.localPosition = new Vector3(i * 1, 2, j * 1);
                _planes[i, j].transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                int materialIdx = GetCorrectCellAsset(_cells[i, j].GetDirection(Direction.Left), _cells[i, j].GetDirection(Direction.Up),
                                                    _cells[i, j].GetDirection(Direction.Right), _cells[i, j].GetDirection(Direction.Down)).Item1;
                int rotation = GetCorrectCellAsset(_cells[i, j].GetDirection(Direction.Left), _cells[i, j].GetDirection(Direction.Up),
                                                    _cells[i, j].GetDirection(Direction.Right), _cells[i, j].GetDirection(Direction.Down)).Item2;
                _planes[i, j].GetComponent<MeshRenderer>().sharedMaterial = materials[materialIdx];
                _planes[i, j].transform.eulerAngles = new Vector3(0, rotation, 0);
                _planes[i, j].name = "Floor tile";
            }
        }
        _floorGenerated = true;
    }

    // Start is called before the first frame update
    private void Start()
    {

    }

    private void OnValidate()
    {
        InitializeCellRules();
        StartCoroutine(nameof(StartFloorGeneration));
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
