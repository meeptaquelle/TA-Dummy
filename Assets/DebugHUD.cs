using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Developer-only debug overlay. Attach to any GameObject in the scene.
/// Shows all chaser states and shared blackboard info in the screen corner.
/// </summary>
public class ChaserDebugHUD : MonoBehaviour
{
    [Header("Settings")]
    public bool showHUD = true;
    public KeyCode toggleKey = KeyCode.F1;

    // Grab all chasers at runtime via reflection-free reference
    List<ChaserAI> chasers = new();

    // Styles — built once
    GUIStyle boxStyle, headerStyle, labelStyle, stateStyle;
    bool stylesBuilt = false;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            showHUD = !showHUD;

        // Keep list fresh (handles runtime spawn/destroy)
        chasers.Clear();
        chasers.AddRange(FindObjectsByType<ChaserAI>(FindObjectsSortMode.None));
    }

    void BuildStyles()
    {
        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.75f));
        boxStyle.padding = new RectOffset(10, 10, 8, 8);

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 13;
        headerStyle.normal.textColor = Color.white;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 11;
        labelStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

        stateStyle = new GUIStyle(GUI.skin.label);
        stateStyle.fontStyle = FontStyle.Bold;
        stateStyle.fontSize = 11;

        stylesBuilt = true;
    }

    void OnGUI()
    {
        if (!showHUD) return;
        if (!stylesBuilt) BuildStyles();

        var bb = SharedBlackboard.Instance;
        if (bb == null) return;

        float panelWidth = 220f;
        float x = Screen.width - panelWidth - 10f;
        float y = 10f;
        float lineH = 20f;

        // ── Calculate panel height dynamically ───────────────────
        float panelHeight = 30f             // header
                          + 20f             // divider gap
                          + 60f             // blackboard section
                          + (chasers.Count * 90f); // per-agent section

        GUILayout.BeginArea(new Rect(x, y, panelWidth, panelHeight), boxStyle);

        // ── Header ───────────────────────────────────────────────
        GUILayout.Label("CHASER DEBUG", headerStyle);
        DrawDivider();

        // ── Shared Blackboard ─────────────────────────────────────
        GUILayout.Label("[ Blackboard ]", headerStyle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Stamina:", labelStyle, GUILayout.Width(70));
        DrawStaminaBar(bb.sharedStamina, 30f, panelWidth - 100f);
        GUILayout.EndHorizontal();

        GUILayout.Label($"Detected:  {(bb.playerDetected ? "YES" : "no")}",
            ColoredStyle(labelStyle, bb.playerDetected ? Color.red : Color.gray));
        GUILayout.Label($"Relay:     {(bb.exhaustedAgent != null ? bb.exhaustedAgent.name : "none")}", labelStyle);

        DrawDivider();

        // ── Per-Agent ─────────────────────────────────────────────
        foreach (var chaser in chasers)
        {
            if (chaser == null) continue;

            GUILayout.Label($"[ {chaser.name} ]", headerStyle);

            // State
            var state = chaser.DebugState;
            Color stateColor = state switch
            {
                "Chase" => Color.red,
                "Search" => Color.yellow,
                _ => Color.green
            };
            GUILayout.Label($"State:     {state}", ColoredStyle(stateStyle, stateColor));

            // Role
            if (bb.roleRegistry.TryGetValue(chaser, out var role))
            {
                Color roleColor = role == SharedBlackboard.AgentRole.Rusher ? Color.red : Color.magenta;
                GUILayout.Label($"Role:      {role}", ColoredStyle(stateStyle, roleColor));
            }
            else
            {
                GUILayout.Label("Role:      —", labelStyle);
            }

            // Distance to player
            GUILayout.Label($"Dist:      {chaser.DebugDistToPlayer:F1}m", labelStyle);

            DrawDivider();
        }

        GUILayout.Label($"F1  toggle HUD", ColoredStyle(labelStyle, new Color(0.5f, 0.5f, 0.5f)));

        GUILayout.EndArea();
    }

    // ── Helpers ───────────────────────────────────────────────────
    void DrawStaminaBar(float current, float max, float width)
    {
        float fill = Mathf.Clamp01(current / max);
        Color fillColor = Color.Lerp(Color.red, Color.green, fill);

        Rect barRect = GUILayoutUtility.GetRect(width, 14f);

        // Background
        GUI.color = new Color(0.2f, 0.2f, 0.2f);
        GUI.DrawTexture(barRect, Texture2D.whiteTexture);

        // Fill
        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(barRect.x, barRect.y, barRect.width * fill, barRect.height), Texture2D.whiteTexture);

        // Value label
        GUI.color = Color.white;
        GUI.Label(barRect, $" {current:F0}", labelStyle);

        GUI.color = Color.white;
    }

    void DrawDivider()
    {
        GUILayout.Space(2);
        Rect r = GUILayoutUtility.GetRect(1, 1);
        GUI.color = new Color(1f, 1f, 1f, 0.15f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUILayout.Space(4);
    }

    GUIStyle ColoredStyle(GUIStyle source, Color color)
    {
        var s = new GUIStyle(source);
        s.normal.textColor = color;
        return s;
    }

    Texture2D MakeTex(int w, int h, Color col)
    {
        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }
}