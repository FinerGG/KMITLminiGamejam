using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Components to Control")]
    public Light targetLight; 
    public MeshRenderer bulbRenderer; 

    [Header("Timing Settings")]
    public float timeOn = 0.05f; // จังหวะไฟติด (เร็วๆ)
    public float timeOff = 0.15f; // จังหวะไฟดับ (นานกว่านิดนึงเพื่อให้ตาเห็นชัด)

    // เปลี่ยนมาใช้ Array เพื่อเก็บ Material ทุกตัวบนโมเดล
    private Material[] bulbMaterials;
    private Color[] originalEmissionColors;

    void Start()
    {
        if (targetLight == null) targetLight = GetComponent<Light>();
        if (bulbRenderer == null) bulbRenderer = GetComponentInChildren<MeshRenderer>(); 

        if (bulbRenderer != null)
        {
            // ดึง Material ทุกตัวที่อยู่บนโมเดลนี้มาให้หมด
            bulbMaterials = bulbRenderer.materials;
            originalEmissionColors = new Color[bulbMaterials.Length];
            
            // วนลูปเก็บค่าสีเดิมของทุก Material
            for (int i = 0; i < bulbMaterials.Length; i++)
            {
                if (bulbMaterials[i].HasProperty("_EmissionColor"))
                {
                    originalEmissionColors[i] = bulbMaterials[i].GetColor("_EmissionColor");
                }
            }
        }

        if (targetLight != null)
        {
            StartCoroutine(FlickerLoop());
        }
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            SetState(false);
            yield return new WaitForSeconds(timeOff);

            SetState(true);
            yield return new WaitForSeconds(timeOn);
        }
    }

    void SetState(bool isOn)
    {
        // สั่งเปิด/ปิด Spot Light
        if (targetLight != null) targetLight.enabled = isOn;

        // สั่งเปิด/ปิด Material ทุกตัวพร้อมกัน
        if (bulbMaterials != null)
        {
            for (int i = 0; i < bulbMaterials.Length; i++)
            {
                if (bulbMaterials[i].HasProperty("_EmissionColor"))
                {
                    if (isOn)
                    {
                        bulbMaterials[i].EnableKeyword("_EMISSION");
                        bulbMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i]);
                    }
                    else
                    {
                        bulbMaterials[i].DisableKeyword("_EMISSION");
                        bulbMaterials[i].SetColor("_EmissionColor", Color.black);
                    }
                }
            }
        }
    }
}