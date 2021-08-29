using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlacePosition : MonoBehaviour
{
    public PlayerUi ui;
    public Image i;
    public Sprite[] sprites;
    void Start()
    {

        ui = GetComponentInParent<PlayerUi>();
    }

    // Update is called once per frame
    void Update()
    {
        i.sprite = sprites[ui.p.lap.Placing];
    }
}
