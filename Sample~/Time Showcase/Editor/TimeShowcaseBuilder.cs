using System.Collections.Generic;
using TMPro;
using Unity.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using PositionTrack = BovineLabs.Timeline.Transform.Authoring.TransformPositionTrack;
using PositionClip = BovineLabs.Timeline.Transform.Authoring.PositionClip;
using PositionType = BovineLabs.Timeline.Transform.Authoring.PositionType;
using TimelineTimeScaleTrack = BovineLabs.Timeline.Time.Authoring.TimelineTimeScaleTrack;
using TimelineTimeScaleClip = BovineLabs.Timeline.Time.Authoring.TimelineTimeScaleClip;
using WorldTimeScaleTrack = BovineLabs.Timeline.Time.Authoring.WorldTimeScaleTrack;
using WorldTimeScaleClip = BovineLabs.Timeline.Time.Authoring.WorldTimeScaleClip;
using TimelineSpeedFromStatAuthoring = BovineLabs.Timeline.Time.Authoring.TimelineSpeedFromStatAuthoring;
using StatAuthoring = BovineLabs.Essence.Authoring.StatAuthoring;
using StatModifierAuthoring = BovineLabs.Essence.Authoring.StatModifierAuthoring;
using StatSchemaObject = BovineLabs.Essence.Authoring.StatSchemaObject;
using StatAuthoringType = BovineLabs.Essence.Authoring.StatAuthoringType;
using TargetSlot = BovineLabs.Reaction.Data.Core.Target;
using TimelineBeginAuthoring = BovineLabs.Timeline.Core.Authoring.TimelineBeginAuthoring;
using TimelineBeginMode = BovineLabs.Timeline.Core.Authoring.TimelineBeginMode;

public static class TimeShowcaseBuilder
{
    private const string SampleFolder = "Assets/Samples/TimeShowcase";
    private const string TimelineFolder = SampleFolder + "/Timelines";
    private const string ParentPath = SampleFolder + "/TimeShowcase.unity";
    private const string SubPath = SampleFolder + "/TimeShowcase_Sub.unity";

    private const string RequiredInSubScenePath = "Assets/Prefabs/Required In Subscene.prefab";
    private const string SlowMoPath = "Assets/Settings/Schemas/Stats/SlowMo.asset";

    private static readonly Color PerTimelineColor = new Color(0.20f, 0.85f, 0.45f);
    private static readonly Color WorldColor = new Color(0.92f, 0.92f, 0.96f);
    private static readonly Color StatColor = new Color(0.95f, 0.55f, 0.20f);
    private static readonly Color ControlColor = new Color(0.55f, 0.58f, 0.65f);
    private static readonly Color RailColor = new Color(0.16f, 0.17f, 0.21f);
    private static readonly Color PadColor = new Color(0.24f, 0.26f, 0.31f);
    private static readonly Color BannerColor = new Color(0.06f, 0.08f, 0.12f);

    private const float PerTimelineX = -18f;
    private const float WorldX = 0f;
    private const float StatX = 18f;
    private const float RowStep = 6.0f;
    private const float ActorY = 1.0f;
    private const float TravelHalf = 2.4f;

    private static readonly Vector3 CameraPos = new Vector3(0f, 17f, -36f);

    private static Scene activeSub;
    private static StatSchemaObject slowMo;

    private sealed class CellWire
    {
        public string DirectorName;
        public string TimelinePath;
        public string TimeScaleTrackName;
        public string StatBindActorName;
    }

    private static readonly List<CellWire> Wires = new List<CellWire>();

    private sealed class CaptionData
    {
        public string Title;
        public string Usage;
        public Vector3 CellPos;
        public Color Color;
    }

    private static readonly List<CaptionData> Captions = new List<CaptionData>();

