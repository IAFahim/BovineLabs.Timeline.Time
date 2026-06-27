using BovineLabs.Timeline.Core.Authoring;
using BovineLabs.Timeline.Time.Authoring;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace Editor.SlowMoDemo
{
public static class SlowMoDemoSetup
{
    private const string Folder = "Assets/SlowMoDemo";
    private const string TimelinePath = Folder + "/SlowMoTimeline.playable";

    [MenuItem("Tools/SlowMo Demo/Build")]
    public static void Build()
    {
        var timeline = BuildTimeline();

        var parent = EditorSceneManager.GetActiveScene();
        var parentPath = parent.path;
        var subPath = FindSubScenePath(parent);
        if (string.IsNullOrEmpty(subPath))
        {
            Debug.LogError("SlowMoDemo: no SubScene found in active scene.");
            return;
        }

        var sub = EditorSceneManager.OpenScene(subPath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(sub);

        RemoveOld(sub, "SlowMo Ground", "SlowMo Ball", "SlowMo Director");
        CreateGround(sub);
        CreateBall(sub);
        CreateDirector(sub, timeline);

        EditorSceneManager.MarkSceneDirty(sub);
        EditorSceneManager.SaveScene(sub);
        SceneManager.SetActiveScene(parent);
        EditorSceneManager.CloseScene(sub, false);
        EditorSceneManager.OpenScene(parentPath, OpenSceneMode.Single);

        Debug.Log("SlowMoDemo: built Ball + Ground + Director (playOnAwake, loop). Timeline: " + TimelinePath);
    }

    private static TimelineAsset BuildTimeline()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
        {
            AssetDatabase.CreateFolder("Assets", "SlowMoDemo");
        }

        if (AssetDatabase.LoadAssetAtPath<TimelineAsset>(TimelinePath) != null)
        {
            AssetDatabase.DeleteAsset(TimelinePath);
        }

        var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        AssetDatabase.CreateAsset(timeline, TimelinePath);

        var track = timeline.CreateTrack<WorldTimeScaleTrack>(null, "World Time Scale");
        var clip = track.CreateClip<WorldTimeScaleClip>();
        clip.start = 1.0;
        clip.duration = 6.0;
        clip.easeInDuration = 0.4;
        clip.easeOutDuration = 0.4;
        clip.displayName = "Slow Mo 0.1x";

        var asset = (WorldTimeScaleClip)clip.asset;
        asset.timeScale = 0.1f;

        EditorUtility.SetDirty(clip.asset);
        EditorUtility.SetDirty(timeline);
        AssetDatabase.SaveAssets();
        return timeline;
    }

    private static string FindSubScenePath(Scene parent)
    {
        foreach (var go in parent.GetRootGameObjects())
        {
            var sub = go.GetComponent<SubScene>();
            if (sub != null && sub.SceneAsset != null)
            {
                return AssetDatabase.GetAssetPath(sub.SceneAsset);
            }
        }

        return null;
    }

    private static void RemoveOld(Scene scene, params string[] names)
    {
        foreach (var n in names)
        {
            var go = GameObject.Find(n);
            if (go != null && go.scene == scene)
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    private static void CreateGround(Scene scene)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "SlowMo Ground";
        ground.transform.position = new Vector3(12f, -1.5f, 0f);
        ground.transform.localScale = new Vector3(10f, 1f, 10f);
        Object.DestroyImmediate(ground.GetComponent<UnityEngine.Collider>());

        var shape = ground.AddComponent<PhysicsShapeAuthoring>();
        shape.SetBox(new BoxGeometry
        {
            Center = float3.zero,
            Size = new float3(1f, 1f, 1f),
            Orientation = quaternion.identity,
            BevelRadius = 0.05f
        });

        SceneManager.MoveGameObjectToScene(ground, scene);
    }

    private static void CreateBall(Scene scene)
    {
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "SlowMo Ball";
        ball.transform.position = new Vector3(12f, 9f, 0f);
        Object.DestroyImmediate(ball.GetComponent<UnityEngine.Collider>());

        var shape = ball.AddComponent<PhysicsShapeAuthoring>();
        shape.SetSphere(new SphereGeometry { Center = float3.zero, Radius = 0.5f }, quaternion.identity);
        shape.OverrideRestitution = true;
        shape.Restitution = new PhysicsMaterialCoefficient
            { Value = 0.85f, CombineMode = Unity.Physics.Material.CombinePolicy.Maximum };

        var body = ball.AddComponent<PhysicsBodyAuthoring>();
        body.MotionType = BodyMotionType.Dynamic;
        body.Mass = 1f;
        body.GravityFactor = 1f;

        SceneManager.MoveGameObjectToScene(ball, scene);
    }

    private static void CreateDirector(Scene scene, TimelineAsset timeline)
    {
        var go = new GameObject("SlowMo Director");
        var director = go.AddComponent<PlayableDirector>();
        director.playableAsset = timeline;
        director.playOnAwake = true;
        director.extrapolationMode = DirectorWrapMode.Loop;

        SceneManager.MoveGameObjectToScene(go, scene);
    }
}
}
