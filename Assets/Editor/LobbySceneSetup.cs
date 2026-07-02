using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Museum.Lobby;

namespace Museum.EditorTools
{
    public static class LobbySceneSetup
    {
        const string MenuPath = "Tools/Museum/Phase 4/Set Up Lobby Scene";

        const string LobbyScenePath = "Assets/Scenes/Lobby.unity";
        const string CountryListPath = "Assets/Data/Countries.asset";
        const string XrOriginPrefab = "Assets/Samples/XR Interaction Toolkit/3.0.8/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        // Palette matches the artifact label. Stone is fully opaque now so the form reads clearly
        // against the textured hieroglyph backdrop instead of letting the wall bleed through.
        static readonly Color Stone = new Color(0.07f, 0.05f, 0.04f, 1f);
        static readonly Color Gold = new Color(0.92f, 0.74f, 0.36f, 1f);
        static readonly Color Cream = new Color(0.98f, 0.96f, 0.90f, 1f);
        static readonly Color Muted = new Color(0.78f, 0.74f, 0.66f, 1f);

        [MenuItem(MenuPath)]
        public static void SetupLobbyScene()
        {
            if (!File.Exists(XrOriginPrefab))
            {
                EditorUtility.DisplayDialog("Set Up Lobby", "XR Origin prefab not found. Run Tools > Museum > Import XR Starter Assets first.", "OK");
                return;
            }

            EnsureFolder("Assets/Scenes");
            var countries = EnsureCountryListAsset();

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var lobby = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(lobby, LobbyScenePath);

            // Lighting — warm key + cool fill so the gold/cream UI reads well against dark stone backdrop.
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.92f, 0.78f);
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);

            var fillGo = new GameObject("Fill Light");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.4f;
            fill.color = new Color(0.55f, 0.70f, 0.95f);
            fillGo.transform.rotation = Quaternion.Euler(140, 30, 0);

            // Backdrop: floor, back wall, two flanking columns. Gives the lobby visual presence
            // instead of rendering against the default HDRP void.
            BuildBackdrop();

