using System;
using System.Collections.Generic;
using System.Text;

namespace FX3ServoController
{
    /// <summary>
    /// A servo angle and the rotation speed to reach that angle
    /// </summary>
    class ServoPosition
    {
        //Private members
        private double m_targetAngle;
        private double m_rotationSpeed;

        /// <summary>
        /// Contstructor. Sets default values.
        /// </summary>
        public ServoPosition()
        {
            m_targetAngle = 0;
            m_rotationSpeed = 15;
        }

        /// <summary>
        /// Get or set the position target angle (degrees). Cannot be more than 360 degrees.
        /// </summary>
        public double TargetAngle
        {
            get
            {
                return m_targetAngle;
            }
            set
            {
                m_targetAngle = value % 360;
            }
        }

        /// <summary>
        /// Get or set the rotation speed to reach the target angle (degrees / second)
        /// </summary>
        public double RotationSpeed
        {
            get
            {
                return m_rotationSpeed;
            }
            set
            {
                m_rotationSpeed = value;
            }
        }

    }
}
