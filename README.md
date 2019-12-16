# Urbanice!

### Information
---

*Urbanice!* is a prototype for a bachelor thesis project developed by Daniel Steegmüller in 2019 for the *Technische Hochschule Lübeck* (THL). It is written in C# for Unity3D version 2019.1.10. The project is hosted on GitHub (https://github.com/Cxyda/Urbanice/) under the MIT license.

### How to install
---

- This project can be downloaded or cloned to you computer

- You should use the UnityHub application to manage your installed unity versions. The UnityHub can be downloaded here (https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.dmg?_ga=2.199271726.1683222043.1576479010-531499030.1522929414)
- After UnityHub has been installed you can click on installs ((1) see screenshot below) and add Unity Version **2019.1.f10**
- Now you can Add (2) the downloaded project and double click (3) to open it
![howToUse_01](https://github.com/Cxyda/Urbanice/blob/master/screenshots/screenshot_002.png)
- Unity should import / recompile the project files

### How to use
---

- *Urbanice!* uses Unity's *ScriptableObjects*
- All ScriptableObjects can be found in the **Assets/Content/Configs/** folder
- Open the DemoScene-scene where everything is setup

![howToUse_01](https://github.com/Cxyda/Urbanice/blob/master/screenshots/screenshot_001.png)
- Your Unity should look similar to the screenshot above. If it doesn't and you want to reset your window layout click Window -> Layouts -> Default
- (1) Is the Hierarchy-Window where you can see all Objects which are currently in the scene, **if you opened the DemoScene.scene file**
- To Open the DemoScene.scene file double click in the the Project-Window (2). It is located below the **Assets/Scenes/** folder.
- Urbanice! uses Scriptable objects as data containers. They are all located below Assets/Content/ (7) folder. You can select any of those files and modify parameters in the inspector-Window (3)
- After you changed parameters, you have to select the CityGenerator GameObject (5) and select GenerateCity (8).
- Afterwards you should see the generated city in the Scene-Window (7)
- The CityRenderer GameObject (4) renders the city map. You can select / deselect different features of the city.

### Screenshots
---

![exampleCity_01](https://github.com/Cxyda/Urbanice/blob/master/screenshots/exampleCity_01.png)
![exampleCity_02](https://github.com/Cxyda/Urbanice/blob/master/screenshots/exampleCity_02.png)
![exampleCity_03](https://github.com/Cxyda/Urbanice/blob/master/screenshots/exampleCity_03.png)
![exampleCity_04](https://github.com/Cxyda/Urbanice/blob/master/screenshots/exampleCity_04.png)
![exampleCity_05](https://github.com/Cxyda/Urbanice/blob/master/screenshots/exampleCity_05.png)
![exampleCity_06](https://github.com/Cxyda/Urbanice/blob/master/screenshots/exampleCity_06.png)
