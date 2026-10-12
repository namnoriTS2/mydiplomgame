using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] private Slider stamina;
    private bool staminaIsUsing = false;
    public float Value => stamina !=null ? stamina.value : 0f;
   
    void Start()
    {
       player = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        staminaIsUsing = false;

        if (player.isRunning){
            if(stamina.value>0.01f){
        stamina.value -= 30f* Time.deltaTime;
        staminaIsUsing = true;
        }
        }
        if(!staminaIsUsing){
            stamina.value += 5f*Time.deltaTime;
        }
        stamina.value = Mathf.Clamp(stamina.value, 0f, 100f);
    }
    public void SpendJump()
    {
        stamina.value = Mathf.Clamp(stamina.value - 20f, 0f, 100f);
    }
}
