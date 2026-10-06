## Local Setup & Installation

### Prerequisites
* **Unity Hub**
* **Unity Editor:** `2022.3.36f1` (LTS)
* **Git LFS:** Make sure Git LFS is installed before cloning to pull large binary assets.

**Local Setup Instructions:**  
1. Clone the repository with Git LFS enabled.  
2. Open the project in Unity version **`2022.3.36f1`**.  
3. In the Project window, navigate to `Assets/Scenes/0-Base/` and open **`IntroCutscene`**.  
4. Press **Play** to start the application.



## Building the Standalone Executable (Full Game Experience)

To experience the full game without editor overhead, dynamic resolution scaling, or input capture constraints, build a standalone executable:

1. In the Unity Editor, go to **File** > **Build Settings...** (`Ctrl + Shift + B` / `Cmd + Shift + B`).
2. **Verify Scenes in Build:**
   * Ensure `IntroCutscene` is checked and placed at index `0` so the game initializes correctly.
   * Verify all subsequent gameplay/level scenes are checked in the list.
3. **Platform Selection:**
   * Select **Windows, Mac, Linux** (or your target OS).
   * Target Platform: **Windows** (Architecture: `x86_64`).
4. Click **Build**.
5. Choose or create an empty destination folder (e.g., `Builds/`).
6. Once the build completes, navigate to the output folder and launch the generated `.exe` file.
