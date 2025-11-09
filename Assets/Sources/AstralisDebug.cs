using UnityEngine;
using System;
using System.Collections.Generic;

namespace Sources
{
    /// <summary>
    /// Custom logging system with category support
    /// Usage: AstralisDebug.Log("CategoryName", "Your message");
    /// </summary>
    public static class AstralisDebug
    {
        // Event to notify Astralis Console of new logs
        public static event Action<string, string, string, LogType> OnLogWithCategory;
        
        // Dictionary to store colors by category
        private static Dictionary<string, Color> categoryColors = new Dictionary<string, Color>();
        
        // Predefined categories with their colors
        static AstralisDebug()
        {
            // Default categories
            RegisterCategory("Player", new Color(0.3f, 0.7f, 1f));      // Light blue
            RegisterCategory("Enemy", new Color(1f, 0.3f, 0.3f));       // Red
            RegisterCategory("UI", new Color(0.9f, 0.9f, 0.3f));        // Yellow
            RegisterCategory("Audio", new Color(0.7f, 0.3f, 1f));       // Purple
            RegisterCategory("Input", new Color(0.3f, 1f, 0.5f));       // Green
            RegisterCategory("GameManager", new Color(1f, 0.6f, 0.2f)); // Orange
            RegisterCategory("AI", new Color(1f, 0.4f, 0.7f));          // Pink
            RegisterCategory("Physics", new Color(0.5f, 0.8f, 0.9f));   // Cyan
            RegisterCategory("Animation", new Color(0.8f, 0.5f, 0.3f)); // Brown
            RegisterCategory("Save", new Color(0.4f, 0.9f, 0.4f));      // Light green
            RegisterCategory("General", new Color(0.8f, 0.8f, 0.8f));   // Light gray
        }
        
        /// <summary>
        /// Registers a new category with a custom color
        /// </summary>
        public static void RegisterCategory(string category, Color color)
        {
            if (!categoryColors.ContainsKey(category))
            {
                categoryColors[category] = color;
            }
        }
        
        /// <summary>
        /// Gets the color of a category
        /// </summary>
        public static Color GetCategoryColor(string category)
        {
            if (categoryColors.TryGetValue(category, out Color color))
            {
                return color;
            }
            return Color.white;
        }
        
        /// <summary>
        /// Gets all registered categories
        /// </summary>
        public static List<string> GetAllCategories()
        {
            return new List<string>(categoryColors.Keys);
        }
        
        /// <summary>
        /// Standard log with category
        /// </summary>
        public static void Log(string category, object message)
        {
            string formattedMessage = FormatMessage(category, message);
            Debug.Log(formattedMessage);
            OnLogWithCategory?.Invoke(category, message.ToString(), GetStackTrace(), LogType.Log);
        }
        
        /// <summary>
        /// Warning with category
        /// </summary>
        public static void LogWarning(string category, object message)
        {
            string formattedMessage = FormatMessage(category, message);
            Debug.LogWarning(formattedMessage);
            OnLogWithCategory?.Invoke(category, message.ToString(), GetStackTrace(), LogType.Warning);
        }
        
        /// <summary>
        /// Error with category
        /// </summary>
        public static void LogError(string category, object message)
        {
            string formattedMessage = FormatMessage(category, message);
            Debug.LogError(formattedMessage);
            OnLogWithCategory?.Invoke(category, message.ToString(), GetStackTrace(), LogType.Error);
        }
        
        /// <summary>
        /// Formats the message with category and color
        /// </summary>
        private static string FormatMessage(string category, object message)
        {
            Color color = GetCategoryColor(category);
            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hexColor}>[{category}]</color> {message}";
        }
        
        /// <summary>
        /// Gets the current stack trace
        /// </summary>
        private static string GetStackTrace()
        {
            // Capture the stack trace and remove the first 2 lines (AstralisDebug)
            string stackTrace = System.Environment.StackTrace;
            string[] lines = stackTrace.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length > 2)
            {
                List<string> relevantLines = new List<string>();
                for (int i = 2; i < lines.Length; i++)
                {
                    relevantLines.Add(lines[i]);
                }
                return string.Join("\n", relevantLines);
            }
            
            return stackTrace;
        }
        
        /// <summary>
        /// Simple log without category (fallback)
        /// </summary>
        public static void Log(object message)
        {
            Log("General", message);
        }
        
        /// <summary>
        /// Simple warning without category (fallback)
        /// </summary>
        public static void LogWarning(object message)
        {
            LogWarning("General", message);
        }
        
        /// <summary>
        /// Simple error without category (fallback)
        /// </summary>
        public static void LogError(object message)
        {
            LogError("General", message);
        }
    }
}
