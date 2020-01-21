# FX3 Servo Controller

This library utilizes the Analog Devices iSensor FX3 platform to create a robust, easy to use servo controller interface.

See: https://github.com/juchong/iSensor-FX3-API

Features:
* Up to 8 servos run by a single controller
* Configurable motion paths, with variable rotation speed across a single motion path
* Continuous motion path execution or single shot motion path execution
* Asyncronous motion path execution with events generated following a path completion.
* Configurable PWM frequency (default is industry standard 20ms period, supports down to 100us)
* Configurable position update interval
* Selectable 5V or 3.3V supply for servo motors using FX3 API