using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ExpansionAlgorithmPhase { Spawning, Extending, Reducing }
public class ExpansionAlgorithm : MonoBehaviour
{
    public ExpansionAlgorithmPhase eap = ExpansionAlgorithmPhase.Spawning;
    public List<Vpoint> points = new List<Vpoint>();
    public int size = 10000;
    public int pointsPerFrame;
    public float minDistance = 2f;
    public float scaller = 1;
    float minDistanceSqrt;
    float thresholdDistanceSqrt;
    public int iterationsPerFrame = 1;
    public float t0 = 1;
    public float t1 = 1;
    public float t2 = 1;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        //algorithm dont work if minDistance<1
        while (minDistance < 1)
        {
            minDistance *= 10;
            scaller /= 10;
        }
        Vpoint.dist = minDistance * 6;
        //yield return new WaitForSeconds(t0);
        thresholdDistanceSqrt = minDistance * minDistance * 0.9f;
        minDistanceSqrt = minDistance * minDistance;

        for (int i = 0; i < size; i++)
        {
            //int overflow = 100;
            Vpoint targetVector = randomUnitVpoint();
            while (targetVector.v3.sqrMagnitude > 1)
                targetVector = randomUnitVpoint();

            points.Add(targetVector);
            if (i % pointsPerFrame == pointsPerFrame - 1 && i < 100)
                yield return new WaitForEndOfFrame();
        }
        eap = ExpansionAlgorithmPhase.Extending;
        //yield return new WaitForSeconds(t1);


        bool anyModified = true;
        int mods = iterationsPerFrame;
        int iteration = 0;
        while (anyModified)
        {
            iteration++;
            float forceFactor = Mathf.Pow(0.01f, 1 / iteration);
            anyModified = false;
            for (int i = 0; i < points.Count; i++)
                for (int j = 0; j < points.Count; j++)
                {
                    Vpoint v1 = points[i];
                    Vpoint v2 = points[j];
                    //calculate the sqrtDistance
                    float sqrtDist = v1.sqrtDistanceTo(v2); // distance is 10, float = 100
                    if (sqrtDist < thresholdDistanceSqrt && sqrtDist > 0.001)
                    {
                        anyModified = true;
                        float realDist = Mathf.Sqrt(sqrtDist);
                        float halfRealDelta = (realDist - minDistance) / 2;
                        //halfRealDelta = Mathf.Clamp(halfRealDelta, -0.1f, 0.1f);
                        v1.move((v2.v3 - v1.v3).normalized * halfRealDelta);
                        v2.move((v1.v3 - v2.v3).normalized * halfRealDelta);
                    }
                }
            points.ForEach(p => p.Apply());
            mods--;
            if (mods == 0)
            {
                mods = iterationsPerFrame;
                //yield return new WaitForEndOfFrame();
            }
        }
        eap = ExpansionAlgorithmPhase.Spawning;
        //yield return new WaitForSeconds(t2);

        points.ForEach(p => SpawnNagrobek(p.v3.X0Z(Random.Range(-2, 0.0f))));

    }
    public GameObject Nagrobek;
    GameObject nagrobekParent;
    public void SpawnNagrobek(Vector3 position)
    {
        if (!nagrobekParent)
            nagrobekParent = new GameObject("nagrobki");
        int pattern = Random.Range(1, 5);
        GameObject bob = Instantiate(Nagrobek, nagrobekParent.transform);
        for (int i = 1; i < 5; i++)
        {
            if (i != pattern)
                bob.transform.GetChild(i).gameObject.SetActive(false);
            else
                bob.transform.GetChild(i).gameObject.SetActive(true);

        }
        bob.transform.position = position;
        bob.transform.rotation = Quaternion.Euler(Random.Range(-20, 20.0f), Random.Range(-20, 20.0f), Random.Range(-20, 20.0f));
    }
    static Vpoint randomUnitVpoint()
    {
        return new Vpoint() { v3 = new Vector3(Random.Range(-1.0f, 1), 0, Random.Range(-1.0f, 1)) };
    }

    // Update is called once per frame
    void Update()
    {

    }
    [Range(0.1f, 2)]
    public float radius = 1;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        points.ForEach(p => Gizmos.DrawSphere(p.v3, radius));
    }
}
[System.Serializable]
public class Vpoint
{
    public Vector3 v3;
    public Vector3 delta;
    public static float dist = 1f;
    public void Apply()
    {
        if (delta.magnitude < dist)
            v3 += delta;
        else
            v3 += delta.normalized * dist;
        delta = new Vector3();
    }
    public float sqrtDistanceTo(Vpoint other)
    {
        return (other.v3 - this.v3).sqrMagnitude;
    }
    public void move(Vector3 delta)
    {
        //if (this.delta.sqrMagnitude > 0.001)
        //    return;
        this.delta += delta;
    }
}

/*
 10
8
if(sqrtDist<sqrtDistMin)
if 64<100 - true

1/10
1/8
if 1/64<1/100 -> false

To make algorithm word minDist must be greater or eq than 1
 */

static class V3Ext
{
    public static Vector3 X0Z(this Vector3 v3, float y = 0)
    {
        return new Vector3(v3.x, y, v3.z);
    }
}