using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LapUi : MonoBehaviour
{
    PlayerUi ui;
    public TextMeshProUGUI current;
    public TextMeshProUGUI max;

    private void Start()
    {
        ui = GetComponentInParent<PlayerUi>();
    }

    // Update is called once per frame
    void Update()
    {
        current.text = "" + (ui.p.lap.lap);
        max.text = "/" + (GameManager.instance.laps);
    }
}
