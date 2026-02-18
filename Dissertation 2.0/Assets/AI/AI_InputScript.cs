using JetBrains.Annotations;
using System;
using System.CodeDom.Compiler;
using UnityEngine;

public class AI_InputSript : MonoBehaviour
{
    public float meanInputFrequency;
    public float stdDevInputFrequency;

    private float elapsedSinceLastAction;
    private float nextActionTime;

    public bool move = false;
    public bool attack = false;
    public bool ability1 = false;
    public bool ability2 = false;
    public bool ability3 = false;
    public GameObject target = null;
    public Vector3 mousePos = Vector3.zero;

    public enum InputType
    {
        move,
        attack,
        ability1,
        ability2,
        ability3,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsedSinceLastAction += Time.deltaTime;

        if(elapsedSinceLastAction >= nextActionTime )
        {
            elapsedSinceLastAction = 0;
            nextActionTime = generateRandomGaussian(meanInputFrequency, stdDevInputFrequency);

            InputType action = decideDiscreteAction();
        }
    }

    private float generateRandomGaussian(float mean, float stdDev, bool clampValue = false, float minVal = 0, float maxVal = 1)
    {
        float u1 = Mathf.Max(UnityEngine.Random.value, float.Epsilon);
        float u2 = Mathf.Max(UnityEngine.Random.value, float.Epsilon);

        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                              Mathf.Sin(2.0f * Mathf.PI * u2);

        float value = mean + stdDev * randStdNormal;

        if( clampValue )
        {
            value = Mathf.Clamp(value, minVal, maxVal);
        }

        return value;
    }

    private InputType decideDiscreteAction()
    {
        int method = 0;

        InputType action = new InputType();
        switch(method)
        {
            case 0:
                {
                    int n = UnityEngine.Random.Range(0, 5);
                    action = (InputType)n;
                }
                break;
            case 1:
                break;
            default:
                break;
        }

        return action;
    }

    private bool generateParametersForAction(InputType action)
    {


        return true;
    }
}
