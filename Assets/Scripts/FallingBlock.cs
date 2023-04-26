using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SingleBlock;

public class FallingBlock : MonoBehaviour
{
    public enum FallingBlockDirection { Up, Right, Down, Left }

    public float FallingSpeed = 3f;
    public Transform Blocks;
    public CollisionProbe ProbeLeft;
    public CollisionProbe ProbeRight;
    public CollisionProbe ProbeDown;

    public SingleBlock CenterBlock;
    public SingleBlock SideBlock;

    private Rigidbody2D _rigidbody;
    private bool _isGrounded = false;
    private FallingBlockDirection _direction;
    private GameController _gameController;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.velocity = new Vector2(0f, -this.FallingSpeed);
        _direction = FallingBlockDirection.Up;
    }

    void Update()
    {
        if (!_isGrounded) {
            int xPosition = Mathf.RoundToInt(transform.position.x);

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (_direction != FallingBlockDirection.Left && transform.position.x > 1 && !this.ProbeLeft.IsColliding())
                {
                    transform.position = new Vector2(transform.position.x - 1, transform.position.y);
                }
                else if (_direction == FallingBlockDirection.Left && transform.position.x > 2 && !this.ProbeLeft.IsColliding())
                {
                    transform.position = new Vector2(transform.position.x - 1, transform.position.y);
                }
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (_direction != FallingBlockDirection.Right && transform.position.x < 6 && !this.ProbeRight.IsColliding())
                {
                    transform.position = new Vector2(transform.position.x + 1, transform.position.y);
                }
                else if (_direction == FallingBlockDirection.Right && transform.position.x < 5 && !this.ProbeRight.IsColliding())
                {
                    transform.position = new Vector2(transform.position.x + 1, transform.position.y);
                }
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (_direction == FallingBlockDirection.Up)
                {
                    if (xPosition < 6 && !ProbeRight.IsColliding())
                    {
                        //Debug.Log("Rotating 90° clockwise");
                        Blocks.rotation = Quaternion.Euler(0f, 0f, Blocks.rotation.eulerAngles.z - 90f);
                        this.RotateBlocksCounterClockwise();
                        _direction = FallingBlockDirection.Right;
                        ProbeRight.transform.localPosition = new Vector2(ProbeRight.transform.localPosition.x + 1, ProbeRight.transform.localPosition.y);
                    }
                    else if (xPosition >= 6 && !ProbeLeft.IsColliding())
                    {
                        //Debug.Log("Moving left and rotating 90° clockwise");
                        transform.position = new Vector2(transform.position.x - 1, transform.position.y);
                        Blocks.rotation = Quaternion.Euler(0f, 0f, Blocks.rotation.eulerAngles.z - 90f);
                        this.RotateBlocksCounterClockwise();
                        _direction = FallingBlockDirection.Right;
                        ProbeRight.transform.localPosition = new Vector2(ProbeRight.transform.localPosition.x + 1, ProbeRight.transform.localPosition.y);
                    }
                    else
                    {
                        Debug.LogWarning("Unable to rotate from current rotation and position");
                    }
                }
                else if (_direction == FallingBlockDirection.Right)
                {
                    //Debug.Log("Rotating 90° clockwise");
                    if (ProbeDown.IsColliding())
                    {
                        transform.position = new Vector2(transform.position.x, transform.position.y + 1);
                    }
                    Blocks.rotation = Quaternion.Euler(0f, 0f, Blocks.rotation.eulerAngles.z - 90f);
                    this.RotateBlocksCounterClockwise();
                    _direction = FallingBlockDirection.Down;
                    ProbeRight.transform.localPosition = new Vector2(ProbeRight.transform.localPosition.x - 1, ProbeRight.transform.localPosition.y);
                }
                else if (_direction == FallingBlockDirection.Down)
                {
                    if (xPosition > 1 && !ProbeLeft.IsColliding())
                    {
                        //Debug.Log("Rotating 90° clockwise");
                        Blocks.rotation = Quaternion.Euler(0f, 0f, Blocks.rotation.eulerAngles.z - 90f);
                        this.RotateBlocksCounterClockwise();
                        _direction = FallingBlockDirection.Left;
                        ProbeLeft.transform.localPosition = new Vector2(ProbeLeft.transform.localPosition.x - 1, ProbeLeft.transform.localPosition.y);
                    }
                    else if (xPosition <= 1 && !ProbeRight.IsColliding())
                    {
                        //Debug.Log("Moving left and rotating 90° clockwise");
                        transform.position = new Vector2(transform.position.x + 1, transform.position.y);
                        Blocks.rotation = Quaternion.Euler(0f, 0f, Blocks.rotation.eulerAngles.z - 90f);
                        this.RotateBlocksCounterClockwise();
                        _direction = FallingBlockDirection.Left;
                        ProbeLeft.transform.localPosition = new Vector2(ProbeLeft.transform.localPosition.x - 1, ProbeLeft.transform.localPosition.y);
                    }
                    else
                    {
                        Debug.LogWarning("Unable to rotate from current rotation and position");
                    }
                }
                else if (_direction == FallingBlockDirection.Left)
                {
                    Debug.Log("Rotating 90° clockwise");
                    Blocks.rotation = Quaternion.Euler(0f, 0f, Blocks.rotation.eulerAngles.z - 90f);
                    this.RotateBlocksCounterClockwise();
                    _direction = FallingBlockDirection.Up;
                    ProbeLeft.transform.localPosition = new Vector2(ProbeLeft.transform.localPosition.x + 1, ProbeLeft.transform.localPosition.y);
                }
            }
            else {
                if (Input.GetKey(KeyCode.Space))
                {
                    _rigidbody.velocity = new Vector2(0f, -this.FallingSpeed * 3);
                }
                else
                {
                    _rigidbody.velocity = new Vector2(0f, -this.FallingSpeed);
                }
            }
        }
    }

    public void RotateBlocksCounterClockwise() { 
        this.CenterBlock.transform.localRotation = Quaternion.Euler(0f, 0f, this.CenterBlock.transform.localRotation.eulerAngles.z + 90f);
        this.SideBlock.transform.localRotation = Quaternion.Euler(0f, 0f, this.SideBlock.transform.localRotation.eulerAngles.z + 90f);
    }

    public void SetGameController(GameController gc) {
        this._gameController = gc;
        this.CenterBlock.SetGameController(gc); 
        this.SideBlock.SetGameController(gc);
    }

    public void SetRandomBlocks() {
        int blockTypes = 6; //NOTE: Cover disabled
        int type = Random.Range(0, blockTypes);
        CenterBlock.SetBlockType(type);
        type = Random.Range(0, blockTypes);
        SideBlock.SetBlockType(type);
    }

    public void SetBlockTypes(BlockType typeBlock1, BlockType typeBlock2) {
        CenterBlock.SetBlockType(typeBlock1);
        SideBlock.SetBlockType(typeBlock2);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Falling block collided. Separating");

        CenterBlock.transform.parent = null;
        SideBlock.transform.parent = null;
        CenterBlock.EnableElements();
        CenterBlock.EnableFall();
        SideBlock.EnableElements();
        SideBlock.EnableFall();

        this._gameController.OnFallingBlockSeparated();

        Destroy(gameObject);
    }
}
