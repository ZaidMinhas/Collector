# Collector

## Adding sound effects

1. Ensure the starting scene contains `Assets/Prefabs/AudioManager.prefab` and an active AudioListener (usually on the camera). The manager persists across scene changes.
2. Select a GameObject and add **Sound Player** from **Add Component > Audio > Sound Player**.
3. Drag an AudioClip into **Clip** and optionally adjust **Volume** (0–1). Clips can live anywhere under Assets.
4. For a UI button, add an **On Click** listener, drag in the GameObject with SoundPlayer, and select **SoundPlayer > Play()**.
5. For gameplay, call `GetComponent<Manager.SoundPlayer>().Play()` at the desired event. Animation events can select `Play` when SoundPlayer is on the Animator's GameObject.

SoundPlayer uses the manager's shared SFX source, so effects obey the SFX and Master sliders without configuring an AudioSource on every object. Repeated calls can overlap. Its Volume scales each playback independently without changing other effects' volume. Missing clips or manager/source references produce a warning.

This is a convenience component built on Unity audio. Use a dedicated AudioSource for positional 3D sounds or looping effects. Use `LevelMusic` for scene music. SoundPlayer does not automatically wire button clicks or slider changes; connect `Play()` to the event you want.
