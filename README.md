BrewMonitor
===========
BrewMonitor is a project for monitoring the fermentation of your home brew.

The idea is pretty simple - to hook up a recording device to your fermenter and record temperature and airlock activity. That way you can easily answer questions like "what was my lag time?", "what was my peak temperature?" and "how well is my fridge controlling the temperature?" without having to keep a hawk eye on your fermentation.

Note: This project is not an alternative for taking specific gravity readings, rather BrewMonitor can help to determine when you should be taking manual specific gravity readings.

![alt tag](https://raw.github.com/DigitalHomebrew/BrewMonitor/branch/path/to/img.png)
<BrewMonitor><Graph><Notifications><Configuration><Fridge><IOS>

The BrewMonitor ecosystem incorporates an electronic box (the BrewMonitor) and some PC software that allows you to explore and export your BrewMonitor's memory contents. You can run the BrewMonitor without a computer by powering it from a spare USB charger or battery bank. In this case it can simply record data to its own internal memory until you connect it to a computer later. Alternatively, you can operate your BrewMonitor while it is connected to a computer (a spare laptop perhaps?) and the computer software can communicate over the internet to send notifications to your phone when certain conditions are met.

I'm currently integrating with Pushover API to send notifications to your phone like:
"You've reached 17C, it's time to pitch your yeast!"
"Your temperature has crept up to 25C. You might wanna ice-bath that baby"
"Your bubble rate has slowed to 50% of its peak, now might be a good time for a diacetyl rest"
"Your airlock is slowing down. Time to take some S.G. readings and free up a keg"

I'm also working on live data uploads to thingspeak.com for those that like to watch their yeast reproducing over the internet ;)
