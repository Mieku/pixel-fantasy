using System;
using InfinityPBR.Modules;
using UnityEditor;
using UnityEngine;

namespace InfinityPBR
{
    [CustomEditor(typeof(FirstPersonMovement))]
    [Serializable]
    public class FirstPersonMovementEditor : InfinityEditorModules<FirstPersonMovement>
    {
        private float minTiltRange = -85;
        private float maxTiltRange = 85;

        protected override void Setup()
        {
            
        }
        
        protected override void Header()
        {
            
        }
        
        protected override void Draw()
        {
            // When this is first created, Unity will need to reimport scripts, which takes a few seconds. This keeps
            // errors from forming, and will show a message until that is complete.
            if (String.IsNullOrEmpty(Script.name))
            {
                Label("Please wait, scripts should re-import shortly...");
                return;
            }
            
#if MODULES_DICTIONARIES
            if (Script.dictionaries == null)
                Script.dictionaries = new Dictionaries(Script.name);
#endif

            TiltOptions();
            BlackLine();
            
            PanOptions();
            BlackLine();

            SpeedOptions();
            BlackLine();
            
            JumpOptions();
            BlackLine();
            
            FlyOptions();
            BlackLine();

            DictionaryOptions();
            Space();
            
            if (ShowFullInspector(name)) DrawDefaultInspector();
            SetDirty(Script);
        }
        
        private void DictionaryOptions()
        {
            Label("Dictionary", true);
            ShowDictionaryHelpBox();
        }

        private bool HeaderRow(bool isOn, string title)
        {
            StartRow();
            isOn = EnabledButton(isOn);
            Header3(title, 150);
            if (!isOn)
                LabelGrey($"{title} is disabled.");
            EndRow();
            Space();

            return isOn;
        }

        private void SpeedOptions()
        {
            CheckSpeedOptions();
            Script.canMove = HeaderRow(Script.canMove, "Movement Speed");
            if (!Script.canMove)
                return;
            
            StartRow();
            Label("Walk Speed", 100);
            Script.walkSpeed = Float(Script.walkSpeed, 50);
            Gap();
            Label("Run Speed", 100);
            Script.runSpeed = Float(Script.runSpeed, 50);
            EndRow();
            StartRow();
            Script.transitionSpeed = LeftCheck($"Transition Speed {symbolInfo}", "When true, speed changes " +
                "will transition over a set amount of time. When false, changes are instant.", Script.transitionSpeed, 176);
            if (Script.transitionSpeed)
            {
                Label("Duration", 100);
                Script.transitionDuration = Float(Script.transitionDuration, 50);
            }
            EndRow();
            StartRow();
            Script.defaultRun = LeftCheck($"Default Run {symbolInfo}", "When true, running is default and holding the \"Fast Movement\" key(s) will slow down " +
                                                                       "rather than speed up movement.", Script.defaultRun, 150);
            EndRow();
        }
        
        private void JumpOptions()
        {
            Script.canJump = HeaderRow(Script.canJump, "Jump");
            if (!Script.canJump)
                return;
            
            StartRow();
            Label("Jump Force", 100);
            Script.jumpForce = Float(Script.jumpForce, 50);
            EndRow();
            
            Script.jumpRepeatedly = LeftCheck($"Jump Repeatedly {symbolInfo}", "When true, the player will " +
                                                                               "jump over and over if they hold down the jump key.", Script.jumpRepeatedly, 250);
            Script.shortJumpIfButtonReleased = LeftCheck($"Short Jump if Button Released {symbolInfo}", "When true, if the player releases the jump key early, the jump will be cut short.", Script.shortJumpIfButtonReleased, 250);
            Script.changeDirectionMidJump = LeftCheck($"Change Direction in Air {symbolInfo}", "When true, the player can " +
                "change direction even when they're in mid-jump.", Script.changeDirectionMidJump, 250);
        }
        
        private void FlyOptions()
        {
            Script.canFly = HeaderRow(Script.canFly, "Flight");
            if (!Script.canFly)
                return;
        
            StartRow();
            Label("Flight Speed Modifier", 100);
            Script.flightSpeedModifier = Float(Script.flightSpeedModifier, 50);
            EndRow();
            
            StartRow();
            Label("Vertical Speed", 100);
            Script.verticalSpeed = Float(Script.verticalSpeed, 50);
            EndRow();
            
            StartRow();
            Label("Minimum Height", 100);
            Script.minHeight = Float(Script.minHeight, 50);
            EndRow();
            
            Script.moveToLookDirection = LeftCheck($"Move to Look Direction {symbolInfo}", "When true and flying, the player " +
                "will move up and down towards the direction they're looking. When false, looking up and down (tilt) will have no impact on height, " +
                "and all movement will be on a flat horizontal plane.", Script.moveToLookDirection, 250);
        }
        
        private void CheckSpeedOptions()
        {
            if (Script.walkSpeed <= 0) Script.walkSpeed = 0.1f;
            if (Script.runSpeed <= 0) Script.runSpeed = 0.1f;
            if (Script.speedPan <= 1) Script.speedPan = 1;
            if (Script.speedTilt <= 1) Script.speedTilt = 1;
            if (Script.runSpeed < Script.walkSpeed) Script.runSpeed = Script.walkSpeed;
        }
        
        private void PanOptions()
        {
            CheckMinMaxTilt();
            
            Script.canPan = HeaderRow(Script.canPan, "Pan");

            if (!Script.canPan)
                return;
            
            StartRow();
            Label("Pan Speed", 100);
            Script.speedPan = Float(Script.speedPan, 50);
            EndRow();
        }

        private void TiltOptions()
        {
            CheckMinMaxTilt();
            
            Script.canTilt = HeaderRow(Script.canTilt, "Tilt");
            if (!Script.canTilt)
                return;
            
            Script.invertTilt = LeftCheck("Invert Tilt", Script.invertTilt);
            StartRow();
            Label("Tilt Speed", 100);
            Script.speedTilt = Float(Script.speedTilt, 50);
            Gap();
            Label("Pan Speed", 100);
            Script.speedPan = Float(Script.speedPan, 50);
            EndRow();
            
            StartRow();
            Label("Min", 100);
            Script.minTilt = Float(Script.minTilt, 50);
            Gap();
            Label("Max", 100);
            Script.maxTilt = Float(Script.maxTilt,50 );
            EndRow();
            EditorGUILayout.MinMaxSlider(ref Script.minTilt, ref Script.maxTilt, minTiltRange, maxTiltRange);

        }

        /// <summary>
        /// This keeps the min/max tilt in range.
        /// </summary>
        private void CheckMinMaxTilt()
        {
            Script.minTilt = Mathf.Clamp(Script.minTilt, minTiltRange, maxTiltRange);
            Script.maxTilt = Mathf.Clamp(Script.maxTilt, minTiltRange, maxTiltRange);
            if (Script.minTilt > Script.maxTilt)
                Script.minTilt = Script.maxTilt;
            if (Script.maxTilt < Script.minTilt)
                Script.maxTilt = Script.minTilt;
        }
    }
}