using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SingleBlock;

public class GameController : MonoBehaviour
{
    public GameObject FallingBlockPrefab;
    public GameObject ItemPrefab;

    public TMP_Text CountdownText;
    public TMP_Text TimerText;
    public TMP_Text ScoreText;
    public TMP_Text ComboText;
    public GameObject TimerHolder;
    public GameObject ScoreHolder;
    public GameObject TitleImage;

    public GameObject GameOverPanel;
    public TMP_Text GameOverScoreText;
    public TMP_Text GameOverPlantsText;
    public TMP_Text GameOverBlocksText;
    public TMP_Text GameOverRank;
    public TMP_Text InspirationalPhrase;

    public PlantSlot[] PlantSlots;

    public GameObject SparkEffect;

    public float RoundDuration = 150.49f;

    public EventLog EventLogManager;

    public BoardBackgroundGenerator BoardBackgroundGenerator;

    private Vector2[] _spawnPositions = new Vector2[2] {
        new Vector2(3, 13),
        new Vector2(4, 13)
    };

    private SingleBlock[,] _gameBoard;
    private List<SingleBlock> _spawnedBlocks;
    private int _spawnCount = 0;
    private int _comboCounter = 0;
    private bool _enablingFall = false;

    private bool[,] _reviewedBlocks;
    private List<SingleBlock> _currentGroupBlocks;
    private int _score;
    private int _harvestedPlants = 0;
    private int _blockBroken = 0;

    private bool _isGameOnGoing = false;

    private static List<BlockType> _typesSeedPriority = new List<BlockType>() {
        BlockType.Seed, BlockType.Seed, BlockType.Seed, BlockType.Water, BlockType.Water, 
        BlockType.Fertilizer, BlockType.Harvest, BlockType.Harvest, BlockType.Pest, BlockType.Pesticide
    };

    private static List<BlockType> _typesDefaultRange = new List<BlockType>() {
        BlockType.Seed, BlockType.Water, BlockType.Fertilizer, BlockType.Harvest, BlockType.Pest, BlockType.Pesticide
    };

    private static List<BlockType> _typesHarvestPriority = new List<BlockType>() {
        BlockType.Seed, BlockType.Water, BlockType.Fertilizer, BlockType.Harvest, BlockType.Harvest, 
        BlockType.Pest, BlockType.Pest, BlockType.Pesticide, BlockType.Pesticide
    };

    void Start()
    {
        this._gameBoard = new SingleBlock[6, 14];
        this._spawnedBlocks = new List<SingleBlock>();  
    }

    void Update()
    {
        if (this._isGameOnGoing) {
            this.RoundDuration -= Time.deltaTime;
            this.TimerText.text = Mathf.RoundToInt(RoundDuration) + "s";
            if (this.RoundDuration <= 0) {
                this.GameOver("Time is up");
            }
        }
    }

    public void StartGameRoutine() {
        this.TitleImage.SetActive(false);

        this.EventLogManager.gameObject.SetActive(true);

        this.TimerHolder.SetActive(true);
        this.ScoreHolder.SetActive(true);

        this.BoardBackgroundGenerator.GenerateBoard();

        this._score = 0;
        this.AddScore(0);

        foreach (PlantSlot slot in PlantSlots)
        {
            slot.gameObject.SetActive(true);
            slot.SetGameController(this);
        }

        this.TimerText.text = Mathf.RoundToInt(RoundDuration) + "s";

        Debug.Log("Triggering countdown");
        StartCoroutine(StartCountdown());
    }

    public void SpawnPiece() {
        Debug.Log("Spawning a falling block");
        this._resetCombo();

        Vector2 spawnPosition = this._spawnPositions[this._spawnCount % 2];
        this._spawnCount++;
        GameObject fallingBlock = Instantiate(FallingBlockPrefab, spawnPosition, Quaternion.identity, null);
        FallingBlock fbScript = fallingBlock.GetComponent<FallingBlock>();
        fbScript.SetGameController(this);
        this.SetIncomingBlocks(fbScript);

        this._spawnedBlocks.Add(fbScript.CenterBlock);
        this._spawnedBlocks.Add(fbScript.SideBlock);
    }

    private void SetIncomingBlocks(FallingBlock fb) {
        //fb.SetRandomBlocks();
        fb.SetBlockTypes(this._getBlockType(), this._getBlockType());
    }

    public void CountHarvestedPlant() {
        this._harvestedPlants++;
    }

    private BlockType _getBlockType() {
        //NOTE: When low on plants, priotizes new seedlings. Later on harvest and dealing with pests.
        int plantCount = this._getCurrentPlantsCount();
        if (plantCount <= 3) {
            return GameController._typesSeedPriority[Random.Range(0, GameController._typesSeedPriority.Count)];
        } else if (plantCount > 3 && plantCount < 6) {
            return GameController._typesDefaultRange[Random.Range(0, GameController._typesDefaultRange.Count)];
        } else {
            return GameController._typesHarvestPriority[Random.Range(0, GameController._typesHarvestPriority.Count)];
        }
    }

