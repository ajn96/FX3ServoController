using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace FX3ServoController
{

    public class ServoController
    {
        //private members
        private FX3Api.FX3Connection m_FX3;
        private double m_updatePeriod;
        private Mutex m_FX3Mutex;
        private System.Timers.Timer m_updateTimer;
        private List<MotionExecutionState> m_ExecutionList;
        private Stopwatch m_stopWatch;

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

            //set up timer period
            m_updateTimer = new System.Timers.Timer(m_updatePeriod);
            //assign callback
            m_updateTimer.Elapsed += UpdateAllMotionProfiles;
            //disable by default
            m_updateTimer.Enabled = false;
        }

        /// <summary>
        /// Period in which to update the PWM signal values
        /// </summary>
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
        /// Run motion profile forever until cancelled. Repeats from the start of the profile.
        /// </summary>
        /// <param name="Motor">The motor to run the motion profile on</param>
        /// <param name="MotionProfile">The profile to run</param>
        public void RunServoMotion(ServoMotor Motor, List<ServoPosition> MotionProfile)
        {
            //call overload with multiple iterations. -1 means infinite
            RunServoMotion(Motor, MotionProfile, -1);
        }

        public void RunServoMotion(ServoMotor Motor, List<ServoPosition> MotionProfile, int NumIterations)
        {
            //Clear any exisiting motion profile for the selected motor
            CancelServoMotion(Motor);

            //add to the execution list
            MotionExecutionState state = new MotionExecutionState();
            state.Motor = Motor;
            state.LastUpdateTime = m_stopWatch.ElapsedMilliseconds;
            state.MotionProfile = MotionProfile;
            state.TotalIterations = NumIterations;
            state.isRunning = (MotionProfile.Count > 0);
            if (state.isRunning)
                state.CurrentAngle = MotionProfile[0].TargetAngle;
            else
                state.CurrentAngle = Motor.MaxAngle / 2;
            m_ExecutionList.Add(state);

            //ensure update timer is enabled
            m_updateTimer.Enabled = true;
        }

        public void CancelServoMotion(ServoMotor Motor)
        {
            bool motorFound = false;
            for (int i = 0; i<m_ExecutionList.Count; i++)
            {
                if(Motor == m_ExecutionList[i].Motor)
                {
                    m_ExecutionList.RemoveAt(i);
                    motorFound = true;
                    break;
                }
            }
            if (!motorFound)
                throw new ArgumentException("ERROR: Could not find motor to cancel motion");
        }

        public void PauseServoMotion(ServoMotor Motor)
        {
            bool motorFound = false;
            for (int i = 0; i < m_ExecutionList.Count; i++)
            {
                if (Motor == m_ExecutionList[i].Motor)
                {
                    m_ExecutionList[i].isRunning = false;
                    motorFound = true;
                    break;
                }
            }
            if (!motorFound)
                throw new ArgumentException("ERROR: Paused motor not found");
        }

        public void ResumeServoMotion(ServoMotor Motor)
        {
            bool motorFound = false;
            for (int i = 0; i < m_ExecutionList.Count; i++)
            {
                if (Motor == m_ExecutionList[i].Motor)
                {
                    m_ExecutionList[i].isRunning = true;
                    motorFound = true;
                    m_updateTimer.Enabled = true;
                    break;
                }
            }
            if (!motorFound)
                throw new ArgumentException("ERROR: Could not find motor to resume");
        }

        public void CancelAllServoMotion()
        {
            m_ExecutionList.Clear();
            m_updateTimer.Enabled = false;
        }

        public bool IsMotorRunning(ServoMotor Motor)
        {
            uint motorID = Motor.PWMHardwareIndex;
            for(int i = 0; i<m_ExecutionList.Count; i++)
            {
                if (m_ExecutionList[i].Motor.PWMHardwareIndex == motorID)
                    return m_ExecutionList[i].isRunning;
            }
            return false;
        }

        private void UpdateAllMotionProfiles(Object source, ElapsedEventArgs e)
        {
            //variables
            double nextAngle;
            double angleIncrement;
            double timeSinceUpdate;
            ServoPosition nextPosition;
            bool targetMet;

            //Get the FX3 mutex
            m_FX3Mutex.WaitOne();

            //update each motor which is running
            try
            {
                for (int i = 0; i < m_ExecutionList.Count; i++)
                {
                    //check profile is non-zero and exits
                    if (m_ExecutionList[i].MotionProfile == null || m_ExecutionList[i].MotionProfile.Count == 0)
                        m_ExecutionList[i].isRunning = false;

                    if (m_ExecutionList[i].isRunning)
                    {
                        //find time since update
                        timeSinceUpdate = m_stopWatch.ElapsedMilliseconds - m_ExecutionList[i].LastUpdateTime;

                        //set new update time
                        m_ExecutionList[i].LastUpdateTime = m_stopWatch.ElapsedMilliseconds;

                        //set target met to false
                        targetMet = false;

                        //get the position the motion is moving towards
                        nextPosition = m_ExecutionList[i].MotionProfile[m_ExecutionList[i].MotionProfileIndex];

                        //get the angle increment based on the current motion speed
                        angleIncrement = (timeSinceUpdate * nextPosition.RotationSpeed) / 1000;

                        //check relative to current position
                        if (nextPosition.TargetAngle > m_ExecutionList[i].CurrentAngle)
                        {
                            //increment next angle to approach target
                            nextAngle = m_ExecutionList[i].CurrentAngle + angleIncrement;

                            if (nextAngle >= nextPosition.TargetAngle)
                            {
                                //set angle to target
                                nextAngle = nextPosition.TargetAngle;

                                //set target met flag
                                targetMet = true;
                            }
                        }
                        else
                        {
                            //decrement next angle to approach target
                            nextAngle = m_ExecutionList[i].CurrentAngle - angleIncrement;

                            if (nextAngle <= nextPosition.TargetAngle)
                            {
                                //set angle to target
                                nextAngle = nextPosition.TargetAngle;

                                //set target met flag
                                targetMet = true;
                            }
                        }
                        //handle target being met
                        if (targetMet)
                        {
                            //move to next motion
                            if (m_ExecutionList[i].MotionProfileIndex < m_ExecutionList[i].MotionProfile.Count)
                                m_ExecutionList[i].MotionProfileIndex++;
                            else
                            {
                                //we have reached the end of the motion
                                m_ExecutionList[i].MotionProfileIndex = 0;
                                m_ExecutionList[i].CurrentIteration++;
                                //only check exit condition if totaliterations != -1
                                if (m_ExecutionList[i].TotalIterations != -1)
                                {
                                    if (m_ExecutionList[i].CurrentIteration >= m_ExecutionList[i].TotalIterations)
                                    {
                                        //done with entire motion
                                        m_ExecutionList[i].isRunning = false;
                                        m_ExecutionList[i].CurrentIteration = 0;
                                    }
                                }
                            }
                        }

                        //apply new angle to servo motor
                        SetServoAngle(m_ExecutionList[i].Motor.ControlPin, nextAngle, m_ExecutionList[i].Motor.MaxAngle, m_ExecutionList[i].Motor.PWMPeriodMs);
                    }
                }
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

        private void SetServoAngle(FX3Api.FX3PinObject Pin, double Angle, double Range, double PWMPeriodMs)
        {
            //Find duty cycle
            double dutyCycle = Angle / Range;
            //find PWM freq
            double freq = 1000 / PWMPeriodMs;
            //call PWM stop if needed
            if(m_FX3.isPWMPin(Pin))
                m_FX3.StopPWM(Pin);
            //start PWM signal generation
            m_FX3.StartPWM(freq, dutyCycle, Pin);
        }
    }
}
