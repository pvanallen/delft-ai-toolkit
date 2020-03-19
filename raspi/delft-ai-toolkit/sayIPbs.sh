#! /bin/sh
# /etc/init.d/sayIPbs
## Some things that run always
# Carry out specific functions when asked to by the system
case "$1" in  start)
    echo "Starting script say IP address "
    sudo killall -9 aplay # for audio
    sleep 4
    ip=`hostname -I`
    [[ "$ip" =~ ${ip//?/(.)} ]]       # splits into array
    printf -v var "%s " "${BASH_REMATCH[@]:1}"
    string="My IP address is, $var"
    echo $string
    pico2wave -l "en-US" -w /home/pi/delft-ai-toolkit/audio/speaknow.wav "$string" && play -q -V1 /home/pi/delft-ai-toolkit/audio/speaknow.wav
    #pico2wave -l "en-US" -w /home/pi/delft-ai-toolkit/audio/speaknow.wav "$string" && sox /home/pi/delft-ai-toolkit/audio/speaknow.wav && aplay /home/pi/delft-ai-toolkit/audio/speaknowstereo.wav
    sleep 3
#    aplay /home/pi/delft-ai-toolkit/audio/speaknowstereo.wav
    play -q -V1 /home/pi/delft-ai-toolkit/audio/speaknow.wav
    ;;  stop)
echo "Stopping script sayIPbs"
    ;;  *)
    echo "Usage: /etc/init.d/sayIPbs {start|stop}"
    exit 1
    ;;esac
exit 0
