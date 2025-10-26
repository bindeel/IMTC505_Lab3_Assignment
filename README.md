# IMTC 505 – Lab 3: Augmented Reality Interactions

**Course:** IMTC 505 – Immersive Technologies  
**Assignment:** Lab 3 – AR Interactions  
**Team Members:**  
- Eléna Binderup – *Cooking Timer Interaction*  
- Mark Liu – *Drop Cube Interaction*  
- Mahdieh Sadat – *Pinch & Rotate Cube Interaction*  
- Lei Chen – *AR Dog Placement & Movement*

---

## 🎯 Overview

This project demonstrates **four interactive augmented reality experiences**, each implemented independently by a different team member using Unity’s **AR Foundation** and the **XR Origin (Mobile AR)** template.

Each interaction is built as a separate prefab or scene, using AR plane detection, raycasts, and touch input to create immersive, real-world digital experiences. All features are designed to run on Android devices.

---

## 🧭 How to Run

### Requirements
- **Unity 2022.3 LTS** or later  
- **AR Foundation (v5.x)**  
- **ARCore XR Plugin (v5.x)**  
- **Android Build Support** with SDK, NDK, and OpenJDK

### Setup
1. Clone or download this repository.  
2. Open the project in Unity.  
3. Ensure XR Plug-in Management is enabled for Android with **ARCore** checked.  
4. Connect an Android device with Developer Mode and USB Debugging enabled.  
5. Select any of the team scenes (e.g., `ElenaScene.unity`, `MarkScene.unity`, `MahdiehScene.unity`, `LeiChenScene.unity`).  
6. Go to **File → Build and Run** to deploy the app.

---

## 🧩 Interactions

### 🕒 Cooking Timer Interaction – *Eléna Binderup*
A placeable AR **cooking timer** that floats above real-world kitchen appliances.  
Users can spawn a timer with a reticle and control countdown functions with tap gestures.

**How to Play**
- Move your device to detect flat surfaces.  
- Tap once to spawn the timer.  
- Tap ➤ to play/pause.  
- Tap ▲ or ▼ to add or remove 30 seconds.

**Implementation Highlights**
- *TimerSimple.cs* manages countdown logic and color state.  
- *ButtonAction.cs* handles button interaction types.  
- *Billboard.cs* ensures the timer card always faces the camera.  
- *TimerPlacer.cs* handles plane detection and prefab placement.  

---

### 🧊 Drop Cube Interaction – *Mark Lie*
An interaction that lets users **drop a cube** from their tap position into the AR scene.  
The cube falls with simulated gravity, collides with detected planes, and comes to rest realistically.

**How to Play**
- Tap anywhere on the screen to drop a cube.

**Implementation Highlights**
- Manual gravity integration in `Update()`.  
- Uses `SphereCast` to detect collisions with AR planes.  
- Snaps to the plane and stops movement on contact.  
- Shrinks slightly while falling for visual realism.  
- Freezes mid-air if no plane is hit after a short time.

---

### 🎨 Pinch & Rotate Cube Interaction – *Mahdieh Sadat*
A refined, interactive cube that responds to gestures for a more dynamic and realistic AR experience.

**How to Play**
- Tap to place the cube on a plane.  
- Tap again to change its color randomly.  
- Pinch two fingers to scale it up or down.  
- Twist two fingers to rotate it around the Y-axis.

**Implementation Highlights**
- *ShinyMat* (URP Material): Metallic (≈0.2), Smoothness (≈0.8) for realistic lighting.  
- *TapChangeColor.cs*: Changes the cube’s color randomly on tap.  
- *PinchToScale.cs*: Dynamically scales based on two-finger distance ratio.  
- *TwoFingerRotate.cs*: Rotates using finger angle difference around Y-axis.  
- Scripts attached to the cube prefab so each instance can be manipulated independently.

---

### 🐕 AR Dog Placement & Movement – *Lei Chen*
A fun AR experience where users can **place and move a Shiba Inu dog** in their environment.

**How to Play**
- Scan to detect surfaces.  
- Tap once to place the dog.  
- Tap again to make it walk to the tapped location.

**Implementation Highlights**
- *DogMover.cs*: Handles movement, rotation, and animation state.  
- *TapToPlaceAndMoveDog.cs*: Controls placement and navigation via AR raycasts.  
- *DogController* Animator: Transitions between Idle ↔ Walk states.  
- Uses `ARRaycastManager` and `ARPlaneManager` for plane detection and movement.

---

## 🧠 Technical Notes

- All interactions use Unity’s **XR Origin (Mobile AR)** prefab with:
  - `ARRaycastManager`
  - `ARPlaneManager`
  - (Optional) `ARAnchorManager`
- Each interaction is isolated in its own prefab or scene to avoid conflicts.
- Device input uses **Touch Input (Input.touches)** for natural mobile gestures.

---

## 📱 Demo & Deliverables

All detailed explanations, implementation notes, and screenshots are available in this shared document:  
🔗 **[Full Lab 3 Report & Explanations (Google Docs)](https://docs.google.com/document/d/123NqOU2XMpAGBi69Dbk4kaUpwmZiIsYm6S2f3w9v4Xk/edit?usp=sharing)**

### 🎥 Demo Video
A full demo video showing all interactions in action can be viewed here:  
👉 **[YouTube Demo Video](https://www.youtube.com/watch?v=pmxDZvxjM9I&feature=youtu.be)**
 
- Proper GitHub commit history with feature branches:

  - `cooking-timers-elena`  
  - `drop-item-on-floor`  
  - `Pinch-to-scale-and-rotate-mahdieh`  
  - `Lei-Chen-Branch`

---

## 🧾 License

This project is for **educational purposes only** as part of UBC Okanagan’s IMTC 505 course.  
All 3D assets and scripts are original or used under free/educational licenses.

---




