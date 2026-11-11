using System.Linq;
using PhoenicianJourney.Landing;
using PhoenicianJourney.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace PhoenicianJourney.Editor
{
    /// <summary>
    /// Generates Assets/Scenes/Harbor.unity from the Blender export, so the scene is
    /// reproducible and nobody has to merge hand-edited scene files. Rerun it after a new
    /// export: Tools ▸ Phoenician Journey ▸ Rebuild Harbor Scene.
    ///
    /// It applies the Blender naming convention (see LandingNaming): colliders for solid
    /// meshes, hidden COL_ colliders, no collision for water and NOCOL_, Station components
    /// on cargo, dye, and scribe footprints, and the player at the SPAWN node.
    /// </summary>
    public static class HarborSceneBuilder
    {
        public const string LandingAssetPath = "Assets/Art/harbor-landing.glb";
        public const string ScenePath = "Assets/Scenes/Harbor.unity";
        private const string StylesPath = "Assets/UI/Harbor.uss";
        private const string ThemePath = "Assets/UI/HarborTheme.tss";
        private const string PanelSettingsPath = "Assets/UI/HarborPanelSettings.asset";

        private static readonly Color Sky = new Color(0.62f, 0.78f, 0.9f);

        [MenuItem("Tools/Phoenician Journey/Rebuild Harbor Scene")]
        public static void Rebuild()
        {
            var landingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LandingAssetPath);
            if (landingPrefab == null)
            {
                throw new System.InvalidOperationException(
                    $"No landing at {LandingAssetPath}. Check that the file exists and that the glTFast package is installed.");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetUpLighting();

            var landing = (GameObject)PrefabUtility.InstantiatePrefab(landingPrefab, scene);
            landing.name = "Landing";
            var report = ApplyNamingConvention(landing);

            var spawn = FindSpawn(landing, out var spawnSource);
            var yaw = FaceTowardCargo(spawn);
            var player = CreatePlayer(spawn, yaw);
            CreateGame(player);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            // Lets MeshColliders use the imported meshes in player builds.
            PlayerSettings.bakeCollisionMeshes = true;
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"Rebuilt {ScenePath}: {report} Spawn from {spawnSource} at {spawn}, facing {yaw:0}°.");
        }

        private static void SetUpLighting()
        {
            var sun = new GameObject("Sun", typeof(Light));
            var light = sun.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.75f, 0.82f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.6f, 0.55f);
            RenderSettings.ambientGroundColor = new Color(0.45f, 0.4f, 0.35f);
            RenderSettings.skybox = null;
            RenderSettings.sun = light;
        }

        /// <summary>Adds colliders and stations by mesh name. Returns a short summary for the log.</summary>
        public static string ApplyNamingConvention(GameObject landing)
        {
            int solid = 0, hidden = 0, visual = 0, stations = 0;
            foreach (var filter in landing.GetComponentsInChildren<MeshFilter>(true))
            {
                var go = filter.gameObject;
                var role = RoleWithParents(go.transform, landing.transform);
                if (role != MeshRole.Visual && go.GetComponent<Collider>() == null)
                {
                    go.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                }

                switch (role)
                {
                    case MeshRole.Collider:
                        var renderer = go.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.enabled = false;
                        }
                        hidden++;
                        break;
                    case MeshRole.Visual:
                        visual++;
                        break;
                    default:
                        solid++;
                        break;
                }
            }

            foreach (var transform in landing.GetComponentsInChildren<Transform>(true))
            {
                if (LandingNaming.StationOf(transform.name) is StationId id && transform.GetComponentInParent<Station>() == null)
                {
                    transform.gameObject.AddComponent<Station>().Configure(id);
                    stations++;
                }
            }

            return $"{solid} solid, {hidden} hidden colliders, {visual} without collision, {stations} stations.";
        }

        /// <summary>
        /// The role from the closest named node. glTFast puts each primitive of a multi-material
        /// mesh on a child object, so a COL_ or water name on the parent node still applies.
        /// </summary>
        private static MeshRole RoleWithParents(Transform transform, Transform root)
        {
            for (var current = transform; current != null && current != root; current = current.parent)
            {
                var role = LandingNaming.RoleOf(current.name);
                if (role != MeshRole.Solid)
                {
                    return role;
                }
            }

            return MeshRole.Solid;
        }

        private static Vector3 FindSpawn(GameObject landing, out string source)
        {
            var spawn = landing.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => LandingNaming.IsSpawn(t.name));
            if (spawn != null)
            {
                source = "the SPAWN node";
                foreach (var renderer in spawn.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }
                return spawn.position;
            }

            source = "the landing bounds (no SPAWN node)";
            var bounds = landing.GetComponentsInChildren<Renderer>().Select(r => r.bounds)
                .Aggregate((a, b) => { a.Encapsulate(b); return a; });
            return new Vector3(bounds.center.x, bounds.max.y + 1f, bounds.center.z);
        }

        /// <summary>Turns the player toward the cargo station, which makes the start independent of export axes.</summary>
        private static float FaceTowardCargo(Vector3 spawn)
        {
            var cargo = Object.FindObjectsByType<Station>(FindObjectsInactive.Include)
                .FirstOrDefault(s => s.Id == StationId.Cargo);
            if (cargo == null)
            {
                return 0f;
            }

            var to = cargo.Footprint.center - spawn;
            to.y = 0f;
            return to.sqrMagnitude < 0.01f ? 0f : Quaternion.LookRotation(to).eulerAngles.y;
        }

        private static FirstPersonWalker CreatePlayer(Vector3 spawn, float yaw)
        {
            var player = new GameObject("Player", typeof(CharacterController), typeof(FirstPersonWalker));
            player.transform.SetPositionAndRotation(spawn, Quaternion.Euler(0f, yaw, 0f));

            var eye = new GameObject("Eye", typeof(Camera), typeof(AudioListener));
            eye.transform.SetParent(player.transform, false);
            eye.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            eye.tag = "MainCamera";
            var camera = eye.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Sky;
            camera.nearClipPlane = 0.05f;
            camera.fieldOfView = 63f;
            return player.GetComponent<FirstPersonWalker>();
        }

        private static void CreateGame(FirstPersonWalker player)
        {
            var game = new GameObject("HarborGame", typeof(UIDocument), typeof(HarborGame));
            game.GetComponent<UIDocument>().panelSettings = LoadOrCreatePanelSettings();
            game.GetComponent<HarborGame>().Configure(player, AssetDatabase.LoadAssetAtPath<StyleSheet>(StylesPath));
        }

        private static PanelSettings LoadOrCreatePanelSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            }

            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1280, 720);
            settings.match = 0.5f;
            EditorUtility.SetDirty(settings);
            return settings;
        }
    }
}
