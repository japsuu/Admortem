using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using CommandTerminal;
using UnityEngine;

public static class ConsoleCommands
{
    [RegisterCommand(Help = "Clear the command console", MaxArgCount = 0)]
    private static void CommandClear(CommandArg[] args)
    {
        Terminal.Buffer.Clear();
    }
    
    [RegisterCommand(Help = "Display help information about a command", MaxArgCount = 1)]
    private static void CommandHelp(CommandArg[] args)
    {
        if (args.Length == 0)
        {
            foreach (KeyValuePair<string, CommandInfo> command in Terminal.Shell.Commands)
            {
                Terminal.Log("{0}: {1}", command.Key.PadRight(16), command.Value.help);
            }
            return;
        }

        string commandName = args[0].String.ToUpper();

        if (!Terminal.Shell.Commands.ContainsKey(commandName))
        {
            Terminal.Shell.IssueErrorMessage("Command {0} could not be found.", commandName);
            return;
        }

        var info = Terminal.Shell.Commands[commandName];

        if (info.help == null)
        {
            Terminal.Log("{0} does not provide any help documentation.", commandName);
        }
        else if (info.hint == null)
        {
            Terminal.Log(info.help);
        }
        else
        {
            Terminal.Log("{0}\nUsage: {1}", info.help, info.hint);
        }
    }
    
    [RegisterCommand(Help = "Bind a hotkey to a console command. Set hotkey command to 'none' to unbind", Hint = "Bind <KeyCode> <Command>", MinArgCount = 2)]
    private static void CommandBind(CommandArg[] args)
    {
        if (Enum.TryParse(args[0].String, true, out KeyCode result))
        {
            void OnActivated()
            {
                if (Input.GetKeyDown(result)) Terminal.Shell.RunCommand(JoinArguments(args, 1));
            }

            if (!HotkeyManager.Instance.HotKeys.ContainsKey(result))
            {
                HotkeyManager.Instance.HotKeys.Add(result, OnActivated);

                HotkeyManager.Instance.OnKeyDownEvent += OnActivated;
                
                Terminal.Log("Keycode '" + result + "' has been bound to " + JoinArguments(args, 1) + "!");
            }
            else
            {
                if (args[1].String == "none")
                {
                    HotkeyManager.Instance.OnKeyDownEvent -= OnActivated;
                    HotkeyManager.Instance.HotKeys.Remove(result);
                    
                    Terminal.Log("Keycode has been unbound!");
                }
                else
                {
                    Terminal.Log("Keycode has been bound already!");
                }
            }
        }
        else
        {
            Terminal.Log("Invalid keycode!");
        }
    }
    
    [RegisterCommand(Help = "Time the execution of a command", Hint = "Time <command>", MinArgCount = 1)]
    private static void CommandTime(CommandArg[] args)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Terminal.Shell.RunCommand(JoinArguments(args));

        sw.Stop();
        Terminal.Log("Time: {0}ms", (double)sw.ElapsedTicks / 10000);
    }
    
    [RegisterCommand(Help = "Output message", Hint = "Print <msg>")]
    private static void CommandPrint(CommandArg[] args)
    {
        Terminal.Log(JoinArguments(args));
    }
    
#if DEBUG
    [RegisterCommand(Help = "Output the stack trace of the previous message", MaxArgCount = 0)]
    private static void CommandTrace(CommandArg[] args)
    {
        int logCount = Terminal.Buffer.Logs.Count;

        if (logCount - 2 < 0)
        {
            Terminal.Log("Nothing to trace.");
            return;
        }

        var logItem = Terminal.Buffer.Logs[logCount - 2];

        if (logItem.stack_trace == "")
        {
            Terminal.Log("{0} (no trace)", logItem.message);
        }
        else
        {
            Terminal.Log(logItem.stack_trace);
        }
    }

    [RegisterCommand(Help = "List all variables or set a variable value")]
    private static void CommandSet(CommandArg[] args)
    {
        if (args.Length == 0)
        {
            foreach (KeyValuePair<string, CommandArg> kv in Terminal.Shell.Variables)
            {
                Terminal.Log("{0}: {1}", kv.Key.PadRight(16), kv.Value);
            }
            return;
        }

        string variableName = args[0].String;

        if (variableName[0] == '$')
        {
            Terminal.Log(TerminalLogType.Warning, "Warning: Variable name starts with '$', '${0}'.", variableName);
        }

        Terminal.Shell.SetVariable(variableName, JoinArguments(args, 1));
    }
