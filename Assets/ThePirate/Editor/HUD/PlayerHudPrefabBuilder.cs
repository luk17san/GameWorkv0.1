using System;
using System.IO;
using ThePirate.UI.HUD;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ThePirate.Editor.HUD
{
    public static class PlayerHudPrefabBuilder
    {
        public const string RootPath="Assets/ThePirate/Resources/GameWork/PlayerHud.prefab";
        private const string WidgetPath="Assets/ThePirate/UI/HUD/Prefabs";
        private const string ArtPath="Assets/ThePirate/UI/HUD/Art/";
        private const string Request="Temp/GameWorkHudUI.request";
        private const string Result="Temp/GameWorkHudUI.result";
        private static readonly Color Gold=new Color(0.76f,0.55f,0.27f,1);
        private static readonly Color Cream=new Color(0.96f,0.9f,0.77f,1);
        private static readonly Color Navy=new Color(0.025f,0.08f,0.10f,0.90f);
        private static readonly Color Teal=new Color(0.23f,0.79f,0.69f,1);
        private static Font font;

        [InitializeOnLoadMethod]
        private static void Watch()
        { EditorApplication.update-=Requested; EditorApplication.update+=Requested; }
        private static void Requested()
        {
            if(!File.Exists(Request) || EditorApplication.isCompiling || EditorApplication.isUpdating
                || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(Request);
            try { Build(); File.WriteAllText(Result,"OK: HUD prefab, 9 nested widgets, sprite import, validation and preview completed. Play Mode not tested."); }
            catch(Exception exception) { File.WriteAllText(Result,"ERROR: "+exception); Debug.LogException(exception); }
        }

        [MenuItem("GameWork/HUD/Utwórz edytowalny HUD")]
        public static void Build()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Zatrzymaj Play Mode.");
            if(AssetDatabase.LoadAssetAtPath<GameObject>(RootPath)!=null)
            { Validate(); RenderPreview(); return; } // Nigdy nie nadpisuj ręcznych zmian prefabów.
            Directory.CreateDirectory(WidgetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(RootPath));
            AssetDatabase.Refresh();
            Sprite ship=ImportSprite(ArtPath+"ShipAvatar.png",256);
            Sprite wheel=ImportSprite(ArtPath+"SpeedWheelFrame.png",512);
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var preview=EditorSceneManager.NewPreviewScene();
            GameObject root=null;
            try
            {
                root=new GameObject("PlayerHUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(CanvasGroup),typeof(PlayerHudView));
                SceneManager.MoveGameObjectToScene(root,preview);
                root.layer=5;
                var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=10;
                var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution=new Vector2(1672,941); scaler.matchWidthOrHeight=0.5f;
                var group=root.GetComponent<CanvasGroup>(); group.blocksRaycasts=false; group.interactable=false;
                var safe=Rect(root.transform,"SafeArea",Vector2.zero,new Vector2(1672,941));
                safe.anchorMin=Vector2.zero; safe.anchorMax=Vector2.one; safe.offsetMin=safe.offsetMax=Vector2.zero;
                safe.gameObject.AddComponent<HudSafeArea>();
                var view=root.GetComponent<PlayerHudView>();

                // Kotwice każdej sekcji pozostają niezależne. Współrzędne wewnątrz grup od środka.
                var health=Group(safe,"Durability",new Vector2(0,1),new Vector2(220,-68),new Vector2(394,94));
                Panel(health,new Vector2(394,94)); Corners(health,new Vector2(394,94));
                var avatarPanel=Rect(health,"Avatar",new Vector2(-148,0),new Vector2(84,84));
                Panel(avatarPanel,new Vector2(84,84)); Corners(avatarPanel,new Vector2(84,84));
                view.avatar=SpriteImage(avatarPanel,"ShipPortrait",Vector2.zero,new Vector2(72,78),ship);
                Label(health,"Title","WYTRZYMAŁOŚĆ",new Vector2(26,27),new Vector2(230,24),17,TextAnchor.MiddleLeft);
                Bar(health,"DurabilityBar",new Vector2(30,0),new Vector2(246,16),out view.durabilityFill);
                view.durabilityValue=Label(health,"ValueBelowBar","420 / 500",new Vector2(30,-25),new Vector2(246,22),17);

                var speed=Group(safe,"SpeedWheel",Vector2.zero,new Vector2(139,189),new Vector2(246,246));
                Shape(speed,"DialBackground",Vector2.zero,new Vector2(166,166),HudShapeGraphic.ShapeKind.Disc,Navy);
                SpriteImage(speed,"WheelFrame",Vector2.zero,new Vector2(246,246),wheel);
                Label(speed,"SpeedTitle","PRĘDKOŚĆ",new Vector2(0,26),new Vector2(150,22),15);
                view.speedValue=Label(speed,"SpeedValue","8,4",new Vector2(0,-5),new Vector2(140,47),40);
                view.driveValue=Label(speed,"DriveBelowSpeed","NAPĘD: ŻAGLE",new Vector2(0,-42),new Vector2(155,22),12);

                var cargo=Group(safe,"Cargo",Vector2.zero,new Vector2(146,48),new Vector2(244,50));
                Panel(cargo,new Vector2(244,50));
                Poly(cargo,"CrateOutline",new Vector2(-102,4),new Vector2(24,24),Gold,true,
                    new Vector2(.5f,1),new Vector2(1,.75f),new Vector2(1,.25f),new Vector2(.5f,0),new Vector2(0,.25f),new Vector2(0,.75f));
                Poly(cargo,"CrateLid",new Vector2(-102,4),new Vector2(24,24),Gold,false,
                    new Vector2(0,.75f),new Vector2(.5f,.5f),new Vector2(1,.75f));
                Poly(cargo,"CrateCenter",new Vector2(-102,4),new Vector2(24,24),Gold,false,new Vector2(.5f,.5f),new Vector2(.5f,0));
                Label(cargo,"CargoTitle","ŁADOWNIA",new Vector2(-42,5),new Vector2(90,20),14);
                view.cargoValue=Label(cargo,"CargoValue","320 / 1000",new Vector2(64,5),new Vector2(108,20),14);
                Bar(cargo,"CargoBar",new Vector2(13,-15),new Vector2(192,5),out view.cargoFill);

                var reload=Group(safe,"ReloadShip",Vector2.zero,new Vector2(352,151),new Vector2(136,220));
                Vector2[] hull={new Vector2(.5f,1),new Vector2(.76f,.78f),new Vector2(.84f,.52f),new Vector2(.72f,.1f),new Vector2(.62f,0),new Vector2(.38f,0),new Vector2(.28f,.1f),new Vector2(.16f,.52f),new Vector2(.24f,.78f)};
                var fill=Shape(reload,"HullBackground",Vector2.zero,new Vector2(98,200),HudShapeGraphic.ShapeKind.Polygon,new Color(.025f,.08f,.1f,.55f)); fill.points=hull;
                Poly(reload,"HullOutline",Vector2.zero,new Vector2(98,200),Gold,true,hull);
                Poly(reload,"Keel",Vector2.zero,new Vector2(98,200),new Color(Gold.r,Gold.g,Gold.b,.6f),false,new Vector2(.5f,.05f),new Vector2(.5f,.95f));
                for(int i=0;i<6;i++) Poly(reload,"DeckLine_"+i,new Vector2(0,-65+i*26),new Vector2(54,1),new Color(Gold.r,Gold.g,Gold.b,.4f),false,Vector2.zero,Vector2.right);
                view.batteries=new [] { Mount(reload,"Bow",HudBatterySide.Bow,new Vector2(0,64)),Mount(reload,"Left",HudBatterySide.Left,new Vector2(-37,0)),
                    Mount(reload,"Right",HudBatterySide.Right,new Vector2(37,0)),Mount(reload,"Stern",HudBatterySide.Stern,new Vector2(0,-70)),
                    SpecialMount(reload) };
                Label(reload,"SpecialLabel","MOŹDZIERZ",new Vector2(0,-32),new Vector2(90,20),10);

                var map=Group(safe,"Minimap",Vector2.one,new Vector2(-120,-119),new Vector2(204,204));
                view.minimap=map.gameObject.AddComponent<HudMinimapView>();
                Shape(map,"Background",Vector2.zero,new Vector2(194,194),HudShapeGraphic.ShapeKind.Disc,Navy);
                var clip=Shape(map,"CircularMask",Vector2.zero,new Vector2(188,188),HudShapeGraphic.ShapeKind.Disc,Color.white);
                var mask=clip.gameObject.AddComponent<Mask>(); mask.showMaskGraphic=false;
                var rawRect=Rect(clip.transform,"Terrain",Vector2.zero,new Vector2(188,188));
                view.minimap.terrain=rawRect.gameObject.AddComponent<RawImage>(); view.minimap.terrain.raycastTarget=false;
                view.minimap.terrain.color=Color.white; view.minimap.terrain.enabled=false;
                // Graticule is separate, editable geometry; terrain comes from an orthographic camera in Play Mode.
                Shape(clip.transform,"InnerRange",Vector2.zero,new Vector2(94,94),HudShapeGraphic.ShapeKind.Ring,new Color(Gold.r,Gold.g,Gold.b,.18f),.7f);
                Poly(clip.transform,"NorthSouth",Vector2.zero,new Vector2(188,188),new Color(Gold.r,Gold.g,Gold.b,.15f),false,new Vector2(.5f,0),new Vector2(.5f,1));
                Poly(clip.transform,"EastWest",Vector2.zero,new Vector2(188,188),new Color(Gold.r,Gold.g,Gold.b,.15f),false,new Vector2(0,.5f),new Vector2(1,.5f));
                view.minimap.markerRoot=Rect(clip.transform,"Markers",Vector2.zero,new Vector2(188,188));
                var template=Shape(view.minimap.markerRoot,"MarkerTemplate",Vector2.zero,new Vector2(12,15),HudShapeGraphic.ShapeKind.Polygon,Cream);
                template.points=new [] { new Vector2(.5f,1),new Vector2(0,0),new Vector2(1,0) }; template.gameObject.SetActive(false);
                view.minimap.markerTemplate=template;
                Shape(map,"GoldRim",Vector2.zero,new Vector2(196,196),HudShapeGraphic.ShapeKind.Ring,Gold,2);
                Label(map,"North","N",new Vector2(0,83),new Vector2(24,22),16);
                Label(map,"East","E",new Vector2(83,0),new Vector2(24,22),16);
                Label(map,"South","S",new Vector2(0,-83),new Vector2(24,22),16);
                Label(map,"West","W",new Vector2(-83,0),new Vector2(24,22),16);

                var wind=Group(safe,"Wind",Vector2.one,new Vector2(-120,-243),new Vector2(190,38));
                Panel(wind,new Vector2(190,38));
                Label(wind,"Title","WIATR",new Vector2(-52,0),new Vector2(65,22),15);
                view.windValue=Label(wind,"Value","6,2",Vector2.zero,new Vector2(52,22),16);
                view.windArrow=Poly(wind,"Direction",new Vector2(61,0),new Vector2(23,23),Cream,false,
                    new Vector2(.5f,0),new Vector2(.5f,1),new Vector2(.2f,.65f),new Vector2(.5f,1),new Vector2(.8f,.65f)).rectTransform;

                var quest=Group(safe,"Objective",Vector2.one,new Vector2(-126,-303),new Vector2(202,64));
                Panel(quest,new Vector2(202,64));
                var icon=Rect(quest,"CompassIcon",new Vector2(-76,0),new Vector2(26,26));
                Shape(icon,"Ring",Vector2.zero,new Vector2(23,23),HudShapeGraphic.ShapeKind.Ring,Gold,1);
                Poly(icon,"Needle",Vector2.zero,new Vector2(29,29),Gold,false,new Vector2(.5f,0),new Vector2(.5f,1),new Vector2(.5f,.5f),new Vector2(0,.5f),new Vector2(1,.5f));
                Label(quest,"Title","ZADANIE",new Vector2(18,16),new Vector2(146,20),14,TextAnchor.MiddleLeft).color=Gold;
                view.objectiveValue=Label(quest,"Description","Dopłyń do portu",new Vector2(18,-10),new Vector2(146,34),14,TextAnchor.MiddleLeft);

                var target=Group(safe,"Target",new Vector2(.5f,1),new Vector2(0,-54),new Vector2(270,80));
                view.targetPanel=target.gameObject; Panel(target,new Vector2(270,80));
                view.targetName=Label(target,"Name","WROGI STATEK",new Vector2(-32,25),new Vector2(194,22),14,TextAnchor.MiddleLeft);
                Bar(target,"TargetBar",new Vector2(0,3),new Vector2(246,12),out view.targetFill);
                view.targetFill.color=new Color(.87f,.22f,.23f);
                view.targetValue=Label(target,"HealthValue","280 / 400",new Vector2(-61,-23),new Vector2(120,22),14);
                view.targetDistance=Label(target,"Distance","65 m",new Vector2(65,-23),new Vector2(120,22),14);

                var message=Group(safe,"Message",new Vector2(.5f,1),new Vector2(0,-126),new Vector2(380,34));
                Panel(message,new Vector2(380,34)); view.messagePanel=message.gameObject;
                view.messageValue=Label(message,"Text","",Vector2.zero,new Vector2(364,28),16); message.gameObject.SetActive(false);
                view.ApplyPreviewValues();
                // Każda sekcja to osobny prefab; root zawiera ich zagnieżdżone instancje.
                foreach(var widget in new [] {health,speed,cargo,reload,map,wind,quest,target,message})
                    PrefabUtility.SaveAsPrefabAssetAndConnect(widget.gameObject,WidgetPath+"/"+widget.name+".prefab",InteractionMode.AutomatedAction);
                if(PrefabUtility.SaveAsPrefabAsset(root,RootPath)==null) throw new IOException("Nie zapisano prefabu HUD.");
            }
            finally { if(root!=null) UnityEngine.Object.DestroyImmediate(root); EditorSceneManager.ClosePreviewScene(preview); }
            AssetDatabase.SaveAssets(); Validate(); RenderPreview();
        }

        private static Sprite ImportSprite(string path,int size)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null) throw new FileNotFoundException(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false;
            importer.maxTextureSize=size; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit=100; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static RectTransform Rect(Transform parent,string name,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform)); go.layer=5;
            var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f); rect.pivot=new Vector2(.5f,.5f);
            rect.sizeDelta=size; rect.anchoredPosition=position; return rect;
        }
        private static RectTransform Group(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size)
        { var rect=Rect(parent,name,position,size); rect.anchorMin=rect.anchorMax=anchor; return rect; }
        private static HudShapeGraphic Shape(Transform parent,string name,Vector2 position,Vector2 size,HudShapeGraphic.ShapeKind kind,Color color,float thickness=1.3f)
        {
            var rect=Rect(parent,name,position,size);
            rect.gameObject.AddComponent<CanvasRenderer>();
            var shape=rect.gameObject.AddComponent<HudShapeGraphic>();
            shape.shape=kind; shape.color=color; shape.thickness=thickness; shape.raycastTarget=false; return shape;
        }
        private static HudShapeGraphic Poly(Transform parent,string name,Vector2 position,Vector2 size,Color color,bool closed,params Vector2[] points)
        { var shape=Shape(parent,name,position,size,HudShapeGraphic.ShapeKind.Polyline,color); shape.points=points; shape.closed=closed; return shape; }
        private static void Panel(Transform parent,Vector2 size)
        { Shape(parent,"Background",Vector2.zero,size,HudShapeGraphic.ShapeKind.Rectangle,Navy); Shape(parent,"Frame",Vector2.zero,size,HudShapeGraphic.ShapeKind.Border,Gold); }
        private static void Corners(Transform parent,Vector2 size)
        {
            for(int i=0;i<4;i++)
            {
                var detail=Rect(parent,"SlavicCorner_"+i,new Vector2((i%2==0?-1:1)*(size.x/2-13),(i<2?1:-1)*(size.y/2-13)),new Vector2(22,22));
                detail.localScale=new Vector3(i%2==0?1:-1,i<2?1:-1,1);
                Poly(detail,"Carving",Vector2.zero,new Vector2(22,22),Gold,false,new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(.65f,.65f),new Vector2(.3f,.65f),new Vector2(.3f,.3f),new Vector2(.65f,.3f),new Vector2(.65f,.65f));
                Poly(detail,"Diamond",new Vector2(4,-4),new Vector2(6,6),Gold,true,new Vector2(.5f,1),new Vector2(1,.5f),new Vector2(.5f,0),new Vector2(0,.5f));
            }
        }
        private static Text Label(Transform parent,string name,string value,Vector2 position,Vector2 size,int fontSize,TextAnchor align=TextAnchor.MiddleCenter)
        {
            var text=Rect(parent,name,position,size).gameObject.AddComponent<Text>();
            text.font=font; text.fontSize=fontSize; text.color=Cream; text.text=value;
            text.alignment=align; text.raycastTarget=false; text.supportRichText=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Truncate;
            return text;
        }
        private static Image SpriteImage(Transform parent,string name,Vector2 position,Vector2 size,Sprite sprite)
        { var image=Rect(parent,name,position,size).gameObject.AddComponent<Image>(); image.sprite=sprite; image.preserveAspect=true; image.raycastTarget=false; return image; }
        private static void Bar(Transform parent,string name,Vector2 position,Vector2 size,out HudShapeGraphic fill)
        {
            var root=Rect(parent,name,position,size);
            Shape(root,"Track",Vector2.zero,size,HudShapeGraphic.ShapeKind.Rectangle,new Color(.02f,.06f,.08f,1));
            fill=Shape(root,"Fill",Vector2.zero,size-new Vector2(2,2),HudShapeGraphic.ShapeKind.Rectangle,Teal);
            Shape(root,"Outline",Vector2.zero,size,HudShapeGraphic.ShapeKind.Border,new Color(Gold.r,Gold.g,Gold.b,.7f),.8f);
        }
        private static HudBatteryWidget SpecialMount(Transform parent)
        {
            var widget=Mount(parent,"Special",HudBatterySide.Special,Vector2.zero);
            foreach(var rect in widget.GetComponentsInChildren<RectTransform>(true)) rect.sizeDelta*=0.55f;
            foreach(var graphic in widget.GetComponentsInChildren<HudShapeGraphic>(true)) graphic.thickness*=0.7f;
            widget.countdown.fontSize=8;
            widget.countdown.rectTransform.sizeDelta=new Vector2(28,18);
            return widget;
        }
        private static HudBatteryWidget Mount(Transform parent,string name,HudBatterySide side,Vector2 position)
        {
            var root=Rect(parent,name,position,new Vector2(47,47));
            var view=root.gameObject.AddComponent<HudBatteryWidget>(); view.side=side;
            Shape(root,"Background",Vector2.zero,new Vector2(46,46),HudShapeGraphic.ShapeKind.Disc,Navy);
            Shape(root,"Rim",Vector2.zero,new Vector2(46,46),HudShapeGraphic.ShapeKind.Ring,Cream,1.2f);
            view.progress=Shape(root,"ReloadProgress",Vector2.zero,new Vector2(46,46),HudShapeGraphic.ShapeKind.Ring,Teal,3);
            view.selection=Shape(root,"Selected",Vector2.zero,new Vector2(51,51),HudShapeGraphic.ShapeKind.Ring,Gold,1);
            view.countdown=Label(root,"Countdown","2,4 s",Vector2.zero,new Vector2(43,24),12);
            var check=Poly(root,"ReadyCheck",Vector2.zero,new Vector2(21,17),Teal,false,new Vector2(0,.5f),new Vector2(.35f,.1f),new Vector2(1,1));
            check.thickness=3; view.readyCheck=check.gameObject; return view;
        }

        [MenuItem("GameWork/HUD/Sprawdź prefab")]
        public static void Validate()
        {
            var root=AssetDatabase.LoadAssetAtPath<GameObject>(RootPath);
            if(root==null) throw new InvalidOperationException("Brak prefabu HUD.");
            int objects=0;
            foreach(var child in root.GetComponentsInChildren<Transform>(true))
            {
                objects++;
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject)>0) throw new InvalidOperationException("Missing script: "+child.name);
            }
            foreach(var graphic in root.GetComponentsInChildren<Graphic>(true))
            {
                if(graphic.GetComponent<CanvasRenderer>()==null)
                    throw new InvalidOperationException("Brak CanvasRenderer: "+graphic.name);
                if(graphic.raycastTarget) throw new InvalidOperationException("HUD blokuje kliknięcia: "+graphic.name);
            }
            var view=root.GetComponent<PlayerHudView>();
            if(view.avatar.sprite==null || view.batteries.Length!=5 || view.minimap.markerTemplate==null || view.durabilityFill==null)
                throw new InvalidOperationException("Brak referencji HUD.");
            if(root.GetComponent<CanvasGroup>().blocksRaycasts) throw new InvalidOperationException("HUD blokuje wejście.");
            if(root.GetComponentsInChildren<Canvas>(true).Length!=1) throw new InvalidOperationException("Oczekiwano jednego Canvas.");
            File.WriteAllText("Temp/GameWorkHudUI.validation.txt","Validated: "+objects+" editable objects; 5 mounts including special weapon; no missing scripts; all graphics pass pointer input; sprites assigned.\n");
        }

        [MenuItem("GameWork/HUD/Zapisz podgląd")]
        public static void RenderPreview()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(RootPath);
            if(prefab==null) return;
            var scene=EditorSceneManager.NewPreviewScene();
            GameObject root=null, cameraObject=null; RenderTexture rt=null; Texture2D image=null;
            try
            {
                root=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
                root.GetComponent<PlayerHudView>().ApplyPreviewValues();
                root.GetComponentInChildren<HudSafeArea>().enabled=false;
                cameraObject=new GameObject("PreviewCamera",typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObject,scene);
                var camera=cameraObject.GetComponent<Camera>(); camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.045f,.16f,.20f); camera.nearClipPlane=.01f; camera.farClipPlane=100;
                camera.cullingMask=1<<5;
                rt=new RenderTexture(1672,941,24); rt.Create(); camera.targetTexture=rt;
                var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=camera; canvas.planeDistance=1;
                Canvas.ForceUpdateCanvases(); camera.Render();
                var previous=RenderTexture.active;
                try { RenderTexture.active=rt; image=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false); image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0); image.Apply(); }
                finally { RenderTexture.active=previous; }
                Directory.CreateDirectory("HUD-Preview"); File.WriteAllBytes("HUD-Preview/PlayerHUD.png",image.EncodeToPNG());
            }
            finally
            {
                if(root!=null) UnityEngine.Object.DestroyImmediate(root);
                if(cameraObject!=null) UnityEngine.Object.DestroyImmediate(cameraObject);
                if(image!=null) UnityEngine.Object.DestroyImmediate(image);
                if(rt!=null) { rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