    [MenuItem("Showcase/Build Time")]
    public static void Build()
    {
        Wires.Clear();
        Captions.Clear();

        slowMo = AssetDatabase.LoadAssetAtPath<StatSchemaObject>(SlowMoPath);
        if (slowMo == null)
        {
            Debug.LogError("TimeShowcase: SlowMo schema missing at " + SlowMoPath);
            return;
        }

        EnsureFolders();
        ResetAssets();

        var parent = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(parent, ParentPath);
        var sub = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SetActiveScene(sub);
        activeSub = sub;

        BuildRequiredInSubScene();
        BuildPads();
        BuildPerTimelineColumn();
        BuildWorldColumn();
        BuildStatColumn();

        EditorSceneManager.SaveScene(sub, SubPath);
        EditorSceneManager.SetActiveScene(parent);
        EditorSceneManager.CloseScene(sub, true);

        sub = EditorSceneManager.OpenScene(SubPath, OpenSceneMode.Additive);
        EditorSceneManager.SetActiveScene(sub);
        activeSub = sub;

        foreach (var w in Wires)
        {
            WireCell(w);
        }

        EditorSceneManager.MarkSceneDirty(sub);
        EditorSceneManager.SaveScene(sub);

        EditorSceneManager.SetActiveScene(parent);
        BuildParent();
        EditorSceneManager.SaveScene(parent);

        EditorSceneManager.CloseScene(sub, true);
        EditorSceneManager.OpenScene(ParentPath, OpenSceneMode.Single);

        Debug.Log("TimeShowcase: built grid at " + ParentPath + " directors=" + Wires.Count +
                  " SlowMoKey=" + slowMo.Key);
    }

    // ============================================================
    //  COLUMN A — PER-TIMELINE TIME SCALE (green)
    //  TimelineTimeScaleTrack scales ONLY this director's own clock,
    //  which drives this director's own position-track mover.
    //  Compared against a normal-speed control mover in front.
    // ============================================================

    private static void BuildPerTimelineColumn()
    {
        // Row 0 — CONTROL (no time-scale clip) = 1x baseline reference.
        BuildMoverCell("PT0", PerTimelineX, 0, ControlColor, ClockMode.Game,
            t => { },
            null, false, default, default,
            "Control 1x (no scale)",
            "Plain TransformPositionTrack, NO TimelineTimeScaleClip. The mover travels A<->B at full clock speed — the reference everything else is compared against.");

        // Row 1 — CONSTANT 0.5x slow (authored timeScale, stat=null).
        BuildMoverCell("PT1", PerTimelineX, 1, PerTimelineColor, ClockMode.Game,
            t => { },
            AddTimelineScaleTrack, true, ScaleSpec.Constant(0.5f), default,
            "Constant 0.5x (half speed)",
            "TimelineTimeScaleTrack + clip timeScale=0.5, stat=null. Bake: clock.DeltaTime*=0.5 -> this mover crawls at HALF the control's speed over the same wall-clock.");

        // Row 2 — CONSTANT 2x fast.
        BuildMoverCell("PT2", PerTimelineX, 2, PerTimelineColor, ClockMode.Game,
            t => { },
            AddTimelineScaleTrack, true, ScaleSpec.Constant(2.0f), default,
            "Constant 2x (double speed)",
            "clip timeScale=2.0 -> clock.DeltaTime*=2 -> this mover races at DOUBLE the control's speed. timeScale is UNCLAMPED above 1.");

        // Row 3 — STAT-DRIVEN (SlowMo schema overrides timeScale when resolvable).
        BuildMoverCell("PT3", PerTimelineX, 3, PerTimelineColor, ClockMode.Game,
            t => { },
            AddTimelineScaleTrack, true, ScaleSpec.FromStat(slowMo, 9.99f), "PT3_Actor",
            "Stat-driven (SlowMo key " + (slowMo != null ? slowMo.Key.ToString() : "?") + ")",
            "clip stat=SlowMo bound to the mover's StatAuthoring; if the SlowMo stat resolves on the entity it OVERRIDES the timeScale field every frame (the 9.99 here is the decoy fallback). Seeded SlowMo base so the clock scales to it.");

        // Row 4 — REVERSE -1x (UNCLAMPED negative runs the clock backward).
        BuildMoverCell("PT4", PerTimelineX, 4, PerTimelineColor, ClockMode.Game,
            t => { },
            AddTimelineScaleTrack, true, ScaleSpec.Constant(-1.0f), default,
            "Reverse -1x (UNCLAMPED)",
            "clip timeScale=-1 -> clock.DeltaTime*=-1 runs this timeline's clock IN REVERSE. No clamp anywhere in the chain (0 would freeze it). The mover plays its loop backward.");
    }

