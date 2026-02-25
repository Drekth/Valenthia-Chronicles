using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ValenthiaChronicles.Core
{
    public enum LogTag
    {
        Combat,
        Quest,
        WorldState,
        Save,
        Audio,
        UI,
        AI,
        Zone,
        Input,
        Progression,
        Game,
        NPC,
        Inventory
    }

    public static class GameLogger
    {
        public static void Info(string message) => Debug.Log(message);
        public static void Info(LogTag tag, string message) => Debug.Log($"[{tag}] {message}");

        public static void Warn(string message) => Debug.LogWarning(message);
        public static void Warn(LogTag tag, string message) => Debug.LogWarning($"[{tag}] {message}");

        public static void Error(string message, Exception ex = null) =>
            Debug.LogError(ex != null ? $"{message}: {ex.Message}" : message);
        public static void Error(LogTag tag, string message, Exception ex = null) =>
            Debug.LogError(ex != null ? $"[{tag}] {message}: {ex.Message}" : $"[{tag}] {message}");

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Dbg(string message) => Debug.Log($"[DBG] {message}");

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        public static void Dbg(LogTag tag, string message) => Debug.Log($"[DBG][{tag}] {message}");
    }
}
