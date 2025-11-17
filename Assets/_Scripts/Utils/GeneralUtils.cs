using UnityEngine;

public static class GeneralUtils
{
    public static float SafeDivide(this float dividend, float divisor, float outputIfZero = Mathf.Infinity) {
        return divisor == 0 ? outputIfZero : (dividend / divisor);
    }
}