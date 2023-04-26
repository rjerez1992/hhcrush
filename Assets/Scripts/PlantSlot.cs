using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSlot : MonoBehaviour
{
    public bool IsEmpty = true;
    public bool IsInfected = false;
    public bool IsFertilized = false;
    public bool IsWatered = false;
    public bool IsCovered = false;

    public SpriteRenderer SoilImage;

    public GameObject[] PlantStagesGraphics;
    public GameObject[] StarsGraphics;

    public GameObject FertilizedGraphics;
    public GameObject CoveredGraphics;
    public GameObject InfestedGraphics;

    public int PlantGrowthStage = 0;
    public int PlantQuality = 0;

    private GameController _gameController;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ApplyElement(SingleBlock.BlockType element, int quality) {
        switch (element)
        {
            case SingleBlock.BlockType.Seed:
                OnReceiveSeed(quality);
                break;
            case SingleBlock.BlockType.Water:
                OnReceiveWater();
                break;
            case SingleBlock.BlockType.Harvest:
                OnReceiveHarvest();
                break;
            case SingleBlock.BlockType.Fertilizer:
                OnReceiveFertilizer();
                break;
            case SingleBlock.BlockType.Pest:
                OnReceivePest();
                break;
            case SingleBlock.BlockType.Pesticide:
                OnReceivePesticide();
                break;
            case SingleBlock.BlockType.Cover:
                OnReceiveCover();
                break;
        }
    }

    public void SetGameController(GameController gc) {
        this._gameController = gc;
    }

    public void DoTick() {
        if (!this.IsEmpty) {
            if (this.IsInfected && this.PlantGrowthStage > 0)
            {
                this.PlantGrowthStage--;
                this.RemovePlantGraphics();
                this.PlantStagesGraphics[this.PlantGrowthStage].SetActive(true);
            }
            else if (this.PlantGrowthStage < 4 && this.IsWatered)
            {
                if (this.IsFertilized)
                {
                    this.PlantGrowthStage += 2;
                    if (this.PlantGrowthStage > 4)
                    {
                        this.PlantGrowthStage = 4;
                    }
                }
                else
                {
                    this.PlantGrowthStage++;
                }
                this.RemovePlantGraphics();
                this.PlantStagesGraphics[this.PlantGrowthStage].SetActive(true);
            }
        }
    }

    public void OnReceiveSeed(int quality) {
        if (IsEmpty)
        {
            this.IsEmpty = false;
            this.RemovePlantGraphics();
            this.PlantStagesGraphics[0].SetActive(true);
            if (quality < 0 || quality > 2)
            {
                quality = 0;
            }
            this.PlantQuality = quality;
            this.RemoveStarsGraphics();
            this.StarsGraphics[quality].SetActive(true);
            this._gameController.EventLogManager.AddEvent($"{gameObject.name} got SEED level ({this.PlantQuality + 1})");
        }
    }

    public void OnReceiveWater() {
        if (!this.IsWatered)
        {
            this.IsWatered = true;
            this.SoilImage.color = new Color(0.5f, 0.5f, 0.7f);
            this._gameController.EventLogManager.AddEvent($"{gameObject.name} got WATERED");
        }
    }

    public void OnReceiveHarvest() {
        if (!this.IsEmpty)
        {
            //TODO: Show harvest effect and money effect
            int scoreValue = this.GetSlotScore();
            this._gameController.AddScore(scoreValue);
            this.ResetSlot();
            this._gameController.CountHarvestedPlant();
            this._gameController.EventLogManager.AddEvent($"{gameObject.name} was HARVESTED for ${scoreValue}");
        }
    }

    public void OnReceiveFertilizer() {
        if (!this.IsFertilized)
        {
            this.FertilizedGraphics.SetActive(true);
            this._gameController.EventLogManager.AddEvent($"{gameObject.name} got FERTILIZED");
        }
    }

    public void OnReceivePest() {
        if (!this.IsCovered && !this.IsInfected && !this.IsEmpty)
        {
            this.IsInfected = true;
            this.InfestedGraphics.SetActive(true);
            this._gameController.EventLogManager.AddEvent($"{gameObject.name} is INFESTED");
        }
    }

    public void OnReceivePesticide() {
        if (this.IsInfected)
        {
            this.IsInfected = false;
            this.InfestedGraphics.SetActive(true);
            this._gameController.EventLogManager.AddEvent($"{gameObject.name} got CLEANSED");
        }
    }

    public void OnReceiveCover() {
        if (!this.IsCovered)
        {
            this.IsCovered = true;
            this.CoveredGraphics.SetActive(true);
            this._gameController.EventLogManager.AddEvent($"{gameObject.name} got COVERED");
        }
    }

    public void RemovePlantGraphics() {
        foreach (GameObject go in this.PlantStagesGraphics)
        {
            go.SetActive(false);
        }
    }

    public void RemoveStarsGraphics()
    {
        foreach (GameObject go in this.StarsGraphics)
        {
            go.SetActive(false);
        }
    }

    public void ResetSlot()
    {
        Debug.LogWarning("Slot values reset");
        this.IsEmpty = true;
        this.IsInfected = false;
        this.IsFertilized = false;
        this.IsWatered = false;
        this.IsCovered = false;
        this.PlantGrowthStage = 0;
        this.PlantQuality = 0;

        this.SoilImage.color = Color.white;
        this.RemovePlantGraphics();
        this.RemoveStarsGraphics();
        this.InfestedGraphics.SetActive(false);
        this.FertilizedGraphics.SetActive(false);
        this.CoveredGraphics.SetActive(false);
    }

    public int GetSlotScore() {
        if (this.IsEmpty) {
            return 0;
        }

        float score = 0f;

        switch (this.PlantGrowthStage) {
            case 0:
                score += 3f;
                break;
            case 1:
                score += 9f;
                break;
            case 2:
                score += 20f;
                break;
            case 3:
                score += 35f;
                break;
            case 4:
                score += 60f;
                break;
        }

        if (this.IsFertilized)
        {
            score *= 1.15f;
        }

        switch (this.PlantQuality) {
            case 0:
                score *= 1f;
                break;
            case 1:
                score *= 1.5f;
                break;
            case 2:
                score *= 2.5f;
                break;
        }

        if (this.IsInfected) {
            score *= 0.35f;
        }

        return Mathf.RoundToInt(score);
    }
}
