namespace Somniloquy {
    using MoonSharp.Interpreter;
    using System;

    public static class ScriptManager {
        public static Script GlobalEnvironment; // Shared Lua state for global functions.

        public static void Initialize() {
            GlobalEnvironment = new Script(CoreModules.Preset_SoftSandbox);
        }

        public static void Register(string name, Delegate function) {
            GlobalEnvironment.Globals[name] = function;
        }

        public static DynValue Execute(string script) {
            try {
                return GlobalEnvironment.DoString(script);
            } catch (ScriptRuntimeException ex) {
                DebugInfo.AddTempLine(() => $"Lua Error: {ex.Message}", 5);
                return null;
            }
        }

        public static Script CreateNewScriptEnvironment() {
            var script = new Script(CoreModules.Preset_SoftSandbox);
            return script;
        }
    }
}