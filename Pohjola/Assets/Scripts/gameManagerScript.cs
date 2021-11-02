using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class gameManagerScript : MonoBehaviour
{
   
    //A lot of the UI does not need to be public, they just are currently if you need to make quick changes in the inspector
    //Changing them to private will not break anything, but you will need to re-enable them to show in the inspector
    [Header("UI GameObjects")]
    public TMP_Text currentTeamUI;
    public Canvas trollWinnerUI;
    public Canvas ajattaraWinnerUI;

    public GameObject playerPhaseBlock;
    private Animator playerPhaseAnim;
    private TMP_Text playerPhaseText;
   
    //Raycast for the update for unitHover info
    private Ray ray;
    private RaycastHit hit;
   
    /// The number of teams is hard coded as 2, if there are changes in the future a few of the
    /// functions in this class need to be altered as well to update this change.
   
    public int numberOfTeams = 2;
    public int currentTeam;
    public GameObject unitsOnBoard;

    public GameObject team1;
    public GameObject team2;

    public GameObject unitBeingDisplayed;
    public GameObject tileBeingDisplayed;
    public bool displayingUnitInfo;

    public tileMapScript TMS;

    //Cursor Info for tileMapScript
    public int cursorX;
    public int cursorY;
    //currentTileBeingMousedOver
    public int selectedXTile;
    public int selectedYTile;

    //Variables for unitPotentialMovementRoute
    List<Node> currentPathForUnitRoute;
    List<Node> unitPathToCursor;

    public bool unitPathExists;

    public Material UIunitRoute;
    public Material UIunitRouteCurve;
    public Material UIunitRouteArrow;
    public Material UICursor;

    public int routeToX;
    public int routeToY;

    //This game object is to record the location of the 2 count path when it is reset to 0 this is used to remember what tile to disable
    public GameObject quadThatIsOneAwayFromUnit;

    [Header("Events")]

    public Button endTurnButton;
    public TMP_Text eventText;
    public TMP_Text eventTextMenu;
    public List<string> events = new List<string>();
    public GameObject eventPanel;

    public int eventCooldown = 6;

    [Header("Trolls")]

    public Canvas marjaCanvas;
    public Canvas kullervoCanvas;
    public Canvas peukaloCanvas;

    public Button marjaObj;
    public Button peukaloObj;
    public Button kullervoObj;

    [Header("Ajattara")]

    public GameObject Ajattara;
    public Canvas ajattaraCanvas;
    public Canvas helperCanvas;

    public Button rockBottom;
    public Button trapCardButtom;
    public Button helperBottom;
    public Button openMenu;

    public TMP_Text showAvaibleRock;
    public TMP_Text showAvaibleTrap;
    public TMP_Text showAvaibleHelper;
    public TMP_Text showCooldownRock;
    public TMP_Text showCooldownTrap;
    public TMP_Text showCooldownHelper;
    public TMP_Text eventCooldownText;

    public GameObject rock;
    public GameObject trapCard;
    public List<GameObject> helper;
    public GameObject helpez;

    public Transform helperPos;

    public bool setRock = false;
    public bool setTrap = false;

    public int availableRocks = 3;
    public int availableTraps = 2;
    public int availableHelpers = 2;

    public int cooldownRocks = 0;
    public int cooldownTraps = 0;
    public int cooldownHelper = 0;

    public GameObject buttomManager;

    [Header("Camera")]

    public Camera Camera;
    protected Plane Plane;

    public bool Rotate;

    public float DecreaseCameraPanSpeed = 1;
    public float CameraUpperHeightBound;
    public float CameraLowerHeightBound;

    private Vector3 cameraStartPosition;

    public float minX;
    public float minZ;
    public float maxX;
    public float maxZ;


    private void Awake()
    {
        if (Camera == null)
            Camera = Camera.main;

        cameraStartPosition = Camera.transform.position;
    }

    public void Start()
    {
        currentTeam = 0;
        setCurrentTeamUI();
        teamHealthbarColorUpdate();
        displayingUnitInfo = false;
        playerPhaseAnim = playerPhaseBlock.GetComponent<Animator>();
        playerPhaseText = playerPhaseBlock.GetComponentInChildren<TextMeshProUGUI>();
        unitPathToCursor = new List<Node>();
        unitPathExists = false;       
      
        TMS = GetComponent<tileMapScript>();

        rockBottom.interactable = false;
        trapCardButtom.interactable = false;
        helperBottom.interactable = false;

        StartCoroutine(DisableHelper());
        StartCoroutine(DisableOpenMenu());  

        playerPhaseText.SetText("Troll Phase");
    }

    //2019/10/17 there is a small blink between disable and re-enable for path, its a bit jarring, try to fix it later
    public void Update()
    {
        //Always trying to see where the mouse is pointing
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            //Update cursorLocation and unit appearing in the topLeft
            cursorUIUpdate();
            //for pc display unit panel with mouse
            //unitUIUpdate();

            //for mobile display unit panel on hold
            if(Input.GetTouch(0).phase == TouchPhase.Began)
            {
                unitUIUpdate();
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                marjaCanvas.enabled = false;
                kullervoCanvas.enabled = false;
                peukaloCanvas.enabled = false;
                ajattaraCanvas.enabled = false;
                helperCanvas.enabled = false;
                displayingUnitInfo = false;
            }

            //If the unit is selected we want to highlight the current path with the UI
            if (TMS.selectedUnit != null && TMS.selectedUnit.GetComponent<UnitScript>().getMovementStateEnum(1) == TMS.selectedUnit.GetComponent<UnitScript>().unitMoveState)
            {
                //Check to see if the cursor is in range, we cant show movement outside of range so there is no point if its outside
                if (TMS.selectedUnitMoveRange.Contains(TMS.graph[cursorX, cursorY]))
                {
                    //Generate new path to cursor try to limit this to once per new cursor location or else its too many calculations
                    if (cursorX != TMS.selectedUnit.GetComponent<UnitScript>().x || cursorY != TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        if (!unitPathExists && TMS.selectedUnit.GetComponent<UnitScript>().movementQueue.Count == 0)
                        {
                            unitPathToCursor = generateCursorRouteTo(cursorX, cursorY);

                            routeToX = cursorX;
                            routeToY = cursorY;

                            if (unitPathToCursor.Count != 0)
                            {

                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;

                                    if (i == 0)
                                    {

                                    }
                                    else if (i != 0 && (i + 1) != unitPathToCursor.Count)
                                    {

                                    }
                                    else if (i == unitPathToCursor.Count - 1)
                                    {

                                    }
                                }
                            }
                            unitPathExists = true;
                        }
                        else if (routeToX != cursorX || routeToY != cursorY)
                        {
                            if (unitPathToCursor.Count != 0)
                            {
                                for (int i = 0; i < unitPathToCursor.Count; i++)
                                {
                                    int nodeX = unitPathToCursor[i].x;
                                    int nodeY = unitPathToCursor[i].y;
                                }
                            }

                            unitPathExists = false;
                        }
                    }
                    else if (cursorX == TMS.selectedUnit.GetComponent<UnitScript>().x && cursorY == TMS.selectedUnit.GetComponent<UnitScript>().y)
                    {
                        unitPathExists = false;
                    }
                }
            }

            //checking available rocks
            if (setRock == true && availableRocks != 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //cannot place rock in the middle zone
                    if (cursorX == 3 && cursorY == 3 || cursorX == 4 && cursorY == 3 || cursorX == 5 && cursorY == 3 || cursorX == 3 && cursorY == 4 || cursorX == 4 && cursorY == 4 || cursorX == 5 && cursorY == 4
                        || cursorX == 3 && cursorY == 5 || cursorX == 4 && cursorY == 5 || cursorX == 5 && cursorY == 5 || cursorX == 4 && cursorY == 2 || cursorX == 8 && cursorY == 0 || cursorX == 8 && cursorY == 4
                        || cursorX == 5 && cursorY == 6 || cursorX == 2 && cursorY == 8 || cursorX == 0 && cursorY == 3 || tileBeingDisplayed.GetComponent<ClickableTileScript>().unitOnTile)
                    {

                    }
                    else
                    {
                        Instantiate(rock, new Vector3(cursorX * 2, 0, cursorY * 2), transform.rotation);
                        setRock = false;
                        availableRocks = availableRocks - 1;
                        rockBottom.interactable = false;
                        endTurn();
                    }
                }
            }

            if (setTrap == true && availableTraps != 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    //cannot place traps in the middle zone
                    if (cursorX == 3 && cursorY == 3 || cursorX == 4 && cursorY == 3 || cursorX == 5 && cursorY == 3 || cursorX == 3 && cursorY == 4 || cursorX == 4 && cursorY == 4 || cursorX == 5 && cursorY == 4
                        || cursorX == 3 && cursorY == 5 || cursorX == 4 && cursorY == 5 || cursorX == 5 && cursorY == 5 || cursorX == 4 && cursorY == 2 || cursorX == 8 && cursorY == 0 || cursorX == 8 && cursorY == 4
                        || cursorX == 5 && cursorY == 6 || cursorX == 2 && cursorY == 8 || cursorX == 0 && cursorY == 3 || tileBeingDisplayed.GetComponent<ClickableTileScript>().unitOnTile)
                    {

                    }
                    else
                    {
                        Instantiate(trapCard, new Vector3(cursorX * 2, 0, cursorY * 2), transform.rotation);
                        setTrap = false;
                        availableTraps = availableTraps - 1;
                        trapCardButtom.interactable = false;
                        endTurn();
                    }
                }
            }
        }

        showAvaibleRock.text = availableRocks.ToString("Available: " + availableRocks);
        showCooldownRock.text = cooldownRocks.ToString("Cooldown: " + cooldownRocks);

        showAvaibleTrap.text = availableTraps.ToString("Available: " + availableTraps);
        showCooldownTrap.text = cooldownTraps.ToString("Cooldown: " + cooldownTraps);

        showAvaibleHelper.text = availableHelpers.ToString("Available: " + availableHelpers);
        showCooldownHelper.text = cooldownHelper.ToString("Cooldown: " + cooldownHelper);

        eventCooldownText.text = eventCooldown.ToString("Next: " + eventCooldown);

        //Camera Controller code by DitzelGames on YouTube
        //Update Plane
        if (Input.touchCount >= 1)
            Plane.SetNormalAndPosition(transform.up, transform.position);

        var Delta1 = Vector3.zero;
        var Delta2 = Vector3.zero;

        //Scroll (Pan function)
        if (Input.touchCount >= 1)
        {
            //Get distance camera should travel
            Delta1 = PlanePositionDelta(Input.GetTouch(0)) / DecreaseCameraPanSpeed;
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Camera.transform.Translate(Delta1, Space.World);

                if (Camera.transform.position.x > minX)
                {
                    Camera.transform.position = new Vector3(minX, Camera.transform.position.y, Camera.transform.position.z);
                }
                if (Camera.transform.position.z > minZ)
                {
                    Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, minZ);
                }
                
                if(Camera.transform.position.x < maxX)
                {
                    Camera.transform.position = new Vector3(maxX, Camera.transform.position.y, Camera.transform.position.z);
                }
                if(Camera.transform.position.z < maxZ)
                {
                    Camera.transform.position = new Vector3(Camera.transform.position.x, Camera.transform.position.y, maxZ);
                }
                
            }
        }

        //Pinch (Zoom Function)
        if (Input.touchCount >= 2)
        {
            var pos1 = PlanePosition(Input.GetTouch(0).position);
            var pos2 = PlanePosition(Input.GetTouch(1).position);
            var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
            var pos2b = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

            //calc zoom
            var zoom = Vector3.Distance(pos1, pos2) /
                       Vector3.Distance(pos1b, pos2b);

            //edge case
            if (zoom == 0 || zoom > 10)
                return;

            //Move cam amount the mid ray
            Vector3 camPositionBeforeAdjustment = Camera.transform.position;
            Camera.transform.position = Vector3.LerpUnclamped(pos1, Camera.transform.position, 1 / zoom);

            //Restricts zoom height 
            //Upper (ZoomOut)
            if (Camera.transform.position.y > (cameraStartPosition.y + CameraUpperHeightBound))
            {
                Camera.transform.position = camPositionBeforeAdjustment;
            }
            //Lower (Zoom in)
            if (Camera.transform.position.y < (cameraStartPosition.y - CameraLowerHeightBound) || Camera.transform.position.y <= 1)
            {
                Camera.transform.position = camPositionBeforeAdjustment;
            }
            
            //Rotation Function
            if (Rotate && pos2b != pos2)
                Camera.transform.RotateAround(pos1, Plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, Plane.normal));
        }
    }

    //In: 
    //Out: void
    //Desc: sets the current player Text in the UI
    public void setCurrentTeamUI()
    {
        if (currentTeam == 1)
        {
            currentTeamUI.SetText("Current Player is : Ajattara");
        }
        else if (currentTeam == 0)
        {
            currentTeamUI.SetText("Current Player is : Trolls");
        }
    }

    //In: 
    //Out: void
    //Desc: increments the current team
    public IEnumerator switchCurrentPlayer()
    {
        resetUnitsMovements(returnTeam(currentTeam));
        currentTeam++;
        if (currentTeam == numberOfTeams)
        {
            currentTeam = 0;

        }
        openMenu.interactable = false;
        yield return new WaitForSeconds(2f);
        openMenu.interactable = true;

    }

    //In: int i, the index for each team
    //Out: gameObject team
    //Desc: return the gameObject of the requested team
    public GameObject returnTeam(int i)
    {
        GameObject teamToReturn = null;
        if (i == 0)
        {
            teamToReturn = team1;
        }
        else if (i == 1)
        {
            teamToReturn = team2;
        }
        return teamToReturn;
    }

    //In: gameObject team - used to reset (re-enable) all the unit movements
    //Out: void
    //Desc: re-enables movement for all units on the team
    public void resetUnitsMovements(GameObject teamToReset)
    {
        foreach (Transform unit in teamToReset.transform)
        {
            unit.GetComponent<UnitScript>().moveAgain();
        }
    }

    //In: 
    //Out: void
    //Desc: ends the turn and plays the animation
    public void endTurn()
    {
        if (TMS.selectedUnit == null && endTurnButton == true)
        {
            StartCoroutine(switchCurrentPlayer());
            if (currentTeam == 1)
            {
                playerPhaseAnim.SetTrigger("slideLeftTrigger");
                playerPhaseText.SetText("Ajattara Phase");
                GameObject.Find("AjattaraMenuManager").GetComponent<ButtomManager>().CloseMenu();

                //cooldown
                if (cooldownRocks > 0)
                {
                    cooldownRocks = cooldownRocks - 1;
                    rockBottom.interactable = false;
                    if (cooldownRocks < 0)
                    {
                        cooldownRocks = 0;
                    }
                }
                if(cooldownRocks == 0 && currentTeam == 1)
                {
                    rockBottom.interactable = true;
                }
                if (availableRocks <= 0)
                {
                    availableRocks = 0;
                    rockBottom.interactable = false;
                }
                if (availableRocks >= 3)
                {
                    availableRocks = 3;
                }
                if (cooldownTraps > 0)
                {
                    cooldownTraps = cooldownTraps - 1;
                    trapCardButtom.interactable = false;
                    if(cooldownTraps < 0)
                    {
                        cooldownTraps = 0;
                    }
                }
                if (cooldownTraps == 0 && currentTeam == 1)
                {
                    trapCardButtom.interactable = true;
                }
                if (availableTraps <= 0)
                {
                    availableTraps = 0;
                    trapCardButtom.interactable = false;
                }
                if (availableTraps >= 2)
                {
                    availableTraps = 2;
                }
                if (cooldownHelper > 0)
                {
                    cooldownHelper = cooldownHelper - 1;
                    helperBottom.interactable = false;
                    if(cooldownHelper < 0)
                    {
                        cooldownHelper = 0;
                    }
                }
                if (cooldownHelper == 0 && currentTeam == 1)
                {
                    helperBottom.interactable = true;
                }
                if (availableHelpers <= 0)
                {
                    availableHelpers = 0;
                    helperBottom.interactable = false;
                }
                if (availableHelpers >= 2)
                {
                    availableHelpers = 2;
                }
                if (eventCooldown > 0)
                {
                    eventCooldown = eventCooldown - 1;
                    if(eventCooldown <= 0)
                    {
                        eventCooldown = 0;

                        //add ap to trolls
                        if (eventText.text == "Trolls have -1 energy.")
                        {
                            if (TMS.peukaloEnteredPutrifiedTile == false)
                            {
                                GameObject.Find("Peukalo").GetComponent<UnitScript>().ActionPointPlus();
                            }
                            else if (peukaloObj.interactable == false)
                            {
                                GameObject.Find("Peukalo").GetComponent<UnitScript>().ActionPointPlus();
                            }
                            if (TMS.kullervoEnteredPutrifiedTile == false)
                            {
                                GameObject.Find("Kullervo").GetComponent<UnitScript>().ActionPointPlus();
                            }
                            else if(kullervoObj.interactable == false)
                            {
                                GameObject.Find("Kullervo").GetComponent<UnitScript>().ActionPointPlus();
                            }
                            if (TMS.marjaEnteredPutrifiedTile == false)
                            {
                                GameObject.Find("Marja").GetComponent<UnitScript>().ActionPointPlus();
                            }
                            else if(marjaObj.interactable == false)
                            {
                                GameObject.Find("Marja").GetComponent<UnitScript>().ActionPointPlus();
                            }
                        }
                    }
                }
            }
            else if (currentTeam == 0)
            {
                playerPhaseAnim.SetTrigger("slideRightTrigger");
                playerPhaseText.SetText("Troll Phase");
                GameObject.Find("AjattaraMenuManager").GetComponent<ButtomManager>().CloseMenu();

                rockBottom.interactable = false;
                trapCardButtom.interactable = false;
                helperBottom.interactable = false;

                //cooldown
                if (cooldownRocks > 0)
                {
                    cooldownRocks = cooldownRocks - 1;
                    rockBottom.interactable = false;
                    if (cooldownRocks < 0)
                    {
                        cooldownRocks = 0;
                    }
                }
                if (cooldownTraps > 0)
                {
                    cooldownTraps = cooldownTraps - 1;
                    trapCardButtom.interactable = false;
                    if (cooldownTraps < 0)
                    {
                        cooldownTraps = 0;
                    }
                }
                if (cooldownHelper > 0)
                {
                    cooldownHelper = cooldownHelper - 1;
                    helperBottom.interactable = false;
                    if (cooldownHelper < 0)
                    {
                        cooldownHelper = 0;
                    }
                }
                if (eventCooldown > 0)
                {
                    eventCooldown = eventCooldown - 1;
                    if (eventCooldown <= 0)
                    {
                        eventCooldown = 0;
                    }

                }
            }
            teamHealthbarColorUpdate();
            setCurrentTeamUI();
        }
    }

    //In: attacking unit and receiving unit
    //Out: void
    //Desc: checks to see if units remain on a team
    public void checkIfUnitsRemain(GameObject unit, GameObject enemy)
    {
        StartCoroutine(checkIfUnitsRemainCoroutine(unit,enemy));
    }

    //In:
    //Out: void
    //Desc: updates the cursor for the UI
    public void cursorUIUpdate()
    {
       //If we are mousing over a tile, highlight it
        if (hit.transform.CompareTag("Tile"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                selectedXTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileY;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.gameObject;
            }
            if(tileBeingDisplayed.GetComponent<ClickableTileScript>().unitOnTile)
            {
                selectedXTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileX;
                selectedYTile = hit.transform.gameObject.GetComponent<ClickableTileScript>().tileY;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
                tileBeingDisplayed = hit.transform.gameObject;
            }

        }
        //If we are mousing over a unit, highlight the tile that the unit is occupying
        else if (hit.transform.CompareTag("Unit"))
        {
            if (tileBeingDisplayed == null)
            {
                selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                cursorX = selectedXTile;
                cursorY = selectedYTile;
                TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                tileBeingDisplayed = hit.transform.parent.gameObject.GetComponent<UnitScript>().tileBeingOccupied;
            }
            else if (tileBeingDisplayed != hit.transform.gameObject)
            {
                if (hit.transform.parent.gameObject.GetComponent<UnitScript>().movementQueue.Count == 0)
                {
                    selectedXTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileX;
                    selectedYTile = tileBeingDisplayed.GetComponent<ClickableTileScript>().tileY;
                    TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;

                    selectedXTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().x;
                    selectedYTile = hit.transform.parent.gameObject.GetComponent<UnitScript>().y;
                    cursorX = selectedXTile;
                    cursorY = selectedYTile;
                    TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = true;
                    tileBeingDisplayed = hit.transform.parent.GetComponent<UnitScript>().tileBeingOccupied;
                }
            }
        }
        //We aren't pointing at anything no cursor.
        if(Input.touchCount <= 0)
        {
            TMS.quadOnMapCursor[selectedXTile, selectedYTile].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    //In: 
    //Out: void
    //Desc: the unit that is being highlighted will have its stats in the UI
    public void unitUIUpdate()
    {
        if (!displayingUnitInfo)
        {
            if (hit.transform.parent.gameObject.name == "Marja")
            {
                marjaCanvas.enabled = true;
                displayingUnitInfo = true;
                unitBeingDisplayed = hit.transform.parent.gameObject;
            }
            if (hit.transform.parent.gameObject.name == "Kullervo")
            {
                kullervoCanvas.enabled = true;
                displayingUnitInfo = true;
                unitBeingDisplayed = hit.transform.parent.gameObject;
            }
            if (hit.transform.parent.gameObject.name == "Peukalo")
            {
                peukaloCanvas.enabled = true;
                displayingUnitInfo = true;
                unitBeingDisplayed = hit.transform.parent.gameObject;
            }
            if (hit.transform.parent.gameObject.name == "Ajattara")
            {
                ajattaraCanvas.enabled = true;
                displayingUnitInfo = true;
                unitBeingDisplayed = hit.transform.parent.gameObject;
            }
            if (hit.transform.parent.gameObject.layer == 8)
            {
                helperCanvas.enabled = true;
                displayingUnitInfo = true;
                unitBeingDisplayed = hit.transform.parent.gameObject;
            }
            
            else if (hit.transform.CompareTag("Tile"))
            {
                if (hit.transform.GetComponent<ClickableTileScript>().unitOnTile != null)
                {
                    unitBeingDisplayed = hit.transform.GetComponent<ClickableTileScript>().unitOnTile;

                    if (hit.transform.parent.gameObject.name == "Marja")
                    {
                        marjaCanvas.enabled = true;
                        displayingUnitInfo = true;
                        unitBeingDisplayed = hit.transform.parent.gameObject;
                    }
                    if (hit.transform.parent.gameObject.name == "Kullervo")
                    {
                        kullervoCanvas.enabled = true;
                        displayingUnitInfo = true;
                        unitBeingDisplayed = hit.transform.parent.gameObject;
                    }
                    if (hit.transform.parent.gameObject.name == "Peukalo")
                    {
                        peukaloCanvas.enabled = true;
                        displayingUnitInfo = true;
                        unitBeingDisplayed = hit.transform.parent.gameObject;
                    }
                    if (hit.transform.parent.gameObject.name == "Ajattara")
                    {
                        ajattaraCanvas.enabled = true;
                        displayingUnitInfo = true;
                        unitBeingDisplayed = hit.transform.parent.gameObject;
                    }
                    if (hit.transform.parent.gameObject.layer == 8)
                    {
                        helperCanvas.enabled = true;
                        displayingUnitInfo = true;
                        unitBeingDisplayed = hit.transform.parent.gameObject;
                    }
                    
                }
            }
        }
    }

    //In: 
    //Out: void
    //Desc: When the current team is active, the healthbars are blue, and the other team is red
    public void teamHealthbarColorUpdate()
    {
        for(int i = 0; i < numberOfTeams; i++)
        {
            GameObject team = returnTeam(i);
            if(team == returnTeam(currentTeam))
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(0);
                }
            }
            else
            {
                foreach (Transform unit in team.transform)
                {
                    unit.GetComponent<UnitScript>().changeHealthBarColour(1);
                }
            }
        }
       
        
    }

    //In: x and y location to go to
    //Out: list of nodes to traverse
    //Desc: generate the cursor route to a position x , y
    public List<Node> generateCursorRouteTo(int x, int y)
    {

        if (TMS.selectedUnit.GetComponent<UnitScript>().x == x && TMS.selectedUnit.GetComponent<UnitScript>().y == y)
        {
            Debug.Log("clicked the same tile that the unit is standing on");
            currentPathForUnitRoute = new List<Node>();

            return currentPathForUnitRoute;
        }
        if (TMS.unitCanEnterTile(x, y) == false)
        {
            //cant move into something so we can probably just return
            //cant set this endpoint as our goal
            return null;
        }

        currentPathForUnitRoute = null;
        //from wiki dijkstra's
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = TMS.graph[TMS.selectedUnit.GetComponent<UnitScript>().x, TMS.selectedUnit.GetComponent<UnitScript>().y];
        Node target = TMS.graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        //Unchecked nodes
        List<Node> unvisited = new List<Node>();

        //Initialize
        foreach (Node n in TMS.graph)
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
                float alt = dist[u] + TMS.costToEnterTile(n.x, n.y);
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
            return null;
        }
        currentPathForUnitRoute = new List<Node>();
        Node curr = target;
        //Step through the current path and add it to the chain
        while (curr != null)
        {
            currentPathForUnitRoute.Add(curr);
            curr = prev[curr];
        }
        //Now currPath is from target to our source, we need to reverse it from source to target.
        currentPathForUnitRoute.Reverse();

        return currentPathForUnitRoute;
    }

    //In: gameObject quad 
    //Out: void
    //Desc: reset its rotation
    public void resetQuad(GameObject quadToReset)
    {
        quadToReset.GetComponent<Renderer>().material = UICursor;
        quadToReset.transform.eulerAngles = new Vector3(90, 0, 0);
    }

    //In: Vector2 cursorPos the location we change, Vector3 the rotation that we will rotate the quad
    //Out: void
    //Desc: the quad is rotated approriately
    public void UIunitRouteArrowDisplay(Vector2 cursorPos,Vector3 arrowRotationVector)
    {
        GameObject quadToManipulate = TMS.quadOnMapForUnitMovementDisplay[(int)cursorPos.x, (int)cursorPos.y];
        quadToManipulate.transform.eulerAngles = arrowRotationVector;
        quadToManipulate.GetComponent<Renderer>().material = UIunitRouteArrow;
        quadToManipulate.GetComponent<Renderer>().enabled = true;
    }

    //In: two gameObjects current vector and the next one in the list
    //Out: vector which is the direction between the two inputs
    //Desc: the direction from current to the next vector is returned
    public Vector2 directionBetween(Vector2 currentVector, Vector2 nextVector)
    {
        Vector2 vectorDirection = (nextVector - currentVector).normalized;
       
        if (vectorDirection == Vector2.right)
        {
            return Vector2.right;
        }
        else if (vectorDirection == Vector2.left)
        {
            return Vector2.left;
        }
        else if (vectorDirection == Vector2.up)
        {
            return Vector2.up;
        }
        else if (vectorDirection == Vector2.down)
        {
            return Vector2.down;
        }
        else
        {
            Vector2 vectorToReturn = new Vector2();
            return vectorToReturn;
        }
    }

    //In: two units that last fought
    //Out: void
    //Desc: waits until all the animations and stuff are finished before calling the game
    public IEnumerator checkIfUnitsRemainCoroutine(GameObject unit, GameObject enemy)
    {
        while (unit.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        while (enemy.GetComponent<UnitScript>().combatQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        if (team1.transform.childCount == 0)
        {
            trollWinnerUI.enabled = true;
            trollWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Ajattara has won!");
        }
        else if (team2.transform.childCount == 0)
        {
            trollWinnerUI.enabled = true;
            trollWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Trolls has won!");
        }
        while (enemy.GetComponent<UnitScript>().unitName == "Ajattara")
        {
            yield return new WaitForEndOfFrame();
            trollWinnerUI.enabled = true;
            trollWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Trolls has won!");
        }


    }

    //In: 
    //Out: void
    //Desc: set the player winning
    public void TrollWin()
    {
        trollWinnerUI.enabled = true;
        trollWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Trolls have saved their home!");

    }
    public void AjattaraWin()
    {
        ajattaraWinnerUI.enabled = true;
        ajattaraWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("You have the forest domain!");
    }

    public IEnumerator WaitForEvent()
    {
        eventPanel.SetActive(true);
        openMenu.interactable = false;
        yield return new WaitForSeconds(3f);

        openMenu.interactable = true;
        eventPanel.SetActive(false);
        endTurnButton.interactable = true;
        endTurn();
    }

    public void EventStart()
    {
        if(eventCooldown <= 0)
        {

            if (TMS.selectedUnit == null)
            {
                endTurnButton.interactable = false;
                buttomManager.GetComponent<ButtomManager>().CloseMenu();
                StartCoroutine(WaitForEvent());
            }

            string eventChoose = events[Random.Range(0, events.Count)];
            eventText.text = eventChoose;
            eventTextMenu.text = eventChoose;
            events.Remove(eventChoose);

            if (events.Count == 0)
            {
                events.Add("Trolls have -1 energy.");
                events.Add("Trolls have -1 energy.");
                events.Add("Trolls have -1 card.");
                events.Add("Trolls have -1 card.");
                events.Add("Minion generation.");
                events.Add("Trolls can't draw any other cards.");
                events.Add("Trolls can't pass card to another troll.");
                events.Add("Trolls switch cards with the right troll.");
                events.Add("Trolls switch cards with the left troll.");
            }
            if(eventChoose == "Minion generation.")
            {
                HelperEvent();
            }
            if (eventChoose == "Trolls have -1 energy.")
            {
                GameObject.Find("Peukalo").GetComponent<UnitScript>().ActionPointSub();
                GameObject.Find("Kullervo").GetComponent<UnitScript>().ActionPointSub();
                GameObject.Find("Marja").GetComponent<UnitScript>().ActionPointSub();
            }

            eventCooldown = 6;
        }
        else
        {
            if (TMS.selectedUnit == null)
            {
                endTurn();
            }
        }
    }

    public void SetRock()
    {
        if(cooldownRocks <= 0)
        {
            setRock = true;
            if (availableRocks <= 0)
            {
                availableRocks = 0;
                rockBottom.interactable = false;
            }
            cooldownRocks = 6;
            helperBottom.interactable = false;
            trapCardButtom.interactable = false;

            Ajattara.GetComponent<UnitScript>().setMovementState(3);
            buttomManager.GetComponent<ButtomManager>().CloseMenu();
        }
    }

    public void SetTrap()
    {
        if (cooldownTraps <= 0)
        {
            setTrap = true;
            if (availableTraps <= 0)
            {
                availableTraps = 0;
                trapCardButtom.interactable = false;
            }
            cooldownTraps = 4;
            rockBottom.interactable = false;
            helperBottom.interactable = false;

            Ajattara.GetComponent<UnitScript>().setMovementState(3);
            buttomManager.GetComponent<ButtomManager>().CloseMenu();
        }

    }

    public void Helper()
    {
        if(cooldownHelper <= 0)
        {
            GameObject obj = Instantiate(helpez, helperPos);
            GetComponent<tileMapScript>().setIfTileIsOccupied();
            obj.SetActive(true);

            if (availableHelpers <= 0)
            {
                availableHelpers = 0;
                helperBottom.interactable = false;
            }

            cooldownHelper = 3;
            helperBottom.interactable = false;
            availableHelpers = availableHelpers - 1;
            rockBottom.interactable = false;
            trapCardButtom.interactable = false;
            endTurn();
        }
    }

    public void HelperEvent()
    {
        if (availableHelpers > 0)
        {
            GameObject obj = Instantiate(helpez, helperPos);
            GetComponent<tileMapScript>().setIfTileIsOccupied();
            obj.SetActive(true);

            if (availableHelpers <= 0)
            {
                availableHelpers = 0;
            }
            cooldownHelper = 3;
            availableHelpers = availableHelpers - 1;
        }
    }

    public IEnumerator DisableHelper()
    {
        for (float ft = 1f; ft >= 0; ft -= 0.1f)
        {
            for (int i = 0; i < helper.Count; i++)
            {
                helper[i].SetActive(true);
            }
        }
        for (int i = 0; i < helper.Count; i++)
        {
            helper[i].SetActive(false);
        }
        yield return new WaitForSeconds(1f);
    }

    //Camera Controller
    //Returns the point between first and final finger position
    protected Vector3 PlanePositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;

        //delta
        var rayBefore = Camera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = Camera.ScreenPointToRay(touch.position);
        if (Plane.Raycast(rayBefore, out var enterBefore) && Plane.Raycast(rayNow, out var enterNow))
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);

        //not on plane
        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = Camera.ScreenPointToRay(screenPos);
        if (Plane.Raycast(rayNow, out var enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }

    public IEnumerator DisableOpenMenu()
    {
        openMenu.interactable = false;
        yield return new WaitForSeconds(2f);
        openMenu.interactable = true;
    }
}
