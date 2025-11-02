using UnityEngine;

public class OrePickuped : MonoBehaviour
{
    [Header("拾取配置")]
    public int oreAmount = 1;
    public float pickupRange = 3f;

    protected Transform player;
    protected UIManager uiManager;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        uiManager = FindObjectOfType<UIManager>();
        if (player == null || uiManager == null)
            Debug.LogError("OrePickuped: 找不到Player或UIManager！");
    }

    protected virtual void Update()
    {
        if (player == null || uiManager == null) return;
        if (Vector3.Distance(transform.position, player.position) < pickupRange)
        {
            uiManager.AddOre(oreAmount);
            Destroy(gameObject);
        }
    }
}