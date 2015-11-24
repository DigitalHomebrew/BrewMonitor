BrewMonitor
===========
BrewMonitor is a project for monitoring the fermentation of your home brew.

The concept is pretty simple - to hook up a recording device to your fermenter and record temperature and airlock activity. That way you can easily answer questions like "what was my lag time?", "what was my peak temperature?" and "how well is my fridge controlling the temperature?" without having to keep a hawk eye on your fermentation.

![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/explore%20screenshot.png?raw=true)

Sidenote: This project is not an alternative for taking specific gravity readings, rather BrewMonitor can help to determine when you should be taking manual specific gravity readings.

![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20enclosure.jpg)

The BrewMonitor ecosystem incorporates an electronic box (the BrewMonitor) and some PC software that allows you to explore and export your BrewMonitor's memory contents. You can run the BrewMonitor without a computer by powering it from a spare USB charger or battery bank. In this case it can simply record data to its own internal memory until you connect it to a computer later. Alternatively, you can operate your BrewMonitor while it is connected to a computer (a spare laptop perhaps?) and the computer software can communicate over the internet to send notifications to your phone when certain conditions are met.

![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/monitor%20screenshot.png?raw=true)

We're currently integrating with Pushover API to send notifications to your phone like:
"You've reached 17C, it's time to pitch your yeast!"
"Your temperature has crept up to 25C. You might wanna ice-bath that baby"
"Your bubble rate has slowed to 50% of its peak, now might be a good time for a diacetyl rest"
"Your airlock is slowing down. Time to take some S.G. readings and free up a keg"

We're also working on live data uploads to thingspeak.com for those that like to watch their yeast reproducing over the internet ;)

Configuration is carried out with the windows software and sent to the brewmonitor over USB. You can watch the ADC values from the airlock in a live view window and tune the mean and hysteresis values to accurately detect the passage of each bubble.
![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/configure%20screenshot.png?raw=true)


The Hardware is completely open source and the eagle files for the PCB are included in this repository also. Here's a pic of a completed board as well as a completed unit in a CNC cut ABS enclosure.
![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20pcb.jpg)
![alt tag](https://github.com/DigitalHomebrew/BrewMonitor/blob/master/Images/brewmonitor%20enclosure.jpg)
