BrewMonitor
===========
Brewing is great fun, but keeping regular tabs on your fermentation for days or weeks on end can become tedious for even the most dedicated home brewer. BrewMonitor is an automated system for monitoring and recording your home brew's fermentation so that you don't have to, allowing you to go about your daily life in confidence that your fermentation is still being watched.

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
1. Downloading, Exploring and Exporting data from your BrewMonitor
2. Configuring your BrewMonitor's sensors
3. Managing notifications that can be pushed to your phone during fermentation

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/explore%20screenshot.png?raw=true"/>
</p>
A screenshot of the "Explore" tab that allows you to review and export data from your BrewMonitor's internal memory.

Configuration is carried out with the windows software and sent to the brewmonitor over USB. You can watch the ADC values from the airlock in a live view window and tune the mean and hysteresis values to accurately detect the passage of each bubble.
<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/configure%20screenshot.png?raw=true"/>
</p>

#The cloud
A major goal of the BrewMonitor is to free you up to go about your daily life without having to keep regular tabs on your fermentation. By integrating with thingspeak.com BrewMinotor's data can be accessed on a computer or mobile device from wherever you are - great for brewers that like to watch their yeast reproduce over the internet ;) Also, by integrating with pushover.net the BrewMonitor can even send notifications directly to your phone when milestomes are reached or intervention is required.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/ios%20notification%20screenshot.jpg?raw=true" width="300"/>
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/ios%20thingspeak%20screenshot.jpg?raw=true" width="300"/>
</p>

These notifications are configured using the "monitor" tab of the brewmonitor software.
<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/monitor%20screenshot.png?raw=true"/>
</p>

We're currently working on a number of notifications like:
"You've reached 17C, it's time to pitch your yeast!"
"Your temperature has crept up to 25C. You might wanna ice-bath that baby"
"Your bubble rate has slowed to 50% of its peak, now might be a good time for a diacetyl rest"
"Your airlock has been nactive for a while. Time to take some S.G. readings and free up a keg"



The Hardware is completely open source and the eagle files for the PCB are included in this repository also. Here's a pic of a completed board as well as a completed unit in a CNC cut ABS enclosure.
![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20pcb.jpg)
![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20enclosure.jpg)
