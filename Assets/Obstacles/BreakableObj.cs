using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class BreakableObj : MonoBehaviour
{
    //Object Settings.
    public float Objhealth = 10f;
    public float Objmaxhealth = 10f;

    //Healthbar Settings.
    public Image Fill; //Image component that shows visually to represent the enemy's HP.
    private Transform HptoObjecthp; //Target enemy transform so the healthbar will follow.
    private Vector3 hptoObjectoffset; //The position offset between the enemy and the HP bar (To show it above the enemy head and for visual appeal).
 //   private Text HPtext;
    //References.
    public GameObject healthbarPrefab;
    private GameObject healthbarinstance;

    public void Start()
    {
        Createhealthbar();
        Updatehealthbar();

        Collider2D playercollider = GameObject.FindWithTag("Player").GetComponent<Collider2D>();
        Collider2D breakableobjcollider = GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playercollider, breakableobjcollider);    

    }
   

    private void Createhealthbar()
    { 
    
        Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);

        healthbarinstance = Instantiate(healthbarPrefab, spawnPos, Quaternion.identity);

        Transform fillTransform = healthbarinstance.transform.Find("Fill");
        if (fillTransform != null)
        {
            Fill = fillTransform.GetComponent<Image>();
        }

        //foreach (Image img in healthbarinstance.GetComponentsInChildren<Image>(true))
        //{
        //    if (img.name == "Fill")
        //    {
        //        Fill = img;
        //        Debug.Log("Correct Fill assigned!");
        //        break;
        //    }
        //}

        //if (Fill == null)
        //{
        //    Debug.LogError("No child named 'Fill' found in prefab!");
        //}

        //HPtext = healthbarinstance.GetComponentInChildren<Text>();
        HptoObjecthp = transform;
        hptoObjectoffset = new Vector3(0,1f,0);
    }

    private void Updatehealthbar()
    {
        if (Fill != null)
        { 
            Fill.fillAmount = Objhealth /Objmaxhealth;
            Debug.Log($"fillAmount set to: {Fill.fillAmount}");

            //if (HPtext != null )
            //{
            //    HPtext.text = $"{Objhealth}/{Objmaxhealth}";
            //}


        }
    }
    public void TakeDamage(float damage)
    {

        Objhealth -= damage;
        Objhealth = Mathf.Max(0, Objhealth); //Clamp health so it wont go below 0.
        Debug.Log($"TakeDamage called with damage: {damage}, current HP: {Objhealth}");

        Updatehealthbar();
        //Check if obj is destroyed.
        if (Objhealth <= 0)
        {
            Break();
        }
    }
    private void Break()
    {
        if (healthbarinstance != null)
        {
            Destroy(healthbarinstance);
        }

        Destroy(gameObject);
    }
    

   
    void LateUpdate() //Called after all update functions have been called. This is for the HPUI to follow.
    {
        if (HptoObjecthp != null && healthbarinstance != null) 
        {
            healthbarinstance.transform.position = HptoObjecthp.position + hptoObjectoffset;
            healthbarinstance.transform.rotation = Quaternion.identity;
        }
    }
}
