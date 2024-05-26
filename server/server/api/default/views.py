import asyncio

from flask import request, send_file, jsonify
from flask_restx import Resource, Namespace
from werkzeug.exceptions import NotFound

from server.api.common import api
from server.api.default import websocket
from server.api.default.models import ControlEngineCommand, ControlSteeringCommand, InitMap, InitInstance, \
    ControlPositionCommand
from server.db.dataops.frame import get_nth_frame_by_ids, insert_updated_vehicle_state, get_latest_frame_by_ids
from server.db.dataops.map import get_map, create_map, delete_map_by_id
from server.db.dataops.vehicle import create_vehicle_by_map_id, get_vehicle, delete_vehicle_by_id, \
    update_vehicle_from_msg
from server.utils import create_set_message, MessageSetType, create_init_map_message, create_init_instance_message, \
    create_delete_message

ns = Namespace('default', description='Default namespace')
api.add_namespace(ns, '/')


@ns.route('/is-alive')
class IsAlive(Resource):
    def get(self):
        return jsonify({"result": 'Flask is alive!'})


@ns.route('/version')
class Version(Resource):
    def get(self):
        return jsonify({"result": 'Flask is alive!'})


@ns.route('/update')
class Update(Resource):
    def post(self):
        message = request.data.decode()
        if message.startswith('screen'):
            insert_updated_vehicle_state(message)
        else:
            update_vehicle_from_msg(message)


@ns.route('/init/instance')
class InitInstance(Resource):
    @ns.response(201, 'Instance initialized successfully')
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
            if get_map(map_id) is None:
                return {'message': 'Invalid map id'}, 400
            response = websocket.send_and_get_message(create_init_instance_message(map_id, max_steer, max_engine,
                                                                                   pos_x, pos_y))
            if response.startswith('Invalid'):
                return {'message': response}, 400
            _, resp_map_id, resp_instance_id, msg = response.split(' ')
            vehicle = create_vehicle_by_map_id(resp_map_id)
            return {'message': f"map_id: {map_id}, vehicle_id: {vehicle.vehicle_id}"}, 201
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except NotFound:
            return {'message': 'Map not found'}, 404
        except Exception as e:
            return {'message': str(e)}, 503


@ns.route('/init/map')
class InitMap(Resource):
    @ns.response(201, 'Map initialized successfully')
    @ns.response(503, 'Failed to initialize map')
    @ns.expect(InitMap)
    def put(self):
        try:
            seed = request.get_json()['seed']
            if seed is None:
                seed = -1
            response = websocket.send_and_get_message(create_init_map_message(seed), 2)
            if "finished initialization" in response[1]:
                # map;<map_id>;finished initialization;<path>
                _, map_id, _, aerial_view_path = response[1].split(';')
                map_id = int(response[1].split(' ')[1])
                map_obj = create_map(map_id, seed, aerial_view_path)
                return {'message': f'Map {map_obj.id} initialized successfully'}, 201
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 503


@ns.route('/<int:map_id>/<int:instance_id>/steering')
class GetSteering(Resource):
    @ns.response(200, 'Control data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch control data')
    @ns.response(404, 'Vehicle not found')
    @ns.response(500, 'Failed to fetch control data')
    def get(self, map_id, instance_id):
        try:
            vehicle = get_vehicle(map_id, instance_id)
            return vehicle.steer, 200
        except ValueError as e:
            return {'message': str(e)}, 400
        except NotFound as e:
            return {'message': str(e)}, 404
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/<int:map_id>/<int:instance_id>/engine')
class GetEngine(Resource):
    @ns.response(200, 'Control data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch control data')
    @ns.response(404, 'Vehicle not found')
    @ns.response(500, 'Failed to fetch control data')
    def get(self, map_id, instance_id):
        try:
            vehicle = get_vehicle(map_id, instance_id)
            return vehicle.engine, 200
        except ValueError as e:
            return {'message': str(e)}, 400
        except NotFound as e:
            return {'message': str(e)}, 404
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/control/<int:map_id>/<int:instance_id>/engine')
class ControlEngine(Resource):
    @ns.response(200, 'Control command sent successfully')
    @ns.response(400, 'Invalid control command')
    @ns.response(500, 'Failed to send control command')
    @ns.expect(ControlEngineCommand)
    def post(self, map_id, instance_id):
        try:
            value = request.get_json()['engine']
            response = websocket.send_and_get_message(create_set_message(map_id, instance_id,
                                                                         MessageSetType.ENGINE, value))
            if response.startswith('Invalid'):
                return {'message': response}, 400
            return {'message': response}, 200
        except (ValueError, KeyError) as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/control/<int:map_id>/<int:instance_id>/steering')
