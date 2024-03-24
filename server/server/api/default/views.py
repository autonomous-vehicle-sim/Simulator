from flask_restx import Resource

from server.api import api


@api.route('/is-alive')
def is_alive():
    return 'Flask is alive!'


@api.route('/version')
def version():
    return '0.1'