    // ============================================================
    //  COLUMN B — WORLD TIME SCALE (white) — GLOBAL bullet-time
    //  WorldTimeScaleClip merges into ONE WorldTimeScale singleton
    //  pushing UnityEngine.Time.timeScale -> affects EVERY GameTime
    //  director's mover. Needs the WorldTimeScale settings singleton
    //  (baked from Required In Subscene's SettingsAuthoring).
    // ============================================================

    private static void BuildWorldColumn()
    {
        // Row 0 — GameTime CONTROL mover with no world clip. It still slows
        // because the world scale is GLOBAL (driven by Row 1's bullet-time clip).
        BuildMoverCell("W0", WorldX, 0, ControlColor, ClockMode.Game,
            t => { },
            null, false, default, default,
            "GameTime mover (no clip)",
            "A plain GameTime mover with NO world clip of its own. Because WorldTimeScale is GLOBAL, the bullet-time clip behind it slows THIS mover too (proves world scope reaches every GameTime timeline).");

        // Row 1 — WORLD BULLET-TIME 0.1x with ease-in/out (the global driver).
        // WINDOWED [1.0, 3.2] so Time.timeScale returns to 1.0 for the rest of
        // each loop — otherwise a full-loop world clip globally crushes EVERY
        // GameTime mover continuously and masks the per-timeline column's deltas.
        BuildMoverCell("W1", WorldX, 1, WorldColor, ClockMode.Game,
            t => { },
            AddWorldScaleTrack, true, ScaleSpec.World(0.1f, 0.4f, 1.0, 2.2), default,
            "World bullet-time 0.1x (windowed ease)",
            "WorldTimeScaleTrack (NO binding) clip timeScale=0.1, easeIn/Out 0.4, active over a WINDOW each loop. While active it pushes Time.timeScale to 0.1 -> the WHOLE world (this mover AND every GameTime mover) crawls; between windows it returns to 1x.");

        // Row 2 — COMPOUNDING: World 0.5x AND a per-timeline 0.5x on the same
        // director -> 0.25x (multiplicative). World clip windowed.
        BuildMoverCell("W2", WorldX, 2, WorldColor, ClockMode.Game,
            t => { },
            AddWorldPlusTimelineScale, true, default, default,
            "Compound World 0.5x x Timeline 0.5x = 0.25x",
            "Same director carries a WorldTimeScaleClip (0.5x global, windowed, one frame latent) AND a full-loop TimelineTimeScaleClip (0.5x same-frame). Where they overlap they COMPOUND multiplicatively -> this mover runs at 0.25x; elsewhere it stays at the per-timeline 0.5x.");

        // Row 3 — FREEZE 0 on an UnscaledGameTime director (the SAFE recipe).
        BuildMoverCell("W3", WorldX, 3, WorldColor, ClockMode.Unscaled,
            t => { },
            AddWorldScaleTrack, true, ScaleSpec.World(0.0f, 0.0f, 3.0, 0.5), default,
            "Freeze 0 (UnscaledGameTime, SAFE)",
            "WorldTimeScaleClip timeScale=0 over a window on an UnscaledGameTime director. Its clock advances at 1x regardless of Time.timeScale, so the clip ENDS on schedule and unfreezes. A GameTime director here would DEADLOCK (frozen clock can never reach clip end).");
    }

    // ============================================================
    //  COLUMN C — SPEED-FROM-STAT (orange) — NON-track component
    //  TimelineSpeedFromStatAuthoring on the director scales its own
    //  playback from a stat, CLAMPED [Min,Max] with a 0.05 floor that
    //  can never freeze (unlike the track's stat mode).
    // ============================================================

