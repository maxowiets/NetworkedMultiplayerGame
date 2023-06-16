using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScaleSwitch : MonoBehaviour
{
    private void Update()
    {
        if (transform.rotation.eulerAngles.z > 90 && transform.rotation.eulerAngles.z < 270)
        {
            transform.localScale = new Vector3(1f, -1f, 1f);
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}
