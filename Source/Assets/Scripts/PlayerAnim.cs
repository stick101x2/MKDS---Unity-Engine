using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct State
{
    public const int NORMAL = 0;
    public const int VICTORY = 1;
    public const int LOST = 2;
    public const int HURT = 3;
}
public struct Anim
{
    public const string TURNX = "TurnX";
    public const string TURNING = "Turning";
    public const string STATE = "State";
    public const string GOOD = "Good!";
    public const string DAMN = "Damn!";

    public const string JUMP = "Jump";
}
public class PlayerAnim : MonoBehaviour
{
    public Kart c_kart;
    public Driver c_driver;
    [Header("Wheels")]
    public float steerSpeed = 5;
    public float turnAngleOffset = 45f;

    public float speedModifier = 1;
    public float maxSpeed = 100;

    Material[] driverMats;
    Material[] kartMats;
    Material wheelMat;
    public void OnSetup(Player p)
    {
        c_driver = p.driver;
        c_kart = p.kart;

        SetTextures();

        driverMats = c_driver.model.sharedMaterials;
        kartMats = c_kart.model.sharedMaterials;
        wheelMat = c_kart.wheels[0].sharedMaterial;
    }
    public void KartJump()
    {
        c_kart.anim.Play(Anim.JUMP, 0, 0);
    }
    public void DriverTurn(float x)
    {
       //c_driver.anim.SetBool("Drifting", drift);

        if (Mathf.Abs(x) > 0)
        {
            c_driver.anim.SetFloat(Anim.TURNX, x);
            c_driver.anim.SetBool(Anim.TURNING, true);
        }
        else
        {
            c_driver.anim.SetBool(Anim.TURNING, false);
        }
    }

    public void WheelsSteer(float dir)
    {
        Vector3 fw = c_kart.front_wheel_left.localEulerAngles;

        if (dir > 0.1f)
        {
            fw.y += steerSpeed * Time.deltaTime;
        }
        else if (dir < -0.1f)
        {
            fw.y -= steerSpeed * Time.deltaTime;
        }
        else
        {
            if (fw.y > 91)
                fw.y -= steerSpeed * Time.deltaTime;
            if (fw.y < 89)
                fw.y += steerSpeed * Time.deltaTime;
            if (fw.y < 91 && fw.y > 89)
                fw.y = 90;
        }

        fw.y = Mathf.Clamp(fw.y, 0 + turnAngleOffset, 180 - turnAngleOffset);

        c_kart.front_wheel_right.localEulerAngles = fw;
        c_kart.front_wheel_left.localEulerAngles = fw;
    }
    public void WheelsMove(float speed, bool inplace = false)
    {
        float c = speed;
        if (c > 0)
            c = Mathf.Min(speed, maxSpeed);
        else if (c < 0)
            c = Mathf.Max(speed, -maxSpeed);
        //c *= -1;

        float turnSpeed = -90 * Time.deltaTime * c * speedModifier;

        float bModifier = inplace ? -1f : 1f;

        c_kart.wheel_back_L.Rotate(new Vector3(0, 0, turnSpeed));
        c_kart.wheel_back_R.Rotate(new Vector3(0, 0, turnSpeed));
        c_kart.wheel_front_L.Rotate(new Vector3(0, 0, turnSpeed * bModifier));
        c_kart.wheel_front_R.Rotate(new Vector3(0, 0, turnSpeed * bModifier));
    }

    public void SetDriverAnimState(int value)
    {
        c_driver.anim.SetInteger(Anim.STATE, value);
    }
    public void Good()
    {
        c_driver.anim.ResetTrigger(Anim.GOOD);
        c_driver.anim.SetTrigger(Anim.GOOD);
    }
    public void Damage()
    {
        c_driver.anim.ResetTrigger(Anim.DAMN);
        c_driver.anim.SetTrigger(Anim.DAMN);
    }
    public void SetTextures()
    {
        int id = (int)c_kart.type;

        int bodyindex = c_kart.bodyIndex;
        int emblemIndex = c_kart.emblemIndex;

        c_kart.model.materials[bodyindex].mainTexture = c_driver.settings.kartTextures[id];
        c_kart.model.materials[emblemIndex].mainTexture = c_driver.settings.emblem;
    }
    public void ResetMaterial()
    {
        c_driver.model.sharedMaterials = driverMats;
        c_kart.model.sharedMaterials = kartMats;
        for (int i = 0; i < c_kart.wheels.Length; i++)
        {
            c_kart.wheels[i].sharedMaterial = wheelMat;
        }
    }
    //driver
    public void SetDriverMaterial(Material mat)
    {
        Material[] m = c_driver.model.materials;

        for (int i = 0; i < m.Length; i++)
        {
            m[i] = mat;
        }

        c_driver.model.materials = m;


    }
    public void SetDriverMaterial(Material[] mats)
    {
        c_driver.model.materials = mats;
    }
    public void SetWheelMaterial(Material mat)
    {
        for (int i = 0; i < c_kart.wheels.Length; i++)
        {
            c_kart.wheels[i].material = mat;
        }
    }
    //kart
    public void SetKartMaterial(Material mat)
    {
        Material[] m = c_kart.model.materials;

        for (int i = 0; i < m.Length; i++)
        {
            if (i == c_kart.ignoreIndex)
                continue;
            m[i] = mat;
        }

        c_kart.model.materials = m;
    }
    public void SetKartMaterial(Material[] mats)
    {
        c_kart.model.materials = mats;
    }
}
