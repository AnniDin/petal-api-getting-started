using Assets.LSL4Unity.Scripts.Examples;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public GameObject LabStreamingLayer;
    public GameObject ConnectionStatusLayer;

    public int samplesToCalculateAverageCount = 20;

    private ExampleFloatInlet LabfloatInlet;
    private ExampleStringInlet LabstringInlet;

    private ExampleFloatInlet ConnectionfloatInlet;
    private ExampleStringInlet ConnectionstringInlet;

    private List<float> ch2Values;
    private List<float> ch3Values;

    private float ch2Average;
    private float ch3Average;

    private float limitToDiscard = 500f;

    private bool averageCalculated = false;

    // Start is called before the first frame update
    void Start()
    {
        ch2Values = new List<float>();
        ch3Values = new List<float>();

        LabfloatInlet = LabStreamingLayer.GetComponent<ExampleFloatInlet>();
        LabstringInlet = LabStreamingLayer.GetComponent<ExampleStringInlet>();

        ConnectionfloatInlet = ConnectionStatusLayer.GetComponent<ExampleFloatInlet>();
        ConnectionstringInlet = ConnectionStatusLayer.GetComponent<ExampleStringInlet>();
    }

    // Update is called once per frame
    void Update()
    {
        if (checkIfLastSampleIsValid(ConnectionfloatInlet))
        {
            if (ConnectionfloatInlet.lastSampleArray.Last() == 1 && checkIfLastSampleIsValid(LabfloatInlet))
            {
                //conexión establecida
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
                        ch2Average = calculateAverage(ch2Values);
                        ch3Average = calculateAverage(ch3Values);
                    }
                }
                else
                {

                }
            }

            else if (ConnectionfloatInlet.lastSampleArray.Last() == 0)
            {
                //conexión perdida
            }
        }
        /*else if(checkIfLastSampleIsValid(null, ConnectionstringInlet))
        {

        }*/
    }

    private bool checkIfLastSampleIsValid(ExampleFloatInlet floatInlet = null, ExampleStringInlet stringInlet = null)
    {
        if (floatInlet != null && floatInlet.lastSampleArray != null)
        {
            return true;
        }
        else if (stringInlet != null && stringInlet.lastSampleArray != null)
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
}
