using System;

[Serializable]
public class SSensorJson
{
    public SSensorItem[] sensor;
}

[Serializable]
public class SNormalize
{
    public int t;
    public int r;
    public int b;
    public int l;
}

[Serializable]
public class SSensorItem
{
    public int monitor;
    public string port;
    public int rotation;
    public int sensibility;
    public int screenfactor;
    public int pwm;
    public float margin_left;
    public float margin_right;
    public float margin_bottom;
    public float margin_top;

    public SNormalize normalize;
}