using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader_CallBack : MonoBehaviour
{
    bool firstUpdate = true;
    // Update is called once per frame
    void Update()
    {
        if(firstUpdate) {
            Loader.LoaderCallBack();
            firstUpdate = false;
        }
    }
}
