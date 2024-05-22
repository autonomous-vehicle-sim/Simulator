using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;



[ExecuteInEditMode]
public class DynamicFloor : MonoBehaviour
{
    [SerializeField]
    private int _seed = 1;
    [SerializeField]
    private int _numberOfPoints = 1;

    [SerializeField]
    private bool _addDisplacement = true;

    [SerializeField]
    private float _difficulty = 1f;
    [SerializeField]
    private float _maxDisp = 0.10f;
    [SerializeField]
    private float _pushApartDistance = 0.25f;

    public bool startFloorGeneration = false;
    public bool isMapReady { get; private set; }

    private GameObject[] points;
    private (float x, float y)[] pointsCoordinates;
    public void SetSeed(int seed)
    {
        _seed = seed;
    }

    public void Generate()
    {
        MeshFilter meshFilter = gameObject.GetComponentInChildren<SplineExtrude>().gameObject.GetComponent<MeshFilter>();
        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = new Mesh();
        }
        isMapReady = false;
        startFloorGeneration = false;
        StartCoroutine(StartFloorGeneration());
    }

    private bool Orientation((float x, float y) p1, (float x, float y) p2, (float x, float y) p3)
    {
        float value = (p2.y - p1.y) * (p3.x - p2.x) - (p2.x - p1.x) * (p3.y - p2.y);
        return value < 0;
    }

    private void PushApart(ref (float x, float y)[] pointsCoordinates)
    {
        float squaredDistance = _pushApartDistance * _pushApartDistance;
        for(int i = 0; i < pointsCoordinates.Length; i++)
        {
            for(int j = i + 1; j < pointsCoordinates.Length; j++)
            {
                float distanceBetweenPoints = Mathf.Pow(pointsCoordinates[i].x - pointsCoordinates[j].x, 2);
                distanceBetweenPoints += Mathf.Pow(pointsCoordinates[i].y - pointsCoordinates[j].y, 2);
                if (distanceBetweenPoints < squaredDistance)
                {
                    float diffX = pointsCoordinates[j].x - pointsCoordinates[i].x;
                    float diffY = pointsCoordinates[j].y - pointsCoordinates[i].y;
                    float currentDistance = Mathf.Sqrt(diffX * diffX + diffY * diffY);
                    diffX /= currentDistance;
                    diffY /= currentDistance;
                    float diff = _pushApartDistance - currentDistance;
                    diffX *= diff;
                    diffY *= diff;
                    pointsCoordinates[j].x += diffX;
                    pointsCoordinates[i].x -= diffX;
                    pointsCoordinates[j].y += diffY;
                    pointsCoordinates[i].y -= diffY;
                }
            }
        }
    }

    private void ConvexHull((float x, float y)[] pointsCoordinates, ref List<int> result)
    {
        result.Clear();
        int leftMost = 0;
        for (int i = 0; i < _numberOfPoints; i++)
        {
            if (pointsCoordinates[leftMost].x > pointsCoordinates[i].x)
                leftMost = i;
        }
        int p = leftMost, q;

        int counter = 0;
        do
        {
            counter++;
            result.Add(p);
            q = (p + 1) % _numberOfPoints;
            for (int i = 0; i < _numberOfPoints; i++)
            {
                if (Orientation(pointsCoordinates[p], pointsCoordinates[i], pointsCoordinates[q]))
                    q = i;
            }
            p = q;
        } while (p != leftMost && counter < 10000);
        if (counter > 10000)
            throw new System.Exception("Convex hull took too long to generate");
    }

    private IEnumerator StartFloorGeneration()
    {
        if (_numberOfPoints < 3)
            throw new System.Exception("Not enough points to create track");
        yield return null;
        Random.InitState(_seed);

        points = new GameObject[_numberOfPoints];
        pointsCoordinates = new (float x, float y)[_numberOfPoints];


        for (int i = 0; i < _numberOfPoints; i++)
        {
            //Range [0, 1] to [0.1, 0.9] without changing distribution:
            //[x, (1 + x) / y] = [0.1, 0.9] => x = 0.125, y = 1.25

            float x = (Random.value + 0.125f) / 1.25f - 0.5f; 
            float y = (Random.value + 0.125f) / 1.25f - 0.5f;
            pointsCoordinates[i] = (x, y);
        }


        List<int> convexHullPoints = new List<int>();
        ConvexHull(pointsCoordinates, ref convexHullPoints);

        (float x, float y)[] trackCoordinates = new (float x, float y)[convexHullPoints.Count];

        for(int i = 0; i < convexHullPoints.Count; i++)
        {
            trackCoordinates[i] = (pointsCoordinates[convexHullPoints[i]].x, pointsCoordinates[convexHullPoints[i]].y);
        }

        for (int i = 0; i < 5; i++)
        {
            PushApart(ref trackCoordinates);
        }

        (float x, float y)[] displacedPoints = new (float x, float y)[convexHullPoints.Count];
        for (int i = 0; i < convexHullPoints.Count; i++)
        {
            float dispLen = Mathf.Pow(Random.value, _difficulty) * _maxDisp;
            float middleX = (trackCoordinates[i].x + trackCoordinates[(i + 1) % convexHullPoints.Count].x) / 2;
            float middleY = (trackCoordinates[i].y + trackCoordinates[(i + 1) % convexHullPoints.Count].y) / 2;
            Vector3 v = Quaternion.AngleAxis(Random.value * 360, Vector3.forward) * new Vector3(dispLen, 0, 0) + new Vector3(middleX, 0, middleY);
            displacedPoints[i] = (v.x, v.z);
        }

        if (_addDisplacement)
        {
            (float x, float y)[] tempCoordinates = trackCoordinates;
            trackCoordinates = new (float x, float y)[convexHullPoints.Count * 2];
            for (int i = 0; i < convexHullPoints.Count; i++)
            {
                float x = tempCoordinates[i].x;
                float y = tempCoordinates[i].y;
                trackCoordinates[i * 2] = (x, y);
                x = displacedPoints[i].x;
                y = displacedPoints[i].y;
                trackCoordinates[i * 2 + 1] = (x, y);
            }
        }
        for (int i = 0; i < 5; i++)
        {
            PushApart(ref trackCoordinates);
        }
        SplineContainer spline = gameObject.GetComponentInChildren<SplineContainer>();
        spline.Spline.Clear();
        spline.Spline.SetTangentMode(TangentMode.AutoSmooth);
        for (int i = 0; i < trackCoordinates.Length; i++)
        {
            float x = trackCoordinates[i].x;
            float y = trackCoordinates[i].y;
            spline.Spline.Add(new BezierKnot(new Unity.Mathematics.float3(x, 0.51f, y), 0, 0));
        }
        spline.Spline.SetTangentMode(TangentMode.AutoSmooth);
        spline.Spline.Closed = true;

        gameObject.GetComponentInChildren<SplineExtrude>().Rebuild();
        isMapReady = true;
    }

    // Start is called before the first frame update
    private void Start()
    {
        isMapReady = false;
        MeshFilter meshFilter = gameObject.GetComponentInChildren<SplineExtrude>().gameObject.GetComponent<MeshFilter>();
        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = new Mesh();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (startFloorGeneration)
            Generate();
    }

}
