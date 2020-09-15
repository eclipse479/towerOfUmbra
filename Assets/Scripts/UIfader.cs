using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIfader : MonoBehaviour
{
    public Image Fade;

    public float FadeStart;

    public float Fadeduration;

    public float FadeEnd;

    void Start()
    {
        Fade.canvasRenderer.SetAlpha(FadeStart);

        FadeinORout();
    }

    void FadeinORout ()
        {
        Fade.CrossFadeAlpha(FadeEnd,Fadeduration,false);
        }
}
