import os

from server.consts import FLASK_PORT

PORT = int(os.environ.get('PORT', FLASK_PORT))
