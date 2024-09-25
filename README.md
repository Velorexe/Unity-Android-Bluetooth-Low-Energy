# Unity Android Bluetooth Low Energy

<p align="center">
    A Unity Android plugin to support basic Bluetooth Low Energy interactions<br>
    <i>Basis for my thesis on applying wearables to Virtual Reality Rehabilitation Programs in Android Virtual Reality devices</i>
</p>

<p align="center">
    <img src="https://i.imgur.com/fL3ybma.png" style="width:40%;">
</p>

## Features

Support for basic BLE operations

* Discovering Devices
* Connecting to a Device
* Writing to a Characteristic
* Subscribing to a Characteristic
* Reading from a Characteristic
* Writing to a Characteristic

## How It Works

By Utilizing the Android JNI and the capabilities for Android plugins on Unity to interact with the MonoBehaviour ecosystem, the .NET side inside Unity can communicate with the plugin, while the plugin can communicate back using the MonoBehaviour `SendMessage` method. The plugin contains a Singleton pattern that gets called from the `BleManager` script as an `AndroidJavaObject`. With this object you can call static methods and pass parameters, which takes care of communication from Unity to Android's BLE system. The plugin will then operate and send changes to Unity using the MonoBehaviour `SendMessage` method to pass JSON information to the `BleAdapter`.

Most of the BLE operations utilize the .NET event system, so information can be passed through all of the classes that need it. The BLE operations rely on the abstract `BleCommand` class. This class can be queued up into the `BleManager`, which executes the queued up commands one by one, or removes them from the queue if they're extending their `TimeOut` time.

If you're interested, I've written an article about the inner workings of the library over on my website. You can check it out over here: [Creating an Android Bluetooth Low Energy plugin for Unity](https://velorexe.com/posts/unity-bluetooth-low-energy/).

## Contact

If you need any information, have questions about the project or found any bugs in this project, please create a new `Issue` and I'll take a look at it! If you've got more pressing questions or questions that aren't related to create an Issue for, you can contact with the methods below.

* Discord: Velorexe#8403
* Email: degenerexe.code@gmail.com
