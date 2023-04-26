using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SingleBlock;

public class ItemController : MonoBehaviour
{
    
    public GameObject[] ItemGraphics;
    private BlockType Type;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetBlockType(BlockType type) {
        this.Type = type;
        switch (this.Type)
        {
            case BlockType.Seed:
                this.ItemGraphics[0].SetActive(true);
                break;
            case BlockType.Water:
                this.ItemGraphics[1].SetActive(true);
                break;
            case BlockType.Harvest:
                this.ItemGraphics[2].SetActive(true);
                break;
            case BlockType.Fertilizer:
                this.ItemGraphics[3].SetActive(true);
                break;
            case BlockType.Pest:
                this.ItemGraphics[4].SetActive(true);
                break;
            case BlockType.Pesticide:
                this.ItemGraphics[5].SetActive(true);
                break;
            case BlockType.Cover:
                this.ItemGraphics[6].SetActive(true);
                break;
            default:
                Debug.LogError("Invalid block type");
                break;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collisioned with " + collision.collider.gameObject.name);
        PlantSlot slot = collision.collider.gameObject.GetComponent<PlantSlot>();
        if (slot != null) {
            Debug.Log("Applying type " + this.Type.ToString());
            slot.ApplyElement(this.Type, 0);
        }
        Destroy(gameObject);
    }
}
