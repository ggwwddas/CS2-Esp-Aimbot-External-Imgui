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
        private void ApplyDarkStyle()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            style.WindowRounding = 10f;
            style.FrameRounding = 4f;
            style.GrabRounding = 4f;
            style.FrameBorderSize = 1f;
            style.WindowBorderSize = 1f;
            style.ItemSpacing = new Vector2(10, 8);
            style.ScrollbarSize = 12f;
            style.FramePadding = new Vector2(10, 8); // Yatay ve dikey padding menubar

            style.WindowMenuButtonPosition = ImGuiDir.Right;
            style.WindowTitleAlign = new Vector2(0.60f, 0.60f);

            //text
            colors[(int)ImGuiCol.Text] = new Vector4(0.95f, 0.96f, 0.98f, 1.00f);

            //window
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.08f, 0.08f, 0.10f, 0.8f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.10f, 0.10f, 0.12f, 0.8f);

            //checkbox
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.12f, 0.12f, 0.15f, 1.00f); // checkbox nonhovered color
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.20f, 0.20f, 0.25f, 1.00f); //hover
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.30f, 0.30f, 0.35f, 1.00f); //active
            style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.0f, 1.0f, 0.0f, 1.0f); //check color

            //title
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.08f, 0.08f, 0.10f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.10f, 0.10f, 0.12f, 1.00f);

            //button
            colors[(int)ImGuiCol.Button] = new Vector4(0.18f, 0.18f, 0.22f, 1.00f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.28f, 0.28f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.40f, 0.40f, 0.50f, 1.00f);

        }

        #endregion

        // draw list
        ImDrawListPtr drawList;
        int selectedTab = 0;


        protected override void Render()
        {

            var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);

                    ApplyDarkStyle();
                    ImGui.SetNextWindowSize(new Vector2(700,500));
                    ImGui.Begin("Hexa CS2", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.MenuBar);


                    if (ImGui.BeginMenuBar())
                    {
                        ImGui.SetCursorPosX(350);
                        ImGui.Text("Hexa CS2");
                        ImGui.EndMenuBar();
                    }
            
                    #region sidebar
                    // Sol Sidebar
                    ImGui.BeginChild("Sidebar", new Vector2(100, 0));

                    ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 4));
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.15f, 0.15f, 0.15f, 1f)); // Normal
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.25f, 0.25f, 0.25f, 1f)); // Hover
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.6f, 0.2f, 0.2f, 1f)); // Aktif

                    // Yukarıdan ve soldan boşluk
                    ImGui.SetCursorPosY(10); // Yukarıdan 10px boşluk
                    ImGui.SetCursorPosX(5); // Soldan 5px boşluk

                    //====================COLORS=========================\\
                    if (ImGui.Button("Colors", new Vector2(90, 30))) selectedTab = 0;

                    //====================RAGE=========================\\
                    ImGui.SetCursorPosX(5); // Soldan 5px boşluk
                    if (ImGui.Button("Rage", new Vector2(90, 30))) selectedTab = 1;

                    //====================THICKNESS=========================\\
                    ImGui.SetCursorPosX(5); // Soldan 5px boşluk
                    if (ImGui.Button("Legit", new Vector2(90, 30))) selectedTab = 2;

                    //====================EXIT=========================\\
                    ImGui.SetCursorPosY(420);
                    ImGui.SetCursorPosX(5); // Soldan 5px boşluk
                    if (ImGui.Button("EXIT", new Vector2(90, 30))) selectedTab = 3;

                    ImGui.PopStyleColor(3);
                    ImGui.PopStyleVar(2);
                    ImGui.EndChild();

                    #endregion

                    ImGui.SameLine();

                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 10.0f);

                    #region tabs
                    // Sağ Panel
                    ImGui.BeginChild("MainPanel", new Vector2(575, 455));

                    switch (selectedTab)
                    {
                        case 0:
                            //===================TEAM COLOR=========================\\
                            ImGui.SetCursorPosX(10);
                            ImGui.SetCursorPosY(10);
                            ImGui.Text("Color Settign");
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
                            ImGui.PopStyleVar(2); break;

                        //===================MISC=========================\\
                        case 1:

                            ImGui.GetStyle().FramePadding = new Vector2(6, 6);

                            ImGui.SetCursorPosX(10);
                            ImGui.Text("Rage Area");

                            ImGui.Separator();
                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Box ESP", ref boxESP);

                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Fill Box ESP", ref fillESP);

                            ImGui.Spacing();

                            ImGui.Separator();
                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Name ESP", ref nameESP);

                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Health ESP", ref healthESP);

                            ImGui.Spacing();

                            ImGui.Separator();
                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Bone ESP", ref enableESP);
                            ImGui.SameLine();
                            ImGui.Checkbox("Fill Head", ref fillheadESP);

                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Snaplines ESP", ref snaplines);

                            ImGui.Spacing();

                            ImGui.Separator();
                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Aimbot - Mouse 5", ref aimbot);

                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("Aim On Mate", ref aimonmate);


                            ImGui.SetCursorPosX(10);
                            if (ImGui.Button("Enable All Esp's", new Vector2(140, 25)))
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

                            ImGui.SetCursorPosX(10);

                            if (ImGui.Button("Disable All Esp's" ,new Vector2(140, 25)))
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

                            break;

                        case 2:

                            ImGui.GetStyle().FramePadding = new Vector2(6, 6);
                            ImGui.SetCursorPosX(10);
                            ImGui.Text("Legit Area");

                            ImGui.SetCursorPosX(10);
                            ImGui.Checkbox("TriggerBot", ref trigger);
                    
                            ImGui.SetCursorPosX(10);
                            ImGui.SliderFloat("FOV Value", ref Fovdes, 60f, 160f);

                            ImGui.SetCursorPosX(10);
                            ImGui.SliderFloat("Snapline PosY", ref snaplinePos, 0f, 1100f);
                            break;

                        //===================EXIT=========================\\
                        case 3:
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