class ControlSteering(Resource):
    @ns.response(200, 'Control command sent successfully')
    @ns.response(400, 'Invalid control command')
    @ns.response(500, 'Failed to send control command')
    @ns.expect(ControlSteeringCommand)
    def post(self, map_id, instance_id):
        try:
            value = request.get_json()['steering']
            response = websocket.send_and_get_message(create_set_message(map_id, instance_id,
                                                                         MessageSetType.STEER, value))
            if response.startswith('Invalid'):
                return {'message': response}, 400
            return {'message': response}, 200
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
    @ns.produces(['image/png'])
    def get(self, map_id, instance_id, camera_id, image_id):
        try:
            response = None
            vehicle_state_in_frame = get_nth_frame_by_ids(map_id=map_id, vehicle_id=instance_id, n=image_id)
            if vehicle_state_in_frame:
                if camera_id == 1:
                    response = vehicle_state_in_frame.path_camera1
                elif camera_id == 2:
                    response = vehicle_state_in_frame.path_camera2
                elif camera_id == 3:
                    response = vehicle_state_in_frame.path_camera3
                if response:
                    return send_file(response, mimetype='image/png')
            return {'message': 'Image data not found'}, 404
        except ValueError as e:
            return {'message': str(e)}, 400
        except NotFound as e:
            return {'message': str(e)}, 404
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/image/<int:map_id>/<int:instance_id>/<int:camera_id>')
class Image(Resource):
    @ns.response(200, 'Image data fetched successfully')
    @ns.response(400, 'Requirements not met to fetch image data')
    @ns.response(404, 'Image data not found')
    @ns.response(500, 'Failed to fetch image data')
    @ns.produces(['image/png'])
    def get(self, map_id, instance_id, camera_id):
        try:
            response = None
            latest_vehicle_state = get_latest_frame_by_ids(map_id=map_id, vehicle_id=instance_id)
            if latest_vehicle_state:
                if camera_id == 1:
                    response = latest_vehicle_state.path_camera1
                elif camera_id == 2:
                    response = latest_vehicle_state.path_camera2
                elif camera_id == 3:
                    response = latest_vehicle_state.path_camera3
                if response:
                    return send_file(response, mimetype='image/png')
            return {'message': 'Image data not found'}, 404
        except ValueError as e:
            return {'message': str(e)}, 400
        except NotFound as e:
            return {'message': str(e)}, 404
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/<int:map_id>/aerial')
class AerialView(Resource):
    @ns.response(200, 'Aerial view fetched successfully')
    @ns.response(400, 'Bad request')
    @ns.response(404, 'Map not found')
    @ns.response(500, 'Failed to fetch aerial view')
    @ns.produces(['image/png'])
    def get(self, map_id):
        try:
            map_obj = get_map(map_id)
            return send_file(map_obj.aerial_view_path, mimetype='image/png')
        except (ValueError, TypeError) as e:
            return {'message': str(e)}, 400
        except NotFound as e:
            return {'message': str(e)}, 404
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/delete/<int:map_id>/<int:instance_id>')
class DeleteInstance(Resource):
    @ns.response(204, 'Instance deleted successfully')
    @ns.response(400, 'Requirements not met to delete instance')
    @ns.response(500, 'Failed to delete instance')
    def delete(self, map_id, instance_id):
        try:
            response = websocket.send_and_get_message(create_delete_message(map_id, instance_id))
            if response.startswith('Invalid'):
                return {'message': response}, 400
            delete_vehicle_by_id(map_id, instance_id)
            return {'message': 'Instance deleted successfully'}, 204
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500


@ns.route('/delete/<int:map_id>')
class DeleteMap(Resource):
    @ns.response(204, 'Map deleted successfully')
    @ns.response(400, 'Requirements not met to delete map')
    @ns.response(500, 'Failed to delete map')
    def delete(self, map_id):
        try:
            response = websocket.send_and_get_message(create_delete_message(map_id))
            if response.startswith('Invalid'):
                return {'message': response}, 400
            delete_map_by_id(map_id)
            return {'message': 'Map deleted successfully'}, 204
        except ValueError as e:
            return {'message': str(e)}, 400
        except Exception as e:
            return {'message': str(e)}, 500
