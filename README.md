BrewMonitor
===========
Brewing beer is great fun and super rewarding. You're in control of the process and in just a few hours you can produce top quality wort for your next award winning home brew. After brew day, the next step is fermentation and this is where the whole process can come unstuck. During fermentation your yeast is in charge and it can take days, weeks or even months to do its job. Yeast is a complex organism and doesn't always behave the way we expect which is why it pays to keep regular tabs on your fermentation to avoid any nasty surprises. This is repetitive and time consuming - a perfect candidate for automation with a system like BrewMonitor.

BrewMonitor monitors and records your home brew's fermentation activity while you're not around. It's like having a super-dedicated assistant brewer working 24/7. This frees you up to go about your daily life in confidence that your fermentation is still being monitored diligently and you can check up on it remotely at any time.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20sketch.jpg" width="700" alt="BrewMonitor's partially assembled PCB"/>
</p>

The concept is pretty simple - to hook up a recording device to your fermenter that monitors temperature and airlock activity. That way you can easily answer important questions that would otherwise have required constant observation, such as:
* "What was my lag time?" - An indicator of yeast viability, health and pitching rate.
* "Is fermentation slowing down yet?" - Good to know if you want to perform a diacetyl rest.
* "How long ago did it stop bubbling?" - Might be time to take some final gravity readings to prepare for bottling.
* "What was my peak temperature during fermentation?" - Necessary to accurately calculate priming additions.

##The hardware
The BrewMonitor is an AVR based circuit that interfaces a digital temperature probe as well as an infrared light gate to monitor bubbles passing through your air lock. It has onboard memory for recording fermentation data and supports PC connectivity over USB.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20pcb.jpg" width="700" alt="BrewMonitor's partially assembled PCB"/>
</p>

Fermentation activity is monitored by counting the bubbles as they pass through a gooseneck airlock. Rather than telling you "how much" your beer has fermented, watching the airlock gives you an indication of "how active" your fermentation currently is. The bubbles are detected using an infrared light gate and the signal is interpreted with an ADC in the microcontroller.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20airlock.jpg" width="700" alt="BrewMonitor's airlock sensor mounted onto a common goose neck airlock"/>
</p>

BrewMonitor's hardware is completely open source. The eagle files for the PCB are included in this repository along with the 3D .stl files for printing an airlock sensor.

##The software
The BrewMonitor application is written in C#.NET and runs on windows. It has three main purposes:
1. Downloading, Exploring and Exporting data from your BrewMonitor
2. Configuring your BrewMonitor's sensors
3. Managing notifications that can be pushed to your phone during fermentation

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/explore%20screenshot.png?raw=true" alt="screenshot of BrewMonitor's explore tab"/>
</p>

From the "explore" tab, you can download the data from your BrewMonitor's internal memory for review and export. Clicking on the export button allows you to save the data in .csv format so you can open it in Excel or other software.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/configure%20screenshot.png?raw=true"/>
</p>

Using the "configure" tab, you're able to tune the parameters of your airlock sensor. In this window you can see raw data streaming from the brewmonitor's sensor and drag the slider bars to determine the mean values and hysteresis to accurately detect the passage of each bubble.

##The cloud
The major goal of the BrewMonitor is to free you up to go about your daily life without having to keep regular tabs on your fermentation. By integrating with thingspeak.com BrewMonitor's data can be accessed from a computer or mobile device wherever you are - great for brewers that like to watch their yeast reproduce over the internet ;) Also, by integrating with pushover.net the BrewMonitor can send push notifications directly to your phone when milestomes are reached or intervention is required.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/ios%20notification%20screenshot.jpg?raw=true" width="300"/>
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/ios%20thingspeak%20screenshot.jpg?raw=true" width="300"/>
</p>

These notifications are configured using the "monitor" tab of the brewmonitor software which is still under heavy development at this point in time.

<p align="center">
  <img src="https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/monitor%20screenshot.png?raw=true"/>
</p>

We're currently working on a number of notifications like:
"You've reached 17C, it's time to pitch your yeast!"
"Your temperature has crept up to 25C. You might wanna ice-bath that baby"
"Your bubble rate has slowed to 50% of its peak, now might be a good time for a diacetyl rest"
"Your airlock has been nactive for a while. Time to take some S.G. readings and free up a keg"

#Project status
While a lot of work has gone into the BrewMonitor project as it stands, there is still plenty of work to be carried out.

1. Finish plumbing and testing pushover and thingspeak integration
2. Allow the BrewMonitor to talk directly with the cloud over WiFi
3. Update the hardware to make it easier for others to build and contribute

You can follow the BrewMonitor discussion over at [the AussieHomebrewer forum here](http://aussiehomebrewer.com/topic/88881-brewmonitor-project/).
