using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinerShipManager : MonoBehaviour
{
    public GameObject minerShipPrefab; // Miner Ship prefabı
    public Transform spawnPoint; // Gemilerin oluşacağı konum
    public Button buildButton; // "Build Miner Ship" düğmesi
    public Text buildButtonText; // Düğme üzerindeki metin
    public float buildTime = 10f; // Bir geminin üretim süresi
    public int ironCost = 100; // Miner Ship için gereken demir miktarı

    private Queue<GameObject> buildQueue = new Queue<GameObject>(); // Üretim kuyruğu
    private bool isBuilding = false; // Şu anda bir gemi üretiliyor mu?

    private ReserveManager reserveManager; // Kaynak yöneticisi referansı

    void Start()
    {
        // ReserveManager'ı bul
        reserveManager = FindAnyObjectByType<ReserveManager>();

        // Düğmeye tıklanma olayını bağla
        buildButton.onClick.AddListener(() => AddToBuildQueue());

        // Düğme durumunu güncelle
        UpdateButtonState();
    }

    void Update()
    {
        // Gerektiğinde düğme durumunu güncelle
        UpdateButtonState();
    }

    void AddToBuildQueue()
    {
        // Yeterli kaynak varsa düğmeye basıldığında üretim kuyruğuna ekle
        if (reserveManager.SpendReserve(ReserveType.Iron, ironCost))
        {
            GameObject newShip = Instantiate(minerShipPrefab, spawnPoint.position, Quaternion.identity);
            newShip.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0f); // Başlangıçta tamamen transparan
            newShip.SetActive(false); // Üretim tamamlanana kadar gizli
            buildQueue.Enqueue(newShip);

            // Eğer şu anda başka bir gemi üretilmiyorsa üretime başla
            if (!isBuilding)
            {
                StartCoroutine(BuildProcess());
            }
        }
    }

    IEnumerator BuildProcess()
    {
        isBuilding = true;

        while (buildQueue.Count > 0)
        {
            GameObject currentShip = buildQueue.Dequeue();
            currentShip.SetActive(true); // Gemi görünür hale gelir

            Material shipMaterial = currentShip.GetComponent<Renderer>().material;
            float elapsedTime = 0f;

            // Üretim süreci boyunca transparanlığı azalt
            while (elapsedTime < buildTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / buildTime); // Şeffaflık 0'dan 1'e kadar artar
                shipMaterial.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }

            // Üretim tamamlandı, gemi tamamen opak
            shipMaterial.color = new Color(1f, 1f, 1f, 1f);
        }

        isBuilding = false;
    }

    void UpdateButtonState()
    {
        // Yeterli kaynak yoksa düğme pasif hale gelir
        if (reserveManager != null && reserveManager.GetReserves().Iron < ironCost)
        {
            buildButton.interactable = false;
            buildButtonText.text = "Not Enough Iron";
        }
        else
        {
            buildButton.interactable = true;
            buildButtonText.text = "Build Miner Ship";
        }
    }
}