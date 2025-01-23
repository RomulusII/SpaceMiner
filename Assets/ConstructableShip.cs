using System.Collections;
using UnityEngine;

public class ConstructableShip : MonoBehaviour
{
    private Renderer[] renderers; // Tüm Renderer bileşenleri (alt nesneler dahil)
    private Material[] materials; // Materyalleri saklayacağız

    void Awake()
    {
        // MinerShip üzerinde ve alt nesnelerdeki tüm Renderer bileşenlerini al
        renderers = GetComponentsInChildren<Renderer>();

        // Her Renderer'ın materyalini sakla
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            materials[i] = renderers[i].material;
        }
    }

    public void StartTransparencyEffect(float buildTime)
    {
        StartCoroutine(TransparencyEffect(buildTime));
    }

    private IEnumerator TransparencyEffect(float buildTime)
    {
        float elapsedTime = 0f;

        // Şeffaflık efektini başlat
        while (elapsedTime < buildTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Sqrt(Mathf.Clamp01(elapsedTime / buildTime));

            // Tüm materyallerin opaklık değerini güncelle
            foreach (Material mat in materials)
            {
                mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);
            }

            yield return null;
        }

        // Efekt tamamlandı, tüm materyalleri tamamen opak yap
        foreach (Material mat in materials)
        {
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1f);
        }
    }
}