    private static void BuildStatColumn()
    {
        // Row 0 — CONTROL 1x for the column.
        BuildMoverCell("ST0", StatX, 0, ControlColor, ClockMode.Game,
            t => { },
            null, false, default, default,
            "Control 1x (no component)",
            "Plain mover, no TimelineSpeedFromStatAuthoring — the normal-speed reference for the stat-driven cells beside it.");

        // Row 1 — SPEED-FROM-STAT clamped to SlowMo, Min 0.05 Max 2 Default 1.
        BuildSpeedFromStatCell("ST1", StatX, 1, StatColor, slowMo, 0.05f, 2f, 1f,
            "Speed-From-Stat (clamped 0.05..2)",
            "TimelineSpeedFromStatAuthoring (Stat=SlowMo, Min=0.05, Max=2, Default=1) ON the director. Reads the SlowMo stat each frame, multiplier*=clamp(stat,Min,Max), then hard-floors the compounded multiplier at 0.05 -> playback follows the stat but NEVER freezes (contrast the track's stat mode).");

        // Row 2 — SPEED-FROM-STAT with a HARD floor demo (Min 0.25): even a tiny
        // stat can't slow below the clamp.
        BuildSpeedFromStatCell("ST2", StatX, 2, StatColor, slowMo, 0.25f, 1f, 1f,
            "Speed-From-Stat (floor Min=0.25)",
            "Same component with Min=0.25: the clamp floor keeps playback at >=0.25x even if the SlowMo stat reads lower. Demonstrates the safety floor that the per-timeline TRACK lacks (the track can freeze on a missing key).");
    }

    // ============================================================
    //  cell construction
    // ============================================================

    private enum ClockMode { Game, Unscaled }

    private struct ScaleSpec
    {
        public float TimeScale;
        public StatSchemaObject Stat;
        public bool IsWorld;
        public float Ease;
        public double Start;
        public double Duration;
        public bool HasWindow;

        public static ScaleSpec Constant(float ts) => new ScaleSpec { TimeScale = ts };
        public static ScaleSpec FromStat(StatSchemaObject s, float decoy) => new ScaleSpec { TimeScale = decoy, Stat = s };
        public static ScaleSpec World(float ts, float ease) => new ScaleSpec { TimeScale = ts, IsWorld = true, Ease = ease };
        public static ScaleSpec World(float ts, float ease, double start, double dur) =>
            new ScaleSpec { TimeScale = ts, IsWorld = true, Ease = ease, Start = start, Duration = dur, HasWindow = true };
    }

    private delegate void ScaleTrackBuilder(TimelineAsset timeline, CellWire wire, ScaleSpec spec);

    private static void BuildMoverCell(string cell, float x, int row, Color color, ClockMode clock,
        System.Action<PositionTrack> extraFill,
        ScaleTrackBuilder scaleBuilder, bool hasScale, ScaleSpec spec, string statBindActorName,
        string label, string usage)
    {
        var z = row * RowStep;
        var pos = new Vector3(x, ActorY, z);
        var actorName = cell + "_Actor";

        var actor = MakeActor(actorName, pos, color);
        if (spec.Stat != null || statBindActorName != null)
        {
            AddStatTo(actor, spec.Stat ?? slowMo);
        }

        var timeline = NewTimeline(TimelineFolder + "/" + cell + ".playable");
        var posTrack = timeline.CreateTrack<PositionTrack>(null, "Move");
        posTrack.ResetPositionOnDeactivate = true;
        FillTravel(posTrack, pos);
        extraFill(posTrack);

        var dirName = cell + "_Director";
        var wire = new CellWire { DirectorName = dirName };

        if (hasScale && scaleBuilder != null)
        {
            scaleBuilder(timeline, wire, spec);
        }

        FixDuration(timeline);
        Dirty(timeline);
        foreach (var tr in timeline.GetOutputTracks()) Dirty(tr);
        AssetDatabase.SaveAssets();

        MakeDirector(dirName, clock);

        wire.TimelinePath = AssetDatabase.GetAssetPath(timeline);
        if (statBindActorName != null) wire.StatBindActorName = statBindActorName;
        Wires.Add(wire);
        Captions.Add(new CaptionData { Title = label, Usage = usage, CellPos = new Vector3(x, 3.8f, z), Color = color });
    }

