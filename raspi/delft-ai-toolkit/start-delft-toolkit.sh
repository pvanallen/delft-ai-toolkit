#! /bin/sh
sudo killall -9 aplay # for audio 
cd delft-ai-toolkit
python3 delft_ai_toolkit.py --server_ip 192.168.1.21
