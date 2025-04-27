using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;
using System.Windows;
using System.Runtime.InteropServices;
using Vortice.Mathematics;
using System.Linq.Expressions;

namespace cs2simpleESPBones
{
    public class Renderer : Overlay
    {
        // render variables

        #region win32
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;


        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        int HOTKEY = 0x10;
        #endregion

        #region setting

        public Vector2 screenSize = new Vector2(1920, 1080); // use your own display screen resolution example (1920, 1080)

        // entities copies, using thread safe
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        #endregion

        #region var
        private bool enableESP = false;
        private bool boxESP = false;
        private bool snaplines = false;
        private bool fillESP = false;
        private bool nameESP = false;
        private bool healthESP = false;
        private bool fillheadESP = false;

        public bool trigger = false;
        public bool aimbot = false;
        public bool aimonmate = false;

        public float Fovdes = 90f; // fov
        private float boneThickness2 = 4.0f; // bone thickness
        private float boxThickness = 4.0f; // box thickness
        private float snaplinePos = 0.0f; // snapline pos

        private Vector4 enemyColor = new Vector4(1, 0, 0, 1); // default red
        private Vector4 teamColor = new Vector4(0, 1, 0, 1); // default green
        private Vector4 fillColor = new Vector4(0.3f, 0.3f, 0.3f, 0.4f); // white transparent
        private Vector4 boneColor = new Vector4(1, 1, 1, 1); // def white
        private Vector4 nameColor = new Vector4(1, 1, 1, 1); // def white


        #endregion

        #region style
        void ApplyStyle()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // Genel stil ayarları
            style.WindowPadding = new Vector2(8, 8);
            style.FramePadding = new Vector2(4, 3);
            style.CellPadding = new Vector2(4, 2);
            style.ItemSpacing = new Vector2(8, 4);
            style.ItemInnerSpacing = new Vector2(4, 4);
            style.TouchExtraPadding = new Vector2(0, 0);
            style.IndentSpacing = 21.0f;
            style.ScrollbarSize = 14.0f;
            style.GrabMinSize = 12.0f;

            style.WindowBorderSize = 1.0f;
            style.ChildBorderSize = 1.0f;
            style.PopupBorderSize = 1.0f;
            style.FrameBorderSize = 0.0f;
            style.TabBorderSize = 0.0f;

            style.WindowRounding = 6.0f;
            style.ChildRounding = 6.0f;
            style.FrameRounding = 3.0f;
            style.PopupRounding = 6.0f;
            style.ScrollbarRounding = 9.0f;
            style.GrabRounding = 3.0f;
            style.TabRounding = 4.0f;

