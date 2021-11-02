using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public gameManagerScript gameManager;
    public ParticleSystem putrefaction;

    private void Start()
    {
        gameManager = GameObject.Find("gameManager").GetComponent<gameManagerScript>();
        putrefaction = GetComponentInChildren<ParticleSystem>();
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.parent.gameObject.name == "Ajattara" || collider.transform.parent.gameObject.GetComponent<UnitScript>().unitName == "Helper" ||collider.transform.parent.gameObject.name == "Marja" || collider.transform.parent.gameObject.name == "Peukalo" || collider.transform.parent.gameObject.name == "Kullervo")
        {
            print("trap");
            gameManager.GetComponent<gameManagerScript>().availableTraps += 1;
            putrefaction.Play();
            Destroy(gameObject, 2);
        }
    }
}
