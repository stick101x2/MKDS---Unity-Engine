using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ItemRoulette : MonoBehaviour
{
    PlayerUi ui;
    Animator anim;
    Transform all;
    public Image[] images;
    public Sound roulette;
    public Sound select;
    public Sound special_select;
    public float timer;
    public float mod = 0.25f;
    bool rouletteWheel;
    Item item;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        all = transform.GetChild(0).transform.GetChild(0);
        ui = GetComponentInParent<PlayerUi>();
        all.gameObject.SetActive(false);

        ui.p.item.OnItemUse -= UseItem;
        ui.p.item.OnItemUse += UseItem;
    }
    private void OnDestroy()
    {
        ui.p.item.OnItemUse -= UseItem;
    }
    public void UseItem()
    {
        if(ui.p.item.held == null)
            all.gameObject.SetActive(false);
    }
    public void StartItemRoulette()
    {
        all.gameObject.SetActive(true);
        Sprite[] sps = ItemManager.instance.table.chances[ui.p.lap.Placing].images;
        for (int i = 0; i < images.Length; i++)
        {
            if(i == 0)
            {
                images[0].sprite = item.icon;
            }else
            {
                

                images[i].sprite = sps[Random.Range(0, sps.Length)];
            }
            
        }
        images[0].sprite = item.icon;
        timer = 1f;
        rouletteWheel = true;
        anim.SetFloat("spin",1f);
        anim.Play("spin",0,0);
        AudioManager.instance.Play2D(roulette);
    }
    public void GetItem(Player p)
    {
        item = ItemManager.instance.table.GetItem(p.lap.Placing);
        p.item.held = Object.Instantiate(item);
        StartItemRoulette();
    }
    private void Update()
    {
        if (rouletteWheel)
        {
            timer -= Time.deltaTime * mod;
            anim.SetFloat("spin", timer);
            
        }
    }
    public void TryEndRoulette()
    {
        if (timer < 0)
        {
            EndRoulette();
            timer = 0f;
            anim.SetFloat("spin", timer);
        }
    }

    void EndRoulette()
    {
        ui.p.item.GotNewItem();
        rouletteWheel = false;
        
        anim.SetFloat("spin", 1f);
        anim.Play("select");
        AudioManager.instance.Stop(roulette);
        AudioManager.instance.Play2D(select);
    }
}
