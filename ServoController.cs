using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace FX3ServoController
{
    struct MotionExecutionState
    {
        public ServoMotor Motor;
        public int MotionProfileIndex;
        public int CurrentIteration;
        public int TotalIterations;
        public double currentAngle;
        public bool isRunning;
    }

    public class ServoController
    {
        //private members
        private FX3Api.FX3Connection m_FX3;
        private double m_updatePeriod;
        private Mutex m_FX3Mutex;
        private System.Timers.Timer m_updateTimer;
        private List<MotionExecutionState> m_ExecutionList;
        private Stopwatch m_stopWatch;
        private long m_lastUpdateTime;

        /// <summary>
        /// Servo controller constructor
        /// </summary>
        /// <param name="FX3Board">The FX3 providing the PWM signals. Must be already connected.</param>
        public ServoController(FX3Api.FX3Connection FX3Board)
        {
            m_FX3 = FX3Board;

            //check connected
            if(m_FX3.ActiveFX3 == null)
            {
                throw new System.ArgumentException("ERROR: FX3 board must be connected before use in ServoController");
            }

            //set default values
            m_FX3Mutex = new Mutex();
            m_ExecutionList = new List<MotionExecutionState>();
            m_updatePeriod = 100;
            m_stopWatch = new Stopwatch();
            m_lastUpdateTime = 0;

            //set up timer period
            m_updateTimer = new System.Timers.Timer(m_updatePeriod);
            //assign callback
            m_updateTimer.Elapsed += UpdateAllMotionProfiles;
            //disable by default
            m_updateTimer.Enabled = false;
        }

        public double SignalUpdatePeriodMs
        {
            get
            {
                return m_updatePeriod;
            }
            set
            {
                m_updatePeriod = value;
            }
        }

        /// <summary>
        /// Run the motion profile on the selected servo until cancelled
        /// </summary>
        /// <param name="Motor">The motor to run</param>
        public void RunServoMotion(ref ServoMotor Motor)
        {
            //call overload with multiple iterations. -1 means infinite
            RunServoMotion(ref Motor, -1);
        }

        public void RunServoMotion(ref ServoMotor Motor, List<ServoPosition> MotionPath)
        {
            //apply the motion path to the motor object
            Motor.MotionPath = MotionPath;

            //call base function
            RunServoMotion(ref Motor);
        }

        public void RunServoMotion(ref ServoMotor Motor, int NumIterations)
        {

        }

        public void RunServoMotion(ref ServoMotor Motor, int NumIteration, List<ServoPosition> MotionPath)
        {
            //Assign the motion path
            Motor.MotionPath = MotionPath;
            //call base function
            RunServoMotion(ref Motor, NumIteration);
        }

        public void CancelServoMotion(ref ServoMotor Motor)
        {

        }

        public void PauseServoMotion(ref ServoMotor Motor)
        {

        }

        public void CancelAllServoMotion()
        {

        }

        public bool IsMotorRunning(ServoMotor Motor)
        {
            uint motorID = Motor.PWMPinIndex;
            for(int i = 0; i<m_ExecutionList.Count; i++)
            {
                if (m_ExecutionList[i].Motor.PWMPinIndex == motorID && m_ExecutionList[i].isRunning)
                    return true;
            }
            return false;
        }

        private void UpdateAllMotionProfiles(Object source, ElapsedEventArgs e)
        {
            //variables
            double nextAngle;
            double timeSinceUpdate;
            ServoPosition nextPosition;

            //Get the FX3 mutex
            m_FX3Mutex.WaitOne();

            timeSinceUpdate = m_stopWatch.ElapsedMilliseconds - m_lastUpdateTime;
            //update each motor which is running
            try
            {
                foreach(MotionExecutionState state in m_ExecutionList)
                {
                    if(state.isRunning)
                    {
                        //get the position the motion is moving towards
                        
                    }
                }
                m_lastUpdateTime = m_stopWatch.ElapsedMilliseconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Exception during PWM signal update. " + ex.ToString());
            }
            finally
            {
                m_FX3Mutex.ReleaseMutex();
            }
        }

    }
}
