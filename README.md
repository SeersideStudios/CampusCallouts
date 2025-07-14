# Campus Callouts  
**v1.0.1**  
**by Seerside Studios**  

---

## 🔓 Open Source Notice

Campus Callouts is an open-source mod. I've done this as I know all too well the lack of help out there (apart from a few special people). This is designed for anyone to learn from and use to help create their own callout pack. Good luck, you aspiring devs 🥺  
You are **free to view, learn from, and reuse parts of this code** in your projects **with credit**.

If you use parts of this plugin or adapt its callouts or logic, please credit **Seerside Studios**.  

---

## Table of Contents  
1. [Introduction](#introduction)  
2. [Requirements](#requirements)  
3. [Integrations](#integrations)  
4. [Installation](#installation)  
5. [Updating Campus Callouts](#updating-campus-callouts)  
6. [Callout Overview](#callout-overview)  
7. [How to Use](#how-to-use)  
8. [CampusCallouts.ini Configuration](#campuscalloutsini-configuration)  
9. [About Seerside Studios](#about-seerside-studios)  
10. [Support and Community](#support-and-community)  
11. [Credits](#credits)  

---

## Introduction

Welcome to Campus Callouts! This callout pack brings the dynamic world of college campus policing to LSPDFR.  
From peaceful protests that could go south, to bizarre clown sightings and serious emergencies, this pack aims to be immersive, entertaining, and unique every time you play.

This is my first major release under Seerside Studios for LSPDFR, and I'm beyond excited to finally share it with the community.  
Each callout is crafted with research, time, and just the right amount of unpredictability.

---

## Requirements

- Latest and legal copy of **Grand Theft Auto V**  
- **LSPDFR 0.4.9** or newer  
- Latest version of **RagePluginHook**  
- `CalloutInterfaceAPI.dll` (included in the download)  
- `NAudio.dll` (included in the download) – Required for callout music  

---

## Integrations

- **StopThePed** – For managing traffic, arrests, and AI behavior  
- **Callout Interface** – Enables immersive MDT pop-ups and tracking during active callouts  
- **Blueline Dispatch (Optional)** – Enables immersive dispatch audio lines (see INI instructions)  

---

## Installation

1. Unzip the mod archive.  
2. Drag the `lspdfr` and `plugins` folders, and the .dll files directly into your **GTA V main directory**.  
3. Allow file replacement if prompted.  
4. Go on duty in LSPDFR to begin receiving callouts.

---

## Updating Campus Callouts

To update from a previous version of Campus Callouts:

1. Backup your `CampusCallouts.ini` file if you've made custom changes.  
2. **Delete the old `CampusCallouts - Audio` folder** inside `LSPDFR/Audio/Scanner/`  
   - This is important as older audio files may no longer be used in the new version.  
3. Drag and drop the new `lspdfr` and `plugins` folders (including `NAudio.dll`) into your GTA V directory.  
4. Replace files when prompted.  
5. Launch the game and enjoy the updated version.  

---

## Callout Overview

Campus Callouts includes 15 fully scripted, immersive scenarios:

- **Drone Use** – Investigate suspicious UAV activity on campus.  
- **Fight** – Respond to two students physically fighting outside a lecture hall.  
- **Hit and Run** – Locate a suspect vehicle after a student is struck.  
- **Noise Complaint** – Address loud music coming from dorm parties.  
- **Stalking** – Assist a student who believes they're being followed.  
- **Student Escort** – Escort a vulnerable student safely across campus.  
- **Trespasser** – Deal with an unauthorized person on school grounds.  
- **Underage Drinking** – Handle reports of alcohol being served at student events.  
- **Vandalism** – Investigate graffiti or damage to a campus landmark.  
- **Weapon Violation** – Respond to reports of a student brandishing a weapon.  
- **Intoxicated Student** – Determine if medical or police intervention is needed.  
- **Protest on Campus** – A protest may escalate, or you may need to escort the dean.  
- **Killer Clown 911 Call** – Respond to a terrifying clown sighting.  
- **Missing Person** – Follow leads to find a student who has disappeared.  
- **School Shooter** – A high-risk active shooter response scenario.  

---

## How to Use

Once you’re on duty in LSPDFR, **Campus Callouts will load automatically**.

Callouts can also be manually triggered using the console: F4, then StartCallout CalloutName.


---

## CampusCallouts.ini Configuration

Located at:  
`plugins > LSPDFR > CampusCallouts.ini`

Options include:
- Change the **Dialogue Key**  
- Enable/disable **specific callouts**  
- Toggle use of **Blueline Dispatch Audio**  

### UseBluelineDispatchAudio  
Only set this to `True` if you use **Blueline Dispatch**.  
If left `False`, it will use the default scanner audio.

---

## About Seerside Studios

Hi! I'm the solo developer behind Seerside Studios.  
I started out creating immersive **Blade and Sorcery** mods in VR that received nearly **70,000 downloads**.  
Now, I’m diving deep into **GTA V modding**, bringing the same attention to detail and creativity.

This project is not only my **first LSPDFR release**, but also my first time using **C#**.  
Before this, I had experience in JavaScript, HTML, and CSS.

Check out more of my work:  
https://next.nexusmods.com/profile/SeersideStudios?gameId=4124

---

## Support and Community

Need help or want to contribute?

- **Discord Server:** [Join for support and updates](https://discord.gg/7ngNaDJbfW)  
- **GitHub Repository:** [View source or contribute](https://github.com/SeersideStudios/CampusCallouts)  

---

## Credits

Huge thanks to everyone who made this project possible:

- **AbelGaming** – Original creator of *University Callouts*, which served as a base for many scenarios.  
  https://www.lcpdfr.com/downloads/gta5mods/scripts/37722-university-callouts/  
- **Jon Jon Games** – For helping with scripting logic.  
- **Opus** & **Charlie 686** – For troubleshooting CalloutInterfaceAPI and providing essential feedback.  
- Everyone who offered feedback, testing help, or answered DMs – thank you.  
- **Audio Credits** – See the included README inside the `Audio` folder for voice line and sound attribution.  

