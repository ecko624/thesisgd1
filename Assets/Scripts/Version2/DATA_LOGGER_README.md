# Data Logger System - Setup Guide

## Overview
The Data Logger system automatically tracks all player choices and NPC responses during gameplay. Each interaction is logged to a CSV file with timestamps.

## Features
✅ Automatic logging of player choices and NPC responses
✅ CSV format for easy analysis in Excel/Google Sheets
✅ Automatic reset on game start
✅ Thread-safe file operations
✅ Timestamps for each interaction
✅ NPC personality tracking

## Files Created

### 1. **DataLogger.cs**
Main logging system that:
- Hooks into DialogueControllerVersion2's OnModelResponse event
- Writes interactions to CSV file
- Resets log on game start
- Stores log in persistent data directory

### 2. **DataLoggerHelper.cs** (Optional)
Helper script for:
- Opening the log folder from code
- Accessing the log file path
- Debugging purposes

### 3. **DialogueControllerVersion2.cs** (Modified)
Updated to:
- Call `DataLogger.UpdateLastPlayerChoice()` when player submits input
- Pass player choices to the logger

## Setup Instructions

### Step 1: Add DataLogger to Your Scene
1. Create a new empty GameObject in your main scene (the one where dialogues happen)
2. Name it "DialogueLogger" or similar
3. Attach the `DataLogger` script to this GameObject
4. In the Inspector, drag your `DialogueControllerVersion2` instance into the "Dialogue Controller" field

### Step 2 (Optional): Add Helper Script
1. Optionally attach `DataLoggerHelper.cs` to any GameObject
2. Use it to debug and access the log folder

### Step 3: Test
1. Run your game
2. Have a conversation with an NPC
3. Make choices and let the NPC respond
4. Each interaction will be logged

## CSV Output Format

The CSV file (`dialogue_log.csv`) will be located at:
- **Windows**: `C:\Users\[YourUsername]\AppData\LocalLow\DefaultCompany\ThesisDatingSimulator\DialogueLogs\dialogue_log.csv`
- **Mac**: `~/Library/Application Support/DefaultCompany/ThesisDatingSimulator/DialogueLogs/dialogue_log.csv`
- **Linux**: `~/.config/unity3d/DefaultCompany/ThesisDatingSimulator/DialogueLogs/dialogue_log.csv`

### Example CSV Content:
```
Timestamp,NPC,PlayerChoice,NPCResponse
2025-11-19 14:32:15.123,aya,Hey there!,"Oh! You came~ ♡ I was hoping to see you today"
2025-11-19 14:32:28.456,aya,How are you doing?,I'm doing well! Thanks for asking
2025-11-19 14:32:45.789,mika,Hi Mika,"*glances up from her book* Oh... it's you."
```

## How It Works

1. **Game Start**: DataLogger.Start() initializes and **resets** the CSV file with headers
2. **Player Input**: When player submits a choice, `DataLogger.UpdateLastPlayerChoice()` is called
3. **NPC Response**: DialogueControllerVersion2 receives response and triggers `OnModelResponse` event
4. **Logging**: DataLogger.LogInteraction() writes the row to CSV with timestamp, NPC name, player choice, and NPC response

## Important Notes

⚠️ **The CSV is reset every time you start the game** - This is intentional so you only log current session data.

💡 If you want to **keep previous logs**, you can:
1. Modify DataLogger.ResetLog() to create timestamped files instead
2. Export/backup the CSV before closing the game

## Troubleshooting

**Issue**: "DataLogger not found in scene"
- **Solution**: Make sure you attached DataLogger to a GameObject in your scene with DialogueControllerVersion2 assigned

**Issue**: CSV file not being created
- **Solution**: Check that the game has write permissions in the Application.persistentDataPath directory

**Issue**: Player choices showing as "Unknown"
- **Solution**: Make sure GetNPCResponse() is being called properly in DialogueControllerVersion2

## Accessing the Log File Programmatically

```csharp
// Get the data logger
DataLogger logger = FindObjectOfType<DataLogger>();

// Get the file path
string logPath = logger.GetLogFilePath();

// Open the folder
logger.OpenLogFolder();
```

## Extending the Logger

To add more data columns, modify the CSV header in `DataLogger.ResetLog()`:

```csharp
writer.WriteLine("Timestamp,NPC,PlayerChoice,NPCResponse,IntimacyLevel,SessionID");
```

Then update `LogInteraction()` to include additional data:

```csharp
writer.WriteLine($"{timestamp},{npc},{choice},\"{response}\",{intimacyLevel},{sessionID}");
```
