# BovineLabs Timeline Time

BovineLabs Timeline Time adds time scale Timeline tracks for DOTS projects built on top of BovineLabs Timeline Core.

It currently provides:
- Time Scale track and clip authoring for controlling `UnityEngine.Time.timeScale` over time (Freeze Frame, Hit Stop, Slow-Mo, Fast Forward)

## Package name

`com.bovinelabs.timeline.time`

## Dependencies

This package depends on:
- `com.bovinelabs.core`
- `com.bovinelabs.timeline`
- `com.bovinelabs.timeline.core`
- `com.unity.burst`
- `com.unity.collections`
- `com.unity.entities`
- `com.unity.mathematics`
- `com.unity.timeline`

See `package.json` for exact versions.

## Tracks

### Time Scale
- Track: `BovineLabs/Timeline/Time/Time Scale`
- Binding: `TimeScaleTargetAuthoring`
- Purpose: control global time scale during timeline playback with smooth blending
- Clip fields:
  - `timeScale` (0 = Freeze Frame, 0.1 = Slow Mo, 1 = Normal, >1 = Fast Forward)

## Usage

1. Add a `TimeScaleTargetAuthoring` to a GameObject in your scene/subscene.
2. In Timeline, add a **BovineLabs -> Timeline -> Time -> Time Scale** track.
3. Bind the track to your `TimeScaleTargetAuthoring` GameObject.
4. Add Time Scale clips and tune the `timeScale` value.
