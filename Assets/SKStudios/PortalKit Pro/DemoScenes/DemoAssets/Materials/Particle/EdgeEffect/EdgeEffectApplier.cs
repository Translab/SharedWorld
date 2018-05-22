using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeEffectApplier : MonoBehaviour
{
    [ColorUsageAttribute(true, true, 0f, 8f, 0.125f, 3f)]
    public new Color Color = Color.white;
    public int TexDepth;

    private int currentFrame;
    // Use this for initialization
    private void Start() {
       GetComponent<Renderer>().material.SetColor("_TintColor", Color);
    }
}