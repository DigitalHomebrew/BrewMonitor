BrewMonitor
===========
Brewing is fun, but fermentation can be tedious. BrewMonitor is a system for monitoring and recording the fermentation of your home brew, allowing you to go about your daily life without keeping constant tabs on your fermentor.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20pcb.jpg" width="700"/>
</p>

The concept is pretty simple - to hook up a recording device to your fermenter and record temperature and airlock activity. That way you can easily answer difficult questions that would otherwise require constant observation, such as:
* "What was my lag time?" - An indicator of yeast viability, health and pitching rate.
* "Is fermentation slowing down yet?" - Good to know if you want to perform a diacetyl rest.
* "How long ago did it stop bubbling?" - Might be time to take some final gravity readings to prepare for bottling.
* "What was my peak temperature during fermentation?" - Necessary to accurately calculate priming additions.

#The hardware
The BrewMonitor is an Atmel AVR based circuit that uses a DS18B20 temperature probe for measuring temperature and a custom infrared light gate to monitor bubbles passing through your air lock. It has onboard memory storage and supports USB connectivity with a PC.

#The software
The BrewMonitor application is written in C#.NET and runs on windows. It has three main purposes:
1. Configuring your BrewMonitor's sensors
2. Downloading, Exploring and Exporting data from your BrewMonitor
3. Managing notifications that can be pushed to your phone during fermentation

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/explore%20screenshot.png?raw=true"/>
</p>
A screenshot of the "Explore" tab that allows you to review and export data from your BrewMonitor's internal memory.

#The cloud
A major goal of the BrewMonitor is to free you up to go about your daily life without having to keep regular tabs on your fermentation. By integrating with cloud services, BrewMinotor's data can be viewed on a computer or mobile device from wherever you are, and it can even push notifications to your phone when milestomes are reached or intervention is required.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/ios%20notification%20screenshot.png?raw=true"/>
</p>
A screenshot sent to my phone when temperature got too high on a hot day.
<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/ios%20thingspeak%20screenshot.jpg?raw=true"/>
</p>
A screenshot of data collected on thingspeak.

The BrewMonitor ecosystem incorporates an electronic recording device (the BrewMonitor), some sensors and some PC software that allows you to explore and export your BrewMonitor's memory contents over USB. You can run the BrewMonitor without a computer by powering it from a spare USB charger or battery bank. In this mode it can simply record data to its own internal memory until you connect it to a computer later. Alternatively, you can operate your BrewMonitor while it is connected to a computer (a spare laptop perhaps?) and the computer software can communicate over the internet to send notifications to your phone when certain conditions are met.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/monitor%20screenshot.png?raw=true"/>
</p>

We're currently integrating with Pushover API to send notifications to your phone like:
"You've reached 17C, it's time to pitch your yeast!"
"Your temperature has crept up to 25C. You might wanna ice-bath that baby"
"Your bubble rate has slowed to 50% of its peak, now might be a good time for a diacetyl rest"
"Your airlock is slowing down. Time to take some S.G. readings and free up a keg"

We're also working on live data uploads to thingspeak.com for those that like to watch their yeast reproducing over the internet ;)

Configuration is carried out with the windows software and sent to the brewmonitor over USB. You can watch the ADC values from the airlock in a live view window and tune the mean and hysteresis values to accurately detect the passage of each bubble.
<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/configure%20screenshot.png?raw=true"/>
</p>

The Hardware is completely open source and the eagle files for the PCB are included in this repository also. Here's a pic of a completed board as well as a completed unit in a CNC cut ABS enclosure.
![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20pcb.jpg)
![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20enclosure.jpg)