    private int _getCurrentPlantsCount() {
        int count = 0;
        foreach (PlantSlot slot in PlantSlots) {
            if (!slot.IsEmpty) {
                count++;
            }
        }
        return count;
    }

    public IEnumerator StartCountdown() {
        this.CountdownText.gameObject.SetActive(true);

        this.CountdownText.text = "3";
        Debug.Log("Starting in 3...");
        yield return new WaitForSeconds(1);

        this.CountdownText.text = "2";
        Debug.Log("Starting in 2...");
        yield return new WaitForSeconds(1);

        this.CountdownText.text = "1";
        Debug.Log("Starting in 1...");
        yield return new WaitForSeconds(1);

        this.CountdownText.gameObject.SetActive(false);
        Debug.Log("Starting");
        StartGame();
    }

    public void StartGame() {
        this._isGameOnGoing = true;
        NextCycle();
    }

    public void NextCycle() {
        if (this._isGameOnGoing) {
            foreach (PlantSlot slot in PlantSlots)
            {
                slot.DoTick();
            }
            SpawnPiece();
        }
    }

    public void OnFallingBlockSeparated() {
        Debug.Log("Received signal that falling block separated");
    }

    public void OnSingleBlockStopped(SingleBlock sb) {
        Debug.Log($"Received signal that single block stopped at world position {sb.GetXSnappedPosition()},{sb.GetYSnappedPosition()}");

        if (sb.GetYSnappedPosition() > 12) {
            this.GameOver("Block above maximum height");
            return;
        }

        this._gameBoard[sb.GetXSnappedPosition()-1, sb.GetYSnappedPosition() - 1] = sb;
        
        if (!this._enablingFall && areAllBlocksGrounded())
        {
            Debug.Log("All blocks grounded. Running group checks");
            this.BlockGroupingCheck();
        }        
    }

    public void BlockGroupingCheck() {

        for (int x = 0; x < 6; x++) {
            for (int y = 0; y < 12; y++) {
                if (this._gameBoard[x, y] != null) {
                    //NOTE: This is not ideal as groups will re-searched for each block
                    this._reviewedBlocks = new bool[6, 14];
                    this._currentGroupBlocks = new List<SingleBlock>();

                    this._reviewedBlocks[x, y] = true;
                    this._currentGroupBlocks.Add(this._gameBoard[x, y]);
                    CheckPiecesArroundOf(this._gameBoard[x, y], x, y);

                    if (this._currentGroupBlocks.Count >= 4) {
                        Debug.LogWarning("Group of 4 or more blocks found. Removing");
                        StartCoroutine(ProcessGroupedBlocks(this._currentGroupBlocks));
                        return;
                    }
                }
            }
        }

        Debug.Log("Group of 4 or more blocks not found. Continuing");
        //NOTE: Starts the next cycle (this is path 1, there two paths to this)
        NextCycle();
    }

    public IEnumerator ProcessGroupedBlocks(List<SingleBlock> group) {
        this.increaseCombo();

        this.AddScore(group.Count);
        this._blockBroken += group.Count;

        foreach (SingleBlock block in group) {
            //Remove element from board
            this._gameBoard[block.GetXSnappedPosition() - 1, block.GetYSnappedPosition() - 1] = null;
            //Remove blocks from spawned blocks
            this._spawnedBlocks.Remove(block);
            //Show spark effect
            GameObject sparks = Instantiate(SparkEffect, block.transform.position, Quaternion.identity, null);
            Destroy(sparks, 0.05f);
            //Spawn item on column
            GameObject item = Instantiate(ItemPrefab, block.transform.position, Quaternion.identity, null);
            item.GetComponent<Rigidbody2D>().AddForce(Vector2.up, ForceMode2D.Impulse);
            item.GetComponent<ItemController>().SetBlockType(block.Type);
            //Remove blocks from game world
            Destroy(block.gameObject);
            yield return new WaitForSeconds(0.05f);
        }

        StartCoroutine(this.ReEnablePiecesFall());
    }

    public IEnumerator ReEnablePiecesFall() {
        bool pieceEnabled = false;
        Debug.LogWarning("Re-enabling falling on blocks");
        this._enablingFall = true;
        for (int y = 1; y < 12; y++)
        {
            for (int x = 0; x < 6; x++)
            {
                if (this._gameBoard[x, y] != null && this._gameBoard[x, y - 1] == null) {
                    this._gameBoard[x, y].EnableFall();
                    this._gameBoard[x, y] = null;
                    pieceEnabled = true;
                    yield return new WaitForSeconds(0.075f);
                } 
            }
        }
        this._enablingFall = false;

        if (!pieceEnabled) {
            //NOTE: Starts next cycle (this is path 2)
            NextCycle();
        }
    }

