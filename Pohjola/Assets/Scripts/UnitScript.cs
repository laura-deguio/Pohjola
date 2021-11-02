using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitScript : MonoBehaviour
{
    public int teamNum;
    public int x;
    public int y;
    public float rotationY;

    //This is a low tier idea, don't use it 
    public bool coroutineRunning;

    //Meta defining play here
    public Queue<int> movementQueue;
    public Queue<int> combatQueue;
    //This global variable is used to increase the units movementSpeed when travelling on the board
    public float visualMovementSpeed = .15f;

    public GameObject tileBeingOccupied;

    public GameObject damagedParticle;

    //UnitStats
    public string unitName;
    public int moveSpeed = 2;    
    public int attackRange = 1;
    public int attackDamage = 1;
    public int maxHealthPoints = 5;
    public int currentHealthPoints;
    public int actionPoints = 2;

    [Header("UI Elements")]

    public Canvas healthBarCanvas;
    public TMP_Text hitPointsText;
    public Image healthBar;

    public Canvas damagePopupCanvas;
    public TMP_Text damagePopupText;
    public Image damageBackdrop;
    
    //This may change in the future if 2d sprites are used instead
    public Material unitMaterial;
    public Material unitWaitMaterial;

    public tileMapScript map;

    //Location for positional update
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeedTime = 1f;
    
    //3D Model or 2D Sprite variables to check which version to use
    //Make sure only one of them are enabled in the inspector
    //public GameObject holder3D;
    public GameObject holder2D;

    //Boolean to startTravelling
    public bool unitInMovement;

    public int currentPoint = 0;

    public GameObject cylinder;

    public GameObject snake;

    //Enum for unit states
    public enum movementStates
    {
        Unselected,
        Selected,
        Moved,
        Wait
    }
    public movementStates unitMoveState;
   
    //Pathfinding

    public List<Node> path = null;

    //Path for moving unit's transform
    public List<Node> pathForMovement = null;
    public bool completedMovement = false;

    private void Awake()
    {
        movementQueue = new Queue<int>();
        combatQueue = new Queue<int>();
        
        unitMoveState = movementStates.Unselected;
        currentHealthPoints = maxHealthPoints;
        hitPointsText.SetText(currentHealthPoints.ToString());

        rotationY = transform.rotation.eulerAngles.y;

        if(unitName == "Ajattara")
        {
            x = 2;
            y = 0;
        }
        if (unitName == "Peukalo")
        {
            x = 7;
            y = 8;
        }
        if (unitName == "Kullervo")
        {
            x = 7;
            y = 7;
        }
        if (unitName == "Marja")
        {
            x = 8;
            y = 7;
        }
        if (unitName == "Helper")
        {
            x = 2;
            y = 0;
        }
    }

    private void Update()
    {
        rotationY = transform.rotation.eulerAngles.y;
    }


    public void MoveNextTile()
    {
        if (path.Count == 0)
        {
            return;
        }
        else
        {
            StartCoroutine(moveOverSeconds(transform.gameObject, path[path.Count - 1]));
        }
        
    }

    public void moveAgain()
    {
        path = null;
        setMovementState(0);
        completedMovement = false;

        if (unitName == "Helper")
        {
            snake.GetComponentInChildren<MeshRenderer>().material = unitMaterial;
        }
        else
        {
            gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material = unitMaterial;
        }
        cylinder.GetComponent<Renderer>().material = unitMaterial;
    }

    public movementStates getMovementStateEnum(int i)
    {
        if (i == 0)
        {
            return movementStates.Unselected;
        }
        else if (i == 1)
        {
            return movementStates.Selected;
        }
        else if (i == 2)
        {
            return movementStates.Moved;
        }
        else if (i == 3)
        {
            return movementStates.Wait;
        }
        return movementStates.Unselected;
        
    }
    public void setMovementState(int i)
    {
        if (i == 0)
        {
            unitMoveState = movementStates.Unselected;
        }
        else if (i == 1)
        {
            unitMoveState = movementStates.Selected;
        }
        else if (i == 2)
        {
            unitMoveState = movementStates.Moved;
        }
        else if (i == 3)
        {
            unitMoveState = movementStates.Wait;
        }
    }

    public void updateHealthUI()
    {
        healthBar.fillAmount = (float)currentHealthPoints / maxHealthPoints;
        hitPointsText.SetText(currentHealthPoints.ToString());
    }

    public void dealDamage(int x)
    {
        currentHealthPoints = currentHealthPoints - x;
        updateHealthUI();
    }

    public void wait()
    {
        
        if (unitName == "Helper")
        {
            snake.GetComponentInChildren<Renderer>().material = unitWaitMaterial;
        }
        else
        {
            gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material = unitWaitMaterial;
        }
        
        cylinder.GetComponent<Renderer>().material = unitWaitMaterial;
    }

    public void changeHealthBarColour(int i)
    {
        if (i == 0)
        {
            healthBar.color = Color.blue;
        }
        else if (i == 1)
        {
           
            healthBar.color = Color.red;
        }
    }

    public void unitDie()
    {
        Destroy(gameObject, 1);
    } 

    public IEnumerator moveOverSeconds(GameObject objectToMove,Node endNode)
    {
        movementQueue.Enqueue(1);

        //remove first thing on path because, its the tile we are standing on
        path.RemoveAt(0);
        while (path.Count != 0)
        {
            Vector3 endPos = map.tileCoordToWorldCoord(path[0].x * 2, path[0].y * 2);
            objectToMove.transform.position = Vector3.Lerp(transform.position, endPos, visualMovementSpeed * Time.deltaTime);
            if ((unitName == "Marja") || (unitName == "Kullervo") || (unitName == "Peukalo"))
            {
                transform.rotation = Quaternion.LookRotation(endPos - transform.position);
                Vector3 rotationVector = endPos - transform.position;
                rotationVector.y = 0;
                transform.rotation = Quaternion.LookRotation(-rotationVector);
            }
            if ((unitName == "Ajattara") || (unitName == "Helper"))
            {
                transform.rotation = Quaternion.LookRotation(endPos - transform.position);
                Vector3 rotationVector = endPos - transform.position;
                rotationVector.y = 0;
                transform.rotation = Quaternion.LookRotation(rotationVector);
            }

            if ((transform.position - endPos).sqrMagnitude < 0.001)
            {

                path.RemoveAt(0);
              
            }
            yield return new WaitForSeconds(0.01f);
        }
        visualMovementSpeed = 15f;
        transform.position = map.tileCoordToWorldCoord(endNode.x * 2, endNode.y * 2);

        x = endNode.x;
        y = endNode.y;
        tileBeingOccupied.GetComponent<ClickableTileScript>().unitOnTile = null;
        tileBeingOccupied = map.tilesOnMap[x, y];
        movementQueue.Dequeue();

    }

    public IEnumerator displayDamageEnum(int damageTaken)
    {

        combatQueue.Enqueue(1);
       
        damagePopupText.SetText(damageTaken.ToString());
        damagePopupCanvas.enabled = true;
        for (float f = 1f; f >=-0.01f; f -= 0.01f)
        {
            
            Color backDrop = damageBackdrop.GetComponent<Image>().color;
            Color damageValue = damagePopupText.color;

            backDrop.a = f;
            damageValue.a = f;
            damageBackdrop.GetComponent<Image>().color = backDrop;
            damagePopupText.color = damageValue;
           yield return new WaitForEndOfFrame();
        }
        combatQueue.Dequeue();
    }

    public void ActionPointSub()
    {
        actionPoints -= 1;
        
            if (unitName == "Peukalo")
            {
                if (actionPoints <= 1)
                {
                    actionPoints = 1;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp3.SetActive(false);
                }
                if (actionPoints == 2)
                {
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp3.SetActive(false);
                }
                if (actionPoints >= 3)
                {
                    actionPoints = 3;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown3.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp3.SetActive(true);
                }
            }
            if (unitName == "Marja")
            {
                if (actionPoints <= 1)
                {
                    actionPoints = 1;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp3.SetActive(false);
                }
                if (actionPoints == 2)
                {
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp3.SetActive(false);
                }
                if (actionPoints >= 3)
                {
                    actionPoints = 3;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown3.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp3.SetActive(true);
                }
            }
            if (unitName == "Kullervo")
            {
                if (actionPoints <= 1)
                {
                    actionPoints = 1;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp3.SetActive(false);
                }
                if (actionPoints == 2)
                {
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp3.SetActive(false);
                }
                if (actionPoints >= 3)
                {
                    actionPoints = 3;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown3.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp3.SetActive(true);
                }
            }
     
        
    }

    public void ActionPointPlus()
    {
        actionPoints += 1;

        if (unitName == "Peukalo")
        {
            if (actionPoints <= 1)
            {
                actionPoints = 1;
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown2.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown3.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp2.SetActive(false);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp3.SetActive(false);
            }
            if (actionPoints == 2)
            {
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown2.SetActive(false);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown3.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp2.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp3.SetActive(false);
            }
            if (actionPoints >= 3)
            {
                if(GameObject.Find("gameManager").GetComponent<gameManagerScript>().peukaloObj.interactable == false)
                {
                    actionPoints = 3;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown3.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp3.SetActive(true);
                }
                else
                {
                    actionPoints = 2;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().peukaloApUp3.SetActive(false);
                }
            }
        }
        if (unitName == "Marja")
        {
            if (actionPoints <= 1)
            {
                actionPoints = 1;
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown2.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown3.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp2.SetActive(false);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp3.SetActive(false);
            }
            if (actionPoints == 2)
            {
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown2.SetActive(false);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown3.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp2.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp3.SetActive(false);
            }
            if (actionPoints >= 3)
            {
                if (GameObject.Find("gameManager").GetComponent<gameManagerScript>().marjaObj.interactable == false)
                {
                    actionPoints = 3;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown3.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp3.SetActive(true);
                }
                else
                {
                    actionPoints = 2;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().marjaApUp3.SetActive(false);
                }
            }
        }
        if (unitName == "Kullervo")
        {
            if (actionPoints <= 1)
            {
                actionPoints = 1;
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown2.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown3.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp2.SetActive(false);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp3.SetActive(false);
            }
            if (actionPoints == 2)
            {
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown2.SetActive(false);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown3.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp2.SetActive(true);
                GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp3.SetActive(false);
            }
            if (actionPoints >= 3)
            {
                if (GameObject.Find("gameManager").GetComponent<gameManagerScript>().kullervoObj.interactable == false)
                {
                    actionPoints = 3;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown3.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp3.SetActive(true);
                }
                else
                {
                    actionPoints = 2;
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown2.SetActive(false);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApDown3.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp2.SetActive(true);
                    GameObject.Find("gameManager").GetComponent<tileMapScript>().kullervoApUp3.SetActive(false);
                }
            }
        }
    }
}
