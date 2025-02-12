# MkvM
A simple mkvmerge wrapper to clean the track titles of video files, in bulk!


This project is only possible thanks to the following open-source libraries and tools 💙:

- **mkvmerge**: Part of the [MKVToolNix](https://mkvtoolnix.download/) suite, used for creating and modifying MKV files.
- **mkvtoolnix**: A set of tools to work with Matroska files (MKV format).

#### This software is in beta. Use it at your own risk!

## Use Case
Videos are composed of tracks. A track could be of type video, audio, subtitle, chapters or attachement. Sometimes, video files are generated with random track names, either due to the recording device, source (DVD, BluRay), transcoding or ripping software, or people that reprocessed the file adding their own track names, like these ones:

![Dirty Track Name 2](https://i.ibb.co/Wsbbyzj/Screenshot-From-2025-01-18-20-50-43.png)

![Dirty Track Name 1](https://i.ibb.co/pnpg7jQ/Screenshot-From-2025-01-18-20-55-13.png)

This software processes all video files and removes specific "values" from the track name:

![Clean Track Name 1](https://i.ibb.co/kBj3vLs/Screenshot-From-2025-01-18-20-49-59.png)

![Clean Track Name 2](https://i.ibb.co/Y7rZrqR/Screenshot-From-2025-01-18-20-50-06.png)

Although you can do this with MKVToolNix, I did not found an option to do it in bulk. That's why this app exists.

## How to use it?

You can run this app locally by downloading the source code and compiling it, or in a Docker environment using the Docker image (recommended). A much more user-friendly documentation will be provided in the future, as well as binaries for those that do not want to build from the source code.

### Docker Image (recommended)
This app has a docker image in [Docker Hub - j4ssgg/mkvm](https://hub.docker.com/r/j4ssgg/mkvm).
I encourage you to use Docker Compose. The following example file should fit most of the cases. 

```
# Make sure to read and understand these settings before executing the app.
# This software is in beta. Use at your own risk!
# Depending on the configuration set, any processed file may be overwritten, so make sure to have a backup of your files.
# Default values are set as I use it when testing, no special meaning on them.
services:
  backend:
    image: 'j4ssgg/mkvm:beta' # This software is in beta. Use at your own risk!
    volumes:
      - /your/path/to/movies:/app/media               # This is the path to the folder where the app will look for the files to process
      - /your/path/to/documents:/app/config           # This is the path to the database file, to the track_names.txt file, and to the replacements file. The app will create the database if it does not exist.
    environment:
      - WorkingDirectory=/app/media                   # This is the default path the app will use to read your media; notice volume matches here ... you can change it to /media/subfolder if the volume contains subfolders, in case you only want to test a specific folder: /your/path/to/movies/subfolder
      - ConfigurationDirectory=/app/config            # This is the default value to read and save the configuration files, which includes the database file, the track_names.txt file, and the replacements file; notice volume matches here too.
      - DatabaseFile=mkvm.sqlite                      # Update this if you want to use a different name for the database file... if file does not exist, it will be created. 
      - ReplacementsFile=replacements.txt             # Update this if you want to use a different name for the replacements file... if file does not exist, no replacements will be done; see example in the README.md
      - TrackNamesFile=track_names.txt                # Update this if you want to use a different name for the track_names file... this is only created when ExtractTrackNamesOnly is set to True
      - ReplaceOriginal=False                         # If you want to replace the original file with the new one or generate a new file with a different name... name format is automatically generated to avoid conflicts
      - OverwriteExisting=False                       # If you want to overwrite the new file if it already exists - not sure if this will ever be useful, but it's there
      - IncludeAllExtensions=False                    # Make sure to not include all extensions if under WorkingDirectory there are files that are not videos, this will make the app to crash (I am too lazy to fix it at this point)
      - Extensions=.mkv,.mp4,.avi                     # Not regex supported, just comma separated extensions with dot, case-insensitive
      - IncludeSubFolders=True                        # If you want to include subfolders in the search or only process file in the WorkingDirectory
      - UpdateListOfFilesProcessed=True               # Whether to save in the DB the list of files processed or not. 
      - IgnoreListOfFilesProcessed=False              # Be careful with this, it will ignore the list of files processed and will process all files in the WorkingDirectory, generating a lot of files if ReplaceOriginal is False and/or IncludeSubFolders is True
      - TimeInMinutesBetweenExecutions=60             # This is the time in minutes between executions of the app loop
      - ExtractTrackNamesOnly=False                   # Only extract track names of all files. This is useful to generate the replacements file, as it returns all the different track names. File is saved in /app/config/{TrackNamesFile} (if exists, it will be overwritten)
```
## Replacements File

This file is a simple text file with the values you want to remove from the track names. There are some details to keep in mind (the app already does this for you, but it's good to know):


- The file is read line by line as-is, so each line is a different value to remove.
- The values are case-insensitive. 
- Duplicates are removed.
- Because values are case-insensitive, if you want to remove a specific value that is part of another value, you need to be careful. For example, if you add `Track` to the replacements file, and you have `Main_Track`, `Track_1` and `Track_2` as track names, you will end up with `Main_` `_1` and `_2` in the track names. In this case, make sure to add any characters you want to remove, like this: `_Track` or `Track_`. I used the underscore as an example, but it could be any character, specially spaces!!!
- Another important detail is to make sure to sort the values in the replacements file from the most specific to the most general, and from the longest to the shortest (this is already done by the app). For example, if you include `J4SSGG` and `By J4SSGG` in the replacements file, make sure to have `By J4SSGG` before `J4SSGG`. The same way, if you add `Track 10000` and `Track 1`, make sure `Track 10000` is before `Track 1` in the replacements file.

The following is a small example of all these details applied in the replacements file:

```
I am a value to remove
value to remove
Track 1000
Track 1
Track
a
```

Again, the app already does this for you, but be careful when adding values to the replacements file. Like the last line being an `a`, it will remove all `a` characters from the track names.