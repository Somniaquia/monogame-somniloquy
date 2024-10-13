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
        public static System System;
        public static ChannelGroup ChannelGroup;
        public static DSP PitchShiftDSP;
        public static DSP BassEnhancerDSP;
        public static DSP ReverbDSP;

        public static Dictionary<string, Channel> Channels { get; set; } = new();
        public static Dictionary<string, Sound> Sounds { get; set; } = new();

        public static float CenterFrequency = 150f;
        public static float Pitch = 1f;
        public static string MusicName;


        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetDllDirectory(string path);

        public static void Initialize(string soundsDirectory) {
            string fmodPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExternalAssemblies", "FMod");
            SetDllDirectory(fmodPath);

            Factory.System_Create(out System);
            System.init(128, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
            System.createChannelGroup(null, out ChannelGroup);

            DirectoryInfo directory = new(soundsDirectory);
            foreach (var path in directory.GetFiles("*.wav")) {
                System.createSound(path.FullName, FMOD.MODE.DEFAULT, out Sound sound);
                Sounds.Add(path.Name[..^4], sound);
            }

            System.createDSPByType(DSP_TYPE.PITCHSHIFT, out PitchShiftDSP);

            System.createDSPByType(FMOD.DSP_TYPE.PARAMEQ, out BassEnhancerDSP);
            BassEnhancerDSP.setParameterFloat((int)FMOD.DSP_PARAMEQ.CENTER, CenterFrequency);
            BassEnhancerDSP.setParameterFloat((int)FMOD.DSP_PARAMEQ.GAIN, 12.0f);

            ChannelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, BassEnhancerDSP);

            var reverbProperties = PRESET.FOREST();
            System.setReverbProperties(0, ref reverbProperties);

            MusicName = "My Song 3";
            // StartLoop(MusicName, 5);
            // SetPitch(MusicName, Pitch);
        }

        public static void StartLoop(string name, float fade_seconds = 0f) {
            if (Sounds.ContainsKey(name)) {
                if (!Channels.ContainsKey(name)) {
                    System.playSound(Sounds[name], ChannelGroup, false, out Channel channel);
                    channel.setMode(MODE.LOOP_NORMAL);
                    channel.setLoopCount(-1);
                    Channels.Add(name, channel);
                }
            } else {
                Console.WriteLine($"No such sound named {name}!");
            }
        }

        public static void StopLoop(string name, float fade_seconds = 0f) {
            if (Sounds.ContainsKey(name)) {
                if (Channels.ContainsKey(name)) {
                    Channels[name].stop();
                    Channels.Remove(name);
                }
            } else {
                Console.WriteLine($"No such sound named {name}!");
            }
        }

        public static void SetPitch(string name, float pitch) {
            if (Sounds.ContainsKey(name)) {
                if (pitch < 0.1f) pitch = 0.1f;
                Channels[name].removeDSP(PitchShiftDSP);
                Channels[name].setPitch(pitch);
            } else {
                Console.WriteLine($"No such sound named {name}!");
            }
        }

        public static void Update() {
            if (InputManager.IsKeyDown(Keys.Left)) {
                if (Pitch > 0.1f) Pitch -= 0.001f;
                SetPitch(MusicName, Pitch);
            } else if (InputManager.IsKeyDown(Keys.Right)) {
                if (Pitch < 2f) Pitch += 0.001f;
                SetPitch(MusicName, Pitch);
            } else if (InputManager.IsKeyDown(Keys.Up)) {
                CenterFrequency *= 1.01f;
            }
            else if (InputManager.IsKeyDown(Keys.Down)) {
                CenterFrequency /= 1.01f;
            }

            BassEnhancerDSP.setParameterFloat((int)FMOD.DSP_PARAMEQ.CENTER, CenterFrequency);
            System.update();
            if (InputManager.IsKeyPressed(Keys.Tab)) {
                StopLoop(MusicName);
                var rnd = new Random();
                MusicName = Sounds.ElementAt(rnd.Next(0, Sounds.Count)).Key;
                StartLoop(MusicName, 5);
                SetPitch(MusicName, Pitch);
            }
        }

        public static void Dispose() {
            foreach (var sound in Sounds.Values) {
                sound.release();
            }

            System.release();
            System.close();
        }
    }
}