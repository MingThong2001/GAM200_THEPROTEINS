using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class BreakableObj : MonoBehaviour
{
    //Object Settings.
    public float Objhealth = 10f;
    public float Objmaxhealth = 10f;



   

    //HP Sprites
    public Sprite fullhp;
    public Sprite midhp;
    public Sprite lowhp;
    private SpriteRenderer sprite;

    public void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        //Createhealthbar();
        //Updatehealthbar();

        Collider2D playercollider = GameObject.FindWithTag("Player").GetComponent<Collider2D>();
        Collider2D breakableobjcollider = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playercollider, breakableobjcollider);

        Updatehealthsprite();


    }


    //private void Createhealthbar()
    //{ 

    //    Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);

    //    healthbarinstance = Instantiate(healthbarPrefab, spawnPos, Quaternion.identity);

    //    Transform fillTransform = healthbarinstance.transform.Find("Fill");
    //    if (fillTransform != null)
    //    {
    //        Fill = fillTransform.GetComponent<Image>();
    //    }

    //    //foreach (Image img in healthbarinstance.GetComponentsInChildren<Image>(true))
    //    //{
    //    //    if (img.name == "Fill")
    //    //    {
    //    //        Fill = img;
    //    //        Debug.Log("Correct Fill assigned!");
    //    //        break;
    //    //    }
    //    //}

    //    //if (Fill == null)
    //    //{
    //    //    Debug.LogError("No child named 'Fill' found in prefab!");
    //    //}

    //    //HPtext = healthbarinstance.GetComponentInChildren<Text>();
    //    HptoObjecthp = transform;
    //    hptoObjectoffset = new Vector3(0,1f,0);
    //}

    //private void Updatehealthbar()
    //{
    //    if (Fill != null)
    //    { 
    //        Fill.fillAmount = Objhealth /Objmaxhealth;
    //        Debug.Log($"fillAmount set to: {Fill.fillAmount}");

    //        //if (HPtext != null )
    //        //{
    //        //    HPtext.text = $"{Objhealth}/{Objmaxhealth}";
    //        //}


    //    }
    //}
    public void TakeDamage(float damage)
    {

        Objhealth -= damage;
        Objhealth = Mathf.Max(0, Objhealth);
        
        //Clamp health so it wont go below 0.
        //Debug.Log($"TakeDamage called with damage: {damage}, current HP: {Objhealth}");

        //Check if obj is destroyed.
        if (Objhealth <= 0)
        {
            Break();
            return;
        }
        Updatehealthsprite();

    }
    private void Break()
    {
        // Disable self to immediately stop rendering and interaction
        gameObject.SetActive(false);

        // Destroy the GameObject after a very short delay
        Destroy(gameObject);



    }
    public void Updatehealthsprite()
    {
        if (Objhealth <= 0)
        { 
            sprite.enabled = false;
            return;
        }

        float hpPercentage = Objhealth / Objmaxhealth;

        if (hpPercentage > 0.66f)
        {
            sprite.sprite = fullhp;
        }
        else if (hpPercentage > 0.33f)
        {
            sprite.sprite = midhp;
        }
        else
        {
          sprite.sprite = lowhp;

        }
    }

    //void LateUpdate() //Called after all update functions have been called. This is for the HPUI to follow.
    //{
    //    if (HptoObjecthp != null && healthbarinstance != null) 
    //    {
    //        healthbarinstance.transform.position = HptoObjecthp.position + hptoObjectoffset;
    //        healthbarinstance.transform.rotation = Quaternion.identity;
    //    }
    //}
}
