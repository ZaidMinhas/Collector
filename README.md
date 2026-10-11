# Collector

## Pause menu

Press **Escape** to open the existing OptionSetting screen over the current scene. Gameplay and player actions pause while the Master, Music, and SFX sliders remain usable. Press **Escape** again or click **Resume** to return without reloading the level. The menu is created automatically, including when starting a level directly in the editor; no prefab setup is needed. OptionSetting must remain enabled in the build scene list.

Opening options normally from MainMenu keeps its existing Back behavior. During a tutorial intro, closing the pause menu restores the intro's paused state; the blink and fade resume where they stopped. Music continues while paused so its volume can be adjusted.

## Adding sound effects

1. Ensure the starting scene contains `Assets/Prefabs/AudioManager.prefab` and an active AudioListener (usually on the camera). The manager persists across scene changes.
2. Select a GameObject and add **Sound Player** from **Add Component > Audio > Sound Player**.
3. Drag an AudioClip into **Clip** and optionally adjust **Volume** (0–1). Clips can live anywhere under Assets.
4. For a UI button, add an **On Click** listener, drag in the GameObject with SoundPlayer, and select **SoundPlayer > Play()**.
5. For gameplay, call `GetComponent<Manager.SoundPlayer>().Play()` at the desired event. Animation events can select `Play` when SoundPlayer is on the Animator's GameObject.

SoundPlayer uses the manager's shared SFX source, so effects obey the SFX and Master sliders without configuring an AudioSource on every object. Repeated calls can overlap. Its Volume scales each playback independently without changing other effects' volume. Missing clips or manager/source references produce a warning.

This is a convenience component built on Unity audio. Use a dedicated AudioSource for positional 3D sounds or looping effects. Use `LevelMusic` for scene music. SoundPlayer does not automatically wire button clicks or slider changes; connect `Play()` to the event you want.

## Defender shooting sounds

Successfully abducting the target plays `Assets/Resources/Sfx/Yay You Won!.ogg` once before advancing to the next level. It stops the Kidnap loop and continues through scene loading via the persistent AudioManager, following the SFX and Master sliders. Failed abductions and the debug level-skip key do not trigger this victory sound. Replace this single Resources asset to update it.

Player death plays `Assets/Resources/Sfx/Oh No You Lost.ogg` once as the circle reload transition starts. The persistent AudioManager lets it finish across the scene reload, following the SFX and Master sliders. The portal loop stops on death, and repeated death notifications do not replay the sound or restart the transition. Replace this single Resources asset to update it.

The Space-bar abduction portal loops `Assets/Resources/Sfx/Kidnap.ogg` while Space is held after successful activation. Releasing Space or disabling the player stops the loop and beam. Failed actions, shield-blocked abduction, and missing beams do not start the sound. A dedicated source follows the SFX and Master sliders and allows the loop to stop independently of other effects.

Enemy defeats play `Assets/Resources/Sfx/Defeat.ogg` when positive damage takes a living enemy's health to zero or below. The sound uses the persistent AudioManager, so destroying the enemy does not cut it off, and follows the SFX and Master sliders. Further damage to an already dead enemy does not replay it. Replace this single Resources asset to update the sound.

Enemy damage plays `Assets/Resources/Sfx/Hit an Ennemy.ogg` through the shared SFX source. Only living objects with `DestroyableEnemy` trigger it for positive damage, including lethal hits; player damage and healing do not. Each enemy limits feedback to once every 0.08 seconds to avoid stacked laser sounds. This asset was moved with its existing `.meta` file from Audio/SFX, so there is only one copy; replace the Resources file to update it.

The main player's UFO plays `Assets/Resources/Sfx/Pew(HigherPitched).ogg` for each launched projectile. Its higher pitch is baked into the audio asset; playback uses the shared SFX source at 0.4 volume and follows the SFX and Master sliders. Shield-blocked attacks and failed actions do not trigger the sound.

Soldiers and Turrets play `Assets/Resources/Sfx/Pew.ogg` each time they launch a projectile, including shots within a burst. This also applies to existing scene instances without adding components. `DefenderShotAudio` varies pitch from 0.9 to 1.1 and volume from 0.35 to 0.45 per shot. Adjust those ranges in that script to tune the effect. Attack timing, damage, and aiming are unchanged.

The AudioManager reuses up to 24 independent SFX voices for varied shots, all routed through its SFX source's mixer group. When all voices are busy, extra sounds are skipped while projectiles still fire. Keep an AudioManager in the starting scene so shooting sounds follow the SFX and Master sliders.
