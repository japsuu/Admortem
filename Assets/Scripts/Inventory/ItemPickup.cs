using UnityEngine;
using TMPro;
using CodeMonkey.Utils;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Tilemaps;

public class ItemPickup : MonoBehaviour
{
    [ReadOnly] public Transform attractionTarget;

    private CircleCollider2D attractionTrigger;
    private Rigidbody2D rb;
    private AbstractItem item;
    private new SpriteRenderer renderer;
    private TextMeshPro amountTxt;
    private new Light2D light;
    private float savedGravityScale;
    private float aliveTime;
    private float startIntensity;
    private const int MAXAliveTime = 120;

    private Tilemap foregroundTilemap;

    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        amountTxt = transform.Find("Amount").GetComponent<TextMeshPro>();
        light = transform.Find("Light").GetComponent<Light2D>();
        rb = GetComponent<Rigidbody2D>();
        attractionTrigger = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        startIntensity = light.intensity;
        savedGravityScale = rb.gravityScale;

        foregroundTilemap = ChunkIt.Instance.VisualTilemap;
    }

    // Slowly fade the item away
    private void Update()
    {
        aliveTime += Time.deltaTime;

        Color color = renderer.color;
        color = new Color(color.r, color.g, color.b, 1 - (aliveTime / MAXAliveTime));
        renderer.color = color;

        light.intensity = startIntensity * (1 - (aliveTime / MAXAliveTime));

        if (aliveTime > MAXAliveTime)
        {
            DestroySelf();
        }

        if(attractionTarget != null)
        {
            if (Vector2.Distance(transform.position - new Vector3(0, -0.5f, 0), attractionTarget.position) < 1f)
            {
                PlayerInventoryController.Instance.PickUpItem(this);
            }
        }

        // Check if the pickup is inside the world walls
        if(foregroundTilemap.GetTile(foregroundTilemap.WorldToCell(transform.position)) != null)
        {
            transform.position += new Vector3(0, 1, 0);
        }
    }

    private void FixedUpdate()
    {
        if (attractionTarget != null)
        {
            rb.gravityScale = savedGravityScale / 4;
            Vector3 attractionTargetPosition = attractionTarget.position;
            Vector3 position = transform.position;
            rb.AddForce((attractionTargetPosition - position) * (5 * (attractionTrigger.radius - Vector2.Distance(attractionTargetPosition, position))));
        }
        else
        {
            rb.gravityScale = savedGravityScale;
        }
    }

    public static ItemPickup SpawnItemPickup(AbstractItem item, Vector2 position, Quaternion rotation)
    {
        ItemPickup pickup = Instantiate(item.PickupPrefab, position, rotation, ItemHolder.Instance.transform.GetChild(0)).GetComponent<ItemPickup>();
        pickup.SetItem(item);

        return pickup;
    }

    public void SetItem(AbstractItem itemToSet)
    {
        this.item = itemToSet;
        renderer.sprite = itemToSet.InventorySprite;

        if(itemToSet.Amount > 1)
            amountTxt.SetText(itemToSet.Amount + "x");
        else
            amountTxt.SetText("");
    }

    public static ItemPickup DropItem(AbstractItem item, Vector3 position)
    {
        Vector3 dir = UtilsClass.GetRandomDir();

        ItemPickup pickup = SpawnItemPickup(item, position, Quaternion.identity);
        Rigidbody2D rb = pickup.GetComponent<Rigidbody2D>();
        rb.AddForce(dir * 5f, ForceMode2D.Impulse);
        rb.AddTorque(dir.x * -15f);

        return pickup;
    }



    public static ItemPickup DropItem(AbstractItem item, PlayerMovementController movementController)
    {
        Vector3 dir = UtilsClass.GetRandomDir();

        Vector3 offset = movementController.facingLeft ? new Vector3(-1, 0.5f, 0) : new Vector3(1, 0.5f, 0);

        ItemPickup pickup = SpawnItemPickup(item, movementController.transform.position + offset, Quaternion.identity);
        Rigidbody2D rb = pickup.GetComponent<Rigidbody2D>();
        rb.AddForce(offset * Random.Range(5f, 10f), ForceMode2D.Impulse);
        rb.AddTorque(dir.x * -15f);

        return pickup;
    }

    public AbstractItem GetItem()
    {
        return item;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
