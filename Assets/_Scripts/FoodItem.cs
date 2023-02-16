/*
 * Code adapted from:
 *      https://answers.unity.com/questions/52664/how-would-one-calculate-a-3d-mesh-volume-in-unity.html
 * by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    public enum FoodType { Carrot, Cheese, Potato, Other };
    public FoodType type;

    [Header("Nutritional information")]
    public float weight;
    public float density;
    public float calories;
    public float protein;
    public float carbohydrates;
    public float sugar;
    public float fibre;
    public float fat;
    public float modifier;

    [Header("Properties")]
    public float lockTimer = 3;

    public bool hasBeenCut = false;
    public bool inPot = false;
    

    // Start is called before the first frame update
    void Start()
    {
        float volume;

        switch (tag)
        { // Nutrition per 100g
            case "Carrot":
                modifier = 2600f;
                volume = MeshVolume();
                type= FoodType.Carrot;
                density = 1.04f; // g/cm3     d = g/v  g = d*v
                calories = 41f;
                protein = 0.9f;
                carbohydrates = 9.6f;
                sugar = 4.7f;
                fibre = 2.8f;
                fat = 0.2f;
                break;
            case "Cheese":
                modifier = 232f;
                volume = MeshVolume();
                type = FoodType.Cheese;
                density = 1.075f; // g/cm3
                calories = 404f;
                protein = 23f;
                carbohydrates = 3.1f;
                sugar = 0.5f;
                fibre = 0f;
                fat = 33f;
                break;
            case "Potato":
                modifier = 1300f;
                volume = MeshVolume();
                type = FoodType.Potato;
                density = 1.09f; // g/cm3
                calories = 93f;
                protein = 2.5f;
                carbohydrates = 21f;
                sugar = 1.2f;
                fibre = 2.2f;
                fat = 0.1f;
                break;
            default:
                //Debug.Log("In the default");
                modifier = 0f;
                volume = 0f;
                density = 0f;
                calories = 0f;
                protein = 0f;
                carbohydrates = 0f;
                sugar = 0f;
                fibre = 0f;
                fat = 0f;
                break;
        }

        // Guard statement to prevent unecessary calculations
        if (tag.Equals("Other")) return;

        weight = density * volume;
        float proportion = weight / 100f; // Per 100g
        calories *= proportion;
        protein *= proportion;
        carbohydrates *= proportion;
        sugar *= proportion;
        fibre *= proportion;
        fat *= proportion;
    }

    private float MeshVolume()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        float volume = VolumeOfMesh(mesh);// * modifier; // Convert to cm3
        volume *= modifier;
        string msg = "The volume of the mesh is " + volume + " cm3.";
        //Debug.Log(msg);
        return volume;
    }

    private float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;

        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    private float VolumeOfMesh(Mesh mesh)
    {
        float volume = 0;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);
        }
        return Mathf.Abs(volume);
    }

    private Coroutine c;

    public void StartCountdowm(GameObject p)
    {
        Debug.Log("Countdown begun");
        c = StartCoroutine(LockInPotCountdown(p));
    }

    public void EndCountDown()
    {
        StopCoroutine(c);
    }

    private IEnumerator LockInPotCountdown(GameObject p)
    {
        yield return new WaitForSeconds(lockTimer);
        transform.parent = p.transform;
    }
}