    private static void BuildSpeedFromStatCell(string cell, float x, int row, Color color,
        StatSchemaObject stat, float min, float max, float def, string label, string usage)
    {
        var z = row * RowStep;
        var pos = new Vector3(x, ActorY, z);
        var actorName = cell + "_Actor";

        var actor = MakeActor(actorName, pos, color);
        AddStatTo(actor, stat);

        var timeline = NewTimeline(TimelineFolder + "/" + cell + ".playable");
        var posTrack = timeline.CreateTrack<PositionTrack>(null, "Move");
        posTrack.ResetPositionOnDeactivate = true;
        FillTravel(posTrack, pos);
        FixDuration(timeline);
        Dirty(timeline);
        foreach (var tr in timeline.GetOutputTracks()) Dirty(tr);
        AssetDatabase.SaveAssets();

        var dirName = cell + "_Director";
        var dirGo = MakeDirector(dirName, ClockMode.Game).gameObject;
        var fromStat = dirGo.AddComponent<TimelineSpeedFromStatAuthoring>();
        fromStat.ReadRootFrom = TargetSlot.Self;
        fromStat.LinkKey = 0;
        fromStat.Fallback = TargetSlot.Self;
        fromStat.Stat = stat;
        fromStat.Min = min;
        fromStat.Max = max;
        fromStat.Default = def;
        EditorUtility.SetDirty(fromStat);

        // The component reads the stat off the SAME entity as the director; the
        // mover and director are separate, so seed the stat on the director GO too.
        AddStatTo(dirGo, stat);

        Wires.Add(new CellWire { DirectorName = dirName, TimelinePath = AssetDatabase.GetAssetPath(timeline) });
        Captions.Add(new CaptionData { Title = label, Usage = usage, CellPos = new Vector3(x, 3.8f, z), Color = color });
    }

    private static void AddTimelineScaleTrack(TimelineAsset timeline, CellWire wire, ScaleSpec spec)
    {
        var track = timeline.CreateTrack<TimelineTimeScaleTrack>(null, "TimeScale");
        var c = AddClip<TimelineTimeScaleClip>(track, 0.0, LoopLen(timeline), spec.Stat != null ? "stat-driven" : ("x" + spec.TimeScale.ToString("0.##")));
        var a = (TimelineTimeScaleClip)c.asset;
        a.timeScale = spec.TimeScale;
        a.stat = spec.Stat;
        Dirty(c.asset);
        wire.TimeScaleTrackName = "TimeScale";
        if (spec.Stat != null) wire.StatBindActorName = wire.DirectorName.Replace("_Director", "_Actor");
    }

    private static void AddWorldScaleTrack(TimelineAsset timeline, CellWire wire, ScaleSpec spec)
    {
        var track = timeline.CreateTrack<WorldTimeScaleTrack>(null, "WorldScale");
        var start = spec.HasWindow ? spec.Start : 0.0;
        var dur = spec.HasWindow ? spec.Duration : LoopLen(timeline);
        var c = AddClip<WorldTimeScaleClip>(track, start, dur, "world x" + spec.TimeScale.ToString("0.##"));
        var a = (WorldTimeScaleClip)c.asset;
        a.timeScale = spec.TimeScale;
        if (spec.Ease > 0f)
        {
            c.easeInDuration = spec.Ease;
            c.easeOutDuration = spec.Ease;
        }
        Dirty(c.asset);
    }

    private static void AddWorldPlusTimelineScale(TimelineAsset timeline, CellWire wire, ScaleSpec spec)
    {
        var len = LoopLen(timeline);

        var worldTrack = timeline.CreateTrack<WorldTimeScaleTrack>(null, "WorldScale");
        var wc = AddClip<WorldTimeScaleClip>(worldTrack, 1.0, 2.2, "world x0.5");
        ((WorldTimeScaleClip)wc.asset).timeScale = 0.5f;
        wc.easeInDuration = 0.3;
        wc.easeOutDuration = 0.3;
        Dirty(wc.asset);

        var tlTrack = timeline.CreateTrack<TimelineTimeScaleTrack>(null, "TimeScale");
        var tc = AddClip<TimelineTimeScaleClip>(tlTrack, 0.0, len, "timeline x0.5");
        var ta = (TimelineTimeScaleClip)tc.asset;
        ta.timeScale = 0.5f;
        ta.stat = null;
        Dirty(tc.asset);
        wire.TimeScaleTrackName = "TimeScale";
    }

