using cs2simpleESPBones;
using Swed64;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Vortice.Mathematics;

[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);

int HOTKEY = 0x06; //mosue 5
int HOTKEY2 = 0x58; //x

Swed swed = new Swed("cs2");
IntPtr client = swed.GetModuleBase("client.dll");

Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();


Vector2 screenSize = renderer.screenSize;

// store entities
List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();


// offsets.cs
int dwEntityList = 0x1A1F670;
int dwViewMatrix = 0x1A89070;
int dwLocalPlayerPawn = 0x1874040;
int dwViewAngles = 0x1A93300;

// client.dll.cs
int m_vOldOrigin = 0x1324;
int m_iTeamNum = 0x3E3;
int m_lifeState = 0x348;
int m_hPlayerPawn = 0x814;
int m_vecViewOffset = 0xCB0;
int m_modelState = 0x170;
int m_pGameSceneNode = 0x328;
int m_iszPlayerName = 0x660;
int m_iHealth = 0x344;
int m_iIDEntIndex = 0x1458;
int m_iFOV = 0x210;
int m_pCameraServices = 0x11E0;
int m_bIsScoped = 0x23E8;

Thread triggerThread = new Thread(TriggerLoop);
triggerThread.Start();
Thread fovThread = new Thread(FovLoop);
fovThread.Start();

// now ESP loop

while (true)
{
    entities.Clear();

    // get entity list
    IntPtr entityList = swed.ReadPointer(client, dwEntityList);

    // list entry
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    // get localplayer
    IntPtr localPlayerPawn = swed.ReadPointer(client, dwLocalPlayerPawn);

    // get team
    localPlayer.team = swed.ReadInt(localPlayerPawn, m_iTeamNum);

    for (int i = 0; i < 64; i++)
    {
        // get current controller
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);

        if (currentController == IntPtr.Zero) continue; // check

        // get pawn handle
        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
        if (pawnHandle == 0) continue;

        // get current pawn, make second entry
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        if (listEntry2 == IntPtr.Zero) continue;

        // get current pawn
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero) continue;

        // check if lifestate
        int lifeState = swed.ReadInt(currentPawn, m_lifeState);
        if (lifeState != 256) continue;

        int team = swed.ReadInt(currentPawn, m_iTeamNum);

        if (team == localPlayer.team && !renderer.aimonmate)
            continue;

        // get matrix
        float[] viewMatrix = swed.ReadMatrix(client + dwViewMatrix);

        IntPtr sceneNode = swed.ReadPointer(currentPawn, m_pGameSceneNode);
        IntPtr boneMatrix = swed.ReadPointer(sceneNode, m_modelState + 0x80); // 0x80 would be dwBoneMatrix))

        // populate entity

        Entity entity = new Entity(); // entity
        entity.health = swed.ReadInt(currentPawn, m_iHealth);
        entity.name = swed.ReadString(currentController, m_iszPlayerName, 16).Split("\0")[0];
        entity.team = swed.ReadInt(currentPawn, m_iTeamNum);
        entity.position = swed.ReadVec(currentPawn, m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);
        entity.distance = Vector3.Distance(entity.position, localPlayer.position);
        entity.bones = Calculate.ReadBones(boneMatrix, swed);
        entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, screenSize);
        entity.origin = entity.position;
        entity.view = entity.viewOffset;
        entities.Add(entity);
    }
    // update renderer data
    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);

    entities = entities.OrderBy(e => e.distance).ToList();

    //aimbot
    if (entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
    {
        localPlayer.origin = swed.ReadVec(localPlayerPawn, m_vOldOrigin);
        localPlayer.view = swed.ReadVec(localPlayerPawn, m_vecViewOffset);

        Vector3 playerview = Vector3.Add(localPlayer.origin, localPlayer.view);
        Vector3 entityview = Vector3.Add(entities[0].origin, entities[0].view);

        Vector2 newAngles = Calculate.ViewCalculate(playerview, entityview);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);

        swed.WriteVec(client, dwViewAngles, newAnglesVec3);
    }
}

void TriggerLoop()
{
    while (true)
    {
        IntPtr attack = client + 0x186C840;
        IntPtr localplayer = swed.ReadPointer(client, dwLocalPlayerPawn);
        int index = swed.ReadInt(localplayer, m_iIDEntIndex);

        if (renderer.trigger && GetAsyncKeyState(HOTKEY2) < 0)
        {
            if (index != -1)
            {
                swed.WriteInt(attack, 65537); // +attack    
                Thread.Sleep(1);
                swed.WriteInt(attack, 256); // -attack    
            }
        }
        Thread.Sleep(1);
    }
}

void FovLoop()
{
    while (true)
    {
        uint desireffov = (uint)renderer.Fovdes;
        IntPtr localplayer = swed.ReadPointer(client, dwLocalPlayerPawn);
        if (localplayer == IntPtr.Zero) continue;

        IntPtr camera = swed.ReadPointer(localplayer, m_pCameraServices);
        if (camera == IntPtr.Zero) continue;

        uint currentfov = swed.ReadUInt(camera + m_iFOV);
        bool isScoped = swed.ReadBool(localplayer + m_bIsScoped);

        if (!isScoped && currentfov != desireffov)
        {
            swed.WriteUInt(camera + m_iFOV, desireffov);
        }

        Thread.Sleep(50);
    }
}
