using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public static readonly Vector3 rotation = new Vector3(32, 135, 65);
    public static readonly Vector3 min = new Vector3(-1, -1.2f, -1.25f);
    public static readonly Vector3 max = new Vector3(1, 1.2f, 1.25f);
    public static readonly Vector3 pingPong = new Vector3(0.025f, 0.05f, 0.01f);

    public static float x;
    public static float y;
    public static float z;
           
    public Transform question;

    public static Vector3 all_rotation;
    public static Vector3 all_quesiton_position;


    [HideInInspector] public bool isFake;

    public static Vector3 GetItemBoxRotation()
    {
        x = Mathf.PingPong(Time.time * pingPong.x, max.x - min.x) + min.x;
        y = Mathf.PingPong(Time.time * pingPong.y, max.y - min.y) + min.y;
        z = Mathf.PingPong(Time.time * pingPong.z, max.z - min.z) + min.z;

        Vector3 rot = new Vector3();

        rot.x = rotation.x * x;
        rot.y = rotation.y * y;
        rot.z = rotation.z * z;

        return rot;
    }
    public static Vector3 GetItemBoxPosition()
    {
        Vector3 pos2 = new Vector3();
        pos2.y = Mathf.Sin(Time.time * 2f) * 0.25f;

        return pos2;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        float speed = isFake ? 0.5f : 1f;

        transform.GetChild(0).Rotate((all_rotation * (Time.deltaTime * speed)), Space.World);
        question.position = question.parent.position + all_quesiton_position * speed;
    }
}