    private static void FillTravel(PositionTrack t, Vector3 home)
    {
        var a = AddWorldPos(t, 0.0, 1.4, "to +X", home + new Vector3(TravelHalf, 0f, 0f));
        var b = AddWorldPos(t, 1.4, 1.4, "to -X", home + new Vector3(-TravelHalf, 0f, 0f));
        var c = AddWorldPos(t, 2.8, 1.4, "home", home);
        a.blendInDuration = 0.35; b.blendInDuration = 0.35; c.blendInDuration = 0.35;
    }

    private static TimelineClip AddWorldPos(PositionTrack t, double start, double dur, string name, Vector3 world)
    {
        var c = AddClip<PositionClip>(t, start, dur, name);
        var a = (PositionClip)c.asset;
        a.Type = PositionType.World;
        a.Position = world;
        Dirty(c.asset);
        return c;
    }

    private static double LoopLen(TimelineAsset timeline)
    {
        var end = 0.0;
        foreach (var track in timeline.GetOutputTracks())
            foreach (var clip in track.GetClips())
            {
                var e = clip.start + clip.duration;
                if (e > end) end = e;
            }

        return end > 0 ? end : 4.2;
    }

    // ============================================================
    //  actors / stats
    // ============================================================

    private static GameObject MakeActor(string name, Vector3 pos, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<MeshRenderer>().sharedMaterial = MakeMaterial(name, color);
        SceneManager.MoveGameObjectToScene(go, activeSub);
        return go;
    }

    private static void AddStatTo(GameObject go, StatSchemaObject stat)
    {
        if (go.GetComponent<StatAuthoring>() != null) return;
        var stats = go.AddComponent<StatAuthoring>();
        stats.AddStats = true;
        stats.StatsCanBeModified = true;
        stats.AddIntrinsics = false;
        stats.StatDefaults = new[]
        {
            new StatModifierAuthoring { Stat = stat, ModifyType = StatAuthoringType.Added, Value = 50f },
        };
    }

    // ============================================================
    //  wire / director plumbing
    // ============================================================

    private static void WireCell(CellWire w)
    {
        var director = GameObject.Find(w.DirectorName).GetComponent<PlayableDirector>();
        var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>(w.TimelinePath);
        director.playableAsset = timeline;

        var actorName = w.DirectorName.Replace("_Director", "_Actor");
        var actor = GameObject.Find(actorName);

        foreach (var track in timeline.GetOutputTracks())
        {
            if (track.name == "Move" && actor != null)
            {
                director.SetGenericBinding(track, actor.transform);
            }
            else if (track.name == w.TimeScaleTrackName && w.StatBindActorName != null)
            {
                var statActor = GameObject.Find(w.StatBindActorName);
                if (statActor != null)
                {
                    var sa = statActor.GetComponent<StatAuthoring>();
                    if (sa != null) director.SetGenericBinding(track, sa);
                }
            }
        }

        EditorUtility.SetDirty(director);
    }

