# Campus Callouts
**Version 1.0.0**  
**By Seerside Studios**

> This project is **open source**! If you use any part of this code or build off this callout pack,
> please give appropriate **credit to Seerside Studios** in your work.

---

## Table of Contents
- [Introduction](#introduction)  
- [Requirements](#requirements)  
- [Integrations](#integrations)  
- [Installation](#installation)  
- [Callout Overview](#callout-overview)  
- [How to Use](#how-to-use)  
- [CampusCallouts.ini Configuration](#campuscalloutsini-configuration)  
- [About Me](#about-me)  
- [Support and Community](#support-and-community)  
- [Credits](#credits)  

---

## Introduction

Welcome to **Campus Callouts**!  
This callout pack brings the dynamic, chaotic, and sometimes hilarious world of college campus policing to LSPDFR. From peaceful protests that could go south, to bizarre clown sightings and serious emergencies, this pack aims to be immersive, entertaining, and unique every time you play.

This is my first major release under Seerside Studios for LSPDFR, and I'm beyond excited to finally share it with the community! Each callout is crafted with research, time, and just the right amount of unpredictability.

---

## Requirements

- Latest and legal copy of **Grand Theft Auto V**  
- **LSPDFR 0.4.9** or newer  
- Latest version of **RagePluginHook**  
- Latest version of **CalloutInterfaceAPI.dll** (Included)

---

## Integrations

- **StopThePed** – For managing traffic, arrests, and AI behavior  
- **Callout Interface** – Enables immersive MDT pop-ups and callout details  
- **Blueline Dispatch (Optional)** – Enables immersive dispatch audio (see `.ini` setup)

---

## Installation

1. Unzip the mod archive  
2. Drag the `lspdfr` and `plugins` folders into your GTA V main directory  
3. Overwrite if prompted  
4. Launch GTA V via RagePluginHook and go on duty  

---

## Callout Overview

**Campus Callouts** includes 15 immersive, fully-scripted scenarios built for a university setting:

- **Drone Use** – Investigate unauthorized UAV activity on campus  
- **Fight** – Break up a student brawl outside a lecture hall *(from University Callouts)*  
- **Hit and Run** – A student is struck near a parked car; find the suspect  
- **Noise Complaint** – Respond to a loud dorm party *(from University Callouts)*  
- **Stalking** – Interview a victim being followed on campus *(from University Callouts)*  
- **Student Escort** – Safely walk a vulnerable student across campus *(from University Callouts)*  
- **Trespasser** – A suspicious individual is inside a restricted school zone  
- **Underage Drinking** – Deal with a group of underage students drinking *(from University Callouts)*  
- **Vandalism** – Investigate graffiti or property damage at a school landmark  
- **Weapon Violation** – Armed student near a gym; proceed with caution *(from University Callouts)*  
- **Intoxicated Student** – Handle a severely drunk student in public  
- **Protest on Campus** – A protest may escalate; you may need to secure the dean  
- **Killer Clown 911 Call** – A clown is reportedly threatening students  
- **Missing Person** – Track down a missing student through clues across the map  
- **School Shooter** – A critical active threat scenario requiring immediate response  

---

## How to Use

After going on duty in LSPDFR, Campus Callouts loads automatically.

To manually start a callout:  
Press `F4` and type:  
```
StartCallout CalloutName
```

---

## CampusCallouts.ini Configuration

The `.ini` file is located in `Plugins/LSPDFR/CampusCallouts.ini`. It allows you to:

- Customize dialogue key  
- Toggle specific callouts  
- Enable/disable custom audio

###  Important Setting

```
UseBluelineDispatchAudio = False
```

**Only** set this to `True` if you use **Blueline Dispatch**, as some audio relies on their custom files. Otherwise, you may experience missing sound.

---

## About Me

Hey there! I'm the solo developer behind **Seerside Studios**.  
I started as a VR modder for Blade and Sorcery, where I crafted immersive mods that earned nearly **70,000 unique downloads**!

This is my **first LSPDFR release** and also my introduction to **C# programming** (with prior experience in JavaScript, HTML, and CSS). Campus Callouts is a passion project that blends realism, humor, and chaos into a fresh GTA V policing experience.

Check out my previous work here:  
https://next.nexusmods.com/profile/SeersideStudios?gameId=4124

---

## Support and Community

Need help or want to get involved?

Join the Discord Server:  
https://discord.gg/7ngNaDJbfW

View the open-source code:  
https://github.com/SeersideStudios/CampusCallouts

---

## Credits

A huge thank you to everyone who helped bring this to life:

- **AbelGaming** – Creator of University Callouts and graciously gave full permission to build on his foundation  
  - Original callouts adapted: Fight, Noise Complaint, Stalking, Student Escort, Underage Drinking, Weapon Violation  
  - [University Callouts](https://www.lcpdfr.com/downloads/gta5mods/scripts/37722-university-callouts/)

- **Jon Jon Games** – Scripting advice and guidance  
- **Opus and Charlie 686** – CalloutInterface troubleshooting  
- **All the amazing devs and testers who answered my DMs!**  
- **Audio Credits** – See the `Audio` folder for a full breakdown

---

Thank you for trying Campus Callouts! I can’t wait to hear what you think 
