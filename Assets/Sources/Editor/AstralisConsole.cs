using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Sources.Editor
{
    public class AstralisConsole : EditorWindow
    {
        // Logs storage
        private List<LogEntry> logs = new List<LogEntry>();
        private Vector2 scrollPosition;
        
        // Filters
        private bool showLogs = true;
        private bool showWarnings = true;
        private bool showErrors = true;
        private string searchFilter = "";
        
        // Category filters
        private Dictionary<string, bool> categoryFilters = new Dictionary<string, bool>();
        
        // List of hardcoded categories
        private static readonly string[] AllCategories = new string[]
        {
            "AI",
            "Animation",
            "Audio",
            "Enemy",
            "GameManager",
            "General",
            "Input",
            "Physics",
            "Player",
            "Save",
            "UI"
        };
        
        // Counts
        private int logCount = 0;
        private int warningCount = 0;
        private int errorCount = 0;
        
        // UI Settings
        private bool errorPause = false;
        private GUIStyle logStyle;
        private GUIStyle warningStyle;
        private GUIStyle errorStyle;
        private bool stylesInitialized = false;
        
        [MenuItem("Tools/Astralis/Console")]
        public static void ShowWindow()
        {
            AstralisConsole window = GetWindow<AstralisConsole>("Astralis Console");
            window.Show();
        }
        
        private void OnEnable()
        {
            // Subscribe to AstralisDebug logs only
            Sources.AstralisDebug.OnLogWithCategory += HandleCategorizedLog;
            
            // Initialize category filters (all enabled by default)
            InitializeCategoryFilters();
        }
        
        private void InitializeCategoryFilters()
        {
            foreach (var category in AllCategories)
            {
                if (!categoryFilters.ContainsKey(category))
                {
                    categoryFilters[category] = true; // Enabled by default
                }
            }
        }
        
        private void OnDisable()
        {
            Sources.AstralisDebug.OnLogWithCategory -= HandleCategorizedLog;
        }
        
        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            logStyle = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(5, 5, 2, 2),
                normal = { background = MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.3f)) }
            };
            
            warningStyle = new GUIStyle(logStyle)
            {
                normal = { 
                    background = MakeTexture(2, 2, new Color(0.8f, 0.6f, 0.2f, 0.3f)),
                    textColor = new Color(1f, 0.9f, 0.5f)
                }
            };
            
            errorStyle = new GUIStyle(logStyle)
            {
                normal = { 
                    background = MakeTexture(2, 2, new Color(0.8f, 0.2f, 0.2f, 0.3f)),
                    textColor = new Color(1f, 0.5f, 0.5f)
                }
            };
            
            stylesInitialized = true;
        }
        
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private void HandleCategorizedLog(string category, string message, string stackTrace, LogType type)
        {
            LogEntry entry = new LogEntry
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
                timestamp = DateTime.Now,
                category = category
            };
            
            logs.Add(entry);
            UpdateCounts();
            
            // Auto-scroll to bottom
            scrollPosition.y = float.MaxValue;
            
            Repaint();
        }
        
        private void UpdateCounts()
        {
            logCount = 0;
            warningCount = 0;
            errorCount = 0;
            
            foreach (var log in logs)
            {
                // Only count logs from enabled categories
                if (categoryFilters.ContainsKey(log.category) && !categoryFilters[log.category])
                {
                    continue;
                }
                
                switch (log.type)
                {
                    case LogType.Log:
                        logCount++;
                        break;
                    case LogType.Warning:
                        warningCount++;
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                    case LogType.Assert:
                        errorCount++;
                        break;
                }
            }
        }
        
        private void OnGUI()
        {
            InitializeStyles();
            
            // Toolbar
            DrawToolbar();
            
            EditorGUILayout.Space(2);
            
            // Search bar
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
            EditorGUILayout.EndHorizontal();
            
            // Log list
            DrawLogList();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            // Clear button
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                ClearLogs();
            }
            
            // Error Pause toggle
            errorPause = GUILayout.Toggle(errorPause, "Error Pause", EditorStyles.toolbarButton, GUILayout.Width(80));
            
            GUILayout.FlexibleSpace();
            
            // Filter buttons with counts
            Color defaultColor = GUI.backgroundColor;
            
            // Log filter
            GUI.backgroundColor = showLogs ? Color.white : Color.gray;
            if (GUILayout.Button($"Log ({logCount})", EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                showLogs = !showLogs;
            }
            
            // Warning filter
            GUI.backgroundColor = showWarnings ? new Color(1f, 0.9f, 0.5f) : Color.gray;
            if (GUILayout.Button($"Warn ({warningCount})", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                showWarnings = !showWarnings;
            }
            
            // Error filter
            GUI.backgroundColor = showErrors ? new Color(1f, 0.5f, 0.5f) : Color.gray;
            if (GUILayout.Button($"Error ({errorCount})", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                showErrors = !showErrors;
            }
            
            GUI.backgroundColor = defaultColor;
            
            // Category filter button with icon
            GUIContent categoryIcon = EditorGUIUtility.IconContent("FilterByType");
            categoryIcon.tooltip = "Filter by category";
            if (GUILayout.Button(categoryIcon, EditorStyles.toolbarButton, GUILayout.Width(30)))
            {
                ShowCategoryPopup();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void ShowCategoryPopup()
        {
            GenericMenu menu = new GenericMenu();
            
            // Add Enable All / Disable All options
            menu.AddItem(new GUIContent("Enable All"), false, () =>
            {
                foreach (var category in AllCategories)
                {
                    categoryFilters[category] = true;
                }
                UpdateCounts();
                Repaint();
            });
            
            menu.AddItem(new GUIContent("Disable All"), false, () =>
            {
                foreach (var category in AllCategories)
                {
                    categoryFilters[category] = false;
                }
                UpdateCounts();
                Repaint();
            });
            
            menu.AddSeparator("");
            
            // Add each category with checkbox
            foreach (var category in AllCategories)
            {
                bool isEnabled = categoryFilters.ContainsKey(category) && categoryFilters[category];
                
                menu.AddItem(new GUIContent(category), isEnabled, () =>
                {
                    categoryFilters[category] = !categoryFilters[category];
                    UpdateCounts();
                    Repaint();
                });
            }
            
            menu.ShowAsContext();
        }
        
        private void DrawLogList()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            
            List<LogEntry> filteredLogs = GetFilteredLogs();
            
            for (int i = 0; i < filteredLogs.Count; i++)
            {
                LogEntry log = filteredLogs[i];
                
                // Get appropriate style
                GUIStyle style = GetLogStyle(log.type);
                
                EditorGUILayout.BeginHorizontal(style);
                
                // Icon
                GUIContent icon = GetLogIcon(log.type);
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
                
                // Category badge
                if (!string.IsNullOrEmpty(log.category))
                {
                    Color categoryColor = Sources.AstralisDebug.GetCategoryColor(log.category);
                    Color originalColor = GUI.contentColor;
                    GUI.contentColor = categoryColor;
                    GUILayout.Label($"[{log.category}]", EditorStyles.miniLabel, GUILayout.Width(80));
                    GUI.contentColor = originalColor;
                }
                
                // Message
                string displayMessage = log.message;
                if (displayMessage.Length > 500)
                    displayMessage = displayMessage.Substring(0, 497) + "...";
                
                GUILayout.Label(displayMessage, EditorStyles.label);
                
                // Timestamp
                GUILayout.Label(log.timestamp.ToString("HH:mm:ss"), EditorStyles.miniLabel, GUILayout.Width(60));
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private List<LogEntry> GetFilteredLogs()
        {
            List<LogEntry> filtered = new List<LogEntry>();
            
            foreach (var log in logs)
            {
                // Type filter
                bool typeMatch = false;
                switch (log.type)
                {
                    case LogType.Log:
                        typeMatch = showLogs;
                        break;
                    case LogType.Warning:
                        typeMatch = showWarnings;
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                    case LogType.Assert:
                        typeMatch = showErrors;
                        break;
                }
                
                if (!typeMatch) continue;
                
                // Category filter
                if (categoryFilters.ContainsKey(log.category) && !categoryFilters[log.category])
                {
                    continue;
                }
                
                // Search filter
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    if (!log.message.ToLower().Contains(searchFilter.ToLower()))
                        continue;
                }
                
                filtered.Add(log);
            }
            
            return filtered;
        }
        
        private GUIStyle GetLogStyle(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    return warningStyle;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    return errorStyle;
                default:
                    return logStyle;
            }
        }
        
        private GUIContent GetLogIcon(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon");
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    return EditorGUIUtility.IconContent("console.erroricon");
                default:
                    return EditorGUIUtility.IconContent("console.infoicon");
            }
        }
        
        private void ClearLogs()
        {
            logs.Clear();
            logCount = 0;
            warningCount = 0;
            errorCount = 0;
            Repaint();
        }
        
        // Log Entry class
        private class LogEntry
        {
            public string message;
            public string stackTrace;
            public LogType type;
            public DateTime timestamp;
            public string category = "General";
        }
    }
}