            colors[(int)ImGuiCol.Text] = new Vector4(0.90f, 0.90f, 0.90f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09f, 0.09f, 0.09f, 0.94f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.11f, 0.11f, 0.11f, 1f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.11f, 0.11f, 0.11f, 0.94f);
            colors[(int)ImGuiCol.Border] = new Vector4(0.27f, 0.27f, 0.27f, 0.50f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.20f, 0.20f, 0.20f, 0.54f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.27f, 0.27f, 0.27f, 0.54f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.32f, 0.32f, 0.32f, 0.54f);
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.12f, 0.12f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.12f, 0.12f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.12f, 0.12f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.45f, 0.45f, 0.45f, 0.54f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.66f, 0.66f, 0.66f, 1.00f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.20f, 0.20f, 0.20f, .34f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.27f, 0.27f, 0.27f, 0.54f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.32f, 0.32f, 0.32f, 0.54f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.20f, 0.20f, 0.20f, 0.54f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.27f, 0.27f, 0.27f, 0.54f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.32f, 0.32f, 0.32f, 0.54f);
            colors[(int)ImGuiCol.Separator] = new Vector4(0.27f, 0.27f, 0.27f, 0.50f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.32f, 0.32f, 0.32f, 0.54f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.37f, 0.37f, 0.37f, 0.54f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.50f, 0.50f, 0.50f, 0.29f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.15f, 0.15f, 0.15f, 0.86f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.25f, 0.25f, 0.25f, 0.86f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.20f, 0.20f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.10f, 0.10f, 0.10f, 0.97f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);
            colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
            colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.00f, 1.00f, 0.00f, 0.90f);
            colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 1.00f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.80f, 0.80f, 0.80f, 0.35f);
        }
        #endregion

        ImDrawListPtr drawList;
        int activeTab = 1;

        protected override void Render()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            ApplyStyle();
            ImGui.SetNextWindowSize(new Vector2(690,460));
            ImGui.Begin("Hexa CS2", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            #region font
            ReplaceFont("C:\\Windows\\Fonts\\arialbd.TTF", 14, FontGlyphRangeType.English);
            #endregion

            if (ImGui.BeginMenuBar())
            {
              ImGui.SetCursorPosX(315);
              ImGui.Text("Hexa CS2");
              ImGui.EndMenuBar();
            }

            #region sidebar
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5f);
            ImGui.BeginChild("LeftTabs", new Vector2(170, 0), ImGuiChildFlags.None);

            //TABS

            ImGui.SetCursorPosY(5);
            ImGui.SetCursorPosX(66.8f);
            ImGui.SetWindowFontScale(1.1f);
            ImGui.Text("Hexa");
            ImGui.Spacing();
            ImGui.Separator();
            //ImGui.Spacing();
            ImGui.SetWindowFontScale(1.0f);

            //Rage Wallhack-Aimbot
            ImGui.SeparatorText("Rage");
            if (ImGui.Button("WallHack", new Vector2(-1, 35)))
            {
                activeTab = 1;
            }
            ImGui.PopStyleColor();


            if (ImGui.Button("Aimbot", new Vector2(-1, 35)))
            {
                activeTab = 2;
            }
            //Rage Wallhack-Aimbot

            // semi safe triggerbot
            ImGui.SeparatorText("Semi-Safe");
            ImGui.Spacing();
            if (ImGui.Button("Trigger", new Vector2(-1, 35)))
            {
                activeTab = 3;
            }
            ImGui.Spacing();
            // semi safe triggerbot

            // safe visuals fov-snaplineposx
            ImGui.SeparatorText("Safe Visuals");
            ImGui.Spacing();
            if (ImGui.Button("Field Of View", new Vector2(-1, 35)))
            {
                activeTab = 4;
            }

            if (ImGui.Button("Snapline PosX", new Vector2(-1, 35)))
            {
                activeTab = 5;
            }
            ImGui.Spacing();
            // safe visuals fov-snaplineposx


            // other misc-exit
            ImGui.SeparatorText("Other");
            ImGui.Spacing();
            if (ImGui.Button("Misc", new Vector2(-1, 35)))
            {
                activeTab = 6;
            }
            ImGui.PopStyleColor();

            if (ImGui.Button("Exit", new Vector2(-1, 35)))
            {
                activeTab = 7;
            }
            // other misc-exit


            ImGui.EndChild();
            ImGui.SameLine();
            #endregion
            
            ImGui.SameLine();

            #region tabs
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.11f, 0.11f, 0.11f, 1f);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 10f);
            ImGui.BeginChild("MainPanel", new Vector2(475, 00));

            switch (activeTab)
              {
                case 1:
                    WallhackTab();
                    break;

                case 2:
                    AimbotTab();
                    break;

                case 3:
                    triggerLegit();
                    break;

                case 4:
                    VisualFovLegit();
                    break;

                case 5:
                    VisualSnaplineLegit();
                    break;

                case 6:
                    MiscTab();
                    break;

                case 7:
                    Environment.Exit(0);
                    break;
              }
            
            ImGui.EndChild();
            #endregion

            DrawOverlay(screenSize);
            drawList = ImGui.GetWindowDrawList();

            // draw stuff
            if (enableESP)
                        {
                            foreach (var entity in entities)
                            {
                                // check if entity on screen
                                if (EntityOnScreen(entity))
                                {
                                    DrawBones(entity);
                                }
                            }
                        }

            if (boxESP)
                        {
                            foreach (var entity in entities)
                            {
                                // check if entity on screen
                                if (EntityOnScreen(entity))
                                {
                                    DrawBox(entity);
                                }
                            }
                        }

            if (fillheadESP)
                        {
                            foreach (var entity in entities)
                            {
                                // check if entity on screen
                                if (EntityOnScreen(entity))
                                {
                                    DrawHeadFilled(entity);
                                }
                            }
                        }

            if (fillESP)
                        {
                            foreach (var entity in entities)
                            {
                                // check if entity on screen
                                if (EntityOnScreen(entity))
                                {
                                    DrawFillBox(entity);
                                }
                            }
                        }
               
            if (healthESP)
                        {
                            foreach (var entity in entities)
                            {
                                // check if entity on screen
                                if (EntityOnScreen(entity))
                                {
                                    DrawHealth(entity);
                                }
                            }
                        }

            if (nameESP)
                        {
                            foreach (var entity in entities)
                            {
                                // check if entity on screen
                                if (EntityOnScreen(entity))
                                {
                                    DrawName(entity, 0);
                                }
                            }
                        }

            if (snaplines)
                        {
                            foreach (var entity in entities)
                            {
                                // check if entity on screen
                                if (EntityOnScreen(entity))
                                {
                                    DrawHeadLine(entity);
                                }
                            }
                        }
            }

  
        private void WallhackTab()
        {
            ImGui.GetStyle().FramePadding = new Vector2(6, 6);

            ImGui.Spacing();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Rage Area");

            ImGui.SeparatorText("Box ESP Settings");
            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Box ESP", ref boxESP);

            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Fill Box ESP", ref fillESP);
            ImGui.Spacing();


            ImGui.SeparatorText("Other ESP Settings");
            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Name ESP", ref nameESP);

            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Health ESP", ref healthESP);
            ImGui.Spacing();


            ImGui.SeparatorText("Bone ESP Settings");
            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Bone ESP", ref enableESP);
            ImGui.SameLine();
            ImGui.Checkbox("Fill Head", ref fillheadESP);

            ImGui.SeparatorText("Snapline");
            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Snaplines ESP", ref snaplines);

            ImGui.SeparatorText("Enable - Disable ESP Settings");
            ImGui.Spacing();
            ImGui.SetCursorPosX(10);
            if (ImGui.Button("Enable All Esp's", new Vector2(140, 35)))
            {
                enableESP = true;
                fillheadESP = true;
                boxESP = true;
                fillESP = true;
                nameESP = true;
                healthESP = true;
                snaplines = true;
                aimbot = true;
            }

            ImGui.SameLine();

            ImGui.SetCursorPosX(170);
            if (ImGui.Button("Disable All Esp's", new Vector2(140, 35)))
            {
                ImGui.SetCursorPosX(10);
                enableESP = false;
                fillheadESP = false;
                boxESP = false;
                fillESP = false;
                nameESP = false;
                healthESP = false;
                snaplines = false;
                aimbot = false;
            }
        }
        
        private void AimbotTab()
        {
            ImGui.GetStyle().FramePadding = new Vector2(6, 6);

            ImGui.Spacing();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Rage Area");
            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Aimbot - Mouse 5", ref aimbot);

            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("Aim On Mate", ref aimonmate);
        }

        private void triggerLegit()
        {
            ImGui.GetStyle().FramePadding = new Vector2(6, 6);

            ImGui.Spacing();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Semi-Legit Area");
            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.Checkbox("TriggerBot", ref trigger);
        }

        private void VisualFovLegit()
        {
            ImGui.GetStyle().FramePadding = new Vector2(6, 6);

            ImGui.Spacing();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Legit Area");
            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.SliderFloat("FOV Value", ref Fovdes, 60f, 160f);
        }

        private void VisualSnaplineLegit()
        {
            ImGui.GetStyle().FramePadding = new Vector2(6, 6);

            ImGui.Spacing();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Legit Area");
            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.SliderFloat("Snapline PosY", ref snaplinePos, 0f, 1100f);
        }

        private void MiscTab()
        {
            //===================TEAM COLOR=========================\\

            ImGui.Spacing();
            ImGui.Spacing();

            ImGui.SetCursorPosY(10);
            ImGui.SetCursorPosX(10);

            ImGui.Text("Color Settings");
            ImGui.Spacing();

            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Team Color");
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

            ImGui.SetCursorPosX(10);
            ImGui.ColorEdit4("##teamcolor", ref teamColor,
                ImGuiColorEditFlags.NoInputs |
                ImGuiColorEditFlags.NoLabel |
                ImGuiColorEditFlags.AlphaPreviewHalf);
            ImGui.PopStyleVar(2);

            ImGui.Spacing();


            //===================ENEMY COLOR=========================\\

            ImGui.SetCursorPosX(10);
            ImGui.Text("Enemy Color");
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

            ImGui.SetCursorPosX(10);
            ImGui.ColorEdit4("##enemycolor", ref enemyColor,
                ImGuiColorEditFlags.NoInputs |
                ImGuiColorEditFlags.NoLabel |
                ImGuiColorEditFlags.AlphaPreviewHalf);
            ImGui.PopStyleVar(2);

            ImGui.Spacing();

            //===================BONECOLOR=========================\\
            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Bone Color");
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

            ImGui.SetCursorPosX(10);
            ImGui.ColorEdit4("##bonecolor", ref boneColor,
                ImGuiColorEditFlags.NoInputs |
                ImGuiColorEditFlags.NoLabel |
                ImGuiColorEditFlags.AlphaPreviewHalf);
            ImGui.PopStyleVar(2);

            ImGui.Spacing();

            //===================FILL COLOR=========================\\
            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Fill Color");
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

            ImGui.SetCursorPosX(10);
            ImGui.ColorEdit4("##fillcolor", ref fillColor,
                ImGuiColorEditFlags.NoInputs |
                ImGuiColorEditFlags.NoLabel |
                ImGuiColorEditFlags.AlphaPreviewHalf);
            ImGui.PopStyleVar(2);

            //===================NAME COLOR=========================\\
            ImGui.Separator();

            ImGui.SetCursorPosX(10);
            ImGui.Text("Name Color");
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 6));
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 3.0f);

            ImGui.SetCursorPosX(10);
            ImGui.ColorEdit4("##namecolor", ref nameColor,
                ImGuiColorEditFlags.NoInputs |
                ImGuiColorEditFlags.NoLabel |
                ImGuiColorEditFlags.AlphaPreviewHalf);
            ImGui.PopStyleVar(2);
        }

            // check position
        bool EntityOnScreen(Entity entity)
        {
            if (entity.position2D.X > 0 && entity.position2D.X < screenSize.X && entity.position2D.Y > 0 && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
            }
        // drawing methods
        private void DrawBones(Entity entity)
        {
            uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);

            float currentBoneThickness = boneThickness2 / entity.distance;

            drawList.AddLine(entity.bones2d[1], entity.bones2d[2], uintColor, currentBoneThickness); // neck to head
            drawList.AddLine(entity.bones2d[1], entity.bones2d[3], uintColor, currentBoneThickness); // neck to left shoulder
            drawList.AddLine(entity.bones2d[1], entity.bones2d[6], uintColor, currentBoneThickness); // neck to shoulder right
            drawList.AddLine(entity.bones2d[3], entity.bones2d[4], uintColor, currentBoneThickness); // shoulder left to aem left
            drawList.AddLine(entity.bones2d[6], entity.bones2d[7], uintColor, currentBoneThickness); // shoulder right to arm right
            drawList.AddLine(entity.bones2d[4], entity.bones2d[5], uintColor, currentBoneThickness); // arm left to hand left
            drawList.AddLine(entity.bones2d[7], entity.bones2d[8], uintColor, currentBoneThickness); // arm right to hand right
            drawList.AddLine(entity.bones2d[1], entity.bones2d[0], uintColor, currentBoneThickness); // neck to waist
            drawList.AddLine(entity.bones2d[0], entity.bones2d[9], uintColor, currentBoneThickness); // waist to knee left
            drawList.AddLine(entity.bones2d[0], entity.bones2d[11], uintColor, currentBoneThickness); // waist to knee right
            drawList.AddLine(entity.bones2d[9], entity.bones2d[10], uintColor, currentBoneThickness); // knee left to feet left
            drawList.AddLine(entity.bones2d[11], entity.bones2d[12], uintColor, currentBoneThickness); // knee right to feet right
            drawList.AddCircle(entity.bones2d[2], 3 + currentBoneThickness, uintColor); // circle on head the  dot
        }

        private void DrawHeadFilled(Entity entity)
        {
            uint uintColor = ImGui.ColorConvertFloat4ToU32(boneColor);

            float currentBoneThickness = boneThickness2 / entity.distance;

            drawList.AddCircleFilled(entity.bones2d[2], 3 + currentBoneThickness, uintColor); // circle on head the  dot
        }

        private void DrawBox(Entity entity)
        {
            uint baseColor = ImGui.ColorConvertFloat4ToU32(boneColor);

            float currentBoneThickness = boxThickness / entity.distance;

            // calculate height
            float entityHeight = entity.viewPosition2D.Y - entity.position2D.Y;

            // calculate dimensions
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);

            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);

            // get correct color
            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));

            // Border kalınlığını entity uzaklığına göre hesapla
            float borderThickness = Math.Max(1.5f, 5.0f / entity.distance); // Uzaklık arttıkça border incelir

            // Border için siyah renk
            Vector4 borderColor = new Vector4(0, 0, 0, 1); // Siyah border

            drawList.AddRect(new Vector2(rectTop.X - 2, rectTop.Y - 1),
                             new Vector2(rectBottom.X + 2, rectBottom.Y + 1),
                             ImGui.ColorConvertFloat4ToU32(borderColor), 0.0f, ImDrawFlags.None, borderThickness);

        }

        private void DrawFillBox(Entity entity)
        {
            uint baseColor = ImGui.ColorConvertFloat4ToU32(boneColor);

            float currentBoneThickness = boxThickness / entity.distance;

            // calculate height
            float entityHeight = entity.viewPosition2D.Y - entity.position2D.Y;

            // calculate dimensions
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);

            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);

            // get correct color
            Vector4 boxColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            drawList.AddRectFilled(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor));
        }
    
        private void DrawHeadLine(Entity entity)
        {
            ///entity.position2D = ayak
            ///entity.viewPosition2D = kafa
            ///(pos + viewPos) / 2 = gövde

            // get correct color
            Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor;

            Vector2 lineStart = new Vector2(screenSize.X / 2, screenSize.Y - snaplinePos);
            Vector2 lineEnd = new Vector2(entity.viewPosition2D.X,entity.viewPosition2D.Y);
            Vector2 headPos = entity.viewPosition2D;

            drawList.AddCircleFilled(lineEnd, 5, ImGui.ColorConvertFloat4ToU32(lineColor));

            drawList.AddLine(lineStart,headPos, ImGui.ColorConvertFloat4ToU32(lineColor));

        }

        private void DrawName(Entity entity, int yOffset)
        {
            if(nameESP)
            {   
                Vector2 textloc = new Vector2(entity.position2D.X - 45, entity.position2D.Y - yOffset); // text location
                drawList.AddText(textloc, ImGui.ColorConvertFloat4ToU32(nameColor), $"{entity.name}"); // draw name
            }
        }

        private void DrawHealth(Entity entity)
        {
            if (entity.health <= 0 || entity.health > 100)
                return; // Skip dead or invalid health

            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;


            float leftBox = entity.viewPosition2D.X - entityHeight / 2.7f;
            float rightBox = entity.position2D.X + entityHeight / 2.7f;

            float barPercentage = 0.05f;
            float barPixelWidth = MathF.Abs(barPercentage * (leftBox - rightBox));

            float barHeight = entityHeight * (entity.health / 100.0f);


            Vector2 barTop = new Vector2(leftBox - barPixelWidth, entity.position2D.Y - barHeight);
            Vector2 barBottom = new Vector2(leftBox, entity.position2D.Y);


            uint topColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0, 1, 0, 1));    // Green
            uint bottomColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)); // Red

            drawList.AddRectFilledMultiColor(
                barTop,
                barBottom,
                topColor,    // top-left
                topColor,    // top-right
                bottomColor, // bottom-right
                bottomColor  // bottom-left
            );

            // Border kalınlığını entity uzaklığına göre hesapla
            float borderThickness = Math.Max(1.0f, 5.0f / entity.distance); // Uzaklık arttıkça border incelir

            // Border için siyah renk
            Vector4 borderColor = new Vector4(0, 0, 0, 1); // Siyah border

            // Border çizimi: Kenarları çizmek için AddRect fonksiyonu
            drawList.AddRect(barTop - new Vector2(borderThickness, borderThickness),
                             barBottom + new Vector2(borderThickness, borderThickness),
                             ImGui.ColorConvertFloat4ToU32(borderColor), 0.0f, ImDrawFlags.None, borderThickness);
        }

        // transfer entity methods
        public void UpdateEntities(IEnumerable<Entity> newEntities) // update entities
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity) // update localplayer
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        public Entity GetlocalPlayer() // get localplayer
        {
            lock (entityLock)
            {
                return localPlayer;
            }
        }

        void DrawOverlay(Vector2 screenSize) // Overlay window
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0)); // beginning
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }

    }
}
