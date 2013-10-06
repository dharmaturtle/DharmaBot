# DharmaBot v7.0.1 Stripped

This bot moderates Destiny's chat on [Twitch](http://www.twitch.tv/chat/embed?channel=destiny&popout_chat=true) and on his [website](http://www.destiny.gg/embed/chat). The variant here monitors the website's chat, but the Twitch version is similar. Certain algorithms (like fuzzy string matching) are explicitly shown, but more sensitive information (like ban logic) have been removed. This is unfortunate since the ban logic is the most complex and interesting part, taking up a third of the script, but such is the nature of open source. Further details on the bot's logic are available on [Reddit](http://www.reddit.com/r/Destiny/comments/1aufkc/dharmabot_autoban_rules/).

## Content warning

The bot was created to meet the needs of the chat owner, Destiny, and does not reflect the programmer's own personal views. Just because the bot moderates the chats does not mean it condones the behaviour of the people within.

## Programming style

This was my first significant programming project, so the code isn't particularly DRY or flexible. However it works well and I'm busy, so refactoring will come later. It was written primarily for personal use, so some conventions like line length limits are ignored. Since I've been working on this bot for more than 2 years, I'm very familiar with the code and have compressed many short lines into single line monstrosities.

## How to use

You will need to create several pickle files holding variables like live/off times and auto-ban links. Please do not test your implementation in the website's chat - modify your version to run in IRC, preferably in your own channel.