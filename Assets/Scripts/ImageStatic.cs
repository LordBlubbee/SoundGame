using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageStatic : MonoBehaviour
{
    float Fade = 1f;
    float FadeDir = 1;
    public Image img;

    // Update is called once per frame
    void Update()
    {
        Fade += FadeDir * Time.deltaTime;
        if (FadeDir > 0 && Fade > 1f) FadeDir *= -1;
        if (FadeDir < 0 && Fade < 0.5f) FadeDir *= -1;
        img.color = new Color(1, 1, 1, Fade);
    }
}
