# SmashTracker

SmashTracker is a desktop user interface and local database tool built to log your match outcomes, analyze win rates, and keep detailed strategical notes for every character matchup in Super Smash Bros. Ultimate.

## What's New in Version 2.2.5 🚀

### 🔍 High-Performance Searchable Dropdowns
* **Intelligent Sorting Filter**: Dropdowns now automatically sort in real-time as you type, instantly floating exact or prefix matches to the top of the roster list.
* **Ultra-Responsive UX**: Eliminated typing lag and caret resetting by migrating the filtering mechanics to high-performance, in-memory LINQ queries, caching database lookups entirely.
* **Seamless Backspacing**: Retained focus states so you can seamlessly backspace through a selected name to alter a search parameter without clearing the textbox layout entirely.
* **Visual Polish**: Realigned selected elements for pixel-perfect centering, customized input fonts using `Cascadia Code`, and added an overlay rendering engine to preserve character stock icons directly on the editing surface.

### 📈 Character Overall Analytics
* **Global Performance Tracking**: Added a new **Character Overall** panel that calculates aggregate wins, aggregate losses, and total win rates across all matchups for your selected fighter.
* **Dynamic Refreshes**: The dashboard automatically recalculates totals instantly whenever a win or loss is logged or altered.

### 🛡️ Safety & Stability Fixes
* **Misclick Underflow Protection**: Hardened the decrement logic (`DEL WIN` / `DEL LOSS`) to verify counts before updating SQLite, preventing database records from dropping below zero if pressed accidentally.

---

## Core Features
* **Granular Matchup Mapping**: Select your fighter and the opponent's character via custom dropdowns. Win rates, game volume, and notes are completely individualized to that unique pair.
* **Precise Log Management**: Log new outcomes or roll back misclicks with dedicated `ADD WIN`, `ADD LOSS`, `DEL WIN`, and `DEL LOSS` actions.
* **Fighter Matchup Notes**: An expansive, multi-line notes section for keeping match strategy records, combos, and frame data reminders tailored to specific opponents.

> 📝 **Data Archiving Notice**: All application records are securely processed inside a single local SQLite database file (`SmashExpTracker.db`) generated on initial launch. **Deleting or displacing this file will permanently clear your record history.**

### Backward Compatibility & Migrations
All previous versions are completely compatible with version 2.2.5. Existing match records built on single-character architectures will automatically migrate safely to a structural `'UNKNOWN'` player identity row upon startup, preserving your notes and numbers intact.

---

## Setup & Installation

1. Download and unzip the target platform package from the latest Releases tab.
2. Run the executable file (`SmashTracker.exe`).
3. **Upgrading?** Extract the contents directly over your previous folder path without moving or deleting the existing `SmashExpTracker.db` database file to ensure your match history safely carries forward.
