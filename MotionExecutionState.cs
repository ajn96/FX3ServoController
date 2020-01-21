using System;
using System.Collections.Generic;
using System.Text;

namespace FX3ServoController
{
    class MotionExecutionState
    {
        public ServoMotor Motor;
        public int MotionProfileIndex;
        public List<ServoPosition> MotionProfile;
        public int CurrentIteration;
        public int TotalIterations;
        public double CurrentAngle;
        public bool isRunning;
        public long LastUpdateTime;

        public MotionExecutionState()
        {
            MotionProfileIndex = 0;
            CurrentIteration = 0;
        }
    }
}
