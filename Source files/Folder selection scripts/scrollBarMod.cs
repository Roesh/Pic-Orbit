using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scrollBarMod : MonoBehaviour {

    private Scrollbar scrollbar; // The scrol bar attached to this object
    private int numExtraRows;
	// Use this for initialization
	void Start () {
        scrollbar = GetComponent<Scrollbar>();
        initializeScrollBar(4);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void initializeScrollBar(int nExtraRows)
    {
        numExtraRows = nExtraRows;
        if (numExtraRows != 0)
        {
            setScrollBarSize(1 / (float)numExtraRows);
            setScrollBarSteps(numExtraRows);
        }
        else
        {
            setScrollBarSize(1);
            setScrollBarSteps(0);
        }
    }
    /// <summary>
    /// Sets the size of the scroll bar.
    /// </summary>
    /// <param name="size">Size of scroll bar (between 0 and 1)</param>
    void setScrollBarSize(float size)
    {        
        scrollbar.size = Mathf.Clamp01(size);   // Size must be a float between 0 and 1  
    }
    /// <summary>
    /// Sets the number of steps of the scroll bar
    /// </summary>
    /// <param name="numSteps"></param>
    void setScrollBarSteps(int numSteps)
    {
        scrollbar.numberOfSteps = Mathf.Max(0,numSteps);   // numSteps must be greater than or equal to 0
    }
}
