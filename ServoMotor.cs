using System;
using System.Collections.Generic;
using System.Text;

namespace FX3ServoController
{
    class ServoMotor
    {
        //Private members
        private FX3Api.FX3PinObject m_pin;
        private double m_period;
        private double m_maxAngle;
        private List<ServoPosition> m_motionPath;

        /// <summary>
        /// Servo motor constructor
        /// </summary>
        /// <param name="ControlPin">The FX3 pin which drives the PWM input signal to the servo</param>
        public ServoMotor(FX3Api.FX3PinObject ControlPin)
        {
            m_pin = ControlPin;
            m_period = 20;
            m_maxAngle = 180;
            m_motionPath = new List<ServoPosition>();
        }

        /// <summary>
        /// Contructor overload which takes the PWM pin GPIO number instead of the pin object
        /// </summary>
        /// <param name="PinNumber">The GPIO number of the FX3 pin which drives the PWM input to the servo</param>
        public ServoMotor(uint PinNumber) : this(new FX3Api.FX3PinObject(PinNumber))
        {
        }

        /// <summary>
        /// Get or set the motion path for this servo
        /// </summary>
        public List<ServoPosition> MotionPath
        {
            get
            {
                return m_motionPath;
            }
            set
            {
                if(value != null)
                {
                    m_motionPath = value;
                }
                else
                {
                    m_motionPath = new List<ServoPosition>();
                }
            }
        }

        /// <summary>
        /// Add point to the end of the motion path.
        /// </summary>
        /// <param name="point">The servo position to add</param>
        public void AddToMotionPath(ServoPosition point)
        {
            m_motionPath.Add(point);
        }

        /// <summary>
        /// Clear the current motion path
        /// </summary>
        public void ClearMotionPath()
        {
            m_motionPath.Clear();
        }

        /// <summary>
        /// Get or set the PWM period (in ms). Min value of 0.1ms
        /// </summary>
        public double PWMPeriodMs
        {
            get
            {
                return m_period;
            }
            set
            {
                //cap minimum period at 100us (10KHz)
                if(value < 0.1)
                    value = 1;
                m_period = value;
            }
        }

        /// <summary>
        /// Get or set the servo max angle, in degrees.
        /// </summary>
        public double MaxAngle
        {
            get
            {
                return m_maxAngle;
            }
            set
            {
                //cap in the range of a single rotation
                value = value % 360.0;

                //if negative subtract from 360 to put in a positive rotation
                if (value < 0)
                    value = 360.0 - value;

                m_maxAngle = value;
            }
        }
    }
}
