using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace Hexbound
{
    // Per-player action maps share a keyboard but each owns one distinct controller.
    public sealed class AcademyInputManager : IDisposable
    {
        readonly InputActionAsset asset;
        readonly InputActionMap[] maps=new InputActionMap[2];
        readonly Gamepad[] pads=new Gamepad[2];
        readonly bool[] controllerUsed=new bool[2];
        readonly Vector2Int[] previous=new Vector2Int[2], navigation=new Vector2Int[2];
        readonly float[] repeatAt=new float[2];
        Keyboard keyboard;
        public bool ControllerDisconnected { get; private set; }
        public AcademyInputManager()
        {
            asset=UnityEngine.Object.Instantiate(Resources.Load<InputActionAsset>("Input/AcademyControls"));
            for(int p=0;p<2;p++) maps[p]=asset.FindActionMap("Player"+(p+1),true);
            RefreshDevices(); asset.Enable();
        }
        void RefreshDevices()
        {
            bool changed=keyboard!=Keyboard.current; keyboard=Keyboard.current;
            for(int p=0;p<2;p++) if(pads[p]!=null && !pads[p].added) {
                pads[p]=null; controllerUsed[p]=false; ControllerDisconnected=true; changed=true;
            }
            foreach(var pad in Gamepad.all) {
                if(pad==pads[0] || pad==pads[1]) continue;
                for(int p=0;p<2;p++) if(pads[p]==null) { pads[p]=pad; changed=true; break; }
            }
            for(int p=0;p<2;p++) if(changed || maps[p].devices==null) {
                var devices=new List<InputDevice>(); if(keyboard!=null) devices.Add(keyboard); if(pads[p]!=null) devices.Add(pads[p]);
                maps[p].devices=devices.ToArray();
            }
        }
        public void Poll(float now)
        {
            ControllerDisconnected=false; RefreshDevices();
            for(int p=0;p<2;p++) {
                var pad=pads[p];
                if(pad!=null && (pad.leftStick.ReadValue().sqrMagnitude>.1f || pad.dpad.ReadValue().sqrMagnitude>.1f || pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)) controllerUsed[p]=true;
                foreach(var action in maps[p].actions) if(action.activeControl?.device==keyboard && (action.WasPressedThisFrame() || action.name=="Move" && action.ReadValue<Vector2>().sqrMagnitude>.01f)) controllerUsed[p]=false;
                Vector2 move=maps[p]["Move"].ReadValue<Vector2>(),legacy=LegacyMove(p);
                if(legacy.sqrMagnitude>move.sqrMagnitude) move=legacy;
                if(legacy.sqrMagnitude>0 || LegacyDown(KeyFor(p,"Punch")) || LegacyDown(KeyFor(p,"Spell"))) controllerUsed[p]=false;
                var dir=new Vector2Int(Mathf.Abs(move.x)>.5f?(int)Mathf.Sign(move.x):0,Mathf.Abs(move.y)>.5f?(int)Mathf.Sign(move.y):0);
                navigation[p]=Vector2Int.zero;
                if(dir!=Vector2Int.zero && (dir!=previous[p] || now>=repeatAt[p])) { navigation[p]=dir; repeatAt[p]=now+(dir!=previous[p]?.35f:.16f); }
                previous[p]=dir;
            }
        }
        static KeyCode KeyFor(int p,string action)
        {
            switch(action) {
                case "Jump":return p==0?KeyCode.W:KeyCode.UpArrow;
                case "Guard":return p==0?KeyCode.S:KeyCode.DownArrow;
                case "Punch":case "Confirm":return p==0?KeyCode.F:KeyCode.J;
                case "Push":return p==0?KeyCode.G:KeyCode.K;
                case "Spell":return p==0?KeyCode.R:KeyCode.U;
                case "Power":return p==0?KeyCode.T:KeyCode.I;
                case "Cancel":case "Pause":return KeyCode.Escape;
                case "Help":return KeyCode.H;
            }
            return KeyCode.None;
        }
        static bool LegacyDown(KeyCode key) {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }
        static bool LegacyHeld(KeyCode key) {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key);
#else
            return false;
#endif
        }
        static Vector2 LegacyMove(int p) => new Vector2((LegacyHeld(p==0?KeyCode.D:KeyCode.RightArrow)?1:0)-(LegacyHeld(p==0?KeyCode.A:KeyCode.LeftArrow)?1:0),(LegacyHeld(p==0?KeyCode.W:KeyCode.UpArrow)?1:0)-(LegacyHeld(p==0?KeyCode.S:KeyCode.DownArrow)?1:0));
        public Vector2Int Navigate(int player) => navigation[player];
        public bool Press(int player,string action) => maps[player][action].WasPressedThisFrame()||LegacyDown(KeyFor(player,action))||(action=="Confirm" && LegacyDown(KeyCode.Return));
        public bool Any(string action) => Press(0,action)||Press(1,action);
        static bool KeyboardPressed() {
            if(Keyboard.current==null) return false;
            foreach(var key in Keyboard.current.allKeys) if(key!=null && key.wasPressedThisFrame) return true;
            return false;
        }
        public bool StartPressed {
            get {
#if ENABLE_LEGACY_INPUT_MANAGER
                if(Input.anyKeyDown || Input.GetMouseButtonDown(0)) return true;
#endif
                return Any("Confirm")||Any("Punch")||Any("Pause")||Any("Cancel")||Any("Spell")||Any("Power")||Any("Help")||Any("Jump")||Any("Guard")||KeyboardPressed()||(Mouse.current!=null && Mouse.current.leftButton.wasPressedThisFrame);
            }
        }
        public DuelInput Read(int p) {
            float move=maps[p]["Move"].ReadValue<Vector2>().x, legacy=LegacyMove(p).x;
            return new DuelInput {
                move=Mathf.Abs(legacy)>Mathf.Abs(move)?legacy:move,jump=Press(p,"Jump"),block=maps[p]["Guard"].IsPressed()||LegacyHeld(KeyFor(p,"Guard")),
                punch=Press(p,"Punch"),push=Press(p,"Push"),special=Press(p,"Spell"),power=Press(p,"Power")
            };
        }
        public string DeviceName(int p) => !controllerUsed[p]?"KEYBOARD":(pads[p] is DualShockGamepad?"PS4 / PLAYSTATION":"XBOX / GAMEPAD");
        public string Prompt(int p,string action)
        {
            if(controllerUsed[p] && pads[p]!=null) {
                bool ps=pads[p] is DualShockGamepad;
                switch(action) {
                    case "Move": return "STICK / D-PAD"; case "Jump": case "Confirm": return ps?"CROSS":"A";
                    case "Guard": return ps?"L1":"LB"; case "Punch": return ps?"SQUARE":"X";
                    case "Push": case "Cancel": return ps?"CIRCLE":"B"; case "Spell": return ps?"TRIANGLE":"Y";
                    case "Power": return ps?"R1":"RB"; case "Pause":return ps?"OPTIONS":"MENU";
                }
            }
            switch(action) {
                case "Move":return p==0?"A / D":"LEFT / RIGHT"; case "Jump":return p==0?"W":"UP";
                case "Guard":return p==0?"S":"DOWN"; case "Punch":case "Confirm":return p==0?"F":"J";
                case "Push":return p==0?"G":"K";case "Spell":return p==0?"R":"U";case "Power":return p==0?"T":"I";
                case "Pause":case "Cancel":return "ESC";
            }
            return action;
        }
        public void Dispose() { asset.Disable(); UnityEngine.Object.DestroyImmediate(asset); }
    }
}
