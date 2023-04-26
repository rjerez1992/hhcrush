using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SingleBlock : MonoBehaviour
{
    public enum BlockType { None, Seed, Water, Harvest, Fertilizer, Pest, Pesticide, Cover }

    public float FallingSpeed = 3f;
    public BlockType Type = BlockType.None;
    public SpriteRenderer SpriteRenderer;
    public GameObject[] BlockGraphics;

    private Rigidbody2D _rigidbody;
    private BoxCollider2D _boxCollider;
    private bool _isGrounded = false;
    
    private GameController _gameController;

    void Start()
    {
        this._rigidbody = GetComponent<Rigidbody2D>();
        this._boxCollider = GetComponent<BoxCollider2D>();
        DisableElements();
        DisableFall();
    }

    void Update()
    {
        if (this._isGrounded) {
            SnapIntoPlace();
        }
    }

    public void SetBlockType(int typeIndex) {
        switch (typeIndex) {
            case 0:
                this.SetBlockType(BlockType.Seed);
                break;
            case 1:
                this.SetBlockType(BlockType.Water);
                break;
            case 2:
                this.SetBlockType(BlockType.Harvest);
                break;
            case 3:
                this.SetBlockType(BlockType.Fertilizer);
                break;
            case 4:
                this.SetBlockType(BlockType.Pest);
                break;
            case 5:
                this.SetBlockType(BlockType.Pesticide);
                break;
            /*case 6:
                this.SetBlockType(BlockType.Cover);
                break;*/
        }
    }

    public void SetBlockType(BlockType type) {
        Debug.Log($"Block type set to {type}");
        this.Type = type;
        this.BlockGraphics[0].SetActive(false);
        switch (this.Type)
        {
            /*case BlockType.None:
                this.SpriteRenderer.color = new Color(0f, 0f, 0f);
                this.BlockText.text = "None";
                break;*/
            case BlockType.Seed:
                this.BlockGraphics[1].SetActive(true);
                break;
            case BlockType.Water:
                this.BlockGraphics[2].SetActive(true);
                break;
            case BlockType.Harvest:
                this.BlockGraphics[3].SetActive(true);
                break;
            case BlockType.Fertilizer:
                this.BlockGraphics[4].SetActive(true);
                break;
            case BlockType.Pest:
                this.BlockGraphics[5].SetActive(true);
                break;
            case BlockType.Pesticide:
                this.BlockGraphics[6].SetActive(true);
                break;
            /*case BlockType.Cover:
                this.SpriteRenderer.color = new Color(0.45f, 0.9f, 0.95f);
                this.BlockText.text = "Cover";
                break;*/
            default:
                Debug.LogError("Invalid block type");
                break;
        }
    }

    public void EnableElements() {
        this._boxCollider.enabled = true;
        this._rigidbody.isKinematic = false;
    }

    public void DisableElements() {
        this._boxCollider.enabled = false;
        this._rigidbody.isKinematic = true;
    }

    public void EnableFall() {
        this._isGrounded = false;
        this._rigidbody.isKinematic = false;
        this._rigidbody.velocity = new Vector2(0f, -this.FallingSpeed);
    }

    public void DisableFall() {
        this._rigidbody.isKinematic = true;
        this._rigidbody.velocity = Vector2.zero;
    }

    public void SnapIntoPlace() {
        //Debug.Log("Snapping into palce");
        this.transform.position = new Vector2(GetXSnappedPosition(), GetYSnappedPosition());
    }

    public void DisableFallAndSnap() {
        DisableFall();
        SnapIntoPlace();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Single block collided");
        if (!_isGrounded) {
            this._isGrounded = true;
            DisableFallAndSnap();
            this._gameController.OnSingleBlockStopped(this);
        }
    }

    public void SetGameController(GameController gc) { 
        this._gameController = gc;
    }

    public bool IsGrounded() {
        return this._isGrounded;
    }

    public int GetXSnappedPosition() {
        return Mathf.RoundToInt(transform.position.x);
    }

    public int GetYSnappedPosition() {
        return Mathf.RoundToInt(transform.position.y);
    }

    public Color GetBlockColorRepresentation() {
        return this.Type switch
        {
            BlockType.None => Color.black,
            BlockType.Seed => Color.green,
            BlockType.Water => Color.blue,
            BlockType.Harvest => new Color(0.9f, 0.9f, 0.6f),
            BlockType.Fertilizer => new Color(0.25f, 0.35f, 0.1f),
            BlockType.Pest => Color.red,
            BlockType.Pesticide => Color.white,
            BlockType.Cover => new Color(0.45f, 0.9f, 0.95f),
            _ => Color.black,
        };
    }
}
