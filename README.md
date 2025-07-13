
# Campus Callouts
**v1.0.1**  
_by Seerside Studios_  

---

## Table of Contents
1. [Introduction](#introduction)
2. [Requirements](#requirements)
3. [Integrations](#integrations)
4. [Installation](#installation)
5. [Troubleshooting Protest Callout](#troubleshooting-protest-callout)
6. [Updating Campus Callouts](#updating-campus-callouts)
7. [Callout Overview](#callout-overview)
8. [How to Use](#how-to-use)
9. [CampusCallouts.ini Configuration](#campuscalloutsini-configuration)
10. [About Me!](#about-me)
11. [Support and Community](#support-and-community)
12. [Credits](#credits)

---

## Introduction
Welcome to **Campus Callouts!** This callout pack brings the dynamic, chaotic, and sometimes hilarious world of college campus policing to LSPDFR. From peaceful protests that could go south, to bizarre clown sightings and serious emergencies, this pack aims to be immersive, entertaining, and unique every time you play.

This is my first major release under Seerside Studios for LSPDFR, and I'm beyond excited to finally share it with the community! Each callout is crafted with research, time, and just the right amount of unpredictability.

---

## Requirements
- Latest and legal copy of Grand Theft Auto V  
- LSPDFR 0.4.9 or newer  
- Latest version of RagePluginHook  
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
2. Drag the `lspdfr` and `plugins` folders directly into your GTA V main directory.  
3. Allow file replacement if prompted.  
4. Launch GTA V via RagePluginHook and go on duty to begin receiving callouts.  

---

## Updating Campus Callouts
To update from a previous version of Campus Callouts:

1. Backup your `CampusCallouts.ini` file if you've made custom changes.  
2. Delete the old `CampusCallouts - Audio` folder inside `LSPDFR/Audio/Scanner/`.  
   - _This is important! Some older audio files may no longer be used in newer versions._  
3. Drag and drop the new files (including the updated `lspdfr` and `plugins` folders and any external `.dll` files) into your GTA V root directory.  
4. Replace files when prompted.  
5. Launch the game.  

---

## Troubleshooting Protest Callout
The **Protest on Campus** callout uses background protest audio that plays in a loop using **NAudio**. If the music doesn’t play or your game crashes at the scene, here are a few things to check:

1. Make sure you’ve included **NAudio.dll** in your `plugins` folder.  
   - Without this, music playback won't function and may cause issues.  
2. Confirm the protest music file is present.  
   - It should be located at: `LSPDFR/Audio/Scanner/CampusCallouts - Audio/Protest/CC_PROTEST_AUDIO.wav`  
   - If this file is missing, the protest scene will still function but no background music will play.  
3. Ensure you're not manually deleting or moving any NAudio dependencies.  

---

## Callout Overview
Campus Callouts includes **15 fully scripted, immersive scenarios** designed to make you feel like you’re in a university environment:

- **Drone Use** – Investigate unauthorized UAVs over campus.  
- **Fight** – Break up an altercation between students. *(Adapted from University Callouts)*  
- **Hit and Run** – Locate a suspect vehicle after a student is struck.  
- **Noise Complaint** – Respond to loud parties and determine compliance. *(Adapted)*  
- **Stalking** – Interview a student who believes they are being followed. *(Adapted)*  
- **Student Escort** – Safely escort a vulnerable student across campus. *(Adapted)*  
- **Trespasser** – Respond to reports of someone in restricted areas.  
- **Underage Drinking** – Manage an illegal party involving minors. *(Adapted)*  
- **Vandalism** – Investigate graffiti or property damage.  
- **Weapon Violation** – Respond to a reported weapon on school grounds. *(Adapted)*  
- **Intoxicated Student** – Help or arrest a severely drunk student.  
- **Protest on Campus** – Navigate a tense student protest with multiple possible outcomes.  
- **Killer Clown 911 Call** – Respond to sightings of a hostile clown.  
- **Missing Person** – Investigate leads to find a missing student.  
- **School Shooter** – High-risk scenario involving an active shooter.  

---

## How to Use
Once you’re on duty in LSPDFR, Campus Callouts will load automatically!

Callouts can be triggered by the game, or started manually using a callout manager or the console:  
**Press F4 and type:** `StartCallout CalloutName`  

---

## CampusCallouts.ini Configuration
The `CampusCallouts.ini` file is located inside `plugins > LSPDFR`. You can:

- Adjust your dialogue key  
- Enable or disable specific audio packages  
- Enable or disable callouts  

**UseBluelineDispatchAudio**  
Only enable this (set to `True`) if you use Blueline Dispatch. Otherwise, leave it `False` to avoid missing audio.

---

## About Me!
Hi! I'm the solo developer behind **Seerside Studios**. I started out creating immersive VR content for *Blade and Sorcery*, which has racked up over 70,000 downloads! Now I’m exploring GTA V modding with the same passion.

Campus Callouts is my first LSPDFR mod and also my intro to **C# programming** (with past experience in JS, HTML, CSS).

🔗 [My Nexus Mods Profile](https://next.nexusmods.com/profile/SeersideStudios?gameId=4124)

---

## Support and Community
Need help? Want to suggest a new callout?  
Join the **official Discord** for support, sneak peeks, and updates:  
🔗 https://discord.gg/7ngNaDJbfW

Or visit the GitHub repo for this **open-source** project:  
🔗 https://github.com/SeersideStudios/CampusCallouts

---

## Credits
Huge thanks to the people who made this possible:

- **AbelGaming** – Original creator of *University Callouts* (Fight, Noise Complaint, Stalking, Escort, etc.)  
  🔗 https://www.lcpdfr.com/downloads/gta5mods/scripts/37722-university-callouts/  
- **Jon Jon Games** – Logic and scripting advice  
- **Opus & Charlie686** – CalloutInterface help and QA feedback  
- **Everyone who responded to my DMs** – Thanks for the support!  
- **Audio** – See `Audio README` inside the folder for attribution  
