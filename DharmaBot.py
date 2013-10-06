#!/usr/bin/python
# -*- coding: utf-8 -*-

'''
DharmaBot v7.0.1
This is a stripped down version with ban logic and other sensitive information removed
An updated version can be seen operating in destiny.gg/embed/chat and twitch.tv/destiny
'''

import sys, traceback, socket, datetime, urllib2, urllib, httplib, urlparse, cPickle, pylast, json, re, time, tweepy, requests, HTMLParser, unicodedata
from websocket import create_connection
from random import randint
from xml.dom.minidom import parseString
from colorama import init, Fore, Back, Style

# initialize colors
init()

# initialize a ton of variables
x = 0
pingcount = 0
solospamlist  = []
solospamlist2 = []
longspamlist  = []
messagequeue  = []
strawpolllist = []
html_parser = HTMLParser.HTMLParser()
fo       = open("log.txt", "a")
shortlog = open("shortlog.txt", "a")
endlag           = datetime.datetime(2007, 12, 6, 15, 29, 43, 79060)
xendlag          = datetime.datetime(2007, 12, 6, 15, 29, 43, 79060)
nuketime         = datetime.datetime(2007, 12, 6, 15, 29, 43, 79060)
messagequeuetime = datetime.datetime(2007, 12, 6, 15, 29, 43, 79060)
submode = 0
nstage1 = 0
nstage2 = 0
unnuke  = 0
nuked   = []
commandcount = 0
messagecount = 0
commandtime = datetime.datetime.now()
messagetime = datetime.datetime.now()
ninjatime   = datetime.datetime.now()
clearedusr = "destiny"
access_token=#token
access_token_secret=#token
auth = tweepy.OAuthHandler(consumer_key="consumerkey", consumer_secret="mysecret")
auth.set_access_token(access_token, access_token_secret)
twitterapi = tweepy.API(auth)
socket.setdefaulttimeout(3)
network = pylast.LastFMNetwork(api_key = "key", api_secret = "secret", username = "username", password_hash = "hash")
recentlog = [("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("","")] #50 entries long
tbanchars =     #zalgo characters
urlshorteners = #a list of URL shorteners
authlist =      #list of mods
faces =         #list of twitch.tv faces
tld =           #list of TLDs

# simplifies loading and writing of pickle files/objects
def xfile(filename, rwi, *value): #read-write-increment
  if rwi == "w":
    with open(filename, 'w') as filecontent:
      cPickle.dump(value[0],filecontent)
  elif rwi == "r":
    with open(filename, 'r') as filecontent:
      return cPickle.load(filecontent)
  else:
    with open(filename, 'r+') as filecontent:
      increment = cPickle.load(filecontent)
      increment += 1
      filecontent.seek(0)
      cPickle.dump(increment,filecontent)
      return increment

# loads pickle files
banlinks       = xfile("banlinksfile","r")
secretbanlinks = #This stays secret
tbanlinks      = xfile("tbanlinksfile","r")
banppl         = xfile("banpplfile","r")
vengeance      = xfile("vengeancefile","r")
ninja          = xfile("ninjafile","r")
oday           = xfile("odayfile","r")
oldusrlist     = xfile("oldusrlistfile","r")
seenlog        = xfile("seenlogfile","r")

# allows bot to send message to channel. Time constraints are to stay within Twitch's message limit
def message(msg):
  global messagecount
  global messagetime
  global messagequeue
  if int((datetime.datetime.now() - messagetime).total_seconds()) <= 30:
    messagecount += 1
  else:
    messagecount = 1
    messagetime = datetime.datetime.now()
  if messagecount < 19:
    try:
      ws.send('MSG {"data":"' + msg.encode('utf-8') + '"}')
      fo.write( "dharmaturtle: " + msg + "\r\n")
    except (UnicodeDecodeError, UnicodeEncodeError):
      ws.send('MSG {"data":"' + msg + '"}')
      fo.write( "dharmaturtle: Unicode error, probably a song name\r\n")
  else:
    messagequeue.append(msg)
    log(Back.BLUE + Style.BRIGHT,"MESSAGE OVERFLOW WITH: " + msg + ":::" + str(messagequeue))

# ban logic, deleted.
def ban(bantime, mess, banee = 0):

# time/date formatter
def tdformat(s,rough=""):
  days, remainder  = divmod(s, 86400)
  hours, remainder = divmod(remainder, 3600)
  minutes, seconds = divmod(remainder, 60)
  if days > 1:
    if hours != 0:
      return '%s days %s%sh' % (days, rough, hours)
    else:
      return '%s days' % (days)
  elif days == 1:
    if hours != 0:
      return '%s day %s%sh' % (days, rough, hours)
    else:
      return 'a day'
  elif days == 0:
    if hours != 0:
      if minutes != 0:
          return '%s%sh%sm' % (rough, hours, minutes)
      else:
        return '%s%sh' % (rough, hours)
    else:
      return '%s%sm' % (rough, minutes)
  else:
    log(Fore.RED, "twat time is negative? " + str(s))
    return 'A few seconds'

# retrieves a user's last posted text
def stalk(usr,custom):
  try:
    msg = seenlog[usr][1]
    #Removed conditional for privacy
      response = tdformat(int((datetime.datetime.now() - seenlog[usr][0]).total_seconds())) + " ago saying something so stupid it shall not be repeated."
    else:
      response = tdformat(int((datetime.datetime.now() - seenlog[usr][0]).total_seconds())) + " ago saying: " + msg.strip()
    message(custom + response)
  except:
    message(usr + " not in logs")

# fuzzy string matching
def stringcomp(fx, fy):
  fx = fx.replace(" ","")
  fy = fy.replace(" ","")
  n, m = len(fx), len(fy)
  if m < n:
    (n, m) = (m, n)
    (fx, fy) = (fy, fx)
  ssnc = 0.
  for length in range(n, 0, -1):
    while 1:
      length_prev_ssnc = ssnc
      for i in range(len(fx)-length+1):
        pattern = fx[i:i+length]
        pattern_prev_ssnc = ssnc
        fx_removed = False
        while True:
          index = fy.find(pattern)
          if index != -1:
            ssnc += (2.*length)**2
            if fx_removed == False:
              fx = fx[:i] + fx[i+length:]
              fx_removed = True
            fy = fy[:index] + fy[index+length:]
          else:
            break
        if ssnc != pattern_prev_ssnc:
          break
      if ssnc == length_prev_ssnc:
        break
  return (ssnc/((n+m)**2.))**0.5

# error handling
def error(e=0):
  if e == 1:
    message("API timed out")
  else:
    message("Unexpected error! Someone wake up Dharma")
  traceback.print_tb(sys.exc_info()[2])
  log(Back.BLUE + Style.BRIGHT,str(sys.exc_info()))

# unshortens URLs in Twitter posts
def untiny(s):
  try:
    while "http://t.co/" in s:
      urller = re.search('.*(http://t\.co/\w+).*', s).group(1)
      s = re.sub(r'(.*)http://t\.co/\w+(.*)', r'\1' + requests.get(urller, timeout=5).url + r'\2', s)
    message(html_parser.unescape(s))
  except:
    error(1)

# colors output in the console
def log(c,s):
  print(Fore.BLUE + Style.BRIGHT + time.asctime() + Fore.RESET + " " + c + s + Fore.RESET + Back.RESET + Style.NORMAL)
  shortlog.write(time.asctime() + " : " + s + "\r\n")

# checks the status of the stream, with a 10 minute offline "fuzzy" length allowed for temporary offline 
def livestatus(s):
  try:
    global modaboos
    livetimefile = open('livetimefile', 'r+')
    offtimefile  = open('offtimefile', 'r+')
    livetime     = cPickle.load(livetimefile)
    offtime      = cPickle.load(offtimefile)
    file   = urllib2.urlopen('https://api.twitch.tv/kraken/streams/destiny')
    jdata  = json.loads(file.read()) # checks stream status
    file.close()
    file   = urllib2.urlopen('https://api.twitch.tv/kraken/channels/destiny')
    jdelay = json.loads(file.read()) # gets viewer count
    file.close()
    if isinstance(jdata["stream"], dict):
      returnval = 1
      if livetime == 0:
        livetime = datetime.datetime.now()
        offtime = 0
      if s == 1:
        log(Fore.YELLOW,"Live with " + str(jdata["stream"]["viewers"]) + " viewers for " + tdformat(int((datetime.datetime.now() - livetime).total_seconds()),"~"))
        if str(jdelay["delay"]) != "0":
          message("On for " + tdformat(int((datetime.datetime.now() - livetime).total_seconds()),"~") + ", " + str(jdata["stream"]["viewers"]) + "☺ & " + str(round(float(jdelay["delay"])/60,2)).rstrip('0').rstrip('.') + "m delay")
        else:
          message("Live for " + tdformat(int((datetime.datetime.now() - livetime).total_seconds()),"~") + " & " + str(jdata["stream"]["viewers"]) + " ☺")
    else:
      if offtime == 0:
        offtime = datetime.datetime.now()
      if livetime != 0 and int((datetime.datetime.now() - offtime).total_seconds()) > 600:
        livetime = 0
        returnval = 1
      else:
        returnval = 0
      if s == 1:
        log(Fore.YELLOW , "Stream offline for " + tdformat(int((datetime.datetime.now() - offtime).total_seconds()),"~"))
        if livetime != 0:
          message("Stream went offline in the past 10m")
        elif modaboos == 1:
          message("Modaboos weakened, stream offline for " + tdformat(int((datetime.datetime.now() - offtime).total_seconds()),"~"))
          modaboos = 2
        else:
          message("Stream offline for " + tdformat(int((datetime.datetime.now() - offtime).total_seconds()),"~"))
    livetimefile.seek(0)
    offtimefile.seek(0)
    cPickle.dump(livetime,livetimefile)
    cPickle.dump(offtime,offtimefile)
    livetimefile.close()
    offtimefile.close()
    if returnval == 0:
      modaboos = 2
    elif submode == 1:
      modaboos = 2
    else:
      modaboos = 1
    return returnval
  except:
    if s == 1:
      error(1)

# connects to server
def connect():
  log(Back.RED + Style.BRIGHT,"Microversion #117")
  global ws
  ws = create_connection("ws://www.destiny.gg:9998/ws", header={"Cookie: sid=MYCOOKIES","Origin: http://www.destiny.gg"})

connect()
livestatus(0) # grabs initial stream status

# begin main chat logic
while True:
  
  # encapsulating try ensures that bot keeps running even if unknown error occurs
  try:

    # !nuke logic, adds user messages to a list, and checks !nuke status to see if people should be whacked
    if len(messagequeue) != 0 and int((datetime.datetime.now() - messagequeuetime).total_seconds()) >= 1:
      message(messagequeue.pop(0))
      messagequeuetime = datetime.datetime.now()
    if nstage1 == 1 and int((datetime.datetime.now() - nuketime).total_seconds()) > 1:
      m = nlist.pop()
      if ( nphrase in m[1] or stringcomp(nphrase,m[1]) > 0.4 ) and m[0] not in nuked:
        nukedcount += 1
        ban("1800","",m[0])
        nuked.append(m[0])
      if len(nlist) == 0:
        nstage1 = 0
        message("Silos rearmed. Bodycount of " + str(nukedcount) + ", but radiation lingers")
    if unnuke == 1:
      m = nuked.pop()
      message(".unban " + m)
      del banppl[m]
      xfile("banpplfile","w",banppl)
      time.sleep( 2 )
      if len(nuked) == 0:
        unnuke = 0
        message("Nuke victims resuscitated")
    
    # store incoming data
    try:
      data = ws.recv().strip('\r\n').lower()
    except:
      try:
        data = ws.recv()
      except:
        data = ""
        try:
          log(Back.BLUE + Fore.RED,"Disconnected! Remember it takes EXACTLY THREE Ctrl C's to kill and save the log file properly!")
          time.sleep( 2 ) # gives us time to hit ctrl c one more time
          connect()
        except (KeyboardInterrupt, SystemExit):
          with open("seenlogfile", 'wb') as filecontent:
            cPickle.dump(seenlog,filecontent)
          log(Back.RED,"seenlogfile has been saved.")
          log(Back.RED,"Keyboard kill!")
          raise
        except:
          pass
    
    # use regex to parse json because json.loads messes up unicode
    m = re.search('msg \{"nick":"(.*?)","features":\[(.*?)\],"connections":(.*?),"timestamp":(.*?),"data":"(.*?)"\}', data)
    
    # begin text parsing
    if m is not None: #to catch the regex nongroup error
      x += 1 #linecount
      senderusr = m.group(1)
      sendermsg = " " + m.group(5).strip() + " " # prepends and appends space mark to lines for simpler regex later
      fo.write( senderusr + ":" + sendermsg + "\r\n")
      seenlog[senderusr] = (datetime.datetime.now(), sendermsg)
      
      # ensures that commands cannot be spammed
      xpleblag = datetime.datetime.now() - xendlag
      
      # ban logic code
      
      # everyone's chat commands, pretty simple & obvious code
      if ( sendermsg.find("!live") == 1 ):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          livestatus(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!song") == 1 ):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            current_track = network.get_user('StevenBonnellII').get_now_playing()
            if current_track is None:
              psong = network.get_user('StevenBonnellII').get_recent_tracks(1)
              message("No song played/scrobbled. Played " + tdformat(int((datetime.datetime.utcnow() - datetime.datetime.strptime(psong[0].playback_date, '%d %b %Y, %H:%M')).total_seconds())) + ' ago: ' + psong[0].track.get_artist().get_name() + ' - ' + psong[0].track.get_title())
            else:
              message(current_track.get_artist().get_name() + ' - ' + current_track.get_title())
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!lastsong") == 1 ):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            psong = network.get_user('StevenBonnellII').get_recent_tracks(2)
            if psong is None:
              message("Destiny isn't playing a song or it isn't being scrobbled")
            else:
              message("Played " + tdformat(int((datetime.datetime.utcnow() - datetime.datetime.strptime(psong[0].playback_date, '%d %b %Y, %H:%M')).total_seconds())) + ' ago: ' + psong[0].track.get_artist().get_name() + ' - ' + psong[0].track.get_title())
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!tweet") == 1 or sendermsg.find("!twitter") == 1):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            twat = twitterapi.user_timeline("steven_bonnell")[0]
            untiny(tdformat(int((datetime.datetime.utcnow() - twat.created_at).total_seconds())) + ' ago: ' + twat.text)
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!blog") == 1 or sendermsg.find("!blag") == 1):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            file = urllib2.urlopen('http://blog.destiny.gg/feed/')
            blogstrong = file.read()
            file.close()
            dom = parseString(blogstrong)
            blogtitle = dom.getElementsByTagName('rss')[0].getElementsByTagName('channel')[0].getElementsByTagName('item')[0].getElementsByTagName('title')[0].toxml().replace('<title>','').replace('</title>','')
            bloglink = dom.getElementsByTagName('rss')[0].getElementsByTagName('channel')[0].getElementsByTagName('item')[0].getElementsByTagName('link')[0].toxml().replace('<link>http://','').replace('/</link>','')
            blogdate = dom.getElementsByTagName('rss')[0].getElementsByTagName('channel')[0].getElementsByTagName('item')[0].getElementsByTagName('pubDate')[0].toxml().replace('<pubDate>','').replace('</pubDate>','')
            message("\"" + unicodedata.normalize('NFKD', blogtitle).encode('ascii','ignore') + "\" posted " + tdformat(int((datetime.datetime.utcnow() - datetime.datetime.strptime(str(blogdate), '%a, %d %b %Y %H:%M:%S +0000')).total_seconds())) + " ago " + str(bloglink))
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!lolking") == 1 ):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            file = urllib2.urlopen('http://www.lolking.net/summoner/na/37544949') # I can't believe LoL doesn't have a public API yet
            ultimastring = file.read().rstrip('\r\n')
            file.close()
            ultima = re.search('.*?data-hoverswitch="(.*?)">', ultimastring)
            ultima = datetime.datetime.strptime(ultima.group(1), '%m/%d/%y %I:%M%p PST')
            file = urllib2.urlopen('http://www.lolking.net/summoner/na/26077457')
            neostring = file.read().rstrip('\r\n')
            file.close()
            neo = re.search('.*?data-hoverswitch="(.*?)">', neostring)
            neo = datetime.datetime.strptime(neo.group(1), '%m/%d/%y %I:%M%p PST')
            if neo > ultima:
              lolrank = re.search('.*Solo 5v5.*?<div class="personal_ratings_rating".*?>(.*?)<span.*?>(.*?)</s.*', neostring,re.DOTALL)
              message("NeoDéstiny: " + lolrank.group(1) + lolrank.group(2) + " - http://www.lolking.net/summoner/na/26077457")
            else:
              lolrank = re.search('.*Solo 5v5.*?<div class="personal_ratings_rating".*?>(.*?)<span.*?>(.*?)</s.*', ultimastring,re.DOTALL)
              message("UltimaDestiny: " + lolrank.group(1) + lolrank.group(2) + " - http://www.lolking.net/summoner/na/37544949")
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!youtube") == 1 ):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            file = urllib2.urlopen('https://gdata.youtube.com/feeds/api/users/StevenBonnell/uploads?alt=json&v=2') #download the file
            jdata = json.loads(file.read())
            file.close()
            yttitle = str(jdata["feed"]["entry"][0]["title"]["$t"])
            ytdate = str(jdata["feed"]["entry"][0]["published"]["$t"])
            ytlink = str(jdata["feed"]["entry"][0]["media$group"]["yt$videoid"]["$t"])
            message( "\"" + yttitle + "\" posted " + tdformat(int((datetime.datetime.utcnow() - datetime.datetime.strptime(str(ytdate), '%Y-%m-%dT%H:%M:%S.000Z')).total_seconds())) + " ago youtube.com/watch/" + str(ytlink))
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!aslan") == 1 ):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            twat = twitterapi.user_timeline("aslanvondran")[0]
            untiny(tdformat(int((datetime.datetime.utcnow() - twat.created_at).total_seconds())) + ' ago, @AslanVondran tweeted: ' + twat.text)
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!randomaslan") == 1 ):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            req = urllib2.Request('https://api.imgur.com/3/album/hCR89')
            req.add_header('Authorization', 'Client-ID GETYOUROWNIDGOAWAY')
            response = json.loads(urllib2.urlopen(req).read())
            imgrand = randint(0, int( response[u'data'][u'images_count'] ) - 1 )
            if response[u'data'][u'images'][imgrand][u'animated'] == False:
              message("Aslan! http://www." + response[u'data'][u'images'][imgrand][u'link'][9:-4])
            else:
              message("Aslan! " + response[u'data'][u'images'][imgrand][u'link'][9:])# randomly pulled from imgur.com/a/hCR89
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!rules") == 1 or sendermsg.find("!unmoddharma") == 1 or sendermsg.find("!evennaziskeptgoodrecords") == 1):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          message('Rules: reddit.com/1aufkc')
          endlag = datetime.datetime.now()
      elif sendermsg.find("!bancount") == 1:
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          bancount = xfile("bancountfile","r")
          message( str(bancount) + " spammers permed")
          endlag = datetime.datetime.now()
      elif sendermsg.find("!irc") == 1:
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          message("IRC will be implemented Soon™. For now, chat is echoed to Rizon IRC at http://qchat.rizon.net/?channels=#destinyecho . Forwarding of IRC chat to DestinyChat is available.")
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!team") == 1):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          try:
            file = urllib2.urlopen('http://lolnexus.com/NA/search?name=ultimadestiny')
            ultimateam = file.read()
            file.close()
            if "No Active" in ultimateam:
              file = urllib2.urlopen('http://lolnexus.com/NA/search?name=NeoDéstiny')
              neoteam = file.read()
              file.close()
              if "No Active" in neoteam:
                message("Not in game")
              else:
                if "Spectate this game!" in neoteam:
                  message("http://www.lolnexus.com/NA/search?name=NeoD%C3%A9stiny")
                elif "champion select" in neoteam:
                  message("Currently in champ select: http://www.lolnexus.com/NA/search?name=NeoD%C3%A9stiny")
                else:
                  message("Site changed, yell at Dharma to fix")
            else:
              if "Spectate this game!" in ultimateam:
                message("http://www.lolnexus.com/NA/search?name=UltimaDestiny")
              elif "champion select" in ultimateam:
                message("Currently in champ select: http://www.lolnexus.com/NA/search?name=UltimaDestiny")
              else:
                message("Site changed, yell at Dharma to fix")
          except:
            error(1)
          endlag = datetime.datetime.now()
      elif ( sendermsg.find("!time") == 1):
        pleblag = datetime.datetime.now() - endlag
        if (( senderusr in authlist ) or ( pleblag.total_seconds() > 15 )):
          hour = datetime.datetime.now().strftime("%I")
          if hour[0] == "0" :
            hour = hour[1]
          #THIS IS FOR CDT message( datetime.datetime.now().strftime("%a, " + hour + ":%M %p") + " Central Destiny Time") #Daylight saving time (DST) is in effect in much of the Central time zone between mid-March and early November.
          message( (datetime.datetime.now() + datetime.timedelta(hours=1)).strftime("%a, " + hour + ":%M %p") + " Eris & Destiny Time") #THIS IS FOR VIRGINIA
          endlag = datetime.datetime.now()
      
      # restricted commands
      if senderusr in authlist:
        
        # searches for popular poll sites
        z = re.search('.*(?:(?:strawpoll\.me)|(?:pollcode\.com))/(\w+).*', sendermsg)
        if z is not None:
          strawpolllist.append(z.group(1))
        
        # adds admin text to the spam list, because people like repeating what admins say
        if len(sendermsg) > #Secret value!:
          if sum(sendermsg.count(c) for c in tld) > 1 or sendermsg.count("http") > 1 or sendermsg.count("www") > 1: #multiple urls
            regmsg = sendermsg
          else:
            regmsg = re.sub("(\S*(www|http|\.com|\.org|\.gg|\.edu|\.gov|\.net|\.tv|\.uk|\.cc|\.xxx|\.mil|\.us|\.se|\.fm|\.me|\.de|\.kr|\.sv|\.fr|\.be|\.ly|\.info|\.biz|\.us|\.in|\.mobi)\S*) ","",sendermsg)
          if regmsg.strip():
            longspamlist.append((regmsg, x))
        
        # !nuke logic
        elif sendermsg.find("!nuke ") == 1 and len(sendermsg.strip().split(" ")) != 1:
          if nstage1 == 1:
            message("Rearming silos... missiles already in flight!")
          else:
            nphrase = str(sendermsg.strip().split(" ",1)[1])
            message("Missiles away...")
            nlist = recentlog[:]
            recentlog = [("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("",""),("","")]
            nuked = []
            log(Back.RED + Style.BRIGHT,"NUKED! " + nphrase)
            nukedcount = 0
            nstage1 = 1
            nstage2 = 100
        elif sendermsg.find("!aegis") == 1:
          message("Oh shit, undo! Undo!!!")
          nstage1 = 0
          nstage2 = 0
          unnuke = 1
        
        elif sendermsg.find("!sing") == 1:
          message("/me sings the body electric ♫")
        elif sendermsg.find("!dance") == 1:
          message("/me roboboogies ⌐[º-°⌐] [¬º-°]¬")
        elif sendermsg.find("!stalk ") == 1:
          stalk(sendermsg.strip().split(" ")[1], sendermsg.strip().split(" ")[1] + " seen ")
        elif sendermsg.find("!status") == 1:
          message("v7.0.1 N:" + str(ninja) + " V:" + str(vengeance) + " A:" + str(oday))
        elif sendermsg.find("!say ") == 1 and len(sendermsg.strip().split(" ")) != 1:
          message(sendermsg.strip().split(" ",1)[1])
        elif (sendermsg.find("!b ") == 1 or sendermsg.find("!ban ") == 1 ) and len(sendermsg.strip().split(" ")) != 1:
          if len(sendermsg.strip().split(" ")) == 2:
            ban(0,"6day",sendermsg.strip().split(" ")[1])
          else:
            try: #allows admin to set a custom time
              ban(str(int(sendermsg.strip().split(" ")[2]) * 60),"",sendermsg.strip().split(" ")[1])
            except: #but defaults to 6 days
              ban(0,"6day",sendermsg.strip().split(" ")[1])
        
        # adding and removing autobanned words from appropriate lists
        elif sendermsg.find("!del ") == 1 and len(sendermsg.strip().split(" ")) != 1:
          word = sendermsg.strip().split(" ",1)[1]
          try:
            banlinks.remove(word)
            xfile("banlinksfile", "w", banlinks)
            log(Style.NORMAL,str(banlinks))
            message(word + " removed from autoban list")
          except:
            message(word + " not found")
        elif sendermsg.find("!tempdel ") == 1 and len(sendermsg.strip().split(" ")) != 1:
          word = sendermsg.strip().split(" ",1)[1]
          try:
            tbanlinks.remove(word)
            xfile("tbanlinksfile", "w", tbanlinks)
            log(Style.NORMAL,str(tbanlinks))
            message(word + " removed from tempban list")
          except:
            message(word + " not found")
        elif sendermsg.find("!tempadd ") == 1 and len(sendermsg.strip().split(" ")) != 1:
          word = str(sendermsg.strip().split(" ",1)[1])
          tbanlinks.append(word)
          xfile("tbanlinksfile", "w", tbanlinks)
          log(Style.NORMAL,str(tbanlinks))
          message(word + " added to 30m ban list")
        elif sendermsg.find("!add ") == 1 and len(sendermsg.strip().split(" ")) != 1:
          word = str(sendermsg.strip().split(" ",1)[1])
          banlinks.append(word)
          xfile("banlinksfile", "w", banlinks)
          log(Style.NORMAL,str(banlinks))
          message(word + " added to autoban list")
        
        # sets values of options
        elif sendermsg.find("!vengeance on") == 1:
          vengeance = 1
          xfile("vengeancefile","w",1)
          message("Memory enabled, seeking revenge")
        elif sendermsg.find("!vengeance off") == 1:
          vengeance = 0
          xfile("vengeancefile","w",0)
          message("Memory disabled")
        elif sendermsg.find("!ninja on") == 1:
          ninja = 1
          xfile("ninjafile","w",1)
          message("Silent kill enabled")
        elif sendermsg.find("!ninja off") == 1:
          ninja = 0
          xfile("ninjafile","w",0)
          message("War is a loud and messy affair")
        elif sendermsg.find("!ageism on") == 1:
          oday = 1
          xfile("odayfile","w",1)
          message("Ageism enabled")
        elif sendermsg.find("!ageism off") == 1:
          oday = 0
          xfile("odayfile","w",0)
          message("Equality restored")
        elif sendermsg.find("!modaboos on") == 1:
          modaboos = 1
          message("Phasers set to kill")
        elif sendermsg.find("!modaboos semi") == 1:
          modaboos = 2
          message("Phasers set to stun")
        elif sendermsg.find("!modaboos off") == 1:
          modaboos = 0
          message("I slumber... for ~3m")
        
        # unbanned command, depreciated
        elif sendermsg.find("!unban ") == 1 and len(sendermsg.strip().split(" ")) != 1:
          message(".unban " + sendermsg.strip().split(" ")[1])
          loud = 0
          if len(sendermsg.strip().split(" ")) > 2:
            if sendermsg.strip().split(" ")[2] == "loudly":
              loud = 1
          try:
            del banppl[sendermsg.strip().split(" ")[1]]
            xfile("banpplfile","w",banppl)
            if loud == 0:
              message(sendermsg.strip().split(" ")[1] + " unbanned")
            else:
              stalk(sendermsg.strip().split(" ")[1], sendermsg.strip().split(" ")[1] + " unbanned, seen ")
            if sendermsg.strip().split(" ")[1] not in oldusrlist:
              oldusrlist.add(sendermsg.strip().split(" ")[1])
              xfile("oldusrlistfile","w",oldusrlist)
            log(Style.NORMAL, sendermsg.strip().split(" ")[1] + " unbanned by " + senderusr)
          except:
            if loud == 0:
              message("I didn't ban " + sendermsg.strip().split(" ")[1] + ", but now unbanned")
            else:
              stalk(sendermsg.strip().split(" ")[1], "I didn't ban " + sendermsg.strip().split(" ")[1] + ", but now unbanned, seen ")
      
      # main ban logic goes below
      elif modaboos > 0:
        try:
          isbanned = 0
          # deleted ban logic
          if isbanned == 0:
            recentlog.append((senderusr,sendermsg))
            del recentlog[0]
            if nstage2 != 0:
              nstage2 -= 1
              if nphrase in sendermsg or stringcomp(nphrase,sendermsg) > 0.4 :
                ban("1800","")
                nuked.append(senderusr)
                nukedcount += 1
                log(Style.BRIGHT + Fore.RED, senderusr + ":nuked:" + sendermsg)
                time.sleep( 1 )
          # more deleted ban logic. Pretty complicated stuff, sad that I have to censor it.

    #debug portion of code, prints all input recieved
    #fo.write( data )
    #if data is not None:
      #log(Style.NORMAL,data)# + "\n" #comment out
    
    # responds to pings from server, also uses these to update the stream status as appropriate
    if data[0:4] == "ping":
      try:
        ws.send("PONG" + data[4:])
        if pingcount % 10 == 0:
          livestatus(0)
          with open("seenlogfile", 'wb') as filecontent:
            cPickle.dump(seenlog,filecontent)
        if pingcount % 200 == 0:
          requests.Session().get('http://www.destiny.gg/ping', headers={"Cookie": "sid=My secret cookie"})
          log(Fore.RED, "SESSION REFRESH")
        pingcount += 1
      except:
        error()
  
  # allows ctrl c to kill the while loop
  except (KeyboardInterrupt, SystemExit):
    with open("seenlogfile", 'wb') as filecontent:
      cPickle.dump(seenlog,filecontent)
    log(Back.RED,"seenlogfile has been saved.")
    raise
  except:
    error()