using Assets.LSL4Unity.Scripts.Examples;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public GameObject LabStreamingLayer;
    public GameObject ConnectionStatusLayer;
    public GameObject LoadingScreen;

    public int samplesToCalculateAverageCount = 20;

    private ExampleFloatInlet LabfloatInlet;
    private ExampleStringInlet LabstringInlet;

    private ExampleFloatInlet ConnectionfloatInlet;
    private ExampleStringInlet ConnectionstringInlet;

    private LoadingScreenManager loadingScreenManager;
    private TextMeshProUGUI textMeshPro; 

    private List<float> ch2Values;
    private List<float> ch3Values;

    private float ch2AverageNormalized;
    private float ch3AverageNormalized;

    private float limitToDiscard = 250;
    private float velocity = 0.5f;

    private bool averageCalculated = false;
    private bool loadingStarted = false;
    private bool loadingFinished = false;
    private bool reconectingStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        ch2Values = new List<float>();
        ch3Values = new List<float>();

        LabfloatInlet = LabStreamingLayer.GetComponent<ExampleFloatInlet>();
        LabstringInlet = LabStreamingLayer.GetComponent<ExampleStringInlet>();

        ConnectionfloatInlet = ConnectionStatusLayer.GetComponent<ExampleFloatInlet>();
        ConnectionstringInlet = ConnectionStatusLayer.GetComponent<ExampleStringInlet>();

        loadingScreenManager = LoadingScreen.GetComponent<LoadingScreenManager>();

        textMeshPro = LoadingScreen.transform.GetChild(LoadingScreen.transform.childCount - 1).gameObject.GetComponent<TextMeshProUGUI>();

        //line just to test the loading screen
        //loadingScreenManager.RevealLoadingScreen();
    }

    // Update is called once per frame
    void Update()
    {
        if (checkIfLastSampleIsValid(ConnectionfloatInlet))
        {
            if (ConnectionfloatInlet.lastSampleArray.Last() == 1 && checkIfLastSampleIsValid(LabfloatInlet))
            {
                //conexión establecida

                if (!loadingStarted)
                {
                    textMeshPro.text = "Calibrating...";
                    loadingScreenManager.RevealLoadingScreen();
                    loadingStarted = true;
                    reconectingStarted = false;
                    loadingFinished = false;
                }
                
                float ch2Value = LabfloatInlet.lastSampleArray[4];
                float ch3Value = LabfloatInlet.lastSampleArray[5];

                if (!averageCalculated)
                {
                    if (samplesToCalculateAverageCount > 0 && !discard(ch2Value) && !discard(ch3Value))
                    {
                        ch2Values.Add(ch2Value);
                        ch3Values.Add(ch3Value);
                        samplesToCalculateAverageCount -= 1;
                    }
                    else
                    {
                        float ch2Average = calculateAverage(ch2Values);
                        float ch3Average = calculateAverage(ch3Values);

                        ch2AverageNormalized = normalizeNumber(ch2Average);
                        ch3AverageNormalized = normalizeNumber(ch3Average);

                        velocity = (ch2AverageNormalized + ch3AverageNormalized) / 2;

                        averageCalculated = true;
                    }
                }
                else
                {
                    if (!loadingFinished)
                    {
                        loadingScreenManager.HideLoadingScreen();
                        loadingFinished = true;
                    }

                    if (!discard(ch2Value) && !discard(ch3Value))
                    {
                        float ch2Normalized = normalizeNumber(ch2Value);
                        float ch3Normalized = normalizeNumber(ch3Value);

                        if (ch2Normalized > ch2AverageNormalized && ch3Normalized < ch3AverageNormalized){
                            //si el canal 2 aumenta y el 3 disminuye se está poniendo nervioso, aumentar velocidad

                            velocity += ((ch2Normalized - ch2AverageNormalized) + (ch3AverageNormalized - ch3Normalized)) / 2;
                        }
                        else if (ch2Normalized < ch2AverageNormalized && ch3Normalized > ch3AverageNormalized)
                        {
                            //si el canal 2 disminuye y el 3 aumenta está tranquilo, disminuir velocidad
                            velocity -= ((ch2AverageNormalized - ch2Normalized) + (ch3Normalized - ch3AverageNormalized)) / 2;
                        }
                        else
                        {
                            //no hay cambios significativos, no hacer nada
                        }
                    }
                }
            }

            else if (ConnectionfloatInlet.lastSampleArray.Last() == 0 && !reconectingStarted)
            {
                //conexión perdida

                textMeshPro.text = "Connection lost. Trying to reconnect...";
                loadingScreenManager.RevealLoadingScreen();
                reconectingStarted = true;
                loadingStarted = false;
            }
        }
        /*else if(checkIfLastSampleIsValid(null, ConnectionstringInlet))
        {

        }*/
    }

    private bool checkIfLastSampleIsValid(ExampleFloatInlet floatInlet = null, ExampleStringInlet stringInlet = null)
    {
        if (floatInlet != null && floatInlet.lastSampleArray != null && floatInlet.lastSampleArray.Length > 0)
        {
            return true;
        }
        else if (stringInlet != null && stringInlet.lastSampleArray != null && stringInlet.lastSampleArray.Length > 0)
        {
            return true;
        }
        return false;
    }

    private float calculateAverage(List<float> values)
    {
        float addition = 0;

        foreach (float value in values)
        {
            addition += value;
        }

        return addition / values.Count;
    }

    private bool discard(float number)
    {
        if (Mathf.Abs(number) > limitToDiscard)
        {
            return true;
        }
        return false;
    }

    private float normalizeNumber(float n)
    {
        return (n - 0) / (limitToDiscard - 0);
    }

    public float getVelocity()
    { return velocity; }
}
