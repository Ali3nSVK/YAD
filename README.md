# YAD
Yet Another Dictaphone

## Purpose
The purpose of this little app is to get familiar with the (NAudio)[https://github.com/naudio/NAudio] library as well as digital audio processing, signal chains, effects, codecs, visualizations and basically anything audio related that piqued my interest during development.
The way I wanted to approach this was slightly different that what most tutorials and code snippets do and that is to apply all of the above in real-time while recording, as opposed to reading from files.

## What It Does
YAD can currently do the following
* Record from a capture device on specific channel (or all channels).
* Record from a loopback adapter device to capture audio played on the soundcard.
* Output into a WAV, MP3 file or simply monitor the audio in real-time.
* Provide basic settings while recording like Gain.
* Draw a waveform of the recorded audio in real-time.

# What It Might Do
* Provide a configurable noise gate to limit background hum.
* Provide basic effects like chorus or reverb.
* Provide a spectrum analyser
** As a visualization option
** As frequency analyzer for an in-built tuner

* Other simple useful tasks like extracting audio from a video file, etc.
