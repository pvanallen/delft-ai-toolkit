#! /bin/bash
unityServerIP=$1
echo "connecting to unity server at $unityServerIP"
sudo killall -9 aplay # for audio
cd delft-ai-toolkit
python3 delft_ai_toolkit.py --server_ip "$unityServerIP"
