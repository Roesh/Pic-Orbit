using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class materialSwapBehavior : MonoBehaviour {

    private inputControls ic;
    private bool isLightMaterial;

    public Material lightMat;
    public Material darkMat;
    public Material wallMat;

    public Material pillarLightMat;
    public Material pillarDarkMat;
    // Use this for initialization
    void Start () {
        ic = FindObjectOfType<inputControls>();
        isLightMaterial = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                swapMaterial();
            }
        }
	}

    void swapMaterial()
    {
        Material mat;
        Material pillarMat;
        if (isLightMaterial)
        {
            mat = darkMat;
            pillarMat = pillarDarkMat;
            isLightMaterial = false;
        }
        else
        {
            mat = lightMat;
            pillarMat = pillarLightMat;
            isLightMaterial = true;
        }
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("outerWall"))
        {
            go.GetComponent<MeshRenderer>().material = mat;
        }
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Pillars"))
        {
            foreach(MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>())
            {
                mr.material = pillarMat;
            }
        }
    }
}

