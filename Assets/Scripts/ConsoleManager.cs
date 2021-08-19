using CommandTerminal;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    [RegisterCommand(Help = "Toggle Fly mode")]
    private static void CommandFly(CommandArg[] args)
    {
        if (Terminal.IssuedError) return; // Error will be handled by Terminal

        PlayerMovementController.Instance.flyModeEnabled = !PlayerMovementController.Instance.flyModeEnabled;

        string state = PlayerMovementController.Instance.flyModeEnabled ? "ON" : "OFF";
        
        Terminal.Log("Toggled Fly mode " + state);
    }
    
    [RegisterCommand(Help = "Toggle Godmode. Disables all damage and negative effects.")]
    private static void CommandGod(CommandArg[] args)
    {
        if (Terminal.IssuedError) return; // Error will be handled by Terminal

        PlayerMovementController.Instance.godModeEnabled = !PlayerMovementController.Instance.godModeEnabled;

        string state = PlayerMovementController.Instance.godModeEnabled ? "ON" : "OFF";
        
        Terminal.Log("Toggled Godmode " + state);
    }
}
