# BovineLabs Timeline Time

Time Scale Timeline tracks for DOTS projects built on BovineLabs Timeline Core.

## Features

- **Time Scale Track**: Control `UnityEngine.Time.timeScale` from Timeline with smooth blending support.
- Support for Freeze Frame (0), Slow-Mo (0.1), Normal (1), and Fast Forward (>1).
- Smooth blend in/out between clips and back to normal time scale.
- Multiple active targets: takes the lowest scale for the most dramatic effect.

## Installation

Add to your project via Package Manager or `manifest.json`.

## Usage

1. Add a `TimeScaleTargetAuthoring` component to a GameObject in your scene/subscene.
2. In Timeline, add a **BovineLabs > Timeline > Time > Time Scale** track.
3. Bind the track to your `TimeScaleTargetAuthoring` GameObject.
4. Add Time Scale clips and set the desired `timeScale` value.
