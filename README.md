# GetEPGs
This Windows application gets XMLTV EPGs from custom web sources and merges them together. 

## Use Case
Can be used e.g. with Kodi's [IPTV Simple Client](https://kodi.wiki/view/Add-on:PVR_IPTV_Simple_Client) add-on or any other tool that may use XMLTV EPGs as a local file. 

## Installation

Download the zip file from the [releases](https://github.com/dichternebel/GetEPGs/releases). Extract the files into a folder where the current user has permissions to write into and the application you are using able to read.
A good practice is to create e.g. `%USERPROFILE%\GetEPGs\` for that.

There should be these files in the folder now:

| `%USERPROFILE%\GetEPGs\`  
|-- `epg_sources.txt`  
|-- `GetEPGs.exe`  
|-- `xmltv.dtd`  

Open the `epg_sources.txt` and add your custom web sources of downloadable XMLTV EPG files.

## Dependencies

Needs 7-zip to work!

### Option 1
Easiest way is to download and install 7-zip from here https://www.7-zip.org/.

### Option 2
If installation is not suitable you may download the portable version from here https://portableapps.com/apps/utilities/7-zip_portable and either put the `7z.exe` and `7z.dll` into the application folder or you may also pass a custom path to the application for example:

`GetEPG 7zpath "%USERPROFILE%\7-ZipPortable\App\7-Zip64"`

This is not stored into a configuration so you have to pass this argument each time you are using the app.

## Usage
### Run (at least once) manually

You can simply run this manually by double clicking the .exe file. When running for the first time you will notice that it will create a folder named `output`. This is where the merged EPG will be stored as `XML` and `gZip`.

| `%USERPROFILE%\GetEPGs\output\`  
|-- `epg.xml`   
|-- `epg.xml.gz`  


### Use Windows Task Scheduler for automation
A daily scheduled task can be created from the `cmd\terminal` with  
`GetEPG install-task`  
To remove the task from the task scheduler use  
`GetEPG remove-task`

This will create a task that will run once a day. If you are familiar with the Windows Task Scheduler you can create a task manually.

**Hint**: To use a custom path for 7-zip you have to manually edit the task in the Windows Task Scheduler app by adding an argument named `7zpath` like described above in `Option 2`.

## Integrate

When using `IPTV Simple Client` go to the settings section of the plugin and navigate to the EPG settings.
Change the location of the EPG file to be locally and point to the `.gz` file in the output folder like `%USERPROFILE%\GetEPGs\output\epg.xml.gz`


Enjoy!
