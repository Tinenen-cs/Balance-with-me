using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BalanceWithMe
{
    // Opt-in release-player measurement. Normal launches collect no samples or files.
    public class RuntimeDiagnostics : MonoBehaviour
    {
        [Serializable] class Report
        {
            public string unityVersion, operatingSystem, cpu, gpu, measuredUtc;
            public int screenWidth, screenHeight, frames, framesOver50ms, missingAssets, errorCount, gameplayFrames, resolvedRounds, completedMatches;
            public bool fullscreen, landscape;
            public double averageFps, p95FrameMs, p99FrameMs, maxFrameMs, allocatedMemoryMB;
            public string[] devices, errors;
        }
        readonly List<float> frames=new List<float>(10000);
        readonly List<string> errors=new List<string>();
        string path; float began, duration=45; bool exit, autoplay;
        Gamepad testBoy,testGirl; int gameplayFrames, resolvedRounds, completedMatches, previousRounds;
        float nextInput;
        void Awake()
        {
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length;i++)
            {
                if(args[i]=="-qa-report" && i+1<args.Length) path=args[++i];
                else if(args[i]=="-qa-duration" && i+1<args.Length) float.TryParse(args[++i],out duration);
                else if(args[i]=="-qa-exit") exit=true;
                else if(args[i]=="-qa-autoplay")autoplay=true;
            }
            if(string.IsNullOrEmpty(path)) { enabled=false;return; }
            duration=Mathf.Max(10,duration);began=Time.realtimeSinceStartup;
            Application.logMessageReceived+=Log;
            if(autoplay)
            {
                Application.runInBackground=true;
                testBoy=InputSystem.AddDevice<Gamepad>();testGirl=InputSystem.AddDevice<Gamepad>();
                var game=GetComponent<BalanceGame>();game.inputs.Assign(false,testBoy);game.inputs.Assign(true,testGirl);
            }
        }
        void Log(string message,string trace,LogType type)
        {
            if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)
                if(errors.Count<30) errors.Add(message);
        }
        void Update()
        {
            float elapsed=Time.realtimeSinceStartup-began;
            var currentGame=GetComponent<BalanceGame>();
            if(currentGame.CanControl)gameplayFrames++;
            if(autoplay && elapsed>1)
            {
                var menu=GetComponent<GameMenu>();if(menu.IsOpen)menu.Resume();
                if(currentGame.CompletedRounds>previousRounds)resolvedRounds+=currentGame.CompletedRounds-previousRounds;
                previousRounds=currentGame.CompletedRounds;
                if(currentGame.MatchComplete) { completedMatches++;currentGame.PlayAgain();previousRounds=0; }
                if(Time.unscaledTime>=nextInput)
                {
                    nextInput=Time.unscaledTime+.15f;
                    float distance=currentGame.girl.body.position.x-currentGame.boy.body.position.x;
                    float direction=Mathf.Sign(distance);float movement=Mathf.Abs(distance)>.8f?direction:0;
                    var boyState=new GamepadState { leftStick=new Vector2(movement,0) };
                    var girlState=new GamepadState { leftStick=new Vector2(-movement,0) };
                    int step=(int)(elapsed/.15f);
                    if(step%4==0)boyState=boyState.WithButton(GamepadButton.West);
                    if(step%5==0)girlState=girlState.WithButton(GamepadButton.West);
                    if(step%17==0)girlState=girlState.WithButton(GamepadButton.South);
                    if(step%19==0)boyState=boyState.WithButton(GamepadButton.East);
                    InputSystem.QueueStateEvent(testBoy,boyState);InputSystem.QueueStateEvent(testGirl,girlState);
                }
            }
            if(elapsed>5) frames.Add(Time.unscaledDeltaTime*1000);
            if(elapsed<duration+5)return;
            frames.Sort();double total=0;int slow=0;
            foreach(float value in frames) { total+=value;if(value>50)slow++; }
            int missing=0;
            foreach(var render in FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(render.sprite==null || render.sharedMaterial==null || !render.sharedMaterial.shader.isSupported) missing++;
            foreach(var image in FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(image.name.Contains("Star") && image.sprite==null) missing++;
            var devices=new List<string>();
            foreach(var device in UnityEngine.InputSystem.InputSystem.devices) devices.Add(device.layout+": "+device.displayName);
            var report=new Report { unityVersion=Application.unityVersion,operatingSystem=SystemInfo.operatingSystem,
                cpu=SystemInfo.processorType,gpu=SystemInfo.graphicsDeviceName,measuredUtc=DateTime.UtcNow.ToString("O"),
                screenWidth=Screen.width,screenHeight=Screen.height,fullscreen=Screen.fullScreen,
                landscape=Screen.width>=Screen.height,frames=frames.Count,framesOver50ms=slow,
                missingAssets=missing,errorCount=errors.Count,errors=errors.ToArray(),devices=devices.ToArray(),
                averageFps=frames.Count*1000/Math.Max(1,total),
                p95FrameMs=frames.Count==0?0:frames[Math.Min(frames.Count-1,(int)(frames.Count*.95))],
                p99FrameMs=frames.Count==0?0:frames[Math.Min(frames.Count-1,(int)(frames.Count*.99))],
                maxFrameMs=frames.Count==0?0:frames[frames.Count-1],gameplayFrames=gameplayFrames,resolvedRounds=resolvedRounds,completedMatches=completedMatches,allocatedMemoryMB=Profiler.GetTotalAllocatedMemoryLong()/1048576.0 };
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            File.WriteAllText(path,JsonUtility.ToJson(report,true));enabled=false;
            if(exit) Application.Quit();
        }
        void OnDestroy()
        {
            Application.logMessageReceived-=Log;
            if(testBoy!=null && testBoy.added)InputSystem.RemoveDevice(testBoy);
            if(testGirl!=null && testGirl.added)InputSystem.RemoveDevice(testGirl);
        }
    }
}
