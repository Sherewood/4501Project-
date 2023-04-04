using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        
        UnitPositions = controller.GetMiniMap();
       
        foreach (GameObject whumpus in UnitPositions)
        {
            if (whumpus.GetComponent<UnitInfo>().GetAllegiance()=="player")
            {
               // GameObject tracker = Instantiate(alliedObject);
             //   tracker.transform.SetParent(minimapPrefab.transform, false);
                Vector3 position = new Vector3();
              //  position.x = whumpus.transform.position.x/FindObjectOfType<Terrain>().GetComponent<Terrain>();
            //    position.y = whumpus.transform.position.y / FindObjectOfType<Terrain>().transform.position.y;
                Debug.Log(position+ "Terrain :"+ FindObjectOfType<Terrain>().transform.position);
               // tracker.transform.Translate(position);
            }
        }
        StartCoroutine(Deletion());
    }
    private void clear()
    {
        UnitPositions.Clear();
    }
    IEnumerator Deletion()
    {
        yield return new WaitForSeconds(.001f);
        clear();
    }
}