    private static PlayableDirector MakeDirector(string name, ClockMode clock)
    {
        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, activeSub);
        var director = go.AddComponent<PlayableDirector>();
        director.playOnAwake = true;
        director.extrapolationMode = DirectorWrapMode.Loop;
        director.timeUpdateMode = clock == ClockMode.Unscaled
            ? DirectorUpdateMode.DSPClock
            : DirectorUpdateMode.GameTime;
        var begin = go.AddComponent<TimelineBeginAuthoring>();
        begin.Mode = TimelineBeginMode.OnLoad;
        begin.DelaySeconds = 0f;
        return director;
    }

    private static TimelineAsset NewTimeline(string path)
    {
        var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        AssetDatabase.CreateAsset(timeline, path);
        return timeline;
    }

    private static TimelineClip AddClip<T>(TrackAsset track, double start, double duration, string name) where T : PlayableAsset
    {
        var clip = track.CreateClip<T>();
        clip.start = start;
        clip.duration = duration;
        clip.displayName = name;
        return clip;
    }

    private static void FixDuration(TimelineAsset timeline)
    {
        var end = 0.0;
        foreach (var track in timeline.GetOutputTracks())
            foreach (var clip in track.GetClips())
            {
                var clipEnd = clip.start + clip.duration;
                if (clipEnd > end) end = clipEnd;
            }

        timeline.durationMode = TimelineAsset.DurationMode.FixedLength;
        timeline.fixedDuration = end;
    }

    // ============================================================
    //  prerequisite singleton + scenery
    // ============================================================

    private static void BuildRequiredInSubScene()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RequiredInSubScenePath);
        if (prefab == null)
        {
            Debug.LogError("TimeShowcase: '" + RequiredInSubScenePath + "' missing; WorldTimeScale singleton will be absent and every world clip is silently inert.");
            return;
        }

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = "Required In Subscene";
        SceneManager.MoveGameObjectToScene(go, activeSub);
    }

    private static void BuildPads()
    {
        float[] xs = { PerTimelineX, WorldX, StatX };
        string[] names = { "PerTimeline", "World", "Stat" };
        var zCenter = RowStep * 2.0f;
        for (var i = 0; i < xs.Length; i++)
            MakePad(names[i] + "_Pad", new Vector3(xs[i], 0.04f, zCenter), new Vector3(12.0f, 0.1f, RowStep * 5f + 2f));

        // Travel rails under each cell so the eye reads displacement against a fixed scale.
        BuildRails(PerTimelineX, 5);
        BuildRails(WorldX, 4);
        BuildRails(StatX, 3);
    }

    private static void BuildRails(float x, int rows)
    {
        for (var r = 0; r < rows; r++)
        {
            var z = r * RowStep;
            MakePad("Rail_" + x + "_" + r, new Vector3(x, 0.12f, z), new Vector3(TravelHalf * 2f + 1.4f, 0.06f, 0.25f), RailColor);
        }
    }

    private static GameObject MakePad(string name, Vector3 pos, Vector3 size)
    {
        return MakePad(name, pos, size, PadColor);
    }

    private static GameObject MakePad(string name, Vector3 pos, Vector3 size, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = size;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<MeshRenderer>().sharedMaterial = MakeMaterial(name, color);
        SceneManager.MoveGameObjectToScene(go, activeSub);
        return go;
    }

    private static Material MakeMaterial(string name, Color color)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var mat = new Material(shader) { name = name + "_Mat" };
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        return mat;
    }

    private static void BuildParent()
    {
        FrameCamera();
        RenderSettings.fog = false;

        MakeBanner("Title_Banner", new Vector3(0f, 15.4f, 0f), new Vector3(56f, 3.4f, 0.1f));
        MakeWorldLabel("Title", "TIME TIMELINE GRID — PER-TIMELINE · WORLD · SPEED-FROM-STAT", new Vector3(0f, 15.8f, -0.4f), 56f, Color.white, 4.4f, TextAlignmentOptions.Center);
        MakeWorldLabel("Subtitle", "slow-mo / bullet-time / freeze-frame — each cell's mover speed vs a 1x control   ·   BovineLabs.Timeline.Time", new Vector3(0f, 14.5f, -0.4f), 56f, new Color(0.85f, 0.9f, 1f), 1.9f, TextAlignmentOptions.Center);

        MakeColumnHeader("PT_Header", "TIMELINE TIME SCALE", PerTimelineX, PerTimelineColor);
        MakeColumnHeader("W_Header", "WORLD TIME SCALE", WorldX, WorldColor);
        MakeColumnHeader("ST_Header", "SPEED FROM STAT", StatX, StatColor);

        foreach (var cap in Captions)
            MakeCaption(cap.Title, cap.Usage, cap.CellPos, cap.Color);

        MakeBanner("Usage_Banner", new Vector3(0f, 0.7f, -8.5f), new Vector3(60f, 2.4f, 0.1f));
        MakeWorldLabel("Usage",
            "Each cube travels +X<->-X on its own PlayableDirector. PER-TIMELINE clips scale ONLY that director's clock (0.5x / 2x / stat / -1x reverse vs a grey 1x control). WORLD clips push UnityEngine.Time.timeScale GLOBALLY (bullet-time slows even clipless GameTime movers; freeze=0 is SAFE only on UnscaledGameTime; World x Timeline COMPOUND to 0.25x). SPEED-FROM-STAT scales playback from a clamped stat with a 0.05 floor (never freezes). PLAY MODE required.",
            new Vector3(0f, 0.7f, -8.8f), 58f, new Color(0.96f, 0.97f, 1f), 1.35f, TextAlignmentOptions.Center);

        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(SubPath);
        if (sceneAsset == null)
        {
            Debug.LogError("TimeShowcase: sub-scene asset missing at " + SubPath);
            return;
        }

        var subSceneGo = new GameObject("Showcase SubScene");
        var subScene = subSceneGo.AddComponent<SubScene>();
        subScene.SceneAsset = sceneAsset;
        subScene.AutoLoadScene = true;
        EditorUtility.SetDirty(subScene);
    }

    private static void MakeColumnHeader(string name, string text, float x, Color color)
    {
        var pos = new Vector3(x, 4.8f, -4.8f);
        MakeBanner(name + "_Banner", pos + new Vector3(0f, 0f, 0.08f), new Vector3(11.0f, 1.4f, 0.1f));
        MakeWorldLabel(name, "<b>" + text + "</b>", pos, 10.8f, color, 2.6f, TextAlignmentOptions.Center);
    }

    private static float CaptionY(float z)
    {
        return 4.6f + z * 0.13f;
    }

    private static void MakeCaption(string title, string usage, Vector3 cellPos, Color color)
    {
        var z = cellPos.z;
        var y = CaptionY(z);
        MakeBanner("CapBanner_" + title + "_" + z, new Vector3(cellPos.x, y, z + 0.06f), new Vector3(10.6f, 2.2f, 0.05f));
        MakeWorldLabel("Cap_" + title + "_" + z, "<b>" + title + "</b>", new Vector3(cellPos.x, y + 0.55f, z), 10.6f, color, 2.1f, TextAlignmentOptions.Center);
        MakeWorldLabel("Use_" + title + "_" + z, usage, new Vector3(cellPos.x, y - 0.45f, z), 10.6f, new Color(0.95f, 0.96f, 1f), 1.05f, TextAlignmentOptions.Center);
    }

    private static void FrameCamera()
    {
        var required = GameObject.Find("Required In Scene");
        if (required == null) return;
        var camTransform = required.transform.Find("Main Camera");
        if (camTransform == null) return;
        camTransform.position = CameraPos;
        camTransform.rotation = Quaternion.Euler(20f, 0f, 0f);
        var cam = camTransform.GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = 60f;
            cam.farClipPlane = 400f;
            EditorUtility.SetDirty(cam);
        }

        EditorUtility.SetDirty(camTransform);
    }

    private static void MakeBanner(string name, Vector3 pos, Vector3 size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.position = pos;
        go.transform.rotation = Quaternion.LookRotation(pos - CameraPos, Vector3.up);
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = MakeMaterial(name, BannerColor);
    }

    private static void MakeWorldLabel(string name, string text, Vector3 pos, float width, Color color, float fontSize, TextAlignmentOptions alignment)
    {
        var holder = new GameObject(name);
        holder.transform.position = pos;
        holder.transform.rotation = Quaternion.LookRotation(pos - CameraPos, Vector3.up);

        var go = new GameObject("Text");
        go.transform.SetParent(holder.transform, false);
        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.rectTransform.sizeDelta = new Vector2(width, 4f);
        tmp.rectTransform.localPosition = Vector3.zero;
        tmp.fontStyle = FontStyles.Bold;
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Samples"))
            AssetDatabase.CreateFolder("Assets", "Samples");
        if (!AssetDatabase.IsValidFolder(SampleFolder))
            AssetDatabase.CreateFolder("Assets/Samples", "TimeShowcase");
        if (!AssetDatabase.IsValidFolder(TimelineFolder))
            AssetDatabase.CreateFolder(SampleFolder, "Timelines");
    }

    private static void ResetAssets()
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(TimelineFolder) != null)
            foreach (var guid in AssetDatabase.FindAssets("t:TimelineAsset", new[] { TimelineFolder }))
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

        foreach (var p in new[] { ParentPath, SubPath })
            if (AssetDatabase.LoadAssetAtPath<Object>(p) != null)
                AssetDatabase.DeleteAsset(p);
    }

    private static void Dirty(params Object[] objects)
    {
        foreach (var o in objects)
            EditorUtility.SetDirty(o);
    }
}
