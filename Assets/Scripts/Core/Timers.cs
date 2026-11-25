using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public static class Timers
{
    struct TimersUpdate { }

    static readonly List<Action> Ticks = new();
    static readonly List<Action<UnityEngine.Object>> Cancels = new();

    public static void Wait<T>(this T owner, float seconds, Action<T> action) where T : UnityEngine.Object => Waits<T>.List.Add((Time.time + seconds, owner, action));

    public static void CancelWaits(this UnityEngine.Object owner) { foreach (var cancel in Cancels) cancel(owner); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Start()
    {
        foreach (var cancel in Cancels) cancel(null);
        var loop = PlayerLoop.GetCurrentPlayerLoop();
        for (int i = 0; i < loop.subSystemList.Length; i++)
        {
            if (loop.subSystemList[i].type != typeof(Update)) continue;
            var systems = new List<PlayerLoopSystem>(loop.subSystemList[i].subSystemList);
            if (systems.Exists(system => system.type == typeof(TimersUpdate))) return;
            systems.Add(new PlayerLoopSystem { type = typeof(TimersUpdate), updateDelegate = RunWaits });
            loop.subSystemList[i].subSystemList = systems.ToArray();
            PlayerLoop.SetPlayerLoop(loop);
            return;
        }
    }

    static void RunWaits() { for (int i = 0; i < Ticks.Count; i++) Ticks[i](); }

    static class Waits<T> where T : UnityEngine.Object
    {
        public static readonly List<(float time, T owner, Action<T> action)> List = new();
        static Waits() { Ticks.Add(Tick); Cancels.Add(Cancel); }

        static void Cancel(UnityEngine.Object owner) { if (owner) { for (int i = List.Count - 1; i >= 0; i--) if (List[i].owner == owner) List.RemoveAt(i); } else List.Clear(); }

        static void Tick()
        {
            for (int i = List.Count - 1; i >= 0; i--)
            {
                if (i >= List.Count) continue;
                var (time, owner, action) = List[i];
                if (owner && Time.time < time) continue;
                List.RemoveAt(i);
                if (owner) try { action(owner); } catch (Exception e) { Debug.LogException(e, owner); }
            }
        }
    }
}