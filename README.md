# OpenVRStartup
Automatically run command files on SteamVR startup and/or shutdown, download the latest release [here](https://github.com/BOLL7708/OpenVRStartup/releases).

# Usage
1. Decompress the release archive to a folder on disk.
2. Run the _.exe_ file in the folder.
3. On first launch it will display some instructions and the application will run the demo files included in the release.
4. Check the _.log_ file that is generated in the same folder, it should provide information about the execution.
5. Delete or edit the _*.cmd_ files in the _start_ and _stop_ folders or add new ones. The application will attemp to run any _*.cmd_ files in those folders.

Delete the generated _.log_ file to get the instructions in step 3 again on the next launch.

That's pretty much it. This is a very simple application. If you only have startup scripts it will terminate immediately after launch, if you have shutdown scripts it will put itself in the background until SteamVR exits so it can launch those scripts as well.
