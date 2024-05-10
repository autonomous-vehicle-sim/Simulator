import os
import threading

from server.connection import WSConnection

websocket = WSConnection()


def start_ws_server():
    websocket.start()


if (os.environ.get('FLASK_DEBUG') == '0'
        or not os.environ.get('FLASK_DEBUG')
        or (os.environ.get('FLASK_DEBUG') == '1'
            and os.environ.get('WERKZEUG_RUN_MAIN') == 'true')):
    ws_thread = threading.Thread(target=websocket.start)
    ws_thread.start()
