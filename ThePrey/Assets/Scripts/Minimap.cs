using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour {

//    public List<Light> lights;

    private void OnPreCull()
    {
        RenderSettings.ambientLight = Color.white;
    }

    private void OnPostRender()
    {
        RenderSettings.ambientLight = Color.black;
    }
}
