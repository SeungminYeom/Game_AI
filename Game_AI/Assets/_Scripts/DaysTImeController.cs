using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaysTImeController : MonoBehaviour
{
    public Light directionalLight;
    public MeshRenderer skydome;
    public float oneDayTime = 60f; //낮 30초, 밤 30초
    private float offsetX = 0f;

    private void Update()
    {
        skydome.material.mainTextureOffset = new Vector2(offsetX, 0);
        directionalLight.intensity = offsetX < 0.5f ? 1 - offsetX * 2f : (offsetX - 0.5f) * 2f;

        offsetX += Time.deltaTime / oneDayTime;

        if (offsetX > 1f) offsetX -= 1f;
    }
}