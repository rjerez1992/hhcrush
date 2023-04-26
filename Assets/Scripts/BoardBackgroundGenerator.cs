using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardBackgroundGenerator : MonoBehaviour
{
    public GameObject[] TilePrefabs;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void GenerateBoard() {
        for (int i = 1; i <= 6; i++)
        {
            for (int j = 1; j <= 12; j++)
            {
                Instantiate(TilePrefabs[Random.Range(0, TilePrefabs.Length)], new Vector2(i, j), Quaternion.identity, transform);
            }
        }
    }
}
