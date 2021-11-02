using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class camShakeScript : MonoBehaviour
{
    private AmbientOcclusion aO;
    private ColorGrading cG;
    private Vignette vignette;

    public void Start()
    {
        PostProcessVolume volume = gameObject.GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out aO);
        volume.profile.TryGetSettings(out cG);
        volume.profile.TryGetSettings(out vignette);
    }

    public void ZonePutrified()
    {
        aO.intensity.value += 0.1f;
        cG.temperature.value -= 3;
        vignette.intensity.value += 0.05f;
    }

    public IEnumerator camShake(float duration,float camShakeStrength,Vector3 direction)
    {
        float updatedShakeStrength = camShakeStrength;
        if (camShakeStrength > 10)
        {
            camShakeStrength = 10;
        }
        Vector3 originalPos = transform.position;
        Vector3 endPoint = new Vector3(direction.x, 0, direction.z)*(camShakeStrength/2);
        
        float timePassed = 0f;
        while (timePassed < duration)
        {

            float xPos = Random.Range(-.1f, .1f)*camShakeStrength;
            float zPos = Random.Range(-.1f, .1f)*camShakeStrength;
            Vector3 newPos = new Vector3(transform.position.x + xPos, transform.position.y, transform.position.z + zPos);
            //Vector3 newPos = endPoint + originalPos;
            transform.position = Vector3.Lerp(transform.position,newPos,0.15f);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();

        }  
    }
}
