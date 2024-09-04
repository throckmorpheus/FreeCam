﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreeCam.Components;

public class PromptController : MonoBehaviour
{
    private ScreenPrompt _togglePrompt, _guiPrompt, _teleportOptions, _scrollPrompt, _scrollSpeedPrompt, _rotatePrompt, _horizontalPrompt, _verticalPrompt;
    private ScreenPrompt _centerPrompt;
    private List<ScreenPrompt> _planetPrompts;

    private List<ScreenPrompt> _timePrompts;

    private ScreenPrompt _flashlightPrompt, _flashlightRangePrompt, _flashlightSpeedPrompt;

    private CustomFlashlight _customFlashlight;
    private CustomLookAround _customLookAround;

    private static readonly UIInputCommands _rotateLeftCmd = new("FREECAM - RotateLeft", KeyCode.Q);
    private static readonly UIInputCommands _rotateRightCmd = new("FREECAM - RotateRight", KeyCode.E);
    private static readonly UIInputCommands _scrollCmd = new("FREECAM - Scroll", KeyCode.Mouse2);
    private static readonly UIInputCommands _resetCmd = new("FREECAM - Reset", KeyCode.DownArrow);
    private static readonly UIInputCommands _rangeDown = new("FREECAM - RangeDown", KeyCode.LeftBracket);
    private static readonly UIInputCommands _rangeUp = new("FREECAM - RangeUp", KeyCode.RightBracket);

    private void Start()
    {
        _customFlashlight = GetComponent<CustomFlashlight>();
        _customLookAround = GetComponent<CustomLookAround>();

        // Top right
        _togglePrompt = AddPrompt("Toggle FreeCam", PromptPosition.UpperLeft, FreeCamController.ToggleKey);
        _guiPrompt = AddPrompt("Hide HUD", PromptPosition.UpperLeft, FreeCamController.GUIKey);

        _scrollPrompt = new ScreenPrompt(_scrollCmd, _resetCmd, "Movement speed   <CMD1> Reset   <CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
        Locator.GetPromptManager().AddScreenPrompt(_scrollPrompt, PromptPosition.UpperLeft, false);

        _scrollSpeedPrompt = new ScreenPrompt("Move Speed: " + _customLookAround.MoveSpeed + " m/s");
        Locator.GetPromptManager().AddScreenPrompt(_scrollSpeedPrompt, PromptPosition.UpperLeft, false);

        _horizontalPrompt = new ScreenPrompt(InputLibrary.moveXZ, "Move   <CMD>");
        Locator.GetPromptManager().AddScreenPrompt(_horizontalPrompt, PromptPosition.UpperLeft, false);

        _verticalPrompt = new ScreenPrompt(InputLibrary.thrustUp, InputLibrary.thrustDown, "Up/Down   <CMD>", ScreenPrompt.MultiCommandType.POS_NEG);
        Locator.GetPromptManager().AddScreenPrompt(_verticalPrompt, PromptPosition.UpperLeft, false);

        _rotatePrompt = new ScreenPrompt(_rotateLeftCmd, _rotateRightCmd, "Rotate   <CMD1> <CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
        Locator.GetPromptManager().AddScreenPrompt(_rotatePrompt, PromptPosition.UpperLeft, false);

        // Top Left
        _teleportOptions = AddPrompt("Parent options   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt), PromptPosition.UpperRight, FreeCamController.TeleportKey);
        _centerPrompt = AddPrompt("Player", PromptPosition.UpperRight, FreeCamController.CenterOnPlayerKey);

        _planetPrompts = new();
        foreach (var planet in FreeCamController.CenterOnPlanetKey.Keys)
        {
            _planetPrompts.Add(AddPrompt(AstroObject.AstroObjectNameToString(planet), PromptPosition.UpperRight, FreeCamController.CenterOnPlanetKey[planet].key));
        }

        // Flashlight
        _flashlightPrompt = new ScreenPrompt(InputLibrary.flashlight, UITextLibrary.GetString(UITextType.FlashlightPrompt) + "   <CMD>" + UITextLibrary.GetString(UITextType.PressPrompt));
        Locator.GetPromptManager().AddScreenPrompt(_flashlightPrompt, PromptPosition.UpperLeft, false);

        _flashlightRangePrompt = new ScreenPrompt(_rangeDown, _rangeUp, "Flashlight range   <CMD1> <CMD2>", ScreenPrompt.MultiCommandType.CUSTOM_BOTH);
        Locator.GetPromptManager().AddScreenPrompt(_flashlightRangePrompt, PromptPosition.UpperLeft, false);

        _flashlightSpeedPrompt = AddPrompt("Adjust range faster   <CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt), PromptPosition.UpperLeft, Key.RightShift);

        _timePrompts = new()
        {
            AddPrompt("0% game speed", PromptPosition.LowerLeft, Key.Comma),
            AddPrompt("50% game speed", PromptPosition.LowerLeft, Key.Period),
            AddPrompt("100% game speed", PromptPosition.LowerLeft, Key.Slash)
        };
    }

    private void Update()
    {
        var visible = !OWTime.IsPaused() && !GUIMode.IsHiddenMode() && MainClass.ShowPrompts;

        // Top right
        _togglePrompt.SetVisibility(visible);

        _guiPrompt.SetVisibility(visible && MainClass.InFreeCam);
        _scrollPrompt.SetVisibility(visible && MainClass.InFreeCam);
        _scrollSpeedPrompt.SetVisibility(visible && MainClass.InFreeCam);
        _scrollSpeedPrompt.SetText("Move Speed: " + _customLookAround.MoveSpeed + " m/s");
        _horizontalPrompt.SetVisibility(visible && MainClass.InFreeCam);
        _verticalPrompt.SetVisibility(visible && MainClass.InFreeCam);
        _rotatePrompt.SetVisibility(visible && MainClass.InFreeCam);

        // Top left
        _teleportOptions.SetVisibility(visible && MainClass.InFreeCam);
        _centerPrompt.SetVisibility(visible && MainClass.InFreeCam && FreeCamController.HoldingTeleport);
        foreach (var planetPrompt in _planetPrompts)
        {
            planetPrompt.SetVisibility(visible && MainClass.InFreeCam && FreeCamController.HoldingTeleport);
        }

        // Flashlight
        _flashlightPrompt.SetVisibility(visible && MainClass.InFreeCam);
        _flashlightRangePrompt.SetVisibility(visible && MainClass.InFreeCam && _customFlashlight.FlashlightOn());
        _flashlightSpeedPrompt.SetVisibility(visible && MainClass.InFreeCam && _customFlashlight.FlashlightOn());

        // Time
        foreach (var prompt in _timePrompts)
        {
            prompt.SetVisibility(visible && MainClass.InFreeCam);
        }
    }

    private static ScreenPrompt AddPrompt(string text, PromptPosition position, Key key)
    {
        Enum.TryParse(key.ToString().Replace("Digit", "Alpha"), out KeyCode keyCode);

        return AddPrompt(text, position, keyCode);
    }

    private static ScreenPrompt AddPrompt(string text, PromptPosition position, KeyCode keyCode)
    {
        var texture = ButtonPromptLibrary.SharedInstance.GetButtonTexture(keyCode);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, Vector4.zero, false);
        sprite.name = texture.name;

        var prompt = new ScreenPrompt(text, sprite);
        Locator.GetPromptManager().AddScreenPrompt(prompt, position, false);

        return prompt;
    }
}
