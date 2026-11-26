using System;
using UnityEngine;

public class MenuKeys
{
    const float FirstRepeatDelay = 0.4f, RepeatDelay = 0.12f;
    int heldStep;
    float repeatTimer;

    public static int Horizontal => Math.Sign(Input.GetAxisRaw("Horizontal"));
    public static int Vertical => -Math.Sign(Input.GetAxisRaw("Vertical"));

    public int Repeat(int step)
    {
        if (step == 0 || step != heldStep)
        {
            heldStep = step;
            repeatTimer = FirstRepeatDelay;
            return step;
        }
        repeatTimer -= Time.unscaledDeltaTime;
        if (repeatTimer > 0) return 0;
        repeatTimer = RepeatDelay;
        return step;
    }

    public void Release() => heldStep = 0;
}