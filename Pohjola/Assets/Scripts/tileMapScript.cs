using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tileMapScript : MonoBehaviour    
{
    //Reference holders for the other two scripts that are currently running
    //alongside this script
    [Header("Manager Scripts")]
    public battleManagerScript BMS;
    public gameManagerScript GMS;
    public UnitScript US;

    //List of tiles that are used to generate the map
    //Try chaging tilesTypes to enum later   
    [Header("Tiles")]
    public Tile[] tileTypes;
    public int[,] tiles;
    public float tileSize = 1;

    //This is used when the game starts and there are pre-existing units
    //It uses this variable to check if there are any units and then maps them to the proper tiles
    [Header("Units on the board")]
    public GameObject unitsOnBoard;

    //This 2d array is the list of tile gameObjects on the board
    public GameObject[,] tilesOnMap;

    //This 2d array is the list of quadUI gameObjects on the board
    public GameObject[,] quadOnMap;
    public GameObject[,] quadOnMapForUnitMovementDisplay;
    public GameObject[,] quadOnMapCursor;
    
    //public is only to set them in the inspector, if you change these to private then you will
    //need to re-enable them in the inspector
    //Game object that is used to overlay onto the tiles to show possible movement
    public GameObject mapUI;
    //Game object that is used to highlight the mouse location
    public GameObject mapCursorUI;
    //Game object that is used to highlight the path the unit is taking
    public GameObject mapUnitMovementUI;

    //Nodes along the path of shortest path from the pathfinding
    public List<Node> currentPath = null;

    //Node graph for pathfinding purposes
    public Node[,] graph;

    //containers (parent gameObjects) for the UI tiles
    [Header("Containers")]
    public GameObject tileContainer;
    public GameObject UIQuadPotentialMovesContainer;
    public GameObject UIQuadCursorContainer;
    public GameObject UIUnitMovementPathContainer;
    

    //Set in the inspector, might change this otherwise.
    //This is the map size (please put positive numbers it probably wont work well with negative numbers)
    [Header("Board Size")]
    public int mapSizeX;
    public int mapSizeY;

    //In the update() function mouse down raycast sets this unit
    [Header("Selected Unit Info")]
    public GameObject selectedUnit;
    //These two are set in the highlightUnitRange() function
    //They are used for other things as well, mainly to check for movement, or finalize function
    public HashSet<Node> selectedUnitTotalRange;
    public HashSet<Node> selectedUnitMoveRange;

    public bool unitSelected = false;

    public int unitSelectedPreviousX;
    public int unitSelectedPreviousY;
    public float unitSelectedPreviousRotationY;

    public GameObject previousOccupiedTile;

    public bool finalizedMoviment = false;

    //public AudioSource selectedSound;
    //public AudioSource unselectedSound;
    //public area to set the material for the quad material for UI purposes
    [Header("Materials")]
    public Material greenUIMat;
    public Material redUIMat;
    public Material blueUIMat;

    [Header("Fake Board")]
    public Slider sliderClose;
    public Slider slider;
    public GameObject currentBoard;
    public GameObject fakeBoard;

    [Header("Win Conditions")]
    public bool searchedObject = false;
    public bool purified = false;

    public ParticleSystem magicBarrier;
    public ParticleSystem magicBarrier1;
    public ParticleSystem magicBarrier2;
    public ParticleSystem explosionBarrier;
    public ParticleSystem goodSpirits;
    public ParticleSystem badSpirits;

    public int objectInPossess = 0;
    public int trollsPurified = 0;

    public bool ajattaraCanEnterTheCenter = true;

    public bool ajattaraInfected = false;
    public bool trollInfected = false;
    public bool firstInfected = false;
    public bool secondInfected = false;
    public bool thirdInfected = false;
    public bool fourthInfected = false;
    public bool fifthInfected = false;
    public bool sixthInfected = false;

    [Header("Zones")]
    public GameObject ajattaraZonePure;
    public GameObject ajattaraZoneInfected;

    public GameObject trollZonePure;
    public GameObject trollZoneInfected;

    public GameObject firstZonePure;
    public GameObject firstZoneInfected;

    public GameObject secondZonePure;
    public GameObject secondZoneInfected;

    public GameObject thirdZonePure;
    public GameObject thirdZoneInfected;

    public GameObject fourthZonePure;
    public GameObject fourthZoneInfected;

    public GameObject fifthZonePure;
    public GameObject fifthZoneInfected;

    public GameObject sixthZonePure;
    public GameObject sixtheZoneInfected;

    public int tapCount;

    [Header ("AP Manager")]
    public bool peukaloExitPutrifiedTile = false;
    public bool peukaloEnteredPutrifiedTile = false;

    public bool marjaExitPutrifiedTile = false;
    public bool marjaEnteredPutrifiedTile = false;

    public bool kullervoExitPutrifiedTile = false;
    public bool kullervoEnteredPutrifiedTile = false;

    public GameObject peukaloApUp2;
    public GameObject peukaloApDown2;
    public GameObject peukaloApUp3;
    public GameObject peukaloApDown3;

    public GameObject kullervoApUp2;
    public GameObject kullervoApDown2;
    public GameObject kullervoApUp3;
    public GameObject kullervoApDown3;

    public GameObject marjaApUp2;
    public GameObject marjaApDown2;
    public GameObject marjaApUp3;
    public GameObject marjaApDown3;

    private void Start()
    {
        //Generate the map info that will be used
        generateMapInfo();
        //Generate pathfinding graph
        generatePathFindingGraph();
        //With the generated info this function will read the info and produce the map
        generateMapVisuals();
        //Check if there are any pre-existing units on the board
        setIfTileIsOccupied();
    }

    private void Update()
    {

        //If input is left mouse down then select the unit
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedUnit == null)
            {
                //mouseClickToSelectUnit();
                mouseClickToSelectUnitV2();
            }
            //After a unit has been selected then if we get a mouse click, we need to check if the unit has entered the selection state (1) 'Selected'
            //Move the unit
            else if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1) && selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0)
            {
                if (selectTileToMoveTo())
                {
                    unitSelectedPreviousX = selectedUnit.GetComponent<UnitScript>().x;
                    unitSelectedPreviousY = selectedUnit.GetComponent<UnitScript>().y;
                    unitSelectedPreviousRotationY = selectedUnit.GetComponent<UnitScript>().rotationY;
                    previousOccupiedTile = selectedUnit.GetComponent<UnitScript>().tileBeingOccupied;
                    moveUnit();

                    StartCoroutine(moveUnitAndFinalize());
                    //The moveUnit function calls a function on the unitScriptm when the movement is completed the finalization is called from that script. 
                }

            }
            //Finalize the movement
            else if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(2))
            {
                finalizeOption();
            }

        }

        //Unselect unit with double tap
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            tapCount += 1;
            StartCoroutine(Countdown());
        }

        if (tapCount == 2 || Input.GetMouseButtonDown(1))
        {
            tapCount = 0;
            if (selectedUnit.GetComponent<UnitScript>().unitName == "Marja" || selectedUnit.GetComponent<UnitScript>().unitName == "Peukalo" || selectedUnit.GetComponent<UnitScript>().unitName == "Kullervo")
            {
                if (selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0 && selectedUnit.GetComponent<UnitScript>().combatQueue.Count == 0)
                {
                    if (selectedUnit.GetComponent<UnitScript>().unitMoveState != selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(3))
                    {
                        deselectUnit();
                    }
                }
                else if (selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 1)
                {
                    selectedUnit.GetComponent<UnitScript>().visualMovementSpeed = 15f;
                }
            }
            StopCoroutine(Countdown());

        }

        //Trolls first Condition
        if (objectInPossess == 3)
        {
            searchedObject = true;
        }

        //trolls second condition
        if (trollsPurified == 3)
        {
            purified = true;
        }

        //if troll obtain all the objects and they are purified then can access the middle zone
        if (searchedObject == true && purified == true)
        {
            tileTypes[3].isWalkable = true;
            tileTypes[4].isWalkable = true;
            tileTypes[5].isWalkable = true;
            tileTypes[6].isWalkable = true;
            tileTypes[7].isWalkable = true;
        }

        //Trolls Third Condition
        if (tilesOnMap[4, 5].GetComponent<ClickableTileScript>().unitOnTile == null || tilesOnMap[5, 4].GetComponent<ClickableTileScript>().unitOnTile == null 
            || tilesOnMap[3, 4].GetComponent<ClickableTileScript>().unitOnTile == null)
        {

        }
        else
        {
            if (searchedObject == true && purified == true
                && (tilesOnMap[4, 5].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Peukalo") || tilesOnMap[4, 5].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Kullervo") 
                || tilesOnMap[4, 5].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Marja"))
                && (tilesOnMap[5, 4].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Peukalo") || tilesOnMap[5, 4].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Kullervo") 
                || tilesOnMap[5, 4].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Marja"))
                && (tilesOnMap[3, 4].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Peukalo") || tilesOnMap[3, 4].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Kullervo") 
                || tilesOnMap[3, 4].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Marja")))
            {
                GetComponent<gameManagerScript>().TrollWin();
            }
        }
    }
    
    //This is from quill18Create's tutorial
    //You can find it by searching for grid based movement on youtube, he goes into explaining how everything works
    //The map layouts a bit different
    //all this does is set the tiles[x,y] to the corresponding tile
    public void generateMapInfo()
    {
        tiles = new int[mapSizeX , mapSizeY];
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                tiles[x, y] = 1;
            }
        }

        //obstales
        tiles[2, 8] = 2;
        tiles[0, 3] = 12;
        tiles[4, 2] = 13;
        tiles[8, 0] = 14;
        tiles[8, 4] = 15;
        tiles[5, 6] = 16;

        //middle zone
        tiles[3, 3] = 3;
        tiles[3, 4] = 4;
        tiles[3, 5] = 3;
        tiles[4, 3] = 3;
        tiles[4, 4] = 7;
        tiles[4, 5] = 5;
        tiles[5, 3] = 3;
        tiles[5, 4] = 6;
        tiles[5, 5] = 3;
    }

    //Creates the graph for the pathfinding, it sets up the neighbours
    //This is also from Quill18Create's tutorial
    public void generatePathFindingGraph()
    {
        graph = new Node[mapSizeX, mapSizeY];

        //initialize graph 
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }
        //calculate neighbours
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {              
                //X is not 0, then we can add left (x - 1)
                if (x > 0)
                {                   
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                }
                //X is not mapSizeX - 1, then we can add right (x + 1)
                if (x < mapSizeX-1)
                {                   
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                }
                //Y is not 0, then we can add downwards (y - 1 ) 
                if (y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                }
                //Y is not mapSizeY -1, then we can add upwards (y + 1)
                if (y < mapSizeY - 1)
                {
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
                }
               
               
            }
        }
    }

    //In: 
    //Out: void
    //Desc: This instantiates all the information for the map, the UI Quads and the map tiles
    public void generateMapVisuals()
    {
        //generate list of actual tileGameObjects
        tilesOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMap = new GameObject[mapSizeX, mapSizeY];
        quadOnMapForUnitMovementDisplay = new GameObject[mapSizeX, mapSizeY];
        quadOnMapCursor = new GameObject[mapSizeX, mapSizeY];
        int index;
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                index = tiles[x, y];
                GameObject newTile = Instantiate(tileTypes[index].tileVisualPrefab, new Vector3(x * tileSize, 0, y * tileSize), Quaternion.identity);
                newTile.GetComponent<ClickableTileScript>().tileX = x;
                newTile.GetComponent<ClickableTileScript>().tileY = y;
                newTile.GetComponent<ClickableTileScript>().map = this;
                newTile.transform.SetParent(tileContainer.transform);
                tilesOnMap[x, y] = newTile;
              
                GameObject gridUI = Instantiate(mapUI, new Vector3(x * tileSize, 0.501f, y * tileSize),Quaternion.Euler(90f,0,0));
                gridUI.transform.SetParent(UIQuadPotentialMovesContainer.transform);
                quadOnMap[x, y] = gridUI;
                GameObject gridUICursor = Instantiate(mapCursorUI, new Vector3(x * tileSize, 0.503f, y * tileSize), Quaternion.Euler(90f, 0, 0));
                gridUICursor.transform.SetParent(UIQuadCursorContainer.transform);              
                quadOnMapCursor[x, y] = gridUICursor;

            }
        }
    }

    //Moves the unit
    public void moveUnit()
    {
        if (selectedUnit != null)
        {
            selectedUnit.GetComponent<UnitScript>().MoveNextTile();

            GetComponent<gameManagerScript>().trapCardButtom.interactable = false;
            GetComponent<gameManagerScript>().rockBottom.interactable = false;
            GetComponent<gameManagerScript>().helperBottom.interactable = false;
        }
        
    }

    //In: the x and y of a tile
    //Out: vector 3 of the tile in world space, theyre .75f off of zero
    //Desc: returns a vector 3 of the tile in world space, theyre .75f off of zero
    public Vector3 tileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, 0.75f, y);
    }

    //In: 
    //Out: void
    //Desc: sets the tile as occupied, if a unit is on the tile
    public void setIfTileIsOccupied()
    {
        foreach (Transform team in unitsOnBoard.transform)
        {
            foreach (Transform unitOnTeam in team) { 
                int unitX= unitOnTeam.GetComponent<UnitScript>().x;
                int unitY = unitOnTeam.GetComponent<UnitScript>().y;
                unitOnTeam.GetComponent<UnitScript>().tileBeingOccupied = tilesOnMap[unitX, unitY];
                tilesOnMap[unitX, unitY].GetComponent<ClickableTileScript>().unitOnTile = unitOnTeam.gameObject;
            }
            
        }
    }

    //In: x and y position of the tile to move to
    //Out: void
    //Desc: generates the path for the selected unit
    //Think this one is also partially from Quill18Create's tutorial
    public void generatePathTo(int x, int y)
    {

        if (selectedUnit.GetComponent<UnitScript>().x == x && selectedUnit.GetComponent<UnitScript>().y == y)
        {
            currentPath = new List<Node>();
            selectedUnit.GetComponent<UnitScript>().path = currentPath;

            return;
        }
        if (unitCanEnterTile(x, y) == false)
        {
            //cant move into something so we can probably just return
            //cant set this endpoint as our goal
            return;
        }

        selectedUnit.GetComponent<UnitScript>().path = null;
        currentPath = null;
        //from wiki dijkstra's
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        Node target = graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        //Unchecked nodes
        List<Node> unvisited = new List<Node>();

        //Initialize
        foreach (Node n in graph)
        {
            //Initialize to infite distance as we don't know the answer
            //Also some places are infinity
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        //if there is a node in the unvisited list lets check it
        while (unvisited.Count > 0)
        {
            //u will be the unvisited node with the shortest distance
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {

                //float alt = dist[u] + u.DistanceTo(n);
                float alt = dist[u] + costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        //if were here we found shortest path, or no path exists
        if (prev[target] == null)
        {
            //No route;
            return;
        }
        currentPath = new List<Node>();
        Node curr = target;
        //Step through the current path and add it to the chain
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }
        //Now currPath is from target to our source, we need to reverse it from source to target.
        currentPath.Reverse();

        selectedUnit.GetComponent<UnitScript>().path = currentPath;
        
        //infect the tiles with Ajattara and the Helper
        if ((selectedUnit.GetComponent<UnitScript>().unitName == "Ajattara" || selectedUnit.GetComponent<UnitScript>().unitName == "Helper"))
        {
            foreach (Node n in currentPath)
            {
                tiles[n.x, n.y] = 0;
            }
        }
        
    }

    //In: tile's x and y position
    //Out: cost that is requiredd to enter the tile
    //Desc: checks the cost of the tile for a unit to enter
    public float costToEnterTile(int x, int y)
    {

        if (unitCanEnterTile(x, y) == false)
        {
            return Mathf.Infinity;
        }

        //Gotta do the math here
        Tile t = tileTypes[tiles[x, y]];
        float dist = t.movementCost;

        return dist;
    }

    //change this when we add movement types
    //In:  tile's x and y position
    //Out: true or false if the unit can enter the tile that was entered
    //Desc: if the tile is not occupied by another team's unit, then you can walk through and if the tile is walkable 
    public bool unitCanEnterTile(int x, int y)
    {
        if (tilesOnMap[x, y].GetComponent<ClickableTileScript>().unitOnTile != null)
        {
            if (tilesOnMap[x, y].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
            {
                return false;
            }
        }
        return tileTypes[tiles[x, y]].isWalkable;
    }
    
    //In:  
    //Out: void
    //Desc: uses a raycast to see where the mouse is pointing, this is used to select units
    public void mouseClickToSelectUnit()
    {
        GameObject tempSelectedUnit;
        
        RaycastHit hit;       
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
       
        if (Physics.Raycast(ray, out hit))
        {
            if (unitSelected == false)
            {
                if (hit.transform.gameObject.CompareTag("Tile"))
                {
                    if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != null)
                    {
                        tempSelectedUnit = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;
                        if (tempSelectedUnit.GetComponent<UnitScript>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript>().getMovementStateEnum(0)
                            && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam)
                        {
                            disableHighlightUnitRange();
                            selectedUnit = tempSelectedUnit;
                            selectedUnit.GetComponent<UnitScript>().map = this;
                            selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                            unitSelected = true;
                            
                            highlightUnitRange();
                        }
                    }
                }

                else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
                {
                    
                    tempSelectedUnit = hit.transform.parent.gameObject;
                    if (tempSelectedUnit.GetComponent<UnitScript>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript>().getMovementStateEnum(0)
                          && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam)
                    {
                        disableHighlightUnitRange();
                        selectedUnit = tempSelectedUnit;
                        selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                        selectedUnit.GetComponent<UnitScript>().map = this;
                        unitSelected = true;
                       
                        highlightUnitRange();
                    }
                }
            }

        }
    }

    //In:  
    //Out: void
    //Desc: finalizes the movement, sets the tile the unit moved to as occupied, etc
    public void finalizeMovementPosition()
    {
        tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTileScript>().unitOnTile = selectedUnit;
        //After a unit has been moved we will set the unitMoveState to (2) the 'Moved' state
        selectedUnit.GetComponent<UnitScript>().setMovementState(2);
       
        highlightUnitAttackOptionsFromPosition();
        highlightTileUnitIsOccupying();
    }

    //In:  
    //Out: void
    //Desc: selects a unit based on the cursor from the other script
    public void mouseClickToSelectUnitV2()
    {
        if (unitSelected == false && GMS.tileBeingDisplayed != null)
        {

            if (GMS.tileBeingDisplayed.GetComponent<ClickableTileScript>().unitOnTile != null)
            {
                GameObject tempSelectedUnit = GMS.tileBeingDisplayed.GetComponent<ClickableTileScript>().unitOnTile;
                if (tempSelectedUnit.GetComponent<UnitScript>().unitMoveState == tempSelectedUnit.GetComponent<UnitScript>().getMovementStateEnum(0)
                    && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam)
                {
                    disableHighlightUnitRange();
                    selectedUnit = tempSelectedUnit;
                    selectedUnit.GetComponent<UnitScript>().map = this;
                    selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                    unitSelected = true;
                    highlightUnitRange();
                }
            }
        }
        
    }

    //In:  
    //Out: void
    //Desc: finalizes the player's option, wait or attack
    public void finalizeOption()
    {
        //verifying if the troll are in a infected tile or left a infected tile to substract or add actions points
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                int tileX = tilesOnMap[x, y].GetComponent<ClickableTileScript>().tileX;
                int tileY = tilesOnMap[x, y].GetComponent<ClickableTileScript>().tileY;

                if (tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile != null)
                {
                    if (peukaloEnteredPutrifiedTile == true && tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Peukalo")
                        && (tiles[tileX, tileY] == 1 || tiles[tileX, tileY] == 8 
                        || tiles[tileX, tileY] == 9 || tiles[tileX, tileY] == 10 || tiles[tileX, tileY] == 11))
                    {
                        if (GMS.eventText.text != "Trolls have -1 energy.")
                        {
                            tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointPlus();
                            peukaloEnteredPutrifiedTile = false;
                            peukaloExitPutrifiedTile = false;
                        }
                        else if (GMS.peukaloObj.interactable == false)
                        {
                            tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointPlus();
                            peukaloEnteredPutrifiedTile = false;
                            peukaloExitPutrifiedTile = false;
                        }
                    }
                    if ((tiles[tileX, tileY] == 1 || tiles[tileX, tileY] == 8
                        || tiles[tileX, tileY] == 9 || tiles[tileX, tileY] == 10 || tiles[tileX, tileY] == 11)
                        && marjaEnteredPutrifiedTile == true && tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Marja"))
                    {
                        if (GMS.eventText.text != "Trolls have -1 energy.")
                        {
                            tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointPlus();
                            marjaEnteredPutrifiedTile = false;
                            marjaExitPutrifiedTile = false;
                        }
                        else if(GMS.marjaObj.interactable == false)
                        {
                            tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointPlus();
                            marjaEnteredPutrifiedTile = false;
                            marjaExitPutrifiedTile = false;
                        }
                    }
                    if ((tiles[tileX, tileY] == 1 || tiles[tileX, tileY] == 8
                        || tiles[tileX, tileY] == 9 || tiles[tileX, tileY] == 10 || tiles[tileX, tileY] == 11)
                        && kullervoEnteredPutrifiedTile == true && tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Kullervo"))
                    {
                        if (GMS.eventText.text != "Trolls have -1 energy.")
                        {
                            tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointPlus();
                            kullervoEnteredPutrifiedTile = false;
                            kullervoExitPutrifiedTile = false;
                        }
                        else if(GMS.kullervoObj.interactable == false)
                        {
                            tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointPlus();
                            kullervoEnteredPutrifiedTile = false;
                            kullervoExitPutrifiedTile = false;
                        }
                    }
                    if ((tiles[tileX, tileY] == 0 || tiles[tileX, tileY] == 23 || tiles[tileX, tileY] == 24 || tiles[tileX, tileY] == 25 || tiles[tileX, tileY] == 26)
                        && peukaloExitPutrifiedTile == false && tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Peukalo"))
                    {
                        tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointSub();
                        peukaloEnteredPutrifiedTile = true;
                        peukaloExitPutrifiedTile = true;
                    }
                    if((tiles[tileX, tileY] == 0 || tiles[tileX, tileY] == 23 || tiles[tileX, tileY] == 24 || tiles[tileX, tileY] == 25 || tiles[tileX, tileY] == 26)
                        && marjaExitPutrifiedTile == false && tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Marja"))
                    {
                        tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointSub();
                        marjaEnteredPutrifiedTile = true;
                        marjaExitPutrifiedTile = true;
                    }
                    if((tiles[tileX, tileY] == 0 || tiles[tileX, tileY] == 23 || tiles[tileX, tileY] == 24 || tiles[tileX, tileY] == 25 || tiles[tileX, tileY] == 26)
                        &&kullervoExitPutrifiedTile == false && tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.gameObject.name.Equals("Kullervo"))
                    {
                        tilesOnMap[tileX, tileY].GetComponent<ClickableTileScript>().unitOnTile.GetComponent<UnitScript>().ActionPointSub();
                        kullervoEnteredPutrifiedTile = true;
                        kullervoExitPutrifiedTile = true;
                    }
                }
            }
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        HashSet<Node> attackableTiles = getUnitAttackOptionsFromPosition();

        if (Physics.Raycast(ray, out hit))
        {

            //This portion is to ensure that the tile has been clicked
            //If the tile has been clicked then we need to check if there is a unit on it
            if (hit.transform.gameObject.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != null)
                {
                    GameObject unitOnTile = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;
                    int unitX = unitOnTile.GetComponent<UnitScript>().x;
                    int unitY = unitOnTile.GetComponent<UnitScript>().y;
                
                    if (unitOnTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX,unitY]))
                    {
                        if (unitOnTile.GetComponent<UnitScript>().currentHealthPoints > 0)
                        {
                            StartCoroutine(BMS.attack(selectedUnit, unitOnTile));

                            StartCoroutine(deselectAfterMovements(selectedUnit, unitOnTile));

                            GetComponent<gameManagerScript>().selectedXTile = 0;
                            GetComponent<gameManagerScript>().selectedYTile = 0;
                            unitSelectedPreviousX = 0;
                            unitSelectedPreviousY = 0;
                            GetComponent<gameManagerScript>().cursorX = 0;
                            GetComponent<gameManagerScript>().cursorY = 0;
                            GMS.tileBeingDisplayed = null;
                        }
                    }                                     
                }
            }
            else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
            {
                GameObject unitClicked = hit.transform.parent.gameObject;
                int unitX = unitClicked.GetComponent<UnitScript>().x;
                int unitY = unitClicked.GetComponent<UnitScript>().y;

                if (unitClicked == selectedUnit)
                {
                    if(selectedUnit.GetComponent<UnitScript>().unitName == "Ajattara" || selectedUnit.GetComponent<UnitScript>().unitName == "Helper")
                    {
                        disableHighlightUnitRange();
                        selectedUnit.GetComponent<UnitScript>().wait();
                        selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                        deselectUnit();

                        GetComponent<gameManagerScript>().selectedXTile = 0;
                        GetComponent<gameManagerScript>().selectedYTile = 0;
                        unitSelectedPreviousX = 0;
                        unitSelectedPreviousY = 0;
                        GetComponent<gameManagerScript>().cursorX = 0;
                        GetComponent<gameManagerScript>().cursorY = 0;
                        GMS.tileBeingDisplayed = null;
                    }
                    else
                    {
                        disableHighlightUnitRange();
                        selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                        deselectUnit();

                        GetComponent<gameManagerScript>().selectedXTile = 0;
                        GetComponent<gameManagerScript>().selectedYTile = 0;
                        unitSelectedPreviousX = 0;
                        unitSelectedPreviousY = 0;
                        GetComponent<gameManagerScript>().cursorX = 0;
                        GetComponent<gameManagerScript>().cursorY = 0;
                        GMS.tileBeingDisplayed = null;
                    }

                }
                else if (unitClicked.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
                {
                    if (unitClicked.GetComponent<UnitScript>().currentHealthPoints > 0)
                    {
                        StartCoroutine(BMS.attack(selectedUnit, unitClicked));

                        StartCoroutine(deselectAfterMovements(selectedUnit, unitClicked));

                        GetComponent<gameManagerScript>().selectedXTile = 0;
                        GetComponent<gameManagerScript>().selectedYTile = 0;
                        unitSelectedPreviousX = 0;
                        unitSelectedPreviousY = 0;
                        GMS.tileBeingDisplayed = null;
                    }
                }

            }
            else if (hit.transform.parent != null && hit.transform.parent.gameObject.tag == ("Rock"))
            {
                disableHighlightUnitRange();
                selectedUnit.GetComponent<UnitScript>().wait();
                selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                deselectUnit();
                GameObject.Find("Rock").GetComponent<Rock>().rockExplosion.Play();
                Destroy(GameObject.Find("Rock"), 0.5f);
                gameObject.GetComponent<gameManagerScript>().availableRocks += 1;

                if (GMS.availableRocks >= 3)
                {
                    GMS.availableRocks = 3;
                }
            }
        }
    }

    //In:  
    //Out: void
    //Desc: de-selects the unit
    public void deselectUnit()
    {
        if (selectedUnit != null)
        {
            if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1))
            {
                disableHighlightUnitRange();
                selectedUnit.GetComponent<UnitScript>().setMovementState(0);

       
                selectedUnit = null;
                unitSelected = false;

                GetComponent<gameManagerScript>().selectedXTile = 0;
                GetComponent<gameManagerScript>().selectedYTile = 0;
                unitSelectedPreviousX = 0;
                unitSelectedPreviousY = 0;
                GetComponent<gameManagerScript>().cursorX = 0;
                GetComponent<gameManagerScript>().cursorY = 0;
                GMS.tileBeingDisplayed = null;
            }
            else if (selectedUnit.GetComponent<UnitScript>().unitMoveState == selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(3) )
            {
                disableHighlightUnitRange();
                //disableUnitUIRoute();
                unitSelected = false;
                selectedUnit = null;

                StartCoroutine(UpdateMap());

                GetComponent<gameManagerScript>().selectedXTile = 0;
                GetComponent<gameManagerScript>().selectedYTile = 0;
                unitSelectedPreviousX = 0;
                unitSelectedPreviousY = 0;
                GetComponent<gameManagerScript>().cursorX = 0;
                GetComponent<gameManagerScript>().cursorY = 0;
                GMS.tileBeingDisplayed = null;
            }
            else
            {
                disableHighlightUnitRange();
                tilesOnMap[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y].GetComponent<ClickableTileScript>().unitOnTile = null;
                tilesOnMap[unitSelectedPreviousX, unitSelectedPreviousY].GetComponent<ClickableTileScript>().unitOnTile = selectedUnit;

                selectedUnit.GetComponent<UnitScript>().x = unitSelectedPreviousX;
                selectedUnit.GetComponent<UnitScript>().y = unitSelectedPreviousY;
                selectedUnit.GetComponent<UnitScript>().rotationY = unitSelectedPreviousRotationY;
                selectedUnit.GetComponent<UnitScript>().tileBeingOccupied = previousOccupiedTile;
                selectedUnit.transform.position = tileCoordToWorldCoord(unitSelectedPreviousX * 2, unitSelectedPreviousY * 2);
                selectedUnit.GetComponent<UnitScript>().setMovementState(0);
                selectedUnit = null;
                unitSelected = false;

                GetComponent<gameManagerScript>().selectedXTile = 0;
                GetComponent<gameManagerScript>().selectedYTile = 0;
                unitSelectedPreviousX = 0;
                unitSelectedPreviousY = 0;
                GetComponent<gameManagerScript>().cursorX = 0;
                GetComponent<gameManagerScript>().cursorY = 0;
                GMS.tileBeingDisplayed = null;

                if (GetComponent<gameManagerScript>().currentTeam == 1)
                {
                    if (GetComponent<gameManagerScript>().cooldownRocks == 0)
                    {
                        GetComponent<gameManagerScript>().rockBottom.interactable = true;
                    }
                    if (GetComponent<gameManagerScript>().cooldownTraps == 0)
                    {
                        GetComponent<gameManagerScript>().trapCardButtom.interactable = true;
                    }
                    if (GetComponent<gameManagerScript>().cooldownHelper == 0)
                    {
                        GetComponent<gameManagerScript>().helperBottom.interactable = true;

                    }
                }
            }

            GetComponent<gameManagerScript>().selectedXTile = 0;
            GetComponent<gameManagerScript>().selectedYTile = 0;
            unitSelectedPreviousX = 0;
            unitSelectedPreviousY = 0;
            GetComponent<gameManagerScript>().cursorX = 0;
            GetComponent<gameManagerScript>().cursorY = 0;
        }
    }

    //In:  
    //Out: void
    //Desc: highlights the units range options (this is the portion shown in the video)
    public void highlightUnitRange()
    {
       
       
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        HashSet<Node> finalEnemyUnitsInMovementRange = new HashSet<Node>();
      
        int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;


        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        finalMovementHighlight = getUnitMovementOptions();
        totalAttackableTiles = getUnitTotalAttackableTiles(finalMovementHighlight, attRange, unitInitialNode);

        foreach (Node n in totalAttackableTiles)
        {

            if (tilesOnMap[n.x, n.y].GetComponent<ClickableTileScript>().unitOnTile != null)
            {
                GameObject unitOnCurrentlySelectedTile = tilesOnMap[n.x, n.y].GetComponent<ClickableTileScript>().unitOnTile;
                if (unitOnCurrentlySelectedTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
                {
                    finalEnemyUnitsInMovementRange.Add(n);
                }
            }
        }

        highlightEnemiesInRange(totalAttackableTiles);
        highlightMovementRange(finalMovementHighlight);
        selectedUnitMoveRange = finalMovementHighlight;

        //This final bit sets the selected Units tiles, which can be accessible in other functions
        //Probably bad practice, but I'll need to get things to work for now (2019-09-30)
        selectedUnitTotalRange = getUnitTotalRange(finalMovementHighlight, totalAttackableTiles);
    }


    //In:  
    //Out: void
    //Desc: disables the quads that are being used to highlight position
    public void disableUnitUIRoute()
    {
        foreach(GameObject quad in quadOnMapForUnitMovementDisplay)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    //In:  
    //Out: HashSet<Node> of the tiles that can be reached by unit
    //Desc: returns the hashSet of nodes that the unit can reach from its position
    public HashSet<Node> getUnitMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();      
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        ///Set-up the initial costs for the neighbouring nodes
        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);

            if (moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (!finalMovementHighlight.Contains(neighbour))
                    {
                        cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];

                        if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                        {
                            tempUIHighlight.Add(neighbour);
                        }
                    }
                }

            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();
           
        }
        return finalMovementHighlight;
    }

    //In:  finalMovement highlight and totalAttackabletiles
    //Out: a hashSet of nodes that are the combination of the two inputs
    //Desc: returns the unioned hashSet
    public HashSet<Node> getUnitTotalRange(HashSet<Node> finalMovementHighlight, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> unionTiles = new HashSet<Node>();
        unionTiles.UnionWith(finalMovementHighlight);
        unionTiles.UnionWith(totalAttackableTiles);
        return unionTiles;
    }

    //In:  finalMovement highlight, the attack range of the unit, and the initial node that the unit was standing on
    //Out: hashSet Node of the total attackable tiles for the unit
    //Desc: returns a set of nodes that represent the unit's total attackable tiles
    public HashSet<Node> getUnitTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attRange, Node unitInitialNode)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        foreach (Node n in finalMovementHighlight)
        {
            neighbourHash = new HashSet<Node>();
            neighbourHash.Add(n);
            for (int i = 0; i < attRange; i++)
            {
                foreach (Node t in neighbourHash)
                {
                    foreach (Node tn in t.neighbours)
                    {
                        tempNeighbourHash.Add(tn);
                    }
                }

                neighbourHash = tempNeighbourHash;
                tempNeighbourHash = new HashSet<Node>();
                if (i < attRange - 1)
                {
                    seenNodes.UnionWith(neighbourHash);
                }

            }
            neighbourHash.ExceptWith(seenNodes);
            seenNodes = new HashSet<Node>();
            totalAttackableTiles.UnionWith(neighbourHash);
        }
        totalAttackableTiles.Remove(unitInitialNode);
        
        return (totalAttackableTiles);
    }

    //In:  
    //Out: hashSet of nodes get all the attackable tiles from the current position
    //Desc: returns a set of nodes that are all the attackable tiles from the units current position
    public HashSet<Node> getUnitAttackOptionsFromPosition()
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node initialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;


        neighbourHash = new HashSet<Node>();
        neighbourHash.Add(initialNode);
        for (int i = 0; i < attRange; i++)
        {
            foreach (Node t in neighbourHash)
            {
                foreach (Node tn in t.neighbours)
                {
                    tempNeighbourHash.Add(tn);
                }
            }
            neighbourHash = tempNeighbourHash;
            tempNeighbourHash = new HashSet<Node>();
            if (i < attRange - 1)
            {
                seenNodes.UnionWith(neighbourHash);
            }
        }
        neighbourHash.ExceptWith(seenNodes);
        neighbourHash.Remove(initialNode);
        return neighbourHash;
    }

    //In:  
    //Out: hashSet node that the unit is currently occupying
    //Desc: returns a set of nodes of the tile that the unit is occupying
    public HashSet<Node> getTileUnitIsOccupying()
    {
        int x = selectedUnit.GetComponent<UnitScript>().x;
        int y = selectedUnit.GetComponent<UnitScript>().y;
        HashSet<Node> singleTile = new HashSet<Node>();
        singleTile.Add(graph[x, y]);
        return singleTile;
    }

    //In:  
    //Out: void
    //Desc: highlights the selected unit's options
    public void highlightTileUnitIsOccupying()
    {
        if (selectedUnit != null)
        {
            highlightMovementRange(getTileUnitIsOccupying());
        }
    }

    //In:  
    //Out: void
    //Desc: highlights the selected unit's attackOptions from its position
    public void highlightUnitAttackOptionsFromPosition()
    {
        if (selectedUnit != null)
        {
            highlightEnemiesInRange(getUnitAttackOptionsFromPosition());
        }
    }

    //In:  Hash set of the available nodes that the unit can range
    //Out: void - it changes the quadUI property in the gameworld to visualize the selected unit's movement
    //Desc: This function highlights the selected unit's movement range
    public void highlightMovementRange(HashSet<Node> movementToHighlight)
    {
        foreach (Node n in movementToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = blueUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    //In:  Hash set of the enemies in range of the selected Unit
    //Out: void - it changes the quadUI property in the gameworld to visualize an enemy
    //Desc: This function highlights the enemies in range once they have been added to a hashSet
    public void highlightEnemiesInRange(HashSet<Node> enemiesToHighlight)
    {
        foreach (Node n in enemiesToHighlight)
        {
            quadOnMap[n.x, n.y].GetComponent<Renderer>().material = redUIMat;
            quadOnMap[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }

    //In:  
    //Out: void 
    //Desc: disables the highlight
    public void disableHighlightUnitRange()
    {
        foreach(GameObject quad in quadOnMap)
        {
            if(quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    //In:  
    //Out: void 
    //Desc: moves the unit then finalizes the movement
    public IEnumerator moveUnitAndFinalize()
    {
        disableHighlightUnitRange();
        //disableUnitUIRoute();
        while (selectedUnit.GetComponent<UnitScript>().movementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        finalizeMovementPosition();
    }


    //In:  both units engaged in a battle
    //Out:  
    //Desc: deselects the selected unit after the action has been taken
    public IEnumerator deselectAfterMovements(GameObject unit, GameObject enemy)
    {
        selectedUnit.GetComponent<UnitScript>().setMovementState(3);
        disableHighlightUnitRange();
        //If i dont have this wait for seconds the while loops get passed as the coroutine has not started from the other script
        //Adding a delay here to ensure that it all works smoothly. (probably not the best idea)
        yield return new WaitForSeconds(.25f);

        while (unit.GetComponent<UnitScript>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        while (enemy.GetComponent<UnitScript>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
          
        }
       
        deselectUnit();
    }

    //In:  
    //Out: true if there is a tile that was clicked that the unit can move to, false otherwise 
    //Desc: checks if the tile that was clicked is move-able for the selected unit
    public bool selectTileToMoveTo()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Tile")){
               
                int clickedTileX = hit.transform.GetComponent<ClickableTileScript>().tileX;
                int clickedTileY = hit.transform.GetComponent<ClickableTileScript>().tileY;
                Node nodeToCheck = graph[clickedTileX, clickedTileY];

                if (selectedUnitMoveRange.Contains(nodeToCheck)) {
                    if ((hit.transform.gameObject.GetComponent<ClickableTileScript>().unitOnTile == null || hit.transform.gameObject.GetComponent<ClickableTileScript>().unitOnTile == selectedUnit) && (selectedUnitMoveRange.Contains(nodeToCheck)))
                    {
                        generatePathTo(clickedTileX, clickedTileY);
                        return true;
                    }
                }
            }
            else if (hit.transform.gameObject.CompareTag("Unit"))
            {
                if (hit.transform.parent.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
                {

                }
                else if(hit.transform.parent.gameObject == selectedUnit)
                {
                    generatePathTo(selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y);
                    return true;
                }
            }

        }
        return false;
    }

    public IEnumerator UpdateMap()
    {
        /* Ajattara first condition */
        //Ajattara base infected
        if (ajattaraCanEnterTheCenter == true && ajattaraInfected == false && tiles[0, 0] == 0 && tiles[0, 1] == 0 && tiles[0, 2] == 0 && tiles[1, 0] == 0 && tiles[2, 0] == 0 && tiles[1, 1] == 0 && tiles[1, 2] == 0 
            && tiles[2, 1] == 0 && tiles[2, 2] == 0)
        {
            ajattaraZonePure.SetActive(false);
            ajattaraZoneInfected.SetActive(true);
            ajattaraInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //Troll base infected
        if (tiles[6, 6] == 0 && trollInfected == false && tiles[7, 6] == 0 && tiles[8, 6] == 0 && tiles[6, 7] == 0 && tiles[7, 7] == 0 && tiles[8, 7] == 0 && tiles[6, 8] == 0 && tiles[7, 8] == 0 && tiles[8, 8] == 0)
        {
            trollZonePure.SetActive(false);
            trollZoneInfected.SetActive(true);
            trollInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //First Zone infected
        if (tiles[3, 0] == 0 && firstInfected == false && tiles[3, 1] == 0 && tiles[3, 2] == 0 && tiles[4, 0] == 0 && tiles[4, 1] == 0 /*&& tiles[4, 2] == 0 tree*/&& tiles[5, 0] == 0 && tiles[5, 1] == 0 && tiles[5, 2] == 0)
        {
            firstZonePure.SetActive(false);
            firstZoneInfected.SetActive(true);
            tiles[4, 2] = 19;
            firstInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //Second Zone Infected
        if (tiles[6, 0] == 0 && secondInfected == false && tiles[6, 1] == 0 && tiles[6, 2] == 0 && tiles[7, 0] == 0 && tiles[7, 1] == 0 && tiles[7, 2] == 0 /*&& tiles[8, 0] == 0 tree */&& tiles[8, 1] == 0 && tiles[8, 2] == 0)
        {
            secondZonePure.SetActive(false);
            secondZoneInfected.SetActive(true);
            tiles[8, 0] = 20;
            secondInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //Third Zone Infected
        if (tiles[6, 3] == 0 && thirdInfected == false && tiles[7, 3] == 0 && tiles[8, 3] == 0 && tiles[6, 4] == 0 && tiles[7, 4] == 0 /*&& tiles[8, 4] == 0 tree*/&& tiles[6, 5] == 0 && tiles[7, 5] == 0 && tiles[8, 5] == 0)
        {
            thirdZonePure.SetActive(false);
            thirdZoneInfected.SetActive(true);
            tiles[8, 4] = 21;
            thirdInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //Fourth Zone Infected
        if (tiles[3, 6] == 0 && fourthInfected == false && tiles[3, 7] == 0 && tiles[3, 8] == 0 && tiles[4, 6] == 0 && tiles[4, 7] == 0 && tiles[4, 8] == 0 /*&& tiles[5, 6] == 0 tree*/&& tiles[5, 7] == 0 && tiles[5, 8] == 0)
        {
            fourthZonePure.SetActive(false);
            fourthZoneInfected.SetActive(true);
            tiles[5, 6] = 22;
            fourthInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //Fifth Zone infected
        if (tiles[0, 6] == 0 && fifthInfected == false && tiles[0, 7] == 0 && tiles[0, 8] == 0 && tiles[1, 6] == 0 && tiles[1, 7] == 0 && tiles[1, 8] == 0 && tiles[2, 6] == 0 && tiles[2, 7] == 0 /*&& tiles[2, 8] == 0 tree*/)
        {
            fifthZonePure.SetActive(false);
            fifthZoneInfected.SetActive(true);
            tiles[2, 8] = 17;
            fifthInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //Sixth Zone Infected
        if (/*tiles[0, 3] == 0 && tree*/sixthInfected == false && tiles[0, 4] == 0 && tiles[0, 5] == 0 && tiles[1, 3] == 0 && tiles[1, 4] == 0 && tiles[1, 5] == 0 && tiles[2, 3] == 0 && tiles[2, 4] == 0 && tiles[2, 5] == 0)
        {
            sixthZonePure.SetActive(false);
            sixtheZoneInfected.SetActive(true);
            tiles[0, 3] = 18;
            sixthInfected = true;
            GameObject.Find("Main Camera").GetComponent<camShakeScript>().ZonePutrified();
        }
        //All the map infected
        if (ajattaraInfected == true && trollInfected == true && firstInfected == true && secondInfected == true && thirdInfected == true && fourthInfected == true && fifthInfected == true && sixthInfected == true 
            && ajattaraCanEnterTheCenter == true)
        {
            AjattaraCanEnterTheCenter();
            explosionBarrier.Play();
            Destroy(magicBarrier, 2);
            Destroy(magicBarrier1, 2);
            Destroy(magicBarrier2, 2);
            Destroy(goodSpirits, 2);
            badSpirits.Play();
        }

        //setting spcial infected tiles in the middle zone
        if (tiles[4, 5] == 0)
        {
            tiles[4, 5] = 24;
        }
        if (tiles[5, 4] == 0)
        {
            tiles[5, 4] = 25;
        }
        if (tiles[3, 4] == 0)
        {
            tiles[3, 4] = 23;
        }
        if (tiles[4, 4] == 0)
        {
            tiles[4, 4] = 26;
        }

        //Ajattara second win condition
        if (tiles[4, 4] == 26)
        {
            GetComponent<gameManagerScript>().AjattaraWin();
        }

        //Remove old tiles
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Destroy(tilesOnMap[x, y]);
            }
        }

        generatePathFindingGraph();
        generateMapVisuals();
        setIfTileIsOccupied();
        yield return new WaitForSeconds(1f);
    }

    public void ChangeVisualMap()
    {
       if(slider.value == 0)
       {
            currentBoard.SetActive(true);
            fakeBoard.SetActive(false);
            GMS.GetComponent<gameManagerScript>().team2.SetActive(true);
       }
       if(slider.value == 1)
       {
            currentBoard.SetActive(false);
            fakeBoard.SetActive(true);
            GMS.GetComponent<gameManagerScript>().team2.SetActive(false);
       }
       else if (sliderClose.value == 0)
       {
           currentBoard.SetActive(true);
           fakeBoard.SetActive(false);
           GMS.GetComponent<gameManagerScript>().team2.SetActive(true);
       }
       if (sliderClose.value == 1)
       {
           currentBoard.SetActive(false);
           fakeBoard.SetActive(true);
           GMS.GetComponent<gameManagerScript>().team2.SetActive(false);
       }
    }

    public void AjattaraCanEnterTheCenter()
    {
        tiles[3, 3] = 1;
        tiles[3, 4] = 9;
        tiles[3, 5] = 1;
        tiles[4, 3] = 1;
        tiles[4, 4] = 8;
        tiles[4, 5] = 10;
        tiles[5, 3] = 1;
        tiles[5, 4] = 11;
        tiles[5, 5] = 1;

        ajattaraCanEnterTheCenter = false;
    }

    public void OneObjectFound()
    {
        objectInPossess = objectInPossess + 1; 
    }

    public void PurifiedTroll()
    {
        trollsPurified = trollsPurified + 1;
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(0.3f);
        tapCount = 0;
    }
}
