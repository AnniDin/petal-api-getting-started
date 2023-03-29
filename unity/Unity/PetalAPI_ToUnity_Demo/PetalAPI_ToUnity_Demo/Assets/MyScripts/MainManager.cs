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
    public GameObject LoadingScreen;

    public int samplesToCalculateAverageCount = 50;

    public TextMeshProUGUI countText;
    public TextMeshProUGUI velocityText;
    public GameObject mainMenu;

    [HideInInspector]
    public bool averageCalculated = false;

    private MyFloatInlet LabfloatInlet;

    private ExampleFloatInlet ConnectionfloatInlet;

    private LoadingScreenManager loadingScreenManager;
    private TextMeshProUGUI textMeshPro; 

    private List<float> ch2Values;
    private List<float> ch3Values;

    private float ch2AverageNormalized;
    private float ch3AverageNormalized;

    private float previousCh2Normalized = 0;
    private float previousCh3Normalized = 0;

    private float limitToDiscard = 150;
    private float velocity = 0.5f;

    private bool loadingStarted = false;
    private bool loadingFinished = false;
    private bool reconectingStarted = false;
    private bool start = false;

    // Start is called before the first frame update
    void Start()
    {
        ch2Values = new List<float>();
        ch3Values = new List<float>();

        LabfloatInlet = LabStreamingLayer.GetComponent<MyFloatInlet>();

        ConnectionfloatInlet = LabStreamingLayer.GetComponent<ExampleFloatInlet>();

        loadingScreenManager = LoadingScreen.GetComponent<LoadingScreenManager>();

        textMeshPro = LoadingScreen.transform.GetChild(LoadingScreen.transform.childCount - 1).gameObject.
            transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        countText.gameObject.SetActive(false);
        velocityText.gameObject.SetActive(false);
        mainMenu.SetActive(true);
        LoadingScreen.SetActive(false);

        //line just to test the loading screen
        //loadingScreenManager.RevealLoadingScreen();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!start)
            {
                start = true;
                mainMenu.SetActive(false);
                LoadingScreen.SetActive(true);
            }
            else
            {
                start = false;
                mainMenu.SetActive(true);
                LoadingScreen.SetActive(false);
            }
        }

        if (start)
        {
            if (!checkIfLastSampleIsValid(ConnectionfloatInlet) || 
                (checkIfLastSampleIsValid(ConnectionfloatInlet) && 
                ConnectionfloatInlet.lastSampleArray.Last() == 1))
            {
                if (checkIfLastSampleIsValid(null, LabfloatInlet))
                {
                    //conexión establecida

                    if (!loadingStarted)
                    {
                        textMeshPro.text = "Calibrating...";
                        LoadingScreen.SetActive(true);
                        //loadingScreenManager.RevealLoadingScreen();
                        loadingStarted = true;
                        reconectingStarted = false;
                        loadingFinished = false;
                    }

                    float ch2Value = LabfloatInlet.lastSampleArray[1];
                    float ch3Value = LabfloatInlet.lastSampleArray[2];

                    if (!averageCalculated)
                    {

                        
                        if (samplesToCalculateAverageCount > 0 && !discard(ch2Value) && !discard(ch3Value))
                        {
                            ch2Values.Add(ch2Value);
                            ch3Values.Add(ch3Value);
                            samplesToCalculateAverageCount -= 1;
                        }
                        else if (samplesToCalculateAverageCount <= 0)
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

                            countText.gameObject.SetActive(true);
                            velocityText.gameObject.SetActive(true);
                            countText.text = "Ch2: " + ch2Normalized + "   Ch3: " + ch3Normalized;

                            if (previousCh2Normalized == 0 && previousCh3Normalized == 0)
                            {
                                if (ch2Normalized > ch2AverageNormalized && ch3Normalized < ch3AverageNormalized)
                                {
                                    //si el canal 2 aumenta y el 3 disminuye se está poniendo nervioso, aumentar velocidad
                                    if (velocity + ((ch2Normalized - ch2AverageNormalized) + (ch3AverageNormalized - ch3Normalized)) / 2 <= 20)
                                    {
                                        velocity += ((ch2Normalized - ch2AverageNormalized) + (ch3AverageNormalized - ch3Normalized)) / 2;
                                    }
                                }
                                else if (ch2Normalized < ch2AverageNormalized && ch3Normalized > ch3AverageNormalized)
                                {
                                    //si el canal 2 disminuye y el 3 aumenta está tranquilo, disminuir velocidad
                                    if (velocity - ((ch2AverageNormalized - ch2Normalized) + (ch3Normalized - ch3AverageNormalized)) / 2 >= 0)
                                    {
                                        velocity -= ((ch2AverageNormalized - ch2Normalized) + (ch3Normalized - ch3AverageNormalized)) / 2;
                                    }
                                }
                                else
                                {
                                    //no hay cambios significativos, no hacer nada
                                }

                                previousCh2Normalized = ch2Normalized;
                                previousCh3Normalized = ch3Normalized;
                            }

                            else
                            {
                                if (ch2Normalized > ch2AverageNormalized && ch3Normalized < ch3AverageNormalized)
                                {
                                    //si el canal 2 aumenta y el 3 disminuye se está poniendo nervioso, aumentar velocidad
                                    if (velocity + ((ch2Normalized - previousCh2Normalized) + (previousCh3Normalized - ch3Normalized)) / 2 <= 20)
                                    {
                                        velocity += ((ch2Normalized - previousCh2Normalized) + (previousCh3Normalized - ch3Normalized)) / 2;
                                    }
                                }
                                else if (ch2Normalized < ch2AverageNormalized && ch3Normalized > ch3AverageNormalized)
                                {
                                    //si el canal 2 disminuye y el 3 aumenta está tranquilo, disminuir velocidad
                                    if (velocity - ((previousCh2Normalized - ch2Normalized) + (ch3Normalized - previousCh3Normalized)) / 2 >= 0)
                                    {
                                        velocity -= ((previousCh2Normalized - ch2Normalized) + (ch3Normalized - previousCh3Normalized)) / 2;
                                    }
                                }
                                else
                                {
                                    //no hay cambios significativos, no hacer nada
                                }

                                previousCh2Normalized = ch2Normalized;
                                previousCh3Normalized = ch3Normalized;
                            }

                        }
                    }
                }

                else
                {
                    //no recibiendo los datos eeg

                    textMeshPro.text = "Incapaz de recibir los datos, reinicia la aplicación.";
                    LoadingScreen.SetActive(true);
                    loadingScreenManager.RevealLoadingScreen();
                    reconectingStarted = true;
                    loadingStarted = false;
                }

            }

            else if (checkIfLastSampleIsValid(ConnectionfloatInlet) && ConnectionfloatInlet.lastSampleArray.Last() == 0 && !reconectingStarted)
            {
                //conexión perdida

                textMeshPro.text = "Connection lost. Trying to reconnect...";
                LoadingScreen.SetActive(true);
                loadingScreenManager.RevealLoadingScreen();
                reconectingStarted = true;
                loadingStarted = false;
            }
        }
    }

    private bool checkIfLastSampleIsValid(ExampleFloatInlet floatInlet = null, MyFloatInlet MyfloatInlet = null)
    {
        if (floatInlet != null && floatInlet.lastSampleArray != null && floatInlet.lastSampleArray.Length > 0)
        {
            return true;
        }
        else if (MyfloatInlet != null && MyfloatInlet.lastSampleArray != null && MyfloatInlet.lastSampleArray.Length > 0)
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
