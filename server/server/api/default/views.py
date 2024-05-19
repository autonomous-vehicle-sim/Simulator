import asyncio

from flask import request, send_file
from flask_restx import Resource, Namespace

from server.api.common import api
from server.api.default import websocket
from server.api.default.models import ControlEngineCommand, ControlSteeringCommand, InitMap, InitInstance, \
    ControlPositionCommand
from server.db.dataops.frame import get_nth_frame
from server.utils import create_set_message, MessageSetType, MessageGetType, create_get_message, \
    create_init_map_message, create_init_instance_message, create_delete_message

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
    @ns.response(202, 'Instance initialized successfully')
    @ns.response(503, 'Failed to initialize connection')
    @ns.expect(InitInstance)
    def put(self):
        try:
            data = request.get_json()
            map_id = data['map_id']
            max_steer = data['max_steer']
            max_engine = data['max_engine']
            pos_x = data['pos_x'] or 0
            pos_y = data['pos_y'] or 0
            asyncio.run(websocket.send_message(create_init_instance_message(map_id, max_steer, max_engine,
                                                                            pos_x, pos_y)))
            return {'message': 'Init instance command sent successfully'}, 202
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 503


@ns.route('/init/map')
class InitMap(Resource):
    @ns.response(202, 'Map initialized successfully')
    @ns.response(503, 'Failed to initialize map')
    @ns.expect(InitMap)
    def put(self):
        try:
            seed = request.get_json()['seed']
            if seed is None:
                seed = -1
            asyncio.run(websocket.send_message(create_init_map_message(seed)))
            return {'message': 'Map init command sent successfully'}, 202
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 503


@ns.route('/<int:map_id>/<int:instance_id>/steering')
class GetSteering(Resource):
    @ns.response(200, 'Control data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch control data')
    @ns.response(500, 'Failed to fetch control data')
    def get(self, map_id, instance_id):
        try:
            return websocket.send_and_get_message(create_get_message(map_id, instance_id, MessageGetType.STEER)), 200
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/<int:map_id>/<int:instance_id>/engine')
class GetEngine(Resource):
    @ns.response(200, 'Control data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch control data')
    @ns.response(500, 'Failed to fetch control data')
    def get(self, map_id, instance_id):
        try:
            return websocket.send_and_get_message(create_get_message(map_id, instance_id, MessageGetType.ENGINE)), 200
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/control/<int:map_id>/<int:instance_id>/engine')
class ControlEngine(Resource):
    @ns.response(202, 'Control command sent successfully')
    @ns.response(400, 'Invalid control command')
    @ns.response(500, 'Failed to send control command')
    @ns.expect(ControlEngineCommand)
    def post(self, map_id, instance_id):
        try:
            value = request.get_json()['engine']
            asyncio.run(websocket.send_message(create_set_message(map_id, instance_id, MessageSetType.ENGINE, value)))
            return {'message': 'Control command sent successfully'}, 202
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/control/<int:map_id>/<int:instance_id>/steering')
class ControlSteering(Resource):
    @ns.response(202, 'Control command sent successfully')
    @ns.response(400, 'Invalid control command')
    @ns.response(500, 'Failed to send control command')
    @ns.expect(ControlSteeringCommand)
    def post(self, map_id, instance_id):
        try:
            value = request.get_json()['steering']
            asyncio.run(websocket.send_message(create_set_message(map_id, instance_id, MessageSetType.STEER, value)))
            return {'message': 'Control command sent successfully'}, 202
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/control/<int:map_id>/<int:instance_id>/position')
class ControlPosition(Resource):
    @ns.response(202, 'Control command sent successfully')
    @ns.response(400, 'Invalid control command')
    @ns.response(500, 'Failed to send control command')
    @ns.response(501, 'Not implemented')
    @ns.expect(ControlPositionCommand)
    def post(self, map_id, instance_id):
        try:
            value = request.get_json()['position']
            asyncio.run(websocket.send_message(create_set_message(map_id, instance_id, MessageSetType.POSITION, value)))
            return {'message': 'Control command sent successfully'}, 202
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except NotImplementedError as e:
            return {'message': str(e)}, 501
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/image/<int:map_id>/<int:instance_id>/<int:camera_id>/<int:image_id>')
class Image(Resource):
    @ns.response(200, 'Image data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch image data')
    @ns.response(404, 'Image data not found')
    @ns.response(500, 'Failed to fetch image data')
    @ns.produces(['image/jpeg'])
    def get(self, map_id, instance_id, camera_id, image_id):
        try:
            response = None
            frame = get_nth_frame(map_id=map_id, vehicle_id=instance_id, n=image_id)
            if frame:
                if camera_id == 1:
                    response = frame.path_camera1
                elif camera_id == 2:
                    response = frame.path_camera2
                elif camera_id == 3:
                    response = frame.path_camera3
                if response:
                    return send_file(response, mimetype='image/jpeg')
            return {'message': 'Image data not found'}, 404
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/delete/<int:map_id>/<int:instance_id>')
class DeleteInstance(Resource):
    @ns.response(202, 'Instance deleted successfully')
    @ns.response(400, 'Requirements not met to delete instance')
    @ns.response(500, 'Failed to delete instance')
    def delete(self, map_id, instance_id):
        try:
            asyncio.run(websocket.send_message(create_delete_message(map_id, instance_id)))
            return {'message': 'Instance delete command sent successfully'}, 202
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/delete/<int:map_id>')
class DeleteMap(Resource):
    @ns.response(202, 'Map deleted successfully')
    @ns.response(400, 'Requirements not met to delete map')
    @ns.response(500, 'Failed to delete map')
    def delete(self, map_id):
        try:
            asyncio.run(websocket.send_message(create_delete_message(map_id)))
            return {'message': 'Map delete command sent successfully'}, 202
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500
