using UnityEngine;
using System.Collections;

public class StaticObjectController : MonoBehaviour
{
    public GameObject animObj;

    public GameObject staticObj;

    private bool _isAnimationStarted;

    // Use this for initialization
    void Start()
    {
        if (animObj == null || staticObj == null)
        {
            Debug.LogError("Anim obj or static obj not set. Trigger disabled. Static object = " + this.name);
            this.collider.enabled = false;
        }
        else
        {
            staticObj.SetActive(true);
            animObj.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == FCConst.LAYER_NEUTRAL_WEAPON_DAMAGE_1)
        {
            this.collider.enabled = false;

            staticObj.SetActive(false);
            GetComponent<NavMeshObstacle>().enabled = false;
            animObj.SetActive(true);

            animObj.animation.Play();
            _isAnimationStarted = true;

            //loot
            StaticObjectLoot lootAgent = GetComponent<StaticObjectLoot>();
            if (lootAgent)
            {
                lootAgent.Loot();
            }
        }
    }

    void Update()
    {
        if (_isAnimationStarted)
        {
            if (!animObj.animation.isPlaying)
            {
                Destroy(this.gameObject);
            }
        }
    }
}