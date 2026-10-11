#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using ThePirate.Combat;
using ThePirate.Cameras;
using ThePirate.Ports;
using ThePirate.UI.HUD;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThePirate.Editor.World
{
    public static class ThreePinesGameplaySetup
    {
        const string Path = "Assets/ThePirate/Scenes/WybrzezeTrzechSosen.unity";
        [MenuItem("GameWork/Map/Wybrzeze Trzech Sosen/Finish pier")]
        public static void FinishPier()
        {
            if(Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first");
            var scene=SceneManager.GetActiveScene();if(scene.path!=Path) throw new InvalidOperationException("Wrong scene");
            var port=scene.GetRootGameObjects().Single(r=>r.name=="Port_Trzech_Sosen");
            var wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/ArchipelagoKit/Generated/Materials/Wood_Warm.mat");
            var dark=AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/ArchipelagoKit/Generated/Materials/Wood_Shadow.mat");
            if(!wood||!dark) throw new InvalidOperationException("Wood materials missing");
            var connector=port.transform.Find("Land");
            connector.localPosition=new Vector3(-21,.7f,0);connector.localScale=new Vector3(24,.6f,3);
            PrefabUtility.RecordPrefabInstancePropertyModifications(connector);
            foreach(var renderer in port.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.sharedMaterial=renderer.name.StartsWith("MooringPost")?dark:wood;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        [MenuItem("GameWork/Map/Wybrzeze Trzech Sosen/Add player and port")]
        public static void Setup()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first");
            var scene=SceneManager.GetActiveScene();
            if(scene.path!=Path) throw new InvalidOperationException("Open ThreePines scene first");
            if(scene.GetRootGameObjects().Any(r=>r.GetComponentInChildren<CombatShip>()!=null)) throw new InvalidOperationException("Player already exists; preserve existing setup");
            var terrain=scene.GetRootGameObjects().Select(r=>r.GetComponent<Terrain>()).First(t=>t!=null);
            var camera=scene.GetRootGameObjects().Select(r=>r.GetComponent<Camera>()).First(c=>c!=null);
            var source=EditorSceneManager.OpenScene("Assets/ThePirate/Scenes/ShipSandbox.unity",OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                var sourceShip=source.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<CombatShip>()).Single(s=>s.Team==1);
                var player=UnityEngine.Object.Instantiate(sourceShip.gameObject);
                player.name="PlayerShip"; SceneManager.MoveGameObjectToScene(player,scene);
                Undo.RegisterCreatedObjectUndo(player,"Add ThreePines player");
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThePirate/Resources/GameWork/PortDock.prefab");
                if(!prefab) throw new InvalidOperationException("PortDock prefab missing");
                var port=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene); port.name="Port_Trzech_Sosen";
                Undo.RegisterCreatedObjectUndo(port,"Add ThreePines port");
                float x=(440-40)/1455f*3000-1500;
                float shore=0;
                // Search the bay's north shore at the illustrated village longitude.
                for(float z=500;z>250;z-=.5f)
                    if(terrain.SampleHeight(new Vector3(x,0,z))+terrain.transform.position.y<0){shore=z;break;}
                if(shore==0) throw new InvalidOperationException("Bay shoreline not found");
                port.transform.SetPositionAndRotation(new Vector3(x,0,shore-24),Quaternion.Euler(0,90,0));
                var dock=port.GetComponent<PortDock>(); dock.Configure("three-pines-northwest-port",dock.Berth);
                var dockSO=new SerializedObject(dock);dockSO.FindProperty("displayName").stringValue="Port Trzech Sosen";dockSO.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(dock);
                PrefabUtility.RecordPrefabInstancePropertyModifications(port.transform);
                // Berth and hull remain over water; template Land reaches the beach.
                player.transform.SetPositionAndRotation(dock.Berth.position+dock.Berth.forward*8,dock.Berth.rotation);
                player.transform.position=new Vector3(player.transform.position.x,sourceShip.transform.position.y,player.transform.position.z);
                var marker=scene.GetRootGameObjects().FirstOrDefault(r=>r.name=="Punkt_startowy_statku");
                if(marker) marker.transform.SetPositionAndRotation(player.transform.position,player.transform.rotation);
                var sourceFollow=source.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<TopDownCamera>()).First();
                var follow=camera.gameObject.AddComponent<TopDownCamera>(); EditorUtility.CopySerialized(sourceFollow,follow);
                var followSO=new SerializedObject(follow);followSO.FindProperty("target").objectReferenceValue=player.transform;
                followSO.FindProperty("offset").vector3Value=new Vector3(0,50,-35);
                followSO.FindProperty("maxPerspectiveDistance").floatValue=120;followSO.ApplyModifiedPropertiesWithoutUndo();
                camera.orthographic=false;camera.fieldOfView=60;camera.nearClipPlane=.1f;camera.farClipPlane=5000;
                camera.transform.position=player.transform.position+new Vector3(0,50,-35);camera.transform.LookAt(player.transform);
                var combat=player.GetComponent<ShipCombatController>();var combatSO=new SerializedObject(combat);
                combatSO.FindProperty("aimCamera").objectReferenceValue=camera;combatSO.FindProperty("waterHeight").floatValue=0;combatSO.ApplyModifiedPropertiesWithoutUndo();
                var controller=player.GetComponent<PlayerHudController>()??player.AddComponent<PlayerHudController>();
                var hudPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThePirate/Resources/GameWork/PlayerHud.prefab");
                if(!hudPrefab) throw new InvalidOperationException("HUD prefab missing");
                var hud=(GameObject)PrefabUtility.InstantiatePrefab(hudPrefab,scene);hud.name="PlayerHUD";
                hud.GetComponent<PlayerHudView>().source=controller;
                PrefabUtility.RecordPrefabInstancePropertyModifications(hud.GetComponent<PlayerHudView>());
                var hull=player.GetComponent<BoxCollider>();Physics.SyncTransforms();
                Vector3 center=player.transform.TransformPoint(hull.center);
                var blockers=Physics.OverlapBox(center,Vector3.Scale(hull.size*.49f,player.transform.lossyScale),player.transform.rotation,~0,QueryTriggerInteraction.Ignore)
                    .Where(c=>c.gameObject.scene==scene&&!c.transform.IsChildOf(player.transform)).ToArray();
                if(blockers.Length!=0) throw new InvalidOperationException("Spawn blocked: "+string.Join(",",blockers.Select(c=>c.name)));
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
                File.WriteAllText("Temp/ThreePinesGameplay.result","PASS: player cloned from ShipSandbox; camera/HUD bound; port="+dock.Id+"; spawn="+player.transform.position+"; shore="+shore+"; spawn blockers=0");
            }
            finally {EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(scene);}
        }
        [MenuItem("GameWork/Map/Wybrzeze Trzech Sosen/Check docking in Play Mode")]
        public static void CheckDocking()
        {
            if(!Application.isPlaying||SceneManager.GetActiveScene().path!=Path) throw new InvalidOperationException("Enter ThreePines Play Mode first");
            var player=CombatShip.ActiveShips.Single(s=>s.Team==1&&s.gameObject.scene.path==Path);
            var docking=player.GetComponent<ShipDocking>();
            var port=PortDock.Find(player.gameObject.scene,"three-pines-northwest-port");
            if(!docking.CanDock(port,out string reason)) throw new InvalidOperationException("Cannot dock: "+reason);
            if(!docking.RestoreDock(port)||!docking.IsDocked||player.WeaponsAllowed) throw new InvalidOperationException("Dock state failed");
            if(!player.GetComponent<PortRepairService>().Available(out reason)) throw new InvalidOperationException("Port unavailable: "+reason);
            docking.Undock();
            if(docking.IsDocked||!player.WeaponsAllowed) throw new InvalidOperationException("Undock failed");
            var hud=UnityEngine.Object.FindAnyObjectByType<PlayerHudView>();
            if(!hud||hud.source!=player.GetComponent<PlayerHudController>()) throw new InvalidOperationException("HUD binding failed");
            File.WriteAllText("Temp/ThreePinesGameplayPlay.result","PASS Play Mode: clear approach, docking, repair service available, undocking, weapons restored, HUD bound; methods invoked directly, physical E input not tested");
        }
    }
}
#endif
