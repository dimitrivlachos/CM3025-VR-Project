/*
 * 
 * Code by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_FacePlayer : MonoBehaviour
{
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        TurnToCamera();
    }

    void TurnToCamera()
    {
        // Get the direction to look
        Vector3 v = cam.transform.position - transform.position;
        // Cancel the other directions of rotation
        v.x = v.z = 0.0f;

        // Turn to the camera
        transform.LookAt(cam.transform.position - v);
        transform.Rotate(0, 180, 0);
    }
}