            // XR Origin from Starter Assets
            var xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrOriginPrefab);
            var xr = (GameObject)PrefabUtility.InstantiatePrefab(xrPrefab, lobby);
            xr.transform.position = Vector3.zero;
            xr.transform.rotation = Quaternion.identity;

            // EventSystem (UI input)
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Form Canvas — bigger, slightly closer so the player isn't reading microscopic text.
            var canvasGo = BuildFormCanvas(out var controller);
            canvasGo.transform.position = new Vector3(0f, 1.55f, 1.6f);
            canvasGo.transform.rotation = Quaternion.identity;

            controller.countries = countries;

            EditorSceneManager.MarkSceneDirty(lobby);
            EditorSceneManager.SaveScene(lobby);

            AddSceneToBuildSettings(LobbyScenePath, asFirst: true);
            AddSceneToBuildSettings("Assets/Scenes/Museum.unity", asFirst: false);

            Selection.activeGameObject = canvasGo;
            EditorGUIUtility.PingObject(canvasGo);

            EditorUtility.DisplayDialog("Lobby Scene Ready",
                $"Created:\n{LobbyScenePath}\n\n" +
                "Includes:\n" +
                "  - Backdrop (floor, walls, two columns)\n" +
                "  - Warm key + cool fill directional lights\n" +
                "  - XR Origin + EventSystem\n" +
                "  - 1.6m × 1.1m world-space form (name / age / country / submit)\n" +
                "  - LobbyController wired to all fields\n" +
                "  - Country list refreshed (~140 entries)\n\n" +
                "Set %APPDATA%/MuseumVR/config.json with your OpenAI key, then press Play.",
                "OK");
        }

        static GameObject BuildFormCanvas(out LobbyController controller)
        {
            var root = new GameObject("Lobby Canvas");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1.6f, 1.1f); // 60% bigger than before; comfortable reading at 1.6m
            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 220f;
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();

            // Background (dark stone)
            AddImage(root.transform, "Background", Stone,
                new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            // Gold border
            AddImage(root.transform, "BorderTop", Gold, new Vector2(0,0.985f), new Vector2(1,1), Vector2.zero, Vector2.zero);
            AddImage(root.transform, "BorderBottom", Gold, new Vector2(0,0), new Vector2(1,0.015f), Vector2.zero, Vector2.zero);
            AddImage(root.transform, "BorderLeft", Gold, new Vector2(0,0), new Vector2(0.01f,1), Vector2.zero, Vector2.zero);
            AddImage(root.transform, "BorderRight", Gold, new Vector2(0.99f,0), new Vector2(1,1), Vector2.zero, Vector2.zero);

            // Title
            AddTmp(root.transform, "Title", "Welcome to the Egyptian Museum",
                fontSize: 0.075f, color: Cream, alignment: TextAlignmentOptions.Center,
                fontStyle: FontStyles.Bold,
                anchorMin: new Vector2(0.05f, 0.85f), anchorMax: new Vector2(0.95f, 0.97f));

            AddTmp(root.transform, "Subtitle", "Tell us a little about yourself",
                fontSize: 0.038f, color: Muted, alignment: TextAlignmentOptions.Center,
                fontStyle: FontStyles.Italic,
                anchorMin: new Vector2(0.05f, 0.78f), anchorMax: new Vector2(0.95f, 0.85f));

            // Name field
            AddTmp(root.transform, "NameLabel", "Name",
                fontSize: 0.038f, color: Gold, alignment: TextAlignmentOptions.Left,
                fontStyle: FontStyles.Normal,
                anchorMin: new Vector2(0.07f, 0.69f), anchorMax: new Vector2(0.40f, 0.74f));

            var nameField = AddInputField(root.transform, "NameField",
                anchorMin: new Vector2(0.07f, 0.62f), anchorMax: new Vector2(0.93f, 0.69f));

            // Age slider
            AddTmp(root.transform, "AgeLabel", "Age",
                fontSize: 0.038f, color: Gold, alignment: TextAlignmentOptions.Left,
                fontStyle: FontStyles.Normal,
                anchorMin: new Vector2(0.07f, 0.52f), anchorMax: new Vector2(0.40f, 0.57f));

            var ageDisplay = AddTmp(root.transform, "AgeDisplay", "Age: 25",
                fontSize: 0.034f, color: Cream, alignment: TextAlignmentOptions.Right,
                fontStyle: FontStyles.Normal,
                anchorMin: new Vector2(0.60f, 0.52f), anchorMax: new Vector2(0.93f, 0.57f));

            var ageSlider = AddSlider(root.transform, "AgeSlider",
                anchorMin: new Vector2(0.07f, 0.46f), anchorMax: new Vector2(0.93f, 0.51f));

            // Country dropdown
            AddTmp(root.transform, "CountryLabel", "Country",
                fontSize: 0.038f, color: Gold, alignment: TextAlignmentOptions.Left,
                fontStyle: FontStyles.Normal,
                anchorMin: new Vector2(0.07f, 0.36f), anchorMax: new Vector2(0.40f, 0.41f));

            var countryDropdown = AddDropdown(root.transform, "CountryDropdown",
                anchorMin: new Vector2(0.07f, 0.29f), anchorMax: new Vector2(0.93f, 0.36f));

            // Status / API key status
            var apiKeyStatus = AddTmp(root.transform, "ApiKeyStatus", "Checking OpenAI key...",
                fontSize: 0.030f, color: Muted, alignment: TextAlignmentOptions.Center,
                fontStyle: FontStyles.Italic,
                anchorMin: new Vector2(0.05f, 0.21f), anchorMax: new Vector2(0.95f, 0.26f));

            var statusText = AddTmp(root.transform, "Status", "",
                fontSize: 0.030f, color: Muted, alignment: TextAlignmentOptions.Center,
                fontStyle: FontStyles.Italic,
                anchorMin: new Vector2(0.05f, 0.16f), anchorMax: new Vector2(0.95f, 0.21f));

            // Submit button
            var submit = AddButton(root.transform, "Submit", "Begin Tour",
                anchorMin: new Vector2(0.30f, 0.05f), anchorMax: new Vector2(0.70f, 0.14f));

            controller = root.AddComponent<LobbyController>();
            controller.nameField = nameField;
            controller.ageSlider = ageSlider;
            controller.ageDisplay = ageDisplay;
            controller.countryDropdown = countryDropdown;
            controller.submitButton = submit;
            controller.statusText = statusText;
            controller.apiKeyStatusText = apiKeyStatus;

            return root;
        }

        static void EnsureRepeatWrap(string path)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            bool dirty = false;
            if (imp.wrapMode != TextureWrapMode.Repeat) { imp.wrapMode = TextureWrapMode.Repeat; dirty = true; }
            if (imp.maxTextureSize < 2048) { imp.maxTextureSize = 2048; dirty = true; }
            if (dirty) imp.SaveAndReimport();
        }

        static void BuildBackdrop()
        {
            var root = new GameObject("Lobby Backdrop");

            var floorMat = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            if (floorMat.HasProperty("_BaseColor")) floorMat.SetColor("_BaseColor", new Color(0.10f, 0.08f, 0.06f, 1f));
            else if (floorMat.HasProperty("_Color")) floorMat.SetColor("_Color", new Color(0.10f, 0.08f, 0.06f, 1f));
            if (floorMat.HasProperty("_Smoothness")) floorMat.SetFloat("_Smoothness", 0.25f);

            // Back wall material: textured with the hieroglyph image. This is what the player faces
            // through the form — the focal background.
            var backWallMat = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            const string WallTexturePath = "Assets/Textures/Lobby/EgyptianHieroglyphs.jpg";
            var wallTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(WallTexturePath);
            if (wallTexture != null)
            {
                EnsureRepeatWrap(WallTexturePath);
                if (backWallMat.HasProperty("_BaseColorMap"))
                {
                    backWallMat.SetTexture("_BaseColorMap", wallTexture);
                    backWallMat.SetTextureScale("_BaseColorMap", new Vector2(2f, 1f)); // 2 horizontal tiles across the 20m wall — readable, no harsh seams
                }
                // Slight tint down so the texture doesn't blow out under the directional light.
                if (backWallMat.HasProperty("_BaseColor")) backWallMat.SetColor("_BaseColor", new Color(0.7f, 0.65f, 0.55f, 1f));
            }
            else
            {
                Debug.LogWarning("[LobbySceneSetup] Hieroglyph wall texture not found at " + WallTexturePath + " — back wall will use solid stone color.");
                if (backWallMat.HasProperty("_BaseColor")) backWallMat.SetColor("_BaseColor", new Color(0.16f, 0.13f, 0.10f, 1f));
            }
            if (backWallMat.HasProperty("_Smoothness")) backWallMat.SetFloat("_Smoothness", 0.10f);

            // Side walls: solid dark stone. Don't compete with the hieroglyph backdrop.
            var sideWallMat = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            if (sideWallMat.HasProperty("_BaseColor")) sideWallMat.SetColor("_BaseColor", new Color(0.13f, 0.10f, 0.08f, 1f));
            else if (sideWallMat.HasProperty("_Color")) sideWallMat.SetColor("_Color", new Color(0.13f, 0.10f, 0.08f, 1f));
            if (sideWallMat.HasProperty("_Smoothness")) sideWallMat.SetFloat("_Smoothness", 0.15f);

            var columnMat = new Material(Shader.Find("HDRP/Lit") ?? Shader.Find("Standard"));
            if (columnMat.HasProperty("_BaseColor")) columnMat.SetColor("_BaseColor", new Color(0.55f, 0.42f, 0.20f, 1f));
            else if (columnMat.HasProperty("_Color")) columnMat.SetColor("_Color", new Color(0.55f, 0.42f, 0.20f, 1f));
            if (columnMat.HasProperty("_Smoothness")) columnMat.SetFloat("_Smoothness", 0.45f);
            if (columnMat.HasProperty("_Metallic")) columnMat.SetFloat("_Metallic", 0.7f);

            // Floor (20m x 20m). Keep the collider — the teleport raycast needs it.
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(2f, 1f, 2f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = floorMat;

            // Mark as a TeleportationArea so the player can teleport around the lobby with the controller.
            var teleportLayer = UnityEngine.XR.Interaction.Toolkit.InteractionLayerMask.GetMask("Default", "Teleport");
            var area = floor.AddComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea>();
            if (teleportLayer != 0) area.interactionLayers = teleportLayer;

            // Back wall (~6m behind the form) — gets the hieroglyph texture.
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Back Wall";
            wall.transform.SetParent(root.transform, false);
            wall.transform.localPosition = new Vector3(0f, 4f, 5f);
            wall.transform.localScale = new Vector3(20f, 8f, 0.3f);
            wall.GetComponent<MeshRenderer>().sharedMaterial = backWallMat;
            Object.DestroyImmediate(wall.GetComponent<Collider>());

            // Side walls — solid dark stone, framing the focal back wall without competing with it.
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -4.5f : 4.5f;
                var sw = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sw.name = i == 0 ? "Left Wall" : "Right Wall";
                sw.transform.SetParent(root.transform, false);
                sw.transform.localPosition = new Vector3(x, 4f, 2.5f);
                sw.transform.localScale = new Vector3(0.3f, 8f, 5f);
                sw.GetComponent<MeshRenderer>().sharedMaterial = sideWallMat;
                Object.DestroyImmediate(sw.GetComponent<Collider>());
            }

            // Two columns flanking the form for some Egyptian flavor.
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -1.6f : 1.6f;
                var col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                col.name = i == 0 ? "Left Column" : "Right Column";
                col.transform.SetParent(root.transform, false);
                col.transform.localPosition = new Vector3(x, 1.5f, 1.9f);
                col.transform.localScale = new Vector3(0.25f, 1.5f, 0.25f);
                col.GetComponent<MeshRenderer>().sharedMaterial = columnMat;
                Object.DestroyImmediate(col.GetComponent<Collider>());

                // Capital (top piece)
                var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cap.name = (i == 0 ? "Left" : "Right") + " Capital";
                cap.transform.SetParent(root.transform, false);
                cap.transform.localPosition = new Vector3(x, 3.05f, 1.9f);
                cap.transform.localScale = new Vector3(0.7f, 0.15f, 0.7f);
                cap.GetComponent<MeshRenderer>().sharedMaterial = columnMat;
                Object.DestroyImmediate(cap.GetComponent<Collider>());

                // Base (bottom piece)
                var basePc = GameObject.CreatePrimitive(PrimitiveType.Cube);
                basePc.name = (i == 0 ? "Left" : "Right") + " Base";
                basePc.transform.SetParent(root.transform, false);
                basePc.transform.localPosition = new Vector3(x, 0.07f, 1.9f);
                basePc.transform.localScale = new Vector3(0.7f, 0.15f, 0.7f);
                basePc.GetComponent<MeshRenderer>().sharedMaterial = columnMat;
                Object.DestroyImmediate(basePc.GetComponent<Collider>());
            }
        }

        static CountryList EnsureCountryListAsset()
        {
            EnsureFolder("Assets/Data");
            var existing = AssetDatabase.LoadAssetAtPath<CountryList>(CountryListPath);
            if (existing != null)
            {
                // Refresh the list from CountryList.Default() so re-running this menu picks up
                // any new countries we've added since the asset was first created.
                existing.countries = CountryList.Default();
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                return existing;
            }
            var asset = ScriptableObject.CreateInstance<CountryList>();
            asset.countries = CountryList.Default();
            AssetDatabase.CreateAsset(asset, CountryListPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static void AddImage(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = offsetMin;
            r.offsetMax = offsetMax;
        }

        static TextMeshProUGUI AddTmp(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions alignment, FontStyles fontStyle, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = alignment;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = fontStyle;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            return tmp;
        }

        static TMP_InputField AddInputField(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.20f, 0.16f, 0.12f, 1f);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            var input = go.AddComponent<TMP_InputField>();

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(go.transform, false);
            var textAreaRt = textArea.AddComponent<RectTransform>();
            textAreaRt.anchorMin = new Vector2(0, 0);
            textAreaRt.anchorMax = new Vector2(1, 1);
            textAreaRt.offsetMin = new Vector2(0.015f, 0.008f);
            textAreaRt.offsetMax = new Vector2(-0.015f, -0.008f);
            textArea.AddComponent<RectMask2D>();

            // Resolve TMP's default font asset explicitly; relying on a fresh TextMeshProUGUI's
            // .font property returns null at scene-build time and TMP_InputField.fontAsset = null
            // makes typed glyphs render as the same color as the background (the bug we're fixing).
            var defaultFont = TMP_Settings.defaultFontAsset;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(textArea.transform, false);
            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            if (defaultFont != null) textTmp.font = defaultFont;
            textTmp.fontSize = 0.040f;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;
            textTmp.text = "";
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(textArea.transform, false);
            var placeTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
            if (defaultFont != null) placeTmp.font = defaultFont;
            placeTmp.fontSize = 0.040f;
            placeTmp.color = Muted;
            placeTmp.fontStyle = FontStyles.Italic;
            placeTmp.alignment = TextAlignmentOptions.MidlineLeft;
            placeTmp.text = "Your name";
            var placeRt = placeholderGo.GetComponent<RectTransform>();
            placeRt.anchorMin = Vector2.zero;
            placeRt.anchorMax = Vector2.one;
            placeRt.offsetMin = Vector2.zero;
            placeRt.offsetMax = Vector2.zero;

            input.textViewport = textAreaRt;
            input.textComponent = textTmp;
            input.placeholder = placeTmp;
            if (defaultFont != null) input.fontAsset = defaultFont;

            // Set typed-text color AFTER input.textComponent assignment so TMP_InputField's internal
            // initialization doesn't reset it.
            textTmp.color = Cream;
            input.caretWidth = 1;
            input.caretBlinkRate = 0.85f;
            // No customCaretColor / selectionColor — TMP defaults render properly. Earlier I tried
            // a translucent gold selection color, which TMP rendered as a wide grey block extending
            // past the text instead of the expected character-bounded highlight.

            return input;
        }

        static Slider AddSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.20f, 0.16f, 0.12f, 1f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 0.4f);
            bgRt.anchorMax = new Vector2(1, 0.6f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.4f);
            faRt.anchorMax = new Vector2(1, 0.6f);
            faRt.offsetMin = Vector2.zero;
            faRt.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = Gold;
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(go.transform, false);
            var haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.offsetMin = new Vector2(0.01f, 0);
            haRt.offsetMax = new Vector2(-0.01f, 0);

            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            var handleImg = handle.AddComponent<Image>();
            handleImg.color = Cream;
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.anchorMin = new Vector2(0, 0);
            handleRt.anchorMax = new Vector2(0, 1);
            handleRt.sizeDelta = new Vector2(0.02f, 0);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.minValue = 5;
            slider.maxValue = 99;
            slider.wholeNumbers = true;
            slider.value = 25;
            return slider;
        }

        static TMP_Dropdown AddDropdown(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.20f, 0.16f, 0.12f, 1f);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.fontSize = 0.025f;
            labelTmp.color = Cream;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0, 0);
            labelRt.anchorMax = new Vector2(1, 1);
            labelRt.offsetMin = new Vector2(0.015f, 0);
            labelRt.offsetMax = new Vector2(-0.04f, 0);

            // Template required for TMP_Dropdown but kept hidden by Unity; we populate at runtime.
            var template = new GameObject("Template");
            template.transform.SetParent(go.transform, false);
            template.SetActive(false);
            var tImg = template.AddComponent<Image>();
            tImg.color = new Color(0.16f, 0.13f, 0.10f, 1f);
            var tRt = template.GetComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0, 0);
            tRt.anchorMax = new Vector2(1, 0);
            tRt.pivot = new Vector2(0.5f, 1f);
            tRt.sizeDelta = new Vector2(0, 0.6f); // taller dropdown panel — fits ~10 rows
            template.AddComponent<UnityEngine.UI.ScrollRect>();
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            viewport.AddComponent<Image>().color = new Color(0.10f, 0.08f, 0.06f, 1f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            var vRt = viewport.GetComponent<RectTransform>();
            vRt.anchorMin = Vector2.zero;
            vRt.anchorMax = Vector2.one;
            vRt.offsetMin = Vector2.zero;
            vRt.offsetMax = Vector2.zero;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var cRt = content.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0, 1);
            cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.sizeDelta = new Vector2(0, 0.06f); // per-item height — bumped from 0.04 for legibility

            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var iRt = item.AddComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0, 0.5f);
            iRt.anchorMax = new Vector2(1, 0.5f);
            iRt.sizeDelta = new Vector2(0, 0.06f);
            var toggle = item.AddComponent<Toggle>();

            var itemBg = new GameObject("Item Background");
            itemBg.transform.SetParent(item.transform, false);
            var itemBgImg = itemBg.AddComponent<Image>();
            itemBgImg.color = new Color(0.20f, 0.16f, 0.12f, 1f);
            var ibRt = itemBg.GetComponent<RectTransform>();
            ibRt.anchorMin = Vector2.zero;
            ibRt.anchorMax = Vector2.one;
            ibRt.offsetMin = Vector2.zero;
            ibRt.offsetMax = Vector2.zero;

            var itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);
            var itemTmp = itemLabel.AddComponent<TextMeshProUGUI>();
            itemTmp.fontSize = 0.034f;
            itemTmp.color = Cream;
            itemTmp.alignment = TextAlignmentOptions.MidlineLeft;
            var itemRt = itemLabel.GetComponent<RectTransform>();
            itemRt.anchorMin = Vector2.zero;
            itemRt.anchorMax = Vector2.one;
            itemRt.offsetMin = new Vector2(0.015f, 0);
            itemRt.offsetMax = Vector2.zero;
            toggle.targetGraphic = itemBgImg;

            var sr = template.GetComponent<UnityEngine.UI.ScrollRect>();
            sr.viewport = vRt;
            sr.content = cRt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 0.06f; // very low — VR thumbstick scroll deltas overshoot at default 1.0
            sr.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped; // no elastic overshoot
            sr.inertia = false; // no momentum after release; keep scroll deterministic

            var dd = go.AddComponent<TMP_Dropdown>();
            dd.captionText = labelTmp;
            dd.itemText = itemTmp;
            dd.template = tRt;
            dd.targetGraphic = img;
            return dd;
        }

        static Button AddButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = Gold;
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var tmp = labelGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 0.030f;
            tmp.color = Stone;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            var lRt = labelGo.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.offsetMin = Vector2.zero;
            lRt.offsetMax = Vector2.zero;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Gold;
            colors.highlightedColor = new Color(1f, 0.85f, 0.5f, 1f);
            colors.pressedColor = new Color(0.65f, 0.50f, 0.20f, 1f);
            colors.disabledColor = new Color(0.4f, 0.35f, 0.25f, 0.7f);
            btn.colors = colors;
            return btn;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parts = path.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        static void AddSceneToBuildSettings(string scenePath, bool asFirst)
        {
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            current.RemoveAll(s => s.path == scenePath);
            var entry = new EditorBuildSettingsScene(scenePath, true);
            if (asFirst) current.Insert(0, entry);
            else current.Add(entry);
            EditorBuildSettings.scenes = current.ToArray();
        }
    }
}
