using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class UnityQuaternion
{
    public static Quaternion Euler(Vector3 euler)
    {
        return ToEuler(euler.y, euler.x, euler.z);
    }
    public static Quaternion Euler(float x, float y, float z)
    {
        return ToEuler(y, x, z);
    }

    private static Quaternion ToEuler(float yaw, float pitch, float roll)
    {
        yaw *= Mathf.Deg2Rad;
        pitch *= Mathf.Deg2Rad;
        roll *= Mathf.Deg2Rad;

        float rollOver2 = roll * 0.5f;
        float sinRollOver2 = (float)Math.Sin((double)rollOver2);
        float cosRollOver2 = (float)Math.Cos((double)rollOver2);

        float pitchOver2 = pitch * 0.5f;
        float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
        float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);

        float yawOver2 = yaw * 0.5f;
        float sinYawOver2 = (float)Math.Sin((double)yawOver2);
        float cosYawOver2 = (float)Math.Cos((double)yawOver2);

        return new Quaternion()
        {
            x = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2,
            y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2,
            z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2,
            w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2
        };
    }


    public static Vector3 FromQ2(Quaternion q1)
    {
        float sqw = q1.w * q1.w;
        float sqx = q1.x * q1.x;
        float sqy = q1.y * q1.y;
        float sqz = q1.z * q1.z;

        float unit = sqx + sqy + sqz + sqw;
        float test = q1.x * q1.w - q1.y * q1.z;
        Vector3 v;

        // singularity at north pole
        if (test > 0.4995f * unit)
        { 
            v.y = 2f * UnityEngine.Mathf.Atan2(q1.y, q1.x);
            v.x = UnityEngine.Mathf.PI / 2;
            v.z = 0;
            return NormalizeAngles(v * Mathf.Rad2Deg);
        }
        // singularity at south pole
        if (test < -0.4995f * unit)
        { 
            v.y = -2f * UnityEngine.Mathf.Atan2(q1.y, q1.x);
            v.x = -UnityEngine.Mathf.PI / 2;
            v.z = 0;
            return NormalizeAngles(v * Mathf.Rad2Deg);
        }

        Quaternion q = new Quaternion(q1.w, q1.z, q1.x, q1.y);

        v.y = (float)Math.Atan2(2f * q.x * q.w + 2f * q.y * q.z, 1 - 2f * (q.z * q.z + q.w * q.w)); // Yaw
        v.x = (float)Math.Asin(2f * (q.x * q.z - q.w * q.y));                                       // Pitch
        v.z = (float)Math.Atan2(2f * q.x * q.y + 2f * q.z * q.w, 1 - 2f * (q.y * q.y + q.z * q.z)); // Roll

        return NormalizeAngles(v * UnityEngine.Mathf.Rad2Deg);
    }
    static Vector3 NormalizeAngles(Vector3 angles)
    {
        angles.x = NormalizeAngle(angles.x);
        angles.y = NormalizeAngle(angles.y);
        angles.z = NormalizeAngle(angles.z);
        return angles;
    }



    static float NormalizeAngle(float angle)
    {
        while (angle > 360)
        {
            angle -= 360;
        }

        while (angle < 0)
        {
            angle += 360;
        }
        return angle;
    }
}
