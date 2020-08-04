using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthBarTest : MonoBehaviour
{
    public Texture2D tex;
    private Rect rect;
    private float hor;
    private Rect hs;
    private Rect label;

    void Start()
    {
        float center = Screen.width / 2.0f;
        rect = new Rect(center - 200, 200, 400, 250);
        hs = new Rect(center - 200, 125, 400, 30);
        label = new Rect(center - 20, 155, 50, 30);
        hor = 0.5f;
    }

    void OnGUI()
    {
        hor = GUI.HorizontalSlider(hs, hor, 1.0f, 1.25f);
        GUI.Label(label, hor.ToString("F3"));
        GUI.DrawTextureWithTexCoords(rect, tex, new Rect(0.0f, 0.0f, hor, hor));
    }
}
