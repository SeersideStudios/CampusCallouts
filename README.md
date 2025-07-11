# Campus Callouts  
**v1.0.0**  
*by Seerside Studios*

---

## Table of Contents  
1. [Introduction](#introduction)  
2. [Requirements](#requirements)  
3. [Integrations](#integrations)  
4. [Installation](#installation)  
5. [Troubleshooting: Protest Music](#troubleshooting-protest-music)  
6. [Callout Overview](#callout-overview)  
7. [How to Use](#how-to-use)  
8. [CampusCallouts.ini Configuration](#campuscalloutsini-configuration)  
9. [About Me](#about-seerside-studios)  
10. [Support and Community](#support-and-community)  
11. [Credits](#credits)

---

## Introduction

Welcome to **Campus Callouts**! This callout pack brings the dynamic, chaotic, and sometimes hilarious world of college campus policing to LSPDFR. From peaceful protests that could go south, to bizarre clown sightings and serious emergencies, this pack aims to be immersive, entertaining, and unique every time you play.

This is my first major release under **Seerside Studios** for LSPDFR, and I'm beyond excited to finally share it with the community! Each callout is crafted with research, time, and just the right amount of unpredictability.

---

## Requirements

- Latest and legal copy of **Grand Theft Auto V**
- **LSPDFR 0.4.9** or newer
- Latest version of **RagePluginHook**
- Latest version of **CalloutInterfaceAPI.dll** (Included in the download)

---

## Integrations

- **StopThePed** – For managing traffic, arrests, and AI behavior  
- **Callout Interface** – Enables immersive MDT pop-ups and tracking during active callouts  
- **Blueline Dispatch (Optional)** – Enables immersive dispatch audio lines (see INI instructions)

---

## Installation

1. Unzip the mod archive.  
2. Drag the `lspdfr` and `plugins` folders directly into your GTA V main directory.  
3. Allow file replacement if prompted.  
4. Launch GTA V via RagePluginHook and go on duty to begin receiving callouts.

---

## Troubleshooting: Protest Music

The protest callout includes custom background music that plays using **Windows Media Player** (WMP). If the game freezes or the music doesn’t play, this may be caused by WMP not being installed or available on the system.

### To resolve this:

#### 1. Check if Windows Media Player is installed
- Open the **Start Menu** and search for `Windows Media Player`
- If it opens, you're all set

#### 2. Enable Windows Media Player (most users)
- Go to `Control Panel → Programs → Turn Windows features on or off`
- Find **Media Features** and make sure **Windows Media Player** is checked
- Click OK and restart your computer

#### 3. If using Windows N or KN editions
These versions do **not** include WMP. You must install the **Media Feature Pack**:

- Download it from Microsoft:  
  https://support.microsoft.com/en-us/help/3145500/media-feature-pack-list-for-windows-n-editions  
- Pick the version that matches your OS
- Install and restart

After completing these steps, the protest music should play without issues. If the music still doesn't play, the callout will continue without crashing or freezing.

---

## Callout Overview

**Campus Callouts** includes 15 immersive and fully-scripted scenarios:

- **Drone Use**  
- **Fight** (from University Callouts)  
- **Hit and Run**  
- **Noise Complaint** (from University Callouts)  
- **Stalking** (from University Callouts)  
- **Student Escort** (from University Callouts)  
- **Trespasser**  
- **Underage Drinking** (from University Callouts)  
- **Vandalism**  
- **Weapon Violation** (from University Callouts)  
- **Intoxicated Student**  
- **Protest on Campus**  
- **Killer Clown 911 Call**  
- **Missing Person**  
- **School Shooter**

---

## How to Use

Once you're on duty in LSPDFR, Campus Callouts will load automatically!

You can also start callouts manually using the console:
F4 -> StartCallout CalloutName


---

## CampusCallouts.ini Configuration

Inside the `plugins > LSPDFR` folder, you'll find `CampusCallouts.ini`.

You can use this file to:
- Change your **dialogue key**
- Toggle **Blueline Audio** support
- Enable or disable individual **callouts**

### `UseBluelineDispatchAudio`
Set to `True` only if you're using **Blueline Dispatch**. This enables support for its custom audio lines. Otherwise, leave it `False`.

---

## About Seerside Studios

Hi! I'm the solo developer behind Seerside Studios. I started my journey in VR modding with **Blade and Sorcery**, which gained nearly **70,000 unique downloads**. Now, I’m diving into GTA V modding with the same energy and passion.

**Campus Callouts** is my first LSPDFR mod, and also my intro to C# development — my background was originally in JavaScript, HTML, and CSS!

Check out my previous work:  
https://next.nexusmods.com/profile/SeersideStudios?gameId=4124

---

## Support and Community

Need help or have suggestions?  
Join the **official Discord server** for updates, troubleshooting, and community input:  
https://discord.gg/7ngNaDJbfW

This project is open-source!  
GitHub: https://github.com/SeersideStudios/CampusCallouts

---

## Credits

Massive thanks to the people and tools that made this possible:

- **AbelGaming** – Original creator of *University Callouts*. I had full permission to expand on his work.
  - [University Callouts on LCPDFR](https://www.lcpdfr.com/downloads/gta5mods/scripts/37722-university-callouts/)
- **Jon Jon Games** – Scripting guidance
- **Opus & Charlie 686** – CalloutInterface feedback
- **Everyone who responded to my DMs** – Your help mattered!
- **Audio Credits** – See `README_Audio.txt` in the `Audio` folder for attribution

---
