using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.DualShock;

namespace Hexbound.Editor
{
    [InitializeOnLoad]
    public static class InputChecks
    {
        static int count;
        static void Check(bool pass,string description) { if(!pass) throw new Exception("INPUT FAIL: "+description); count++; }
        static InputChecks() { EditorApplication.update+=RunWhenPlaying; }
        public static void ValidateBatch() {
            SessionState.SetBool("Hexbound.InputChecks",true);
            EditorApplication.isPlaying=true;
        }
        static void RunWhenPlaying() {
            if(!SessionState.GetBool("Hexbound.InputChecks",false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            SessionState.SetBool("Hexbound.InputChecks",false);
            try { Run(); EditorApplication.Exit(0); }
            catch(Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        }
        static void Run()
        {
            count=0;
            InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard=InputSystem.AddDevice<Keyboard>();
            var a=InputSystem.AddDevice<Gamepad>(); var b=InputSystem.AddDevice<Gamepad>();
            using(var input=new AcademyInputManager()) {
                InputSystem.QueueStateEvent(a,new GamepadState { leftStick=new Vector2(1,0) }.WithButton(GamepadButton.West));
                InputSystem.QueueStateEvent(b,new GamepadState { leftStick=new Vector2(-1,0) }.WithButton(GamepadButton.North));
                InputSystem.Update();input.Poll(0);

                Check(input.Read(0).move>.9f && input.Read(0).punch,"first pad drives only player one");
                Check(input.Read(1).move<-.9f && input.Read(1).special && !input.Read(1).punch,"second pad independently casts");
                Check(input.Prompt(0,"Punch")=="X","Xbox positional prompts");
                Check(input.Navigate(0).x==1 && input.Navigate(1).x==-1,"independent menu directions");
                InputSystem.QueueStateEvent(a,new GamepadState { leftStick=new Vector2(.05f,0) });
                InputSystem.QueueStateEvent(b,new GamepadState());InputSystem.Update();input.Poll(.1f);
                Check(input.Read(0).move==0,"stick deadzone prevents drift");
                Check(!input.Read(0).punch,"attack is pressed once, not held/repeated");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.RightArrow,Key.J));InputSystem.Update();input.Poll(.2f);
                Check(input.Read(1).move>.9f && input.Read(1).punch,"player two keyboard bindings");
                Check(input.Read(0).move==0 && !input.Read(0).punch,"keyboard bindings isolated");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                InputSystem.QueueStateEvent(a,new GamepadState().WithButton(GamepadButton.South).WithButton(GamepadButton.LeftShoulder).WithButton(GamepadButton.RightShoulder).WithButton(GamepadButton.Start));
                InputSystem.Update();input.Poll(.3f);
                Check(input.Read(0).jump && input.Read(0).block && input.Read(0).power,"jump guard power mapped");
                Check(input.Press(0,"Confirm") && input.Any("Pause"),"controller confirms and pauses menus");
                InputSystem.RemoveDevice(a);input.Poll(.4f);
                Check(input.ControllerDisconnected,"disconnect is reported for auto pause");
                InputSystem.QueueStateEvent(b,new GamepadState().WithButton(GamepadButton.East));InputSystem.Update();input.Poll(.5f);
                Check(input.Read(1).push && !input.Read(0).push,"remaining pad keeps player slot after disconnect");
                Check(input.Press(1,"Cancel"),"east face button navigates back");
                var replacement=InputSystem.AddDevice<DualShockGamepad>();
                input.Poll(.6f);InputSystem.QueueStateEvent(replacement,new GamepadState().WithButton(GamepadButton.West));InputSystem.Update();input.Poll(.7f);
                Check(input.Read(0).punch && !input.Read(1).punch,"replacement pad fills vacant slot");
                Check(input.Prompt(0,"Punch")=="SQUARE" && input.Prompt(0,"Confirm")=="CROSS","PlayStation positional prompts");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F));InputSystem.Update();input.Poll(.8f);
                Check(input.Prompt(0,"Punch")=="F" && input.Prompt(1,"Push")=="B","keyboard prompt switches only its own player");
                InputSystem.RemoveDevice(replacement);
            }
            InputSystem.RemoveDevice(b);InputSystem.RemoveDevice(keyboard);
            Debug.Log("HEXBOUND INPUT CHECKS PASSED: "+count+" virtual-device assertions");
        }
    }
}
