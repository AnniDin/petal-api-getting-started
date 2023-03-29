using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public GameObject Camera;
    public GameObject enviromentPrefab;
    public MainManager mainManager;
    public TextMeshProUGUI velocityText;

    private float distanceFromCarToCamera = 10f;
    private float cameraHeight = 1f;

    private float acceleration = 1;
    private Vector3 forward = new Vector3(0, 0, 1);
    private float timeToActualizeVelocity = 1;
    private float timer = 0;
    private float averageVelocity = 0;

    private List<float> velocityList = new List<float>();

    private GameObject firstEnviroment;
    private GameObject previousEnviroment;
    private GameObject actualEnviroment;

    private float terrainWidth;

    // Start is called before the first frame update
    void Start()
    {
        firstEnviroment = GameObject.Find("enviroment");
        previousEnviroment = firstEnviroment;
        actualEnviroment = firstEnviroment;

        terrainWidth = GameObject.Find("Terrain").gameObject.GetComponent<Terrain>().terrainData.size.x;

        velocityList = new List<float>();
        velocityText.text = "Velocity: Calculating...";
    }

    // Update is called once per frame
    void Update()
    {
        Camera.transform.position = new Vector3(transform.position.x, transform.position.y + cameraHeight, transform.position.z - distanceFromCarToCamera);

        if (mainManager.averageCalculated && 
            mainManager.LoadingScreen.GetComponent<LoadingScreenManager>().getCurrentAnimation() == "[LS4] Hide")
        {
            velocityList.Add(mainManager.getVelocity());
            timer += Time.deltaTime;

            if (timer >= timeToActualizeVelocity)
            {
                changeVelocity();
                timer = 0;
            }

            //if (Input.GetKey("w"))
            //{
            transform.Translate(averageVelocity * forward * acceleration * Time.deltaTime);
            gameObject.GetComponent<Animator>().SetFloat("speed", averageVelocity);
            //}
        }
    }

    private void changeVelocity()
    {
        foreach (float v in velocityList)
        {
            averageVelocity += v;
        }

        averageVelocity = averageVelocity / velocityList.Count;

        velocityList.Clear();
        velocityText.text = "Velocity: " + averageVelocity.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "initialLimit" && previousEnviroment != actualEnviroment)
        {
            Destroy(previousEnviroment, 0.5f);
        }

        else if (other.gameObject.name == "endingLimit")
        {
            Vector3 actualEnviromentPosition = actualEnviroment.transform.position;
            GameObject newEnviroment = Instantiate(enviromentPrefab, 
                new Vector3(actualEnviromentPosition.x + terrainWidth, actualEnviromentPosition.y, actualEnviromentPosition.z), Quaternion.identity);

            Destroy(actualEnviroment.transform.GetChild(actualEnviroment.transform.childCount - 1).gameObject);

            previousEnviroment = actualEnviroment;
            actualEnviroment = newEnviroment;
        }
    }
}