#endif
    
    [RegisterCommand(Help = "Toggle Fly mode", MaxArgCount = 0)]
    private static void CommandFly(CommandArg[] args)
    {
        PlayerMovementController.Instance.flyModeEnabled = !PlayerMovementController.Instance.flyModeEnabled;

        string state = PlayerMovementController.Instance.flyModeEnabled ? "ON" : "OFF";
        
        Terminal.Log("Toggled Fly mode " + state);
    }
    
    [RegisterCommand(Help = "Change the player reach. Default = 7.", MinArgCount = 1, MaxArgCount = 1)]
    private static void CommandReach(CommandArg[] args)
    {
        try
        {
            BuildingController.Instance.reachDistance = args[0].Float;
            Terminal.Log("Reach set to: " + BuildingController.Instance.reachDistance);
        }
        catch
        {
            Terminal.Log("Invalid reach value. ");
        }
    }
    
    [RegisterCommand(Help = "Change the player block breaking speed. Default = 10.", MinArgCount = 1, MaxArgCount = 1)]
    private static void CommandBreakSpeed(CommandArg[] args)
    {
        try
        {
            BuildingController.Instance.breakSpeed = args[0].Int;
            Terminal.Log("Break speed set to: " + BuildingController.Instance.breakSpeed);
        }
        catch
        {
            Terminal.Log("Invalid break speed value. ");
        }
    }
    
    [RegisterCommand(Help = "Toggle Godmode. Disables all damage and negative effects.", MaxArgCount = 0)]
    private static void CommandGod(CommandArg[] args)
    {
        PlayerMovementController.Instance.godModeEnabled = !PlayerMovementController.Instance.godModeEnabled;

        string state = PlayerMovementController.Instance.godModeEnabled ? "ON" : "OFF";
        
        Terminal.Log("Toggled Godmode " + state);
    }
    
    [RegisterCommand(Help = "Teleport to coordinates.", Hint = "Teleport [x] [y]", MinArgCount = 2, MaxArgCount = 2)]
    private static void CommandTeleport(CommandArg[] args)
    {
        PlayerMovementController.Instance.transform.position = new Vector3(args[0].Float, args[1].Float);
        
        Terminal.Log("Teleported to " + new Vector2(args[0].Float, args[1].Float));
    }
    
    [RegisterCommand(Help = "Control the music playback.", Hint = "Music <Volume [percentage]> | <Start> | <Stop> | <Next> | <Previous>", MinArgCount = 1, MaxArgCount = 2)]
    private static void CommandMusic(CommandArg[] args)
    {
        Enum.TryParse(args[0].String, true, out MusicCommand result);

        switch (result)
        {
            case MusicCommand.Volume:
            {
                try
                {
                    float volume = args[1].Int / 100f;

                    AudioManager.Instance.SetVolume(volume);
            
                    Terminal.Log("Set music volume to " + (volume * 100) + "%");
                }
                catch (Exception e)
                {
                    Terminal.Log("Invalid input: " + e);
                }
            }
                break;
            
            case MusicCommand.Start:
            {
                AudioManager.Instance.StartPlayback();
        
                Terminal.Log("Started music playback!");
            }
                break;
            
            case MusicCommand.Stop:
            {
                AudioManager.Instance.StopPlayback();
        
                Terminal.Log("Stopped music playback!");
            }
                break;
            
            case MusicCommand.Next:
            {
                AudioManager.Instance.StartCoroutine(AudioManager.Instance.Next());
        
                Terminal.Log("Skipping to the next song...");
            }
                break;

            case MusicCommand.Previous:
            {
                AudioManager.Instance.StartCoroutine(AudioManager.Instance.Previous());
        
                Terminal.Log("Skipping to the previous song...");
            }
                break;
            
            default:
                Terminal.Log("Invalid input.");
                break;
        }
    }
    
    [RegisterCommand(Help = "Enable debug visuals.", Hint = "DebugVisuals <true> | <false>", MinArgCount = 1, MaxArgCount = 1)]
    private static void CommandDebugVisuals(CommandArg[] args)
    {
        Settings.DebugVisualsEnabled = args[0].Bool;
        
        Terminal.Log("Debug Visuals Enabled: " + Settings.DebugVisualsEnabled);
    }

    [RegisterCommand(Help = "No operation")]
    private static void CommandNoop(CommandArg[] args) { }

    [RegisterCommand(Help = "Quit the running application", MaxArgCount = 0)]
    private static void CommandQuit(CommandArg[] args)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private static string JoinArguments(CommandArg[] args, int start = 0)
    {
        var sb = new StringBuilder();
        int argLength = args.Length;

        for (int i = start; i < argLength; i++)
        {
            sb.Append(args[i].String);

            if (i < argLength - 1)
            {
                sb.Append(" ");
            }
        }

        return sb.ToString();
    }
    
    private enum MusicCommand
    {
        Null,
        Volume,
        Start,
        Stop,
        Next,
        Previous
    }
}
