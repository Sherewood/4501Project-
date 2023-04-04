using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimap : MonoBehaviour
{
    public GameObject minimapPrefab;
    public UserInterfaceController controller;
    public GameObject alliedObject;
    public GameObject enemyObject;
    public GameObject items;
    public GameObject buildings;
    public List<GameObject> UnitPositions;
    // Start is called before the first frame update
    void Start()
    {
        UnitPositions = new List<GameObject>();
        //_display= controller
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Mortis");
        UnitPositions.Equals(controller.GetMiniMap());
        foreach (GameObject whumpus in UnitPositions)
        {
            Debug.Log("whump"+whumpus.name);
        }
        
    }
}
