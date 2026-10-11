using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ThePirate.EditorTools.PortVisualKit
{
    // Osobny zestaw wizualny. Nie modyfikuje PortMenu.prefab ani scen gry.
    public static class PortVisualKitBuilder
    {
        private const string Kit = "Assets/ThePirate/UI/PortVisualKit";
        private const string Art = Kit + "/Sprites/";
        private static readonly Color Cream = new Color(0.96f, 0.89f, 0.72f);
        private static readonly Color Ink = new Color(0.13f, 0.09f, 0.05f);
        private static readonly Color Gold = new Color(0.96f, 0.67f, 0.22f);
        private static readonly string[] IconNames = {
            "Ship", "Anchor", "Tools", "Scroll", "Trade", "Cargo", "Tavern", "Gold",
            "Wheel", "Crew", "Wood", "Arrow"
        };
        private static readonly string[] ArtNames = {
            "harbor-background", "outer-frame", "button-navy", "button-red",
            "parchment-panel", "icons-atlas", "ship-portrait"
        };
        private sealed class Skin
        {
            public Sprite background, frame, navy, red, parchment, portrait;
            public readonly Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();
            public Font font;
        }

        [MenuItem("Tools/GameWork/Ports/Visual kit/Create editable port objects")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogWarning("Wyjdź z Play Mode przed generowaniem zestawu UI."); return; }
            foreach (var name in ArtNames)
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(Art + name + ".png") == null)
                { Debug.LogError("Brak grafiki: " + Art + name + ".png. Skopiuj cały folder Assets z zestawu."); return; }

            var skin = LoadSkin();
            EnsureFolder(Kit + "/Generated");
            var folder = Kit + "/Generated/Build-01";
            for (int index = 2; AssetDatabase.IsValidFolder(folder); index++)
                folder = Kit + "/Generated/Build-" + index.ToString("00");
            EnsureFolder(folder);

            var root = new GameObject("PortVisualCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            try
            {
                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 60;
                var scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

                var window = Rect("Window", root.transform, 0, 0, 1920, 1080);
                window.anchorMin = window.anchorMax = new Vector2(0.5f, 0.5f);
                window.pivot = new Vector2(0.5f, 0.5f);
                window.anchoredPosition = Vector2.zero;
                Picture("HarborBackground", window, skin.background, 0, 0, 1920, 1080);
                var shade = Picture("BackgroundShade", window, null, 0, 0, 1920, 1080);
                shade.color = new Color(0, 0, 0, 0.14f);
                // Rama jest pod treścią, żeby górna drewniana belka nie zasłaniała tytułu.
                Picture("OuterFrame", window, skin.frame, 0, 0, 1920, 1080);

                var header = Rect("Header", window, 50, 22, 1820, 205);
                Picture("TitlePlate", header, skin.navy, 530, 0, 760, 100, true);
                Label("PanelTitle", header, skin, "PORT", 530, 12, 760, 28, 24, Cream);
                Label("PortName", header, skin, "ZŁOTA ZATOKA", 550, 42, 720, 45, 37, Cream);
                var gold = Status(header, skin, "GoldStatus", "Gold", "Złoto", "2450", 0);
                Status(header, skin, "ShipStatus", "Ship", "Statek", "Czarna Perła", 455);
                var hullStatus = Status(header, skin, "HullStatus", "Wheel", "Kadłub", "76%", 910);
                Progress("Progress", hullStatus, skin, 83, 61, 240, 13, 0.76f, new Color(0.35f, 0.72f, 0.26f));
                var cargoStatus = Status(header, skin, "CargoStatus", "Cargo", "Ładownia", "68 / 100", 1365);
                Progress("Progress", cargoStatus, skin, 83, 61, 240, 13, 0.68f, Gold);

                var nav = Rect("Navigation", window, 48, 241, 320, 694);
                Picture("Panel", nav, skin.navy, 0, 0, 320, 694, true);
                var captions = new[] { "PORT", "EKWIPUNEK", "WARSZTAT", "STOCZNIA", "FLOTA", "ZADANIA", "HANDEL", "MAGAZYN", "TAWERNA" };
                var keys = new[] { "Port", "Equipment", "Workshop", "Shipyard", "Fleet", "Quests", "Trade", "Warehouse", "Tavern" };
                var glyphs = new[] { "Ship", "Cargo", "Tools", "Ship", "Ship", "Scroll", "Trade", "Cargo", "Tavern" };
                Button selected = null, normal = null;
                for (int index = 0; index < captions.Length; index++)
                {
                    var button = Action("Nav" + keys[index], nav, skin, captions[index], glyphs[index], 8, 8 + index * 75, 304, 69, index == 0, 21);
                    if (index == 0) selected = button;
                    if (index == 1) normal = button;
                }

                var scene = Rect("SceneView", window, 385, 241, 952, 694);
                // Przezroczysty środek zachowuje jedno wspólne tło, bez powielania obrazów portu.
                var badge = Rect("DockedBadge", scene, 312, 610, 330, 60);
                Picture("Background", badge, skin.navy, 0, 0, 330, 60, true);
                Icon("Icon", badge, skin, "Anchor", 18, 12, 36, 36);
                Label("Label", badge, skin, "ZACUMOWANO", 59, 5, 255, 50, 24, Cream);

                var summary = Rect("ShipSummary", window, 1355, 241, 515, 694);
                Picture("Parchment", summary, skin.parchment, 0, 0, 515, 694, true);
                var portrait = Picture("ShipPortrait", summary, skin.portrait, 56, 38, 403, 233);
                portrait.preserveAspect = true;
                Label("ShipName", summary, skin, "Czarna Perła", 38, 278, 439, 48, 31, Ink);
                var divider = Picture("Divider", summary, null, 48, 333, 419, 2);
                divider.color = new Color(0.4f, 0.27f, 0.11f, 0.65f);
                var hullRow = Stat(summary, skin, "Hull", "Wheel", "Kadłub", "76%", 357, 0.76f, new Color(0.35f, 0.72f, 0.26f));
                Stat(summary, skin, "Crew", "Crew", "Załoga", "24 / 30", 407, null, Color.white);
                Stat(summary, skin, "Cargo", "Cargo", "Ładownia", "68 / 100", 457, 0.68f, Gold);
                var description = Label("Description", summary, skin,
                    "Twój statek czeka przy nabrzeżu. Sprawdź wyposażenie i uzupełnij zapasy przed wyprawą.",
                    48, 516, 419, 82, 20, Ink);
                description.alignment = TextAnchor.UpperLeft;
                var repair = Action("RepairButton", summary, skin, "NAPRAW STATEK", "Tools", 40, 611, 435, 59, false, 23);

                var footer = Rect("Footer", window, 48, 951, 1822, 91);
                Picture("Background", footer, skin.navy, 0, 0, 1822, 91, true);
                var quest = Rect("ActiveQuest", footer, 8, 5, 1310, 81);
                Icon("QuestIcon", quest, skin, "Scroll", 10, 18, 44, 44);
                var questTitle = Label("Title", quest, skin, "AKTYWNE ZADANIE", 68, 8, 400, 23, 17, Cream);
                questTitle.alignment = TextAnchor.MiddleLeft;
                var questName = Label("QuestName", quest, skin, "DOSTARCZ DREWNO", 68, 32, 410, 35, 25, Cream);
                questName.alignment = TextAnchor.MiddleLeft;
                Icon("ResourceIcon", quest, skin, "Wood", 478, 10, 60, 60);
                var questProgress = Progress("Progress", quest, skin, 558, 16, 350, 20, 0.6f, Gold);
                Label("ProgressValue", quest, skin, "12 / 20", 916, 8, 113, 33, 21, Cream);
                var questDescription = Label("Description", quest, skin, "Dostarcz deski do stoczni w Złotej Zatoce.", 558, 44, 471, 24, 17, Cream);
                questDescription.alignment = TextAnchor.MiddleLeft;
                Action("QuestsButton", quest, skin, "ZADANIA", "Scroll", 1050, 7, 242, 66, false, 19);
                var depart = Action("DepartButton", footer, skin, "WYPŁYŃ", "Ship", 1330, 7, 482, 77, true, 32);

                SavePart(folder, "NavButton", normal.gameObject);
                SavePart(folder, "NavButtonSelected", selected.gameObject);
                SavePart(folder, "PrimaryButton", depart.gameObject);
                SavePart(folder, "StatusCell", gold.gameObject);
                SavePart(folder, "StatRow", hullRow.gameObject);
                SavePart(folder, "ProgressBar", questProgress.gameObject);
                SavePart(folder, "ShipSummary", summary.gameObject);
                SavePart(folder, "ActiveQuest", quest.gameObject);
                SavePart(folder, "Header", header.gameObject);
                SavePart(folder, "Navigation", nav.gameObject);
                SavePart(folder, "DockedBadge", badge.gameObject);
                SavePart(folder, "RepairButton", repair.gameObject);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, folder + "/PortVisualCanvas.prefab");
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log("Utworzono 12 elementów UI i PortVisualCanvas w " + folder +
                    ". Napisy i paski są edytowalne. Przyciski nie mają połączenia z usługami gry.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static Skin LoadSkin()
        {
            foreach (var name in ArtNames) ImportSprite(name, name == "icons-atlas");
            var skin = new Skin {
                background = Sprite("harbor-background"), frame = Sprite("outer-frame"),
                navy = Sprite("button-navy"), red = Sprite("button-red"),
                parchment = Sprite("parchment-panel"), portrait = Sprite("ship-portrait"),
                font = AssetDatabase.LoadAssetAtPath<Font>(Kit + "/Fonts/PortSerif.ttf")
                    ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
            };
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(Art + "icons-atlas.png"))
                if (asset is Sprite icon) skin.icons[icon.name] = icon;
            foreach (var name in IconNames)
                if (!skin.icons.ContainsKey(name)) throw new InvalidOperationException("Nie zaimportowano ikony " + name);
            return skin;
        }

        private static void ImportSprite(string name, bool atlas)
        {
            var path = Art + name + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Brak TextureImporter: " + path);
            // Wymiary oryginalnego PNG, niezależne od poprzedniego Max Size importera.
            var raw = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            float w, h;
            UnityEngine.Rect bounds;
            UnityEngine.Rect[] glyphBounds = null;
            try
            {
                if (!raw.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("Nieprawidłowy PNG: " + path);
                w = raw.width; h = raw.height;
                bounds = OpaqueBounds(raw);
                if (atlas)
                {
                    // AI nie zachowuje idealnie równych marginesów: granice rzędów w pustych pasach.
                    var rows = new[] { 0, RowBoundary(raw, 1f / 3), RowBoundary(raw, 2f / 3), raw.height };
                    glyphBounds = new UnityEngine.Rect[12];
                    for (int index = 0; index < glyphBounds.Length; index++)
                    {
                        int column = index % 4, row = index / 4;
                        int left = column * raw.width / 4, right = (column + 1) * raw.width / 4;
                        glyphBounds[index] = OpaqueBounds(raw, new RectInt(left, raw.height - rows[row + 1],
                            right - left, rows[row + 1] - rows[row]));
                    }
                }
            }
            finally { Object.DestroyImmediate(raw); }
            bool sliced = name == "button-navy" || name == "button-red" || name == "parchment-panel";
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = atlas || sliced ? SpriteImportMode.Multiple : SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.spritePixelsPerUnit = 100;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.spriteBorder = Vector4.zero;
            if (atlas)
            {
                var cells = new SpriteMetaData[12];
                for (int index = 0; index < cells.Length; index++)
                    cells[index] = new SpriteMetaData {
                        name = IconNames[index], alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f), border = Vector4.zero,
                        rect = glyphBounds[index]
                    };
                // API importu atlasu dostępne także bez instalowania pakietu 2D Sprite.
                #pragma warning disable CS0618
                importer.spritesheet = cells;
                #pragma warning restore CS0618
            }
            else if (sliced)
            {
                #pragma warning disable CS0618
                importer.spritesheet = new[] { new SpriteMetaData {
                    name = name, rect = bounds, alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = new Vector4(bounds.width * 0.12f, bounds.height * 0.20f,
                        bounds.width * 0.12f, bounds.height * 0.20f)
                } };
                #pragma warning restore CS0618
            }
            importer.SaveAndReimport();
        }

        private static UnityEngine.Rect OpaqueBounds(Texture2D texture, RectInt? region = null)
        {
            var pixels = texture.GetPixels32();
            var area = region ?? new RectInt(0, 0, texture.width, texture.height);
            int left = texture.width, bottom = texture.height, right = -1, top = -1;
            for (int y = area.yMin; y < area.yMax; y++)
                for (int x = area.xMin; x < area.xMax; x++)
                    if (pixels[y * texture.width + x].a > 32)
                    { left = Math.Min(left, x); right = Math.Max(right, x); bottom = Math.Min(bottom, y); top = Math.Max(top, y); }
            if (right < left) throw new InvalidOperationException("Grafika jest całkowicie przezroczysta.");
            return new UnityEngine.Rect(left, bottom, right - left + 1, top - bottom + 1);
        }

        private static int RowBoundary(Texture2D texture, float fraction)
        {
            var pixels = texture.GetPixels32();
            int nominal = Mathf.RoundToInt(texture.height * fraction);
            int start = Math.Max(1, nominal - Mathf.RoundToInt(texture.height * 0.12f));
            int end = Math.Min(texture.height - 1, nominal + Mathf.RoundToInt(texture.height * 0.12f));
            int best = nominal, bestCount = int.MaxValue;
            for (int row = start; row <= end; row++)
            {
                int count = 0, offset = (texture.height - 1 - row) * texture.width;
                for (int x = 0; x < texture.width; x++) if (pixels[offset + x].a > 32) count++;
                if (count < bestCount || (count == bestCount && Math.Abs(row - nominal) < Math.Abs(best - nominal)))
                { best = row; bestCount = count; }
            }
            return best;
        }

        private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");
        private static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
            return rect;
        }
        private static Image Picture(string name, Transform parent, Sprite sprite, float x, float y, float w, float h, bool sliced = false)
        {
            var rect = Rect(name, parent, x, y, w, h);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite; image.color = Color.white; image.raycastTarget = false;
            if (sliced) { image.type = Image.Type.Sliced; image.pixelsPerUnitMultiplier = 6; }
            return image;
        }
        private static Text Label(string name, Transform parent, Skin skin, string value, float x, float y, float w, float h, int size, Color color)
        {
            var rect = Rect(name, parent, x, y, w, h);
            var text = rect.gameObject.AddComponent<Text>(); text.font = skin.font;
            text.text = value; text.fontSize = size; text.color = color; text.raycastTarget = false;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
        private static Image Icon(string name, Transform parent, Skin skin, string key, float x, float y, float w, float h)
        {
            var image = Picture(name, parent, skin.icons[key], x, y, w, h); image.preserveAspect = true; return image;
        }
        private static Button Action(string name, Transform parent, Skin skin, string caption, string glyph, float x, float y, float w, float h, bool selected, int fontSize)
        {
            var rect = Rect(name, parent, x, y, w, h);
            var background = rect.gameObject.AddComponent<Image>(); background.sprite = selected ? skin.red : skin.navy;
            background.type = Image.Type.Sliced; background.pixelsPerUnitMultiplier = 6; background.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = background;
            var colors = button.colors; colors.highlightedColor = new Color(1, 0.95f, 0.8f);
            colors.pressedColor = new Color(0.75f, 0.65f, 0.52f); colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            button.colors = colors;
            float iconSize = Mathf.Min(44, h - 22);
            Icon("Icon", rect, skin, glyph, 22, (h - iconSize) / 2, iconSize, iconSize);
            Label("Label", rect, skin, caption, 72, 5, w - 92, h - 10, fontSize, Cream);
            return button;
        }
        private static RectTransform Status(Transform parent, Skin skin, string name, string icon, string label, string value, float x)
        {
            var rect = Rect(name, parent, x, 111, 455, 83);
            Picture("Background", rect, skin.navy, 0, 0, 455, 83, true);
            Icon("Icon", rect, skin, icon, 22, 18, 48, 48);
            var title = Label("Label", rect, skin, label + ":", 83, 8, 340, 24, 19, Cream);
            title.alignment = TextAnchor.MiddleLeft;
            var number = Label("Value", rect, skin, value, 83, 31, 340, 29, 24, Cream);
            number.alignment = TextAnchor.MiddleLeft;
            return rect;
        }
        private static RectTransform Stat(Transform parent, Skin skin, string name, string icon, string label, string value, float y, float? amount, Color color)
        {
            var rect = Rect(name, parent, 48, y, 419, 43);
            var glyph = Icon("Icon", rect, skin, icon, 0, 5, 30, 30); glyph.color = Ink;
            var title = Label("Label", rect, skin, label + ":", 40, 0, 139, 36, 21, Ink);
            title.alignment = TextAnchor.MiddleLeft;
            Label("Value", rect, skin, value, 312, 0, 107, 36, 21, Ink);
            if (amount.HasValue) Progress("Progress", rect, skin, 174, 12, 126, 15, amount.Value, color);
            return rect;
        }
        private static RectTransform Progress(string name, Transform parent, Skin skin, float x, float y, float w, float h, float value, Color color)
        {
            var rect = Rect(name, parent, x, y, w, h);
            var track = Picture("Track", rect, null, 0, 0, w, h); track.color = new Color(0.08f, 0.08f, 0.08f);
            var area = Rect("FillArea", rect, 2, 2, w - 4, h - 4);
            var fill = Picture("Fill", area, null, 0, 0, w - 4, h - 4);
            fill.color = color;
            var slider = rect.gameObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false;
            slider.fillRect = fill.rectTransform; slider.interactable = false;
            slider.navigation = new Navigation { mode = Navigation.Mode.None };
            slider.value = Mathf.Clamp01(value);
            return rect;
        }
        private static void SavePart(string folder, string name, GameObject source)
        {
            var clone = Object.Instantiate(source);
            try
            {
                clone.name = name;
                var rect = clone.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero; rect.localScale = Vector3.one;
                PrefabUtility.SaveAsPrefabAsset(clone, folder + "/" + name + ".prefab");
            }
            finally { Object.DestroyImmediate(clone); }
        }
        private static void EnsureFolder(string path)
        {
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
