from flask import request
from flask_restx import Resource, Namespace

from server.api.common import api #, only_one_int_arg
#from server.api.default.models import
# from server.utils import create_set_message

ns = Namespace('default', description='Default namespace')
api.add_namespace(ns, '/')


@ns.route('/is-alive')
class IsAlive(Resource):
    def get(self):
        return 'Flask is alive!'


@ns.route('/version')
class Version(Resource):
    def get(self):
        return '0.1'


@ns.route('/init/instance')
class InitInstance(Resource):
    @ns.response(200, 'Connection initialized successfully')
    @ns.response(503, 'Failed to initialize connection')
    def put(self):
        try:
            # zawoła funkcję łączącą się z symulatorem tworzącą nowy pojazd
            # zwraca informacje o pojeździe
            return {'message': 'Connection initialized successfully', 'params': {}}, 200
        except Exception as e:
            return {'message': str(e)}, 503


@ns.route('/init/map')
class InitMap(Resource):
    @ns.response(200, 'Map initialized successfully')
    @ns.response(503, 'Failed to initialize map')
    def put(self):
        try:
            # zawoła funkcję łączącą się z symulatorem tworzącą nową mapę
            # zwraca informacje o mapie
            return {'message': 'Map initialized successfully'}, 200
        except Exception as e:
            return {'message': str(e)}, 503


@ns.route('/control')
class Control(Resource):
    @ns.route('/engine')
    @ns.response(200, 'Control command sent successfully')
    @ns.response(400, 'Invalid control command')
    @ns.response(500, 'Failed to send control command')
    @ns.response(501, 'Control command not implemented')
    #@ns.expect(ControlCommand)
    def post(self):
        try:
            data = request.get_json().get('engine')
            #send_message(create_set_message())
            return {'message': 'Control command sent successfully'}, 200
        except ValueError as e:
            return {'message': str(e)}, 400
        except NotImplementedError as e:
            return {'message': str(e)}, 501
        except Exception as e:
            return {'message': str(e)}, 500

    @ns.response(200, 'Control data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch control data')
    @ns.response(500, 'Failed to fetch control data')
    def get(self):
        try:
            # zawoła funkcję zwracającą aktualne dane o pojeździe
            # zwróć dane w formacie JSON
            return {'speed': 0.5, 'steering': 0.0}, 200
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/image')
class Image(Resource):
    @ns.response(200, 'Image data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch image data')
    @ns.response(404, 'Image data not found')
    @ns.response(500, 'Failed to fetch image data')
    def get(self):
        try:
            # zawoła funkcję zwracającą obraz z kamery
            # zwróć obraz w formacie base64
            return {'image': 'base64'}, 200
        except ValueError as e:
            return {'message': str(e)}, 400
        except FileNotFoundError as e:
            return {'message': str(e)}, 404
        except Exception as e:
            return {'message': str(e)}, 500
