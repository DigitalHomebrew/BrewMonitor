BrewMonitor
===========
Brewing beer is great fun and very rewarding. You're in control of the process and in just a few hours you can produce top quality wort for your next award winning home brew. After brew day, the next step is fermentation and this is where the whole process can come unstuck. During fermentation your yeast is in charge and it can take days, weeks or even months for your fermentation to complete. Yeast is a complex organism and doesn't always behave the way we expect which is why it pays to keep regular tabs on your brew during fermentation to avoid any nasty surprises. Keeping tabs on your fermentation is repetitive and time consuming - a perfect candidate for automation with a system like BrewMonitor.

BrewMonitor is a device that monitors and records your home brew's fermentation activity while you're not around. It's like having a super-dedicated assistant brewer. This frees you up to go about your daily life without being tied to your fermenter, in confidence that your fermentation is still being monitored diligently and you can check up on it remotely at any time.

The concept is pretty simple - to hook up a recording device to your fermenter that monitors temperature and airlock activity. That way you can easily answer important questions that would otherwise have required constant observation, such as:
* "What was my lag time?" - An indicator of yeast viability, health and pitching rate.
* "Is fermentation slowing down yet?" - Good to know if you want to perform a diacetyl rest.
* "How long ago did it stop bubbling?" - Might be time to take some final gravity readings to prepare for bottling.
* "What was my peak temperature during fermentation?" - Necessary to accurately calculate priming additions.

##The hardware
The BrewMonitor is an AVR based circuit that uses a DS18B20 temperature probe for measuring temperature and a custom infrared light gate to monitor bubbles passing through your air lock. It also has onboard memory storage and supports PC connectivity over USB.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20pcb.jpg" width="700"/>
</p>

Fermentation activity is monitored by counting the bubbles passing through a common gooseneck airlock. Rather than telling you "how much" your beer has fermented, watching the airlock gives you an indication of "how active" your fermentation currently is. The bubbles are detected using an infrared light gate and the signal is interpreted with an ADC on the microcontroller.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20airlock.jpg" width="700"/>
</p>

BrewMonitor's hardware is completely open source and the eagle files for the PCB are included in this repository also.

##The software
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

##The cloud
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