    public void CheckPiecesArroundOf(SingleBlock piece, int x, int y) {
        //NOTE: Checks piece at left side
        if (x > 1 && !this._reviewedBlocks[x-1, y]) {
            this._reviewedBlocks[x - 1, y] = true;
            if (this._gameBoard[x - 1, y] != null && piece.Type == this._gameBoard[x - 1, y].Type) {
                this._currentGroupBlocks.Add(this._gameBoard[x - 1, y]);
                CheckPiecesArroundOf(this._gameBoard[x - 1, y], x - 1, y);
            }
        } 

        //NOTE: Checks piece at right side
        if (x < 5 && !this._reviewedBlocks[x + 1, y]) {
            this._reviewedBlocks[x + 1, y] = true;
            if (this._gameBoard[x + 1, y] != null && piece.Type == this._gameBoard[x + 1, y].Type)
            {
                this._currentGroupBlocks.Add(this._gameBoard[x + 1, y]);
                CheckPiecesArroundOf(this._gameBoard[x + 1, y], x + 1, y);
            }
        }

        //NOTE: Checks piece above
        if (!this._reviewedBlocks[x, y + 1]) {
            this._reviewedBlocks[x, y + 1] = true;
            if (this._gameBoard[x, y + 1] != null && piece.Type == this._gameBoard[x, y + 1].Type)
            {
                this._currentGroupBlocks.Add(this._gameBoard[x, y + 1]);
                CheckPiecesArroundOf(this._gameBoard[x, y + 1], x, y + 1);
            }
        }

        //NOTE: Checks piece below
        if (y > 1 && !this._reviewedBlocks[x, y - 1]) {
            this._reviewedBlocks[x, y - 1] = true;
            if (this._gameBoard[x, y - 1] != null && piece.Type == this._gameBoard[x, y - 1].Type)
            {
                this._currentGroupBlocks.Add(this._gameBoard[x, y - 1]);
                CheckPiecesArroundOf(this._gameBoard[x, y - 1], x, y - 1);
            }
        }
    }

    public void GameOver(string reason) {
        //TODO: Wait for last blocks to settle before ending
        Debug.Log("Gameover because " + reason);
        this._isGameOnGoing = false;

        this.EventLogManager.gameObject.SetActive(false);
        this.TimerHolder.SetActive(false);
        this.ScoreHolder.SetActive(false);
        this.BoardBackgroundGenerator.gameObject.SetActive(false);

        this.GameOverPanel.SetActive(true);
        this.GameOverScoreText.text = this._score.ToString().PadLeft(5, '0');
        this.GameOverPlantsText.text = this._harvestedPlants.ToString().PadLeft(5, '0');
        this.GameOverBlocksText.text = this._blockBroken.ToString().PadLeft(5, '0');

        if (this._score > 3000) {
            this.GameOverRank.text = "SSS";
            this.InspirationalPhrase.text = "We mortals bow to you. Don't forget to share";
        } else if (this._score > 1500)
        {
            this.GameOverRank.text = "SS";
            this.InspirationalPhrase.text = "How did you made this much? Don't forget to share";
        } else if (this._score > 500)
        {
            this.GameOverRank.text = "S";
            this.InspirationalPhrase.text = "You are a great player. Don't forget to share";
        }
        else if (this._score > 150)
        {
            this.GameOverRank.text = "A";
            this.InspirationalPhrase.text = "Well done. Don't forget to share";
        }
        else if (this._score > 75)
        {
            this.GameOverRank.text = "B";
            this.InspirationalPhrase.text = "I bet you can do better than this";
        }
        else if (this._score > 30)
        {
            this.GameOverRank.text = "C";
            this.InspirationalPhrase.text = "You're learning. Give it another shot.";
        }
        else if (this._score > 10)
        {
            this.GameOverRank.text = "D";
            this.InspirationalPhrase.text = "Not so good. Try reading the tutorial";
        }
        else 
        {
            this.GameOverRank.text = "E";
            this.InspirationalPhrase.text = "Oh god why. How?";
        }
    }

    private bool areAllBlocksGrounded() {
        foreach (SingleBlock sb in this._spawnedBlocks) {
            if (!sb.IsGrounded()) {
                return false;
            }
        }
        return true;
    }

    public void increaseCombo() {
        this.ComboText.gameObject.SetActive(true);
        this._comboCounter++;
        this.ComboText.text = "Combo x"+this._comboCounter.ToString();
    }

    public void _resetCombo() {
        this.ComboText.gameObject.SetActive(false);
        this._comboCounter = 0;
    }

    public void AddScore(int value) {
        //NOTE: Score is multiplied by the combo counter
        this._score += value * (this._comboCounter);
        this.ScoreText.text = this._score.ToString().PadLeft(5, '0');
    }

    public void ReloadGame() {
        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }
}
