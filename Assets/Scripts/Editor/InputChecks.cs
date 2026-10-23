using System;
using System.Reflection;
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
                var game=UnityEngine.Object.FindFirstObjectByType<AcademyGame>();
                Check(game!=null,"runtime game exists for start regression checks");
                var type=typeof(AcademyGame);var screen=type.GetField("screen",BindingFlags.Instance|BindingFlags.NonPublic);
                var update=type.GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
                var title=Enum.Parse(screen.FieldType,"Title");
                screen.SetValue(game,title);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));InputSystem.Update();update.Invoke(game,null);
                Check(screen.GetValue(game).ToString()=="Select","actual game Update starts from arbitrary keyboard key");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                var mouse=InputSystem.AddDevice<Mouse>();screen.SetValue(game,title);
                InputSystem.QueueStateEvent(mouse,new MouseState { buttons=1 });InputSystem.Update();update.Invoke(game,null);
                Check(screen.GetValue(game).ToString()=="Select","actual game Update starts from mouse click");
                InputSystem.RemoveDevice(mouse);
                var eventHandler=type.GetMethod("TryStartFromEvent",BindingFlags.Instance|BindingFlags.NonPublic);
                foreach(var eventType in new[]{EventType.KeyDown,EventType.MouseDown}) {
                    screen.SetValue(game,title);
                    bool handled=(bool)eventHandler.Invoke(game,new object[]{eventType});
                    Check(handled && screen.GetValue(game).ToString()=="Select","Game view event fallback starts on "+eventType);
                }
                screen.SetValue(game,title);
                Check(!(bool)eventHandler.Invoke(game,new object[]{EventType.Repaint}) && screen.GetValue(game).ToString()=="Title","repaint never starts game");
                // Run full menu/game flow through the same Update method as real input.
                Action<Key[]> keyboardStep=keys=> {
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.Update();update.Invoke(game,null);
                };
                Action release=()=>keyboardStep(new Key[0]);
                keyboardStep(new[]{Key.Space});release();
                var selected=(int[])type.GetField("selected",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                int oldP1=selected[0],oldP2=selected[1];
                keyboardStep(new[]{Key.D,Key.LeftArrow});release();
                Check(selected[0]==(oldP1+1)%8 && selected[1]==(oldP2+7)%8,"keyboard navigates independent fighter selections");
                keyboardStep(new[]{Key.F});release();
                Check(screen.GetValue(game).ToString()=="Select","one player lock does not skip other player");
                keyboardStep(new[]{Key.J});release();
                Check(screen.GetValue(game).ToString()=="StageSelect","both keyboard locks advance to arena selection");
                int beforeArena=(int)type.GetField("selectedArena",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                keyboardStep(new[]{Key.RightArrow});release();
                int arena=(int)type.GetField("selectedArena",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                Check(arena==(beforeArena+1)%3,"keyboard chooses opening arena");
                keyboardStep(new[]{Key.Enter});release();
                Check(screen.GetValue(game).ToString()=="Fight","keyboard starts selected duel");
                var duel=(DuelModel)type.GetField("model",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                Check(duel.ArenaIndex==arena,"selected arena reaches combat model");
                keyboardStep(new[]{Key.Escape});release();
                var paused=type.GetField("paused",BindingFlags.Instance|BindingFlags.NonPublic);
                Check(screen.GetValue(game).ToString()=="Fight" && (bool)paused.GetValue(game),"Escape pauses without leaving duel");
                keyboardStep(new[]{Key.Escape});release();
                Check(!(bool)paused.GetValue(game),"Escape resumes same duel");
                type.GetField("countdown",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(game,0f);
                keyboardStep(new[]{Key.F});release();
                Check(duel.players[0].actionTime>0,"keyboard attack reaches actual combat model");
                type.GetMethod("OpenSelect",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);release();
                var runtimeControls=(AcademyInputManager)type.GetField("controls",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
                var assigned=(Gamepad[])typeof(AcademyInputManager).GetField("pads",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(runtimeControls);
                Action<GamepadState,GamepadState> padStep=(one,two)=> {
                    InputSystem.QueueStateEvent(assigned[0],one);InputSystem.QueueStateEvent(assigned[1],two);
                    InputSystem.Update();update.Invoke(game,null);
                };
                padStep(new GamepadState().WithButton(GamepadButton.South),new GamepadState());
                padStep(new GamepadState(),new GamepadState());
                Check(screen.GetValue(game).ToString()=="Select","one controller confirms only its own fighter");
                padStep(new GamepadState(),new GamepadState().WithButton(GamepadButton.South));
                padStep(new GamepadState(),new GamepadState());
                Check(screen.GetValue(game).ToString()=="StageSelect","two controllers confirm both fighters");
                padStep(new GamepadState().WithButton(GamepadButton.South),new GamepadState());
                padStep(new GamepadState(),new GamepadState());
                Check(screen.GetValue(game).ToString()=="Fight","controller confirms arena and starts duel");
                padStep(new GamepadState().WithButton(GamepadButton.Start),new GamepadState());
                padStep(new GamepadState(),new GamepadState());
                Check((bool)paused.GetValue(game),"controller Menu/Options pauses actual duel");
                padStep(new GamepadState().WithButton(GamepadButton.East),new GamepadState());
                Check(screen.GetValue(game).ToString()=="Select","controller back returns from pause to roster");
                InputSystem.RemoveDevice(replacement);
            }
            InputSystem.RemoveDevice(b);InputSystem.RemoveDevice(keyboard);
            Debug.Log("HEXBOUND INPUT CHECKS PASSED: "+count+" virtual-device assertions");
        }
    }
}
