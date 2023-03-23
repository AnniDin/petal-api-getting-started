using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public GameObject Camera;
    public MainManager mainManager;

    private float distanceFromCarToCamera = 10f;
    private float cameraHeight = 1f;

    private float acceleration = 5;
    private Vector3 forward = new Vector3(0, 0, 1);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Camera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraHeight, transform.position.z - distanceFromCarToCamera);

        if (Input.GetKey("w"))
        {
            transform.Translate(mainManager.getVelocity() * forward * acceleration * Time.deltaTime);
        }
    }
}
