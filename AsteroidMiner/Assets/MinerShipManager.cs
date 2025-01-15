using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinerShipManager : MonoBehaviour
{
    public GameObject minerShipPrefab; // Miner Ship prefabı
    public Transform spawnPoint; // Gemilerin oluşacağı konum
    public Button buildButton; // "Build Miner Ship" düğmesi
    public float buildTime = 10f; // Bir geminin üretim süresi
    public int ironCost = 100; // Miner Ship için gereken demir miktarı

    private Queue<GameObject> buildQueue = new Queue<GameObject>(); // Üretim kuyruğu
    private bool isBuilding = false; // Şu anda bir gemi üretiliyor mu?

    private ReserveManager reserveManager; // Kaynak yöneticisi referansı

    void Start()
    {
        // ReserveManager'ı bul
        reserveManager = Object.FindFirstObjectByType<ReserveManager>();

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
            // Ölçeği ayarla
            newShip.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

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

            // MinerShip script'inden transparanlık efektini başlat
            ConstructableShip minerShip = currentShip.GetComponent<ConstructableShip>();
            if (minerShip != null)
            {
                minerShip.StartTransparencyEffect(buildTime);
            }

            // Üretim süresi kadar bekle
            yield return new WaitForSeconds(buildTime);
        }

        isBuilding = false;
    }


    void UpdateButtonState()
    {
        // Yeterli kaynak yoksa düğme pasif hale gelir
        if (reserveManager != null && reserveManager.GetReserves().Iron < ironCost)
        {
            buildButton.interactable = false;
        }
        else
        {
            buildButton.interactable = true;
        }
    }
}