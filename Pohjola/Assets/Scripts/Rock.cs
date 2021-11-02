using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    public gameManagerScript gameManager;
    public ParticleSystem rockExplosion;

    private void Start()
    {
        gameManager = GameObject.Find("gameManager").GetComponent<gameManagerScript>();
        rockExplosion = GetComponentInChildren<ParticleSystem>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.parent.gameObject.name == "Ajattara" || collider.transform.parent.gameObject.name == "Helper")
        {
            gameManager.GetComponent<gameManagerScript>().availableRocks += 1;
            rockExplosion.Play();
            Destroy(gameObject, 0.5f);
        }
    }
}
