using System;
using System.Collections.Generic;
using System.Text;

namespace FX3ServoController
{
    public class ServoMotor
    {
        //Private members
        private FX3Api.FX3PinObject m_pin;
        private double m_period;
        private double m_maxAngle;

        /// <summary>
        /// Servo motor constructor
        /// </summary>
        /// <param name="ControlPin">The FX3 pin which drives the PWM input signal to the servo</param>
        public ServoMotor(FX3Api.FX3PinObject ControlPin)
        {
            m_pin = ControlPin;
            m_period = 20;
            m_maxAngle = 180;
        }

        /// <summary>
        /// Contructor overload which takes the PWM pin GPIO number instead of the pin object
        /// </summary>
        /// <param name="PinNumber">The GPIO number of the FX3 pin which drives the PWM input to the servo</param>
        public ServoMotor(uint PinNumber) : this(new FX3Api.FX3PinObject(PinNumber))
        {
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

        /// <summary>
        /// Get the PWM pin object associated with the motor
        /// </summary>
        public FX3Api.FX3PinObject ControlPin
        {
            get
            {
                return m_pin;
            }
        }

        /// <summary>
        /// Get the PWM block associated with the servo motor. This is essentially the port identifier for the motor.
        /// </summary>
        public uint PWMHardwareIndex
        {
            get
            {
                return m_pin.pinConfig % 8;
            }
        }
    }
}
