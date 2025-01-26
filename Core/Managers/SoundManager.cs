namespace Somniloquy
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.IO;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    
    using FMOD;
    using System.Linq;

    public static class SoundManager {
        public static System FMODSystem;
        public static ChannelGroup ChannelGroup;
        public static DSP PitchShiftDSP;
        public static DSP BassEnhancerDSP;
        public static DSP ReverbDSP;

        public static Dictionary<string, Channel> Channels { get; set; } = new();
        public static Dictionary<string, Sound> Sounds { get; set; } = new();

        public static float CenterFrequency = 150f;
        public static float Pitch = 1f;
        public static string CurrentMusic;

        public static List<Keybind> Keybinds = new(); 

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetDllDirectory(string path);

        public static void Initialize(string soundsDirectory) {
            string fmodPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExternalAssemblies", "FMod");
            SetDllDirectory(fmodPath);

            Factory.System_Create(out FMODSystem);
            FMODSystem.init(128, INITFLAGS.NORMAL, IntPtr.Zero);
            FMODSystem.createChannelGroup(null, out ChannelGroup);

            DirectoryInfo directory = new(soundsDirectory);
            foreach (var path in directory.GetFiles("*.wav")) {
                AddSound(path);
            }

            FMODSystem.createDSPByType(DSP_TYPE.PITCHSHIFT, out PitchShiftDSP);

            FMODSystem.createDSPByType(DSP_TYPE.PARAMEQ, out BassEnhancerDSP);
            BassEnhancerDSP.setParameterFloat((int)DSP_PARAMEQ.CENTER, CenterFrequency);
            BassEnhancerDSP.setParameterFloat((int)DSP_PARAMEQ.GAIN, 12.0f);

            ChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, BassEnhancerDSP);

            var reverbProperties = PRESET.FOREST();
            FMODSystem.setReverbProperties(0, ref reverbProperties);

            CurrentMusic = "My Song 3";

            Keybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.M, Keys.Left}, _ => { if (Pitch > 0.1f) Pitch -= 0.001f; SetPitch(CurrentMusic, Pitch); }, TriggerOnce.False, true ));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.M, Keys.Right}, _ => { if (Pitch < 2f) Pitch += 0.001f; SetPitch(CurrentMusic, Pitch); }, TriggerOnce.False, true ));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.M, Keys.Up}, _ => { CenterFrequency *= 1.01f; }, TriggerOnce.False, true ));
            Keybinds.Add(InputManager.RegisterKeybind(new object[] {Keys.M, Keys.Down}, _ => { CenterFrequency /= 1.01f; }, TriggerOnce.False, true ));
        }

        public static string AddSound(FileInfo path) {
            var name = path.Name[..^4];
            if (Sounds.ContainsKey(name)) return name;
            FMODSystem.createSound(path.FullName, MODE.DEFAULT, out Sound sound);
            Sounds.Add(name, sound);
            return name;
        }

        public static void StartLoop(string name, float fade_seconds = 0f) {
            if (Sounds.ContainsKey(name)) {
                if (!Channels.ContainsKey(name)) {
                    StopLoop(CurrentMusic);
                    FMODSystem.playSound(Sounds[name], ChannelGroup, false, out Channel channel);
                    channel.setMode(MODE.LOOP_NORMAL);
                    channel.setLoopCount(-1);
                    Channels.Add(name, channel);
                    CurrentMusic = name;
                    SetPitch(CurrentMusic, Pitch);
                }
            } else {
                DebugInfo.AddTempLine(() => $"No such sound named {name}!", 5);
            }
        }

        public static void StopLoop(string name, float fade_seconds = 0f) {
            if (Sounds.ContainsKey(name)) {
                if (Channels.ContainsKey(name)) {
                    Channels[name].stop();
                    Channels.Remove(name);
                }
            } else {
                DebugInfo.AddTempLine(() => $"No such sound named {name}!", 5);
            }
        }

        public static void SetPitch(string name, float pitch) {
            if (Channels.ContainsKey(name)) {
                if (pitch < 0.1f) pitch = 0.1f;
                Channels[name].removeDSP(PitchShiftDSP);
                Channels[name].setPitch(pitch);
            } else {
                DebugInfo.AddTempLine(() => $"No such sound named {name}!", 5);
            }
        }

        public static void Update() {
            BassEnhancerDSP.setParameterFloat((int)DSP_PARAMEQ.CENTER, CenterFrequency);
            FMODSystem.update();
        }

        public static void Dispose() {
            foreach (var sound in Sounds.Values) {
                sound.release();
            }

            FMODSystem.release();
            FMODSystem.close();
        }
    }
